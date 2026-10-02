using UnityEngine;
using UnityEngine.EventSystems;

namespace Machi
{
    /// <summary>
    /// 45° top-down orthographic camera. Drag to pan, scroll / pinch to zoom, no rotation
    /// (product doc §12). Short taps are reported to the town for building selection.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class TownCamera : MonoBehaviour
    {
        public Vector3 Focus = new Vector3(-40f, 0f, 11f);
        public float MinSize = 10f, MaxSize = 70f;
        public Rect Bounds = new Rect(-95f, -80f, 190f, 150f);   // x, z, width, depth
        public System.Action<Vector2> Tapped;
        public bool InputEnabled = true;

        Camera _cam;
        Vector3 _pressPos;
        Vector3 _lastPos;
        bool _pressing, _dragging;
        const float DragThreshold = 12f;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = 26f;
            _cam.nearClipPlane = 0.3f;
            _cam.farClipPlane = 1000f;
            transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            Apply();
        }

        void Apply()
        {
            Focus.x = Mathf.Clamp(Focus.x, Bounds.xMin, Bounds.xMax);
            Focus.z = Mathf.Clamp(Focus.z, Bounds.yMin, Bounds.yMax);
            transform.position = Focus - transform.forward * 200f;
        }

        void Update()
        {
            if (!InputEnabled) { _pressing = false; return; }

            if (Input.touchCount == 2)
            {
                var a = Input.GetTouch(0); var b = Input.GetTouch(1);
                float prev = ((a.position - a.deltaPosition) - (b.position - b.deltaPosition)).magnitude;
                float now = (a.position - b.position).magnitude;
                Zoom(prev / Mathf.Max(1f, now));
                _pressing = false;
                return;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f && !OverUi()) Zoom(1f - scroll * 0.1f);

            if (Input.GetMouseButtonDown(0) && !OverUi())
            {
                _pressing = true; _dragging = false;
                _pressPos = _lastPos = Input.mousePosition;
            }
            if (_pressing && Input.GetMouseButton(0))
            {
                if (!_dragging && (Input.mousePosition - _pressPos).magnitude > DragThreshold) _dragging = true;
                if (_dragging)
                {
                    var delta = Input.mousePosition - _lastPos;
                    // world units per pixel for an ortho camera; z is foreshortened by the 45° pitch
                    float k = _cam.orthographicSize * 2f / Screen.height;
                    Focus -= new Vector3(delta.x * k, 0f, delta.y * k / Mathf.Sin(45f * Mathf.Deg2Rad));
                    Apply();
                }
                _lastPos = Input.mousePosition;
            }
            if (_pressing && Input.GetMouseButtonUp(0))
            {
                if (!_dragging) Tapped?.Invoke(Input.mousePosition);
                _pressing = false;
            }
        }

        void Zoom(float factor)
        {
            _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize * factor, MinSize, MaxSize);
        }

        public void LookAt(Vector3 world)
        {
            Focus = new Vector3(world.x, 0f, world.z);
            Apply();
        }

        static bool OverUi()
        {
            var es = EventSystem.current;
            if (es == null) return false;
            if (Input.touchCount > 0) return es.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            return es.IsPointerOverGameObject();
        }
    }
}
