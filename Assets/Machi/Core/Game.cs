using System;
using System.Collections.Generic;
using System.Linq;

namespace Machi.Core
{
    public enum TapResult { NotGenerator, NoEnergy, BoardFull, Produced }

    public enum HintKind { None, Merge, Generator }

    /// <summary>What the board should nudge the player toward: a pair to merge (A + B) or a generator (A).</summary>
    public struct BoardHint
    {
        public HintKind Kind;
        public int Ax, Ay, Bx, By;
    }
    public enum RestoreResult { Unknown, MaxStage, RegionLocked, NotEnoughMaterials, Restored }

    /// <summary>
    /// The whole game state and its rules. Unity views only call into this and listen to events;
    /// they never change state directly. That keeps the 2D board and the 3D town in sync.
    /// </summary>
    public class Game
    {
        public readonly ConfigIndex Cfg;
        public readonly MergeBoard Board;
        readonly IRandom _rng;

        public int Coins { get; private set; }
        public int Materials { get; private set; }
        public int Energy { get; private set; }
        public long EnergyUpdatedAt { get; private set; }
        public int Revival { get; private set; }

        readonly Dictionary<string, int> _stages = new Dictionary<string, int>();
        readonly HashSet<string> _completedOrders = new HashSet<string>();
        readonly HashSet<string> _unlockedRegions = new HashSet<string>();

        public event Action WalletChanged;
        public event Action OrdersChanged;
        public event Action<string, int> BuildingStageChanged;   // building id, new stage
        public event Action<string> RegionUnlocked;

        public Game(ConfigIndex cfg, IRandom rng)
        {
            Cfg = cfg;
            _rng = rng;
            Board = new MergeBoard(cfg, cfg.Config.boardWidth, cfg.Config.boardHeight);
        }

        // ------------------------------------------------------------ lifecycle
        public void NewGame(long now)
        {
            var c = Cfg.Config;
            Coins = c.startCoins;
            Materials = c.startMaterials;
            Energy = c.startEnergy;
            EnergyUpdatedAt = now;
            Revival = 0;
            _stages.Clear();
            _completedOrders.Clear();
            _unlockedRegions.Clear();
            foreach (var r in Cfg.Regions.Values) if (r.unlockedAtStart) _unlockedRegions.Add(r.id);
            Board.Restore(null);
            foreach (var cell in c.startingBoard ?? new BoardCell[0])
                if (Board.InBounds(cell.x, cell.y)) Board.Set(cell.x, cell.y, cell.item);
            RaiseAll();
        }

        public void Load(SaveData s, long now)
        {
            Coins = s.coins;
            Materials = s.materials;
            Energy = s.energy;
            EnergyUpdatedAt = s.energyUpdatedAt;
            Revival = s.revival;
            _stages.Clear();
            foreach (var e in s.buildingStages ?? new StageEntry[0]) _stages[e.id] = e.stage;
            _completedOrders.Clear();
            foreach (var o in s.completedOrders ?? new string[0]) _completedOrders.Add(o);
            _unlockedRegions.Clear();
            foreach (var r in s.unlockedRegions ?? new string[0]) _unlockedRegions.Add(r);
            Board.Restore(s.board);
            TickEnergy(now);
            RaiseAll();
        }

        public SaveData ToSave()
        {
            return new SaveData
            {
                board = Board.Snapshot(),
                coins = Coins,
                materials = Materials,
                energy = Energy,
                energyUpdatedAt = EnergyUpdatedAt,
                revival = Revival,
                buildingStages = _stages.Select(kv => new StageEntry { id = kv.Key, stage = kv.Value }).ToArray(),
                completedOrders = _completedOrders.ToArray(),
                unlockedRegions = _unlockedRegions.ToArray(),
            };
        }

        void RaiseAll()
        {
            WalletChanged?.Invoke();
            OrdersChanged?.Invoke();
            foreach (var b in Cfg.Buildings.Keys) BuildingStageChanged?.Invoke(b, GetStage(b));
        }

