using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Machi.Core;

static class Program
{
    static int _fail, _pass;

    static void Check(bool cond, string what)
    {
        if (cond) _pass++; else { _fail++; Console.WriteLine("FAIL: " + what); }
    }

    class FixedRandom : IRandom
    {
        readonly Queue<int> _q;
        public FixedRandom(params int[] v) { _q = new Queue<int>(v); }
        public int Next(int max) => _q.Count > 0 ? _q.Dequeue() % max : 0;
    }

    static GameConfig LoadConfig()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, "Assets"))) dir = Path.GetDirectoryName(dir);
        var path = Path.Combine(dir, "Assets/Machi/Resources/Config/game_config.json");
        return JsonSerializer.Deserialize<GameConfig>(File.ReadAllText(path), new JsonSerializerOptions { IncludeFields = true });
    }

    static int Main()
    {
        var cfg = new ConfigIndex(LoadConfig());
        var errors = cfg.Validate();
        foreach (var e in errors) Console.WriteLine("config: " + e);
        Check(errors.Count == 0, "shipped config validates");

        // --- merge rules
        var g = new Game(cfg, new FixedRandom(0, 0, 0, 0, 0, 0, 0, 0));
        g.NewGame(1000);
        var b = g.Board;
        Check(b.Get(3, 4) == "gen_toolbox", "starting generator placed");
        Check(g.MoveItem(2, 4, 4, 4) == MoveResult.Merged, "two tool_01 merge");
        Check(b.Get(4, 4) == "tool_02" && b.Get(2, 4) == null, "merge result in target cell");
        Check(g.MoveItem(4, 4, 0, 0) == MoveResult.Moved, "move to empty");
        Check(g.MoveItem(0, 0, 3, 4) == MoveResult.Swapped, "different items swap");
        Check(b.Get(0, 0) == "gen_toolbox" && b.Get(3, 4) == "tool_02", "swap contents");
        Check(g.MoveItem(0, 0, 0, 0) == MoveResult.Invalid, "same cell invalid");
        b.Set(5, 5, "tool_05"); b.Set(6, 5, "tool_05");
        Check(g.MoveItem(5, 5, 6, 5) == MoveResult.Swapped, "max level does not merge");

        // --- generator
        int e0 = g.Energy;
        Check(g.TapGenerator(0, 0, out var ox, out var oy) == TapResult.Produced, "generator produces");
        Check(g.Energy == e0 - 1 && b.Get(ox, oy) == "tool_01", "energy spent, roll picks first weight");
        Check(Math.Abs(ox - 0) + Math.Abs(oy - 0) == 1, "drops next to generator");
        Check(g.TapGenerator(3, 4, out _, out _) == TapResult.NotGenerator, "item is not generator");

        // --- orders and restoration (First Hour loop)
        var active = g.ActiveOrders().Select(o => o.id).ToList();
        Check(active.SequenceEqual(new[] { "o_office" }), "only first order active at start: " + string.Join(",", active));
        Check(g.Fulfill("o_office"), "fulfil o_office with tool_02 on board");
        Check(g.Materials == 2 && g.Coins == 10, "rewards granted");
        Check(!g.Fulfill("o_office"), "cannot fulfil twice");
        Check(g.ActiveOrders().Count == 0, "sign order locked until office restored");
        Check(g.NextRestoreTarget() == "office_hut", "hint points at office");
        Check(g.Restore("office_hut") == RestoreResult.Restored && g.GetStage("office_hut") == 1, "restore office");
        Check(g.Revival == 1 && g.Materials == 0, "revival up, materials spent");
        Check(g.Restore("office_hut") == RestoreResult.MaxStage, "office max stage");
        Check(g.ActiveOrders().Any(o => o.id == "o_sign"), "sign order unlocked");
        Check(g.NextRestoreTarget() == "station_sign", "hint moves on to the sign");
        Check(g.Restore("station_sign") == RestoreResult.NotEnoughMaterials, "not enough materials");
        Check(!g.Fulfill("o_sign"), "cannot fulfil without items");

        // --- board hints
        var gh = new Game(cfg, new FixedRandom());
        gh.NewGame(0);
        foreach (var c in gh.Board.All().ToList()) gh.Board.Set(c.x, c.y, null);
        gh.Board.Set(0, 0, "gen_toolbox");
        var hint = gh.FindHint();
        Check(hint.Kind == HintKind.Generator && hint.Ax == 0 && hint.Ay == 0, "empty board points at generator");
        gh.Board.Set(2, 2, "tool_01");
        gh.Board.Set(4, 3, "tool_01");
        hint = gh.FindHint();
        Check(hint.Kind == HintKind.Merge && hint.Ax == 2 && hint.Ay == 2 && hint.Bx == 4 && hint.By == 3, "pair of tool_01 suggested");
        gh.Board.Set(4, 3, "tool_02");
        Check(gh.FindHint().Kind == HintKind.Generator, "single items give no merge hint");
        Check(gh.NeededItems().Contains("tool_02"), "first order needs tool_02");
        gh.Board.Set(5, 3, "tool_02");
        Check(gh.FindHint().Kind == HintKind.Generator, "keeps the tool_02 the order needs");
        gh.Board.Set(6, 3, "tool_02");
        Check(gh.FindHint().Kind == HintKind.Merge, "spare tool_02 pair can merge");

        // --- region lock
        Check(!g.IsRegionUnlocked("R3_public_bath") && g.IsRegionUnlocked("R1_station"), "regions at start");
        Check(g.UnlockRegion("R3_public_bath") && !g.UnlockRegion("R3_public_bath"), "unlock once");

        // --- energy regen
        var g2 = new Game(cfg, new FixedRandom());
        g2.NewGame(0);
        g2.Board.Set(0, 0, "gen_toolbox");
        for (int i = 0; i < 5; i++) g2.Board.Set(1 + i, 0, null);
        g2.TapGenerator(0, 0, out _, out _); g2.TapGenerator(0, 0, out _, out _);
        Check(g2.Energy == 98, "energy 98 after two taps");
        g2.TickEnergy(119); Check(g2.Energy == 98, "no regen before interval");
        g2.TickEnergy(240); Check(g2.Energy == 100, "regen to max");
        g2.TickEnergy(100000); Check(g2.Energy == 100, "capped at max");

        // --- save / load round trip (through JSON like Unity does)
        var json = JsonSerializer.Serialize(g.ToSave(), new JsonSerializerOptions { IncludeFields = true });
        var s = JsonSerializer.Deserialize<SaveData>(json, new JsonSerializerOptions { IncludeFields = true });
        var g3 = new Game(cfg, new FixedRandom());
        g3.Load(s, 1000);
        Check(g3.GetStage("office_hut") == 1 && g3.IsOrderDone("o_office"), "stages and orders persist");
        Check(g3.Board.Snapshot().SequenceEqual(g.Board.Snapshot()), "board persists");
        Check(g3.Materials == g.Materials && g3.Revival == g.Revival && g3.Coins == g.Coins, "wallet persists");
        Check(g3.IsRegionUnlocked("R3_public_bath"), "regions persist");

        // --- full Vertical Slice is completable with the shipped config
        Check(PlayThrough(cfg), "Ch1 config can be played to the end");

        Console.WriteLine($"{_pass} passed, {_fail} failed");
        return _fail == 0 ? 0 : 1;
    }

    /// <summary>Greedy bot: taps generator, merges up, fulfils orders, restores buildings.</summary>
    static bool PlayThrough(ConfigIndex cfg)
    {
        var g = new Game(cfg, new SystemRandom(42));
        long now = 0;
        g.NewGame(now);
        for (int step = 0; step < 5000; step++)
        {
            now += 30;
            g.TickEnergy(now);
            foreach (var o in g.ActiveOrders()) g.Fulfill(o.id);
            foreach (var bid in cfg.Buildings.Keys) g.Restore(bid);
            if (cfg.Buildings.Keys.All(id => g.GetStage(id) == g.MaxStage(id))) return true;
            if (MergeOnce(g)) continue;
            var gen = g.Board.All().First(c => cfg.IsGenerator(c.id));
            if (g.TapGenerator(gen.x, gen.y, out _, out _) == TapResult.BoardFull)
            {
                // sell the lowest item that no order needs right now (simple stand-in for a sell button)
                var needed = new HashSet<string>(g.ActiveOrders().SelectMany(o => o.requires).Select(r => r.item));
                var junk = g.Board.All().Where(c => !cfg.IsGenerator(c.id) && !needed.Contains(c.id)).ToList();
                if (junk.Count == 0) return false;
                g.Board.Set(junk[0].x, junk[0].y, null);
            }
        }
        Console.WriteLine("playthrough stuck: " + string.Join(",", cfg.Buildings.Keys.Select(id => id + "=" + g.GetStage(id))));
        return false;
    }

    static bool MergeOnce(Game g)
    {
        var needed = g.ActiveOrders().SelectMany(o => o.requires).ToList();
        var cells = g.Board.All().ToList();
        foreach (var grp in cells.GroupBy(c => c.id))
        {
            if (!g.Cfg.Items.TryGetValue(grp.Key, out var def) || string.IsNullOrEmpty(def.next)) continue;
            // keep items an order still needs at this level
            var need = needed.Where(r => r.item == grp.Key).Sum(r => r.count);
            var list = grp.ToList();
            if (list.Count - need < 2) continue;
            g.MoveItem(list[0].x, list[0].y, list[1].x, list[1].y);
            return true;
        }
        return false;
    }
}
