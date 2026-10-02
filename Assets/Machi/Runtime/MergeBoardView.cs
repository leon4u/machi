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
        RectTransform _area;
        RectTransform _dragLayer;
        CellView[] _cells;
        Text _info;

        public void Build(Game game, ItemVisuals visuals, RectTransform area, RectTransform dragLayer)
        {
            _game = game;
            _visuals = visuals;
            _dragLayer = dragLayer;
            _area = area;
            var b = game.Board;

            _info = UiKit.Label("Info", area, "点击工具箱产出物品，拖动相同物品合成", 30, UiKit.Ink);
            ((RectTransform)_info.transform).Anchor(0, 0.94f, 1, 1);

            // Square grid centred in the remaining space.
            _grid = UiKit.Rect("Grid", area);
            _grid.anchorMin = _grid.anchorMax = new Vector2(0.5f, 0.47f);
            float cell = 112f, gap = 8f;
            _grid.sizeDelta = new Vector2(b.Width * (cell + gap), b.Height * (cell + gap));
            var frame = UiKit.Panel("Frame", _grid, UiKit.Wood);
            ((RectTransform)frame.transform).Anchor(0, 0, 1, 1);
            ((RectTransform)frame.transform).offsetMin = new Vector2(-16, -16);
            ((RectTransform)frame.transform).offsetMax = new Vector2(16, 16);

            _cells = new CellView[b.Width * b.Height];
            for (int y = 0; y < b.Height; y++)
                for (int x = 0; x < b.Width; x++)
                {
                    var bg = UiKit.Panel($"Cell_{x}_{y}", _grid, UiKit.Tile);
                    var rt = (RectTransform)bg.transform;
                    rt.anchorMin = rt.anchorMax = new Vector2(0, 1);   // y = 0 is the top row
                    rt.pivot = new Vector2(0, 1);
                    rt.sizeDelta = new Vector2(cell, cell);
                    rt.anchoredPosition = new Vector2(gap / 2 + x * (cell + gap), -gap / 2 - y * (cell + gap));
                    var cv = bg.gameObject.AddComponent<CellView>();
                    cv.Init(this, x, y);
                    _cells[y * b.Width + x] = cv;
                }

            b.CellChanged += Refresh;
            for (int y = 0; y < b.Height; y++)
                for (int x = 0; x < b.Width; x++) Refresh(x, y);
        }

        void OnDestroy()
        {
            if (_game != null) _game.Board.CellChanged -= Refresh;
        }

        // Shrink the grid to fit its area (below the info line) so a wide or short window
        // never pushes it over the order cards.
        void LateUpdate()
        {
            if (_grid == null) return;
            var avail = _area.rect.size;
            float frame = 32f;
            float s = Mathf.Min(1f, avail.x / (_grid.sizeDelta.x + frame), avail.y * 0.92f / (_grid.sizeDelta.y + frame));
            if (s > 0f && Mathf.Abs(_grid.localScale.x - s) > 0.001f) _grid.localScale = new Vector3(s, s, 1f);
        }

        void Refresh(int x, int y)
        {
            if (_cells == null) return;
            _cells[y * _game.Board.Width + x].Show(_game.Board.Get(x, y), _visuals);
        }

        public RectTransform DragLayer => _dragLayer;

        public void Say(string text) => _info.text = text;

        internal void OnTap(CellView c)
        {
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
        bool _hasItem;
        Vector3 _popT = Vector3.one;

        public void Init(MergeBoardView board, int x, int y)
        {
            _board = board; X = x; Y = y;
            _itemImage = UiKit.Panel("Item", transform, Color.clear);
            _item = (RectTransform)_itemImage.transform;
            _item.Anchor(0.06f, 0.06f, 0.94f, 0.94f);
            _itemImage.raycastTarget = false;
            _itemText = UiKit.Label("Text", _item, "", 20, UiKit.Ink);
            ((RectTransform)_itemText.transform).Anchor(0, 0, 1, 1);
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
        }

        public void Pop() => _popT = Vector3.one * 1.25f;

        void Update()
        {
            if (_item != null && _item.parent == transform)
                _item.localScale = _popT = Vector3.Lerp(_popT, Vector3.one, Time.deltaTime * 12f);
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (!e.dragging) _board.OnTap(this);
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (!_hasItem) return;
            // Switch from stretch anchors to a fixed size so the item keeps its size on the drag layer.
            var size = _item.rect.size * (_item.lossyScale.x / _board.DragLayer.lossyScale.x);
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