        // ------------------------------------------------------------ energy
        /// <summary>Regenerate energy for the time passed. Call every frame or on resume.</summary>
        public void TickEnergy(long now)
        {
            var c = Cfg.Config;
            if (Energy >= c.maxEnergy || c.energyRegenSeconds <= 0) { EnergyUpdatedAt = now; return; }
            long elapsed = now - EnergyUpdatedAt;
            if (elapsed < c.energyRegenSeconds) return;
            int gained = (int)(elapsed / c.energyRegenSeconds);
            Energy = Math.Min(c.maxEnergy, Energy + gained);
            EnergyUpdatedAt = Energy >= c.maxEnergy ? now : EnergyUpdatedAt + (long)gained * c.energyRegenSeconds;
            WalletChanged?.Invoke();
        }

        // ------------------------------------------------------------ board
        /// <summary>Tap a generator at (x,y): spends energy and drops an item in the nearest empty cell.</summary>
        public TapResult TapGenerator(int x, int y, out int outX, out int outY)
        {
            outX = outY = -1;
            var id = Board.Get(x, y);
            if (!Cfg.Generators.TryGetValue(id ?? "", out var gen)) return TapResult.NotGenerator;
            if (Energy < gen.energyCost) return TapResult.NoEnergy;
            if (!Board.TryFindEmptyNear(x, y, out outX, out outY)) return TapResult.BoardFull;
            Energy -= gen.energyCost;
            Board.Set(outX, outY, Roll(gen));
            WalletChanged?.Invoke();
            return TapResult.Produced;
        }

        string Roll(GeneratorDef gen)
        {
            int total = 0;
            foreach (var o in gen.outputs) total += Math.Max(0, o.weight);
            int r = _rng.Next(Math.Max(1, total));
            foreach (var o in gen.outputs)
            {
                r -= Math.Max(0, o.weight);
                if (r < 0) return o.item;
            }
            return gen.outputs[0].item;
        }

        /// <summary>Item ids that an active order still asks for.</summary>
        public HashSet<string> NeededItems()
        {
            var set = new HashSet<string>();
            foreach (var o in ActiveOrders())
                foreach (var r in o.requires ?? new ItemCount[0]) set.Add(r.item);
            return set;
        }

        /// <summary>
        /// A merge that does not eat items an order is waiting for, else a generator that can produce now.
        /// </summary>
        public BoardHint FindHint()
        {
            var need = new Dictionary<string, int>();
            foreach (var o in ActiveOrders())
                foreach (var r in o.requires ?? new ItemCount[0])
                    need[r.item] = (need.TryGetValue(r.item, out var n) ? n : 0) + r.count;

            var seen = new Dictionary<string, (int x, int y)>();
            foreach (var (x, y, id) in Board.All())
            {
                if (!Cfg.Items.TryGetValue(id, out var def) || string.IsNullOrEmpty(def.next)) continue;
                if (Board.Count(id) - (need.TryGetValue(id, out var keep) ? keep : 0) < 2) continue;
                if (seen.TryGetValue(id, out var a))
                    return new BoardHint { Kind = HintKind.Merge, Ax = a.x, Ay = a.y, Bx = x, By = y };
                seen[id] = (x, y);
            }

            if (Board.CountEmpty() > 0)
                foreach (var (x, y, id) in Board.All())
                    if (Cfg.Generators.TryGetValue(id, out var gen) && Energy >= gen.energyCost)
                        return new BoardHint { Kind = HintKind.Generator, Ax = x, Ay = y, Bx = -1, By = -1 };

            return new BoardHint { Kind = HintKind.None, Ax = -1, Ay = -1, Bx = -1, By = -1 };
        }

        public MoveResult MoveItem(int fx, int fy, int tx, int ty)
        {
            var r = Board.Move(fx, fy, tx, ty);
            if (r != MoveResult.Invalid) OrdersChanged?.Invoke();  // fulfillability may change
            return r;
        }

        // ------------------------------------------------------------ orders
        public bool IsOrderDone(string id) => _completedOrders.Contains(id);

