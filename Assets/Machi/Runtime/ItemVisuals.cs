using System.Collections.Generic;
using Machi.Core;
using UnityEngine;

namespace Machi
{
    /// <summary>
    /// Sprite for an item or generator id. Uses Resources/Items/{id} when the 2D art exists,
    /// otherwise a coloured placeholder with the item's name and level.
    /// </summary>
    public class ItemVisuals
    {
        readonly ConfigIndex _cfg;
        readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        static readonly Dictionary<string, Color> ChainColors = new Dictionary<string, Color>
        {
            { "tool", UiKit.Hex("6FA3B5") },
            { "food", UiKit.Hex("D9A940") },
            { "inn", UiKit.Hex("9DB0D9") },
            { "material", UiKit.Hex("B9835A") },
        };

        public ItemVisuals(ConfigIndex cfg) { _cfg = cfg; }

        /// <summary>Real sprite if one exists, else null (caller shows placeholder).</summary>
        public Sprite Sprite(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!_cache.TryGetValue(id, out var s))
            {
                s = Resources.Load<Sprite>("Items/" + id);
                _cache[id] = s;
            }
            return s;
        }

        public Color PlaceholderColor(string id)
        {
            if (_cfg.IsGenerator(id)) return UiKit.WoodDark;
            if (_cfg.Items.TryGetValue(id, out var d) && ChainColors.TryGetValue(d.chain, out var c))
                return Color.Lerp(c, Color.white, 0.35f - 0.07f * d.level);
            return Color.gray;
        }

        public string PlaceholderText(string id)
        {
            if (_cfg.Generators.TryGetValue(id, out var g)) return g.name + "\n⚡" + g.energyCost;
            if (_cfg.Items.TryGetValue(id, out var d)) return d.name + "\nLv" + d.level;
            return id;
        }

        public string DisplayName(string id)
        {
            if (_cfg.Generators.TryGetValue(id ?? "", out var g)) return g.name;
            if (_cfg.Items.TryGetValue(id ?? "", out var d)) return d.name;
            return id;
        }
    }
}
