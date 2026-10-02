using System;
using System.Collections.Generic;

namespace Machi.Core
{
    /// <summary>Lookup tables over a GameConfig, plus validation.</summary>
    public class ConfigIndex
    {
        public readonly GameConfig Config;
        public readonly Dictionary<string, ItemDef> Items = new Dictionary<string, ItemDef>();
        public readonly Dictionary<string, GeneratorDef> Generators = new Dictionary<string, GeneratorDef>();
        public readonly Dictionary<string, OrderDef> Orders = new Dictionary<string, OrderDef>();
        public readonly Dictionary<string, BuildingDef> Buildings = new Dictionary<string, BuildingDef>();
        public readonly Dictionary<string, RegionDef> Regions = new Dictionary<string, RegionDef>();

        public ConfigIndex(GameConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            foreach (var d in config.items ?? new ItemDef[0]) Items[d.id] = d;
            foreach (var d in config.generators ?? new GeneratorDef[0]) Generators[d.id] = d;
            foreach (var d in config.orders ?? new OrderDef[0]) Orders[d.id] = d;
            foreach (var d in config.buildings ?? new BuildingDef[0]) Buildings[d.id] = d;
            foreach (var d in config.regions ?? new RegionDef[0]) Regions[d.id] = d;
        }

        public bool IsGenerator(string id) => !string.IsNullOrEmpty(id) && Generators.ContainsKey(id);

        /// <summary>Returns a list of problems; empty means the config is consistent.</summary>
        public List<string> Validate()
        {
            var errors = new List<string>();
            foreach (var it in Items.Values)
                if (!string.IsNullOrEmpty(it.next) && !Items.ContainsKey(it.next))
                    errors.Add($"item {it.id}: next '{it.next}' not found");
            foreach (var g in Generators.Values)
            {
                if (g.outputs == null || g.outputs.Length == 0) errors.Add($"generator {g.id}: no outputs");
                else foreach (var o in g.outputs)
                    if (!Items.ContainsKey(o.item)) errors.Add($"generator {g.id}: output '{o.item}' not found");
            }
            foreach (var o in Orders.Values)
            {
                foreach (var r in o.requires ?? new ItemCount[0])
                    if (!Items.ContainsKey(r.item)) errors.Add($"order {o.id}: item '{r.item}' not found");
                if (!string.IsNullOrEmpty(o.prereqBuilding) && !Buildings.ContainsKey(o.prereqBuilding))
                    errors.Add($"order {o.id}: building '{o.prereqBuilding}' not found");
                if (!string.IsNullOrEmpty(o.prereqOrder) && !Orders.ContainsKey(o.prereqOrder))
                    errors.Add($"order {o.id}: prereq order '{o.prereqOrder}' not found");
            }
            foreach (var b in Buildings.Values)
                if (!Regions.ContainsKey(b.region)) errors.Add($"building {b.id}: region '{b.region}' not found");
            foreach (var c in Config.startingBoard ?? new BoardCell[0])
                if (!Items.ContainsKey(c.item) && !Generators.ContainsKey(c.item))
                    errors.Add($"startingBoard ({c.x},{c.y}): '{c.item}' not found");
            return errors;
        }
    }
}