        public bool IsOrderAvailable(OrderDef o)
        {
            if (_completedOrders.Contains(o.id)) return false;
            if (!string.IsNullOrEmpty(o.prereqOrder) && !_completedOrders.Contains(o.prereqOrder)) return false;
            if (!string.IsNullOrEmpty(o.prereqBuilding) && GetStage(o.prereqBuilding) < o.prereqStage) return false;
            return true;
        }

        /// <summary>Orders the player can see now, in config order.</summary>
        public List<OrderDef> ActiveOrders()
        {
            var list = new List<OrderDef>();
            foreach (var o in Cfg.Config.orders ?? new OrderDef[0])
                if (IsOrderAvailable(o)) list.Add(o);
            return list;
        }

        public bool CanFulfill(OrderDef o)
        {
            foreach (var r in o.requires ?? new ItemCount[0])
                if (Board.Count(r.item) < r.count) return false;
            return true;
        }

        public bool Fulfill(string orderId)
        {
            if (!Cfg.Orders.TryGetValue(orderId, out var o) || !IsOrderAvailable(o) || !CanFulfill(o)) return false;
            foreach (var r in o.requires ?? new ItemCount[0]) Board.TryRemove(r.item, r.count);
            Coins += o.rewardCoins;
            Materials += o.rewardMaterials;
            _completedOrders.Add(o.id);
            WalletChanged?.Invoke();
            OrdersChanged?.Invoke();
            return true;
        }

        // ------------------------------------------------------------ town
        public int GetStage(string buildingId) => _stages.TryGetValue(buildingId, out var s) ? s : 0;

        public int MaxStage(string buildingId) =>
            Cfg.Buildings.TryGetValue(buildingId, out var b) && b.stages != null ? b.stages.Length : 0;

        public bool IsRegionUnlocked(string regionId) => _unlockedRegions.Contains(regionId);

        /// <summary>Cost to reach the next stage, or -1 if already at max / unknown.</summary>
        public int NextStageCost(string buildingId)
        {
            if (!Cfg.Buildings.TryGetValue(buildingId, out var b)) return -1;
            int s = GetStage(buildingId);
            return b.stages != null && s < b.stages.Length ? b.stages[s].cost : -1;
        }

        /// <summary>
        /// The building to restore next: the one a locked order waits on (its other prerequisites
        /// met), else any unfinished building in an open region. Null when nothing is left.
        /// </summary>
        public string NextRestoreTarget()
        {
            foreach (var o in Cfg.Config.orders ?? new OrderDef[0])
            {
                if (_completedOrders.Contains(o.id) || string.IsNullOrEmpty(o.prereqBuilding)) continue;
                if (!string.IsNullOrEmpty(o.prereqOrder) && !_completedOrders.Contains(o.prereqOrder)) continue;
                if (GetStage(o.prereqBuilding) < o.prereqStage) return o.prereqBuilding;
            }
            foreach (var b in Cfg.Config.buildings ?? new BuildingDef[0])
                if (IsRegionUnlocked(b.region) && GetStage(b.id) < MaxStage(b.id)) return b.id;
            return null;
        }

        public RestoreResult Restore(string buildingId)
        {
            if (!Cfg.Buildings.TryGetValue(buildingId, out var b)) return RestoreResult.Unknown;
            int s = GetStage(buildingId);
            if (b.stages == null || s >= b.stages.Length) return RestoreResult.MaxStage;
            if (!IsRegionUnlocked(b.region)) return RestoreResult.RegionLocked;
            var st = b.stages[s];
            if (Materials < st.cost) return RestoreResult.NotEnoughMaterials;
            Materials -= st.cost;
            Revival += st.revival;
            _stages[buildingId] = s + 1;
            WalletChanged?.Invoke();
            BuildingStageChanged?.Invoke(buildingId, s + 1);
            OrdersChanged?.Invoke();  // new orders may unlock
            return RestoreResult.Restored;
        }

        public bool UnlockRegion(string regionId)
        {
            if (!Cfg.Regions.ContainsKey(regionId) || !_unlockedRegions.Add(regionId)) return false;
            RegionUnlocked?.Invoke(regionId);
            return true;
        }
    }
}
