using System.Collections.Generic;
using Machi.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Machi
{
    /// <summary>Row of order cards above the board. Rebuilt whenever orders or the board change.</summary>
    public class OrdersView : MonoBehaviour
    {
        Game _game;
        ItemVisuals _visuals;
        RectTransform _row;
        Text _empty;
        bool _dirty = true;

        public void Build(Game game, ItemVisuals visuals, RectTransform area)
        {
            _game = game;
            _visuals = visuals;
            _row = UiKit.Rect("Orders", area).Anchor(0, 0, 1, 1);
            _empty = UiKit.Label("Empty", area, "暂时没有委托。去小镇看看吧。", 30, UiKit.Ink);
            ((RectTransform)_empty.transform).Anchor(0, 0, 1, 1);
            game.OrdersChanged += MarkDirty;
            game.Board.CellChanged += OnCell;
        }

        void OnDestroy()
        {
            if (_game == null) return;
            _game.OrdersChanged -= MarkDirty;
            _game.Board.CellChanged -= OnCell;
        }

        void OnCell(int x, int y) => _dirty = true;
        void MarkDirty() => _dirty = true;

        void LateUpdate()
        {
            if (!_dirty || _row == null) return;
            _dirty = false;
            Rebuild();
        }

        void Rebuild()
        {
            for (int i = _row.childCount - 1; i >= 0; i--) Destroy(_row.GetChild(i).gameObject);
            var orders = _game.ActiveOrders();
            _empty.gameObject.SetActive(orders.Count == 0);
            if (orders.Count == 0) _empty.text = EmptyHint();
            int n = Mathf.Max(1, Mathf.Min(orders.Count, 3));
            for (int i = 0; i < orders.Count && i < 3; i++)
                Card(orders[i], i, n);
        }

        string EmptyHint()
        {
            var id = _game.NextRestoreTarget();
            if (id == null) return "暂时没有委托。";
            int cost = _game.NextStageCost(id), have = _game.Materials;
            var name = _game.Cfg.Buildings[id].name;
            return have >= cost
                ? $"暂时没有委托。点「回到小镇」，修复「{name}」吧（需要修复材料 {cost}，现有 {have}）"
                : $"暂时没有委托。下一步是修复「{name}」（需要修复材料 {cost}，现有 {have}）";
        }

        void Card(OrderDef o, int index, int count)
        {
            float w = 1f / count;
            var card = UiKit.Panel("Order_" + o.id, _row, UiKit.Cream);
            ((RectTransform)card.transform).Anchor(index * w + 0.01f, 0.04f, (index + 1) * w - 0.01f, 0.96f);

            var who = UiKit.Label("Npc", card.transform, o.npc, 30, Color.white);
            var whoBg = UiKit.Panel("NpcBg", card.transform, UiKit.Vermilion);
            ((RectTransform)whoBg.transform).Anchor(0.05f, 0.78f, 0.45f, 0.96f);
            who.transform.SetParent(whoBg.transform, false);
            ((RectTransform)who.transform).Anchor(0, 0, 1, 1);

            var title = UiKit.Label("Title", card.transform, o.title, 26, UiKit.Ink, TextAnchor.UpperLeft);
            ((RectTransform)title.transform).Anchor(0.05f, 0.5f, 0.95f, 0.76f);

            var lines = new List<string>();
            foreach (var r in o.requires)
            {
                int have = Mathf.Min(_game.Board.Count(r.item), r.count);
                lines.Add($"{_visuals.DisplayName(r.item)} {have}/{r.count}");
            }
            var req = UiKit.Label("Req", card.transform, string.Join("\n", lines), 24, UiKit.WoodDark, TextAnchor.UpperLeft);
            ((RectTransform)req.transform).Anchor(0.05f, 0.22f, 0.95f, 0.5f);

            bool ok = _game.CanFulfill(o);
            var btn = UiKit.Button("Give", card.transform, ok ? $"交付  +{o.rewardMaterials}材料" : "收集中",
                                   ok ? UiKit.Vermilion : Color.gray, () => _game.Fulfill(o.id), 24);
            ((RectTransform)btn.transform).Anchor(0.05f, 0.04f, 0.95f, 0.2f);
            btn.interactable = ok;
        }
    }
}
