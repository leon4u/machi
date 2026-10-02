using System;
using System.Collections.Generic;

namespace Machi.Core
{
    public enum MoveResult { Invalid, Moved, Swapped, Merged }

    /// <summary>Grid of item / generator ids. Pure logic, no rendering.</summary>
    public class MergeBoard
    {
        readonly ConfigIndex _cfg;
        readonly string[] _cells;
        public int Width { get; }
        public int Height { get; }

        /// <summary>Fired when any cell changes. Arguments: x, y.</summary>
        public event Action<int, int> CellChanged;

        public MergeBoard(ConfigIndex cfg, int width, int height)
        {
            _cfg = cfg;
            Width = width;
            Height = height;
            _cells = new string[width * height];
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public string Get(int x, int y) => InBounds(x, y) ? _cells[y * Width + x] : null;

        public void Set(int x, int y, string id)
        {
            if (!InBounds(x, y)) throw new ArgumentOutOfRangeException($"({x},{y})");
            _cells[y * Width + x] = string.IsNullOrEmpty(id) ? null : id;
            CellChanged?.Invoke(x, y);
        }

        public bool IsEmpty(int x, int y) => Get(x, y) == null;

        public int CountEmpty()
        {
            int n = 0;
            foreach (var c in _cells) if (c == null) n++;
            return n;
        }

        public int Count(string itemId)
        {
            int n = 0;
            foreach (var c in _cells) if (c == itemId) n++;
            return n;
        }

        /// <summary>Drag the content of (fx,fy) onto (tx,ty).</summary>
        public MoveResult Move(int fx, int fy, int tx, int ty)
        {
            if (!InBounds(fx, fy) || !InBounds(tx, ty) || (fx == tx && fy == ty)) return MoveResult.Invalid;
            var a = Get(fx, fy);
            if (a == null) return MoveResult.Invalid;
            var b = Get(tx, ty);
            if (b == null)
            {
                Set(tx, ty, a); Set(fx, fy, null);
                return MoveResult.Moved;
            }
            if (a == b && _cfg.Items.TryGetValue(a, out var def) && !string.IsNullOrEmpty(def.next))
            {
                Set(tx, ty, def.next); Set(fx, fy, null);
                return MoveResult.Merged;
            }
            Set(tx, ty, a); Set(fx, fy, b);
            return MoveResult.Swapped;
        }

        /// <summary>Nearest empty cell to (x,y) by Manhattan distance, scanning row by row for ties.</summary>
        public bool TryFindEmptyNear(int x, int y, out int ex, out int ey)
        {
            ex = ey = -1;
            int best = int.MaxValue;
            for (int j = 0; j < Height; j++)
                for (int i = 0; i < Width; i++)
                {
                    if (!IsEmpty(i, j)) continue;
                    int d = Math.Abs(i - x) + Math.Abs(j - y);
                    if (d < best) { best = d; ex = i; ey = j; }
                }
            return ex >= 0;
        }

        /// <summary>Remove `count` copies of an item. Returns false (and removes nothing) if not enough.</summary>
        public bool TryRemove(string itemId, int count)
        {
            if (Count(itemId) < count) return false;
            for (int i = 0; i < _cells.Length && count > 0; i++)
                if (_cells[i] == itemId)
                {
                    Set(i % Width, i / Width, null);
                    count--;
                }
            return true;
        }

        public string[] Snapshot()
        {
            var copy = new string[_cells.Length];
            for (int i = 0; i < copy.Length; i++) copy[i] = _cells[i] ?? "";
            return copy;
        }

        public void Restore(string[] cells)
        {
            for (int i = 0; i < _cells.Length; i++)
                _cells[i] = cells != null && i < cells.Length && !string.IsNullOrEmpty(cells[i]) ? cells[i] : null;
            for (int i = 0; i < _cells.Length; i++) CellChanged?.Invoke(i % Width, i / Width);
        }

        public IEnumerable<(int x, int y, string id)> All()
        {
            for (int i = 0; i < _cells.Length; i++)
                if (_cells[i] != null) yield return (i % Width, i / Width, _cells[i]);
        }
    }
}
