using UnityEngine;
using UnityEngine.UI;

namespace Machi
{
    /// <summary>Builds uGUI elements from code so the prototype needs no prefabs.</summary>
    public static class UiKit
    {
        public static readonly Color Cream = Hex("F7F0E3");
        public static readonly Color Vermilion = Hex("D9614C");
        public static readonly Color Wood = Hex("B9835A");
        public static readonly Color WoodDark = Hex("8A5A3B");
        public static readonly Color Ink = Hex("3A322B");
        public static readonly Color Tile = Hex("E8DCC4");

        static Font _font;
        public static Font Font
        {
            get
            {
                if (_font == null)
                {
                    // OS fonts first so Chinese / Japanese text renders; fall back to Unity's built-in font.
                    _font = Font.CreateDynamicFontFromOSFont(new[] {
                        "PingFang SC", "Hiragino Sans GB", "Hiragino Sans", "Microsoft YaHei", "Noto Sans CJK SC",
                        "Noto Sans CJK JP", "Yu Gothic", "Arial" }, 32);
                    if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return _font;
            }
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Anchor a rect to a fraction of its parent: (xMin, yMin, xMax, yMax) in 0..1.</summary>
        public static RectTransform Anchor(this RectTransform rt, float x0, float y0, float x1, float y1)
        {
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Panel(string name, Transform parent, Color color)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text Label(string name, Transform parent, string text, int size, Color color,
                                 TextAnchor align = TextAnchor.MiddleCenter)
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(string name, Transform parent, string text, Color bg, System.Action onClick, int size = 34)
        {
            var img = Panel(name, parent, bg);
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(() => onClick());
            var t = Label("Text", img.transform, text, size, Color.white);
            ((RectTransform)t.transform).Anchor(0, 0, 1, 1);
            return b;
        }

        public static void SetButtonText(Button b, string text) => b.GetComponentInChildren<Text>().text = text;
    }
}
