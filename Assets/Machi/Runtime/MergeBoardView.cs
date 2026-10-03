using System.Collections.Generic;
using Machi.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Machi
{
    /// <summary>Draws the merge board and turns taps / drags into Game calls.</summary>
    public class MergeBoardView : MonoBehaviour
    {
        Game _game;
        ItemVisuals _visuals;
        RectTransform _grid;
        RectTransform _dragLayer;
        CellView[] _cells;
        Text _info;
        RectTransform _arrow;
        float _idle;
        bool _hintsDirty = true;
        BoardHint _hint;

        const float HintDelay = 3f;

        public void Build(Game game, ItemVisuals visuals, RectTransform area, RectTransform dragLayer)
        {
            _game = game;
            _visuals = visuals;
            _dragLayer = dragLayer;
            var b = game.Board;

            _info = UiKit.Label("Info", area, "点击工具箱产出物品，拖动相同物品合成", 30, UiKit.Washi);
            ((RectTransform)_info.transform).Anchor(0, 0.94f, 1, 1);
            _info.gameObject.AddComponent<Outline>().effectColor = UiKit.WoodFrame;

            // Grid scaled to fit the space below the info line (leaves room for the 16px frame).
            var fit = UiKit.Rect("GridArea", area).Anchor(0.02f, 0.01f, 0.98f, 0.93f);
            fit.offsetMin = new Vector2(16, 16);
            fit.offsetMax = new Vector2(-16, -16);
            _grid = UiKit.Rect("Grid", fit);
            var aspect = _grid.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspect.aspectRatio = (float)b.Width / b.Height;
            // Wooden door: a dark outer frame, then lattice bars showing through the gaps between washi cells.
            float gap = 4f;
            var frame = UiKit.Panel("Frame", _grid, UiKit.WoodFrame);
            ((RectTransform)frame.transform).Anchor(0, 0, 1, 1);
            ((RectTransform)frame.transform).offsetMin = new Vector2(-16, -16);
            ((RectTransform)frame.transform).offsetMax = new Vector2(16, 16);
            var bars = UiKit.Panel("Lattice", _grid, UiKit.Lattice);
            ((RectTransform)bars.transform).Anchor(0, 0, 1, 1);
            ((RectTransform)bars.transform).offsetMin = new Vector2(-4, -4);
            ((RectTransform)bars.transform).offsetMax = new Vector2(4, 4);

            _cells = new CellView[b.Width * b.Height];
            for (int y = 0; y < b.Height; y++)
                for (int x = 0; x < b.Width; x++)
                {
                    var bg = UiKit.Panel($"Cell_{x}_{y}", _grid, UiKit.Washi);
                    var rt = (RectTransform)bg.transform;
                    // y = 0 is the top row
                    rt.Anchor((float)x / b.Width, 1f - (float)(y + 1) / b.Height,
                              (float)(x + 1) / b.Width, 1f - (float)y / b.Height);
                    rt.offsetMin = new Vector2(gap, gap);
                    rt.offsetMax = new Vector2(-gap, -gap);
                    var cv = bg.gameObject.AddComponent<CellView>();
                    cv.Init(this, x, y);
                    _cells[y * b.Width + x] = cv;
                }

            // "Tap here" marker shown over the generator when there is nothing to merge.
            var arrowBg = UiKit.Panel("TapHint", _grid, UiKit.Vermilion);
            arrowBg.raycastTarget = false;
            _arrow = arrowBg.rectTransform;
            var arrowText = UiKit.Label("Text", _arrow, "点我", 26, Color.white);
            ((RectTransform)arrowText.transform).Anchor(0, 0, 1, 1);
            _arrow.gameObject.SetActive(false);

            b.CellChanged += Refresh;
            game.OrdersChanged += MarkHintsDirty;
            game.WalletChanged += MarkHintsDirty;
            for (int y = 0; y < b.Height; y++)
                for (int x = 0; x < b.Width; x++) Refresh(x, y);
        }

        void OnDestroy()
        {
            if (_game == null) return;
            _game.Board.CellChanged -= Refresh;
            _game.OrdersChanged -= MarkHintsDirty;
            _game.WalletChanged -= MarkHintsDirty;
        }

        void MarkHintsDirty() => _hintsDirty = true;

        /// <summary>Any player action restarts the idle timer and hides the current hint.</summary>
        internal void Poke()
        {
            _idle = 0;
            ShowHint(false);
        }

        void Update()
        {
            if (_cells == null) return;
            if (_hintsDirty)
            {
                _hintsDirty = false;
                var needed = _game.NeededItems();
                foreach (var c in _cells)
                {
                    var id = _game.Board.Get(c.X, c.Y);
                    c.SetNeeded(id != null && needed.Contains(id));
                }
                _hint = _game.FindHint();
                ShowHint(false);
            }
            _idle += Time.deltaTime;
            if (_idle >= HintDelay) ShowHint(true);
            if (_arrow.gameObject.activeSelf)
                _arrow.anchoredPosition = new Vector2(0, 10f * Mathf.Abs(Mathf.Sin(Time.time * 4f)));
        }

        void ShowHint(bool on)
        {
            foreach (var c in _cells) c.SetPulse(false);
            _arrow.gameObject.SetActive(false);
            if (!on) return;
            var b = _game.Board;
            if (_hint.Kind == HintKind.Merge)
            {
                _cells[_hint.Ay * b.Width + _hint.Ax].SetPulse(true);
                _cells[_hint.By * b.Width + _hint.Bx].SetPulse(true);
            }
            else if (_hint.Kind == HintKind.Generator)
            {
                _cells[_hint.Ay * b.Width + _hint.Ax].SetPulse(true);
                // Sit in the row above the generator (or on it, for the top row).
                int row = Mathf.Max(0, _hint.Ay - 1);
                _arrow.anchorMin = new Vector2((float)_hint.Ax / b.Width, 1f - (float)(row + 1) / b.Height + 0.3f / b.Height);
                _arrow.anchorMax = new Vector2((float)(_hint.Ax + 1) / b.Width, 1f - (float)row / b.Height - 0.2f / b.Height);
                _arrow.offsetMin = _arrow.offsetMax = Vector2.zero;
                _arrow.SetAsLastSibling();
                _arrow.gameObject.SetActive(true);
            }
        }

        void Refresh(int x, int y)
        {
            if (_cells == null) return;
            _hintsDirty = true;
            _idle = 0;
            _cells[y * _game.Board.Width + x].Show(_game.Board.Get(x, y), _visuals);
        }

        public RectTransform DragLayer => _dragLayer;

        public void Say(string text) => _info.text = text;

        internal void OnTap(CellView c)
        {
            Poke();
            var id = _game.Board.Get(c.X, c.Y);
            if (id == null) return;
            if (_game.Cfg.IsGenerator(id))
            {
                switch (_game.TapGenerator(c.X, c.Y, out var ox, out var oy))
                {
                    case TapResult.NoEnergy: Say("体力不足，稍等一会儿会恢复"); break;
                    case TapResult.BoardFull: Say("棋盘满了，先合成一些物品"); break;
                    case TapResult.Produced: _cells[oy * _game.Board.Width + ox].Pop(); break;
                }
            }
            else
            {
                var def = _game.Cfg.Items[id];
                var next = string.IsNullOrEmpty(def.next) ? "已是最高级" : "两个合成为「" + _visuals.DisplayName(def.next) + "」";
                Say(def.name + " Lv" + def.level + "：" + next);
            }
        }

        internal void OnDrop(CellView from, Vector2 screenPos, Camera cam)
        {
            foreach (var c in _cells)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)c.transform, screenPos, cam)) continue;
                if (_game.MoveItem(from.X, from.Y, c.X, c.Y) == MoveResult.Merged) c.Pop();
                return;
            }
        }
    }

    /// <summary>One board cell. Handles tap and drag for the item inside it.</summary>
    public class CellView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        MergeBoardView _board;
        RectTransform _item;
        Image _itemImage;
        Text _itemText;
        Image _badge;
        Text _badgeText;
        Image _needMark;
        bool _hasItem, _pulse;
        Vector3 _popT = Vector3.one;

        public void Init(MergeBoardView board, int x, int y)
        {
            _board = board; X = x; Y = y;
            // Green glow behind items an active order asks for.
            _needMark = UiKit.Panel("Needed", transform, new Color(0.45f, 0.78f, 0.42f, 0.85f));
            _needMark.raycastTarget = false;
            ((RectTransform)_needMark.transform).Anchor(0, 0, 1, 1);
            _needMark.gameObject.SetActive(false);
            _itemImage = UiKit.Panel("Item", transform, Color.clear);
            _item = (RectTransform)_itemImage.transform;
            _item.Anchor(0.06f, 0.06f, 0.94f, 0.94f);
            _itemImage.raycastTarget = false;
            _itemText = UiKit.Label("Text", _item, "", 20, UiKit.Ink);
            ((RectTransform)_itemText.transform).Anchor(0, 0, 1, 1);
            // Level badge in the bottom-right corner, shown once real art replaces the placeholder text.
            _badge = UiKit.Panel("Badge", _item, UiKit.WoodDark);
            _badge.raycastTarget = false;
            ((RectTransform)_badge.transform).Anchor(0.66f, 0f, 1f, 0.34f);
            _badgeText = UiKit.Label("Level", _badge.transform, "", 18, UiKit.Cream);
            ((RectTransform)_badgeText.transform).Anchor(0, 0, 1, 1);
            _badgeText.resizeTextForBestFit = true;
            _badgeText.resizeTextMinSize = 8;
            _badgeText.resizeTextMaxSize = 28;
        }

        public void Show(string id, ItemVisuals v)
        {
            _hasItem = id != null;
            _item.gameObject.SetActive(_hasItem);
            if (!_hasItem) return;
            var sprite = v.Sprite(id);
            _itemImage.sprite = sprite;
            _itemImage.color = sprite != null ? Color.white : v.PlaceholderColor(id);
            _itemText.text = sprite != null ? "" : v.PlaceholderText(id);
            _itemText.color = _board != null && id.StartsWith("gen_") ? Color.white : UiKit.Ink;
            var level = sprite != null ? v.LevelBadge(id) : "";
            _badge.gameObject.SetActive(level.Length > 0);
            _badgeText.text = level;
        }

        public void Pop() => _popT = Vector3.one * 1.25f;

        public void SetNeeded(bool on) => _needMark.gameObject.SetActive(on && _hasItem);

        public void SetPulse(bool on) => _pulse = on && _hasItem;

        void Update()
        {
            if (_item == null || _item.parent != transform) return;
            _popT = Vector3.Lerp(_popT, Vector3.one, Time.deltaTime * 12f);
            float pulse = _pulse ? 1f + 0.1f * Mathf.Abs(Mathf.Sin(Time.time * 4f)) : 1f;
            _item.localScale = _popT * pulse;
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (!e.dragging) _board.OnTap(this);
        }

        public void OnBeginDrag(PointerEventData e)
        {
            _board.Poke();
            if (!_hasItem) return;
            // Switch from stretch anchors to a fixed size so the item keeps its size on the drag layer.
            var size = _item.rect.size;
            var world = _item.position;
            _item.anchorMin = _item.anchorMax = new Vector2(0.5f, 0.5f);
            _item.sizeDelta = size;
            _item.SetParent(_board.DragLayer, false);
            _item.position = world;
            _item.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData e)
        {
            if (!_hasItem || _item.parent == transform) return;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_board.DragLayer, e.position, e.pressEventCamera, out var p);
            _item.position = p;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!_hasItem || _item.parent == transform) return;
            _item.SetParent(transform, false);
            _item.Anchor(0.06f, 0.06f, 0.94f, 0.94f);
            _item.localScale = Vector3.one;
            _board.OnDrop(this, e.position, e.pressEventCamera);
        }
    }
}
