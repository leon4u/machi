using System;
using System.Collections;
using System.Collections.Generic;
using Machi.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Machi
{
    [Serializable] public class LayoutFile { public LayoutInstance[] instances; }
    [Serializable] public class LayoutInstance { public string asset; public string region; public string tag; public BlenderXf blender; }
    [Serializable] public class BlenderXf { public float[] pos; public float rot_z_deg; }

    /// <summary>
    /// Builds the 3D town from Resources/Config/town_layout.json and the FBX models in Resources/Town.
    /// The layout uses Blender coordinates; the town root is rotated 180° so building fronts face -Z
    /// (toward the camera, which sits south of the town looking north).
    /// </summary>
    public class TownView : MonoBehaviour
    {
        public const string RestorablePrefix = "restorable:";
        Game _game;
        Material _palette, _glow;
        Light _sun;
        Camera _cam;
        readonly Dictionary<string, RestorableBuilding> _buildings = new Dictionary<string, RestorableBuilding>();
        public bool Night { get; private set; }

        public event Action<RestorableBuilding> BuildingTapped;

        public void Build(Game game, Camera cam, Light sun)
        {
            _game = game;
            _cam = cam;
            _sun = sun;
            CreateMaterials();

            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var ground = Resources.Load<GameObject>("Town/town_ground");
            if (ground != null) Spawn(ground, transform, "Ground");
            else Debug.LogWarning("[Machi] Resources/Town/town_ground not found");

            var json = Resources.Load<TextAsset>("Config/town_layout");
            if (json == null) { Debug.LogError("[Machi] Resources/Config/town_layout.json missing"); return; }
            var layout = JsonUtility.FromJson<LayoutFile>(json.text);
            var regions = new Dictionary<string, Transform>();
            foreach (var inst in layout.instances)
            {
                if (!regions.TryGetValue(inst.region, out var parent))
                {
                    parent = new GameObject(inst.region).transform;
                    parent.SetParent(transform, false);
                    regions[inst.region] = parent;
                }
                var p = inst.blender.pos;
                var go = new GameObject(inst.asset);
                go.transform.SetParent(parent, false);
                go.transform.localPosition = new Vector3(-p[0], p[2], -p[1]);
                go.transform.localRotation = Quaternion.Euler(0f, -inst.blender.rot_z_deg, 0f);

                if (!string.IsNullOrEmpty(inst.tag) && inst.tag.StartsWith(RestorablePrefix))
                    BuildRestorable(go, inst.tag.Substring(RestorablePrefix.Length));
                else
                {
                    var prefab = Resources.Load<GameObject>("Town/" + inst.asset);
                    if (prefab != null) Spawn(prefab, go.transform, inst.asset);
                    else Debug.LogWarning("[Machi] missing model Town/" + inst.asset);
                }
            }

            game.BuildingStageChanged += OnStage;
            foreach (var b in _buildings.Values) b.SetStage(game.GetStage(b.Id), false);
            SetNight(false);
        }

        void OnDestroy()
        {
            if (_game != null) _game.BuildingStageChanged -= OnStage;
        }

        void BuildRestorable(GameObject holder, string id)
        {
            var rb = holder.AddComponent<RestorableBuilding>();
            var stages = new List<GameObject>();
            for (int s = 0; ; s++)
            {
                var prefab = Resources.Load<GameObject>($"Town/{id}_s{s}");
                if (prefab == null) break;
                var go = Spawn(prefab, holder.transform, $"{id}_s{s}");
                foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
                    r.gameObject.AddComponent<BoxCollider>();
                stages.Add(go);
            }
            rb.Init(id, stages);
            _buildings[id] = rb;
        }

        GameObject Spawn(GameObject prefab, Transform parent, string name)
        {
            var go = Instantiate(prefab, parent, false);
            go.name = name;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = mats[i] != null && mats[i].name.Contains("Glow") ? _glow : _palette;
                r.sharedMaterials = mats;
                r.shadowCastingMode = ShadowCastingMode.On;
            }
            return go;
        }

        void OnStage(string id, int stage)
        {
            if (_buildings.TryGetValue(id, out var b)) b.SetStage(stage, true);
        }

        public RestorableBuilding Find(string id) => _buildings.TryGetValue(id, out var b) ? b : null;

        /// <summary>Raycast from a screen point; returns the building under it, if any.</summary>
        public RestorableBuilding Pick(Vector2 screen)
        {
            var ray = _cam.ScreenPointToRay(screen);
            return Physics.Raycast(ray, out var hit, 2000f) ? hit.collider.GetComponentInParent<RestorableBuilding>() : null;
        }

        public void HandleTap(Vector2 screen)
        {
            var b = Pick(screen);
            if (b != null) BuildingTapped?.Invoke(b);
        }

        // ---------------------------------------------------------------- look
        void CreateMaterials()
        {
            var tex = Resources.Load<Texture2D>("Town/T_TownPalette");
            if (tex != null) { tex.filterMode = FilterMode.Point; tex.wrapMode = TextureWrapMode.Clamp; }
            bool urp = GraphicsSettings.currentRenderPipeline != null;
            var shader = Shader.Find(urp ? "Universal Render Pipeline/Lit" : "Standard");
            _palette = new Material(shader) { name = "M_TownPalette" };
            SetTex(_palette, tex);
            _palette.SetFloat("_Glossiness", 0.1f);
            _palette.SetFloat("_Smoothness", 0.1f);
            _glow = new Material(_palette) { name = "M_Glow" };
            _glow.EnableKeyword("_EMISSION");
            _glow.SetTexture("_EmissionMap", tex);
            _glow.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }

        static void SetTex(Material m, Texture tex)
        {
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
            if (m.HasProperty("_Color")) m.SetColor("_Color", Color.white);
        }

        public void SetNight(bool night)
        {
            Night = night;
            _glow.SetColor("_EmissionColor", night ? Color.white * 2.5f : Color.black);
            if (_sun != null)
            {
                _sun.intensity = night ? 0.15f : 1.2f;
                _sun.color = night ? new Color(0.6f, 0.7f, 1f) : new Color(1f, 0.96f, 0.88f);
            }
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = night ? new Color(0.16f, 0.19f, 0.32f) : new Color(0.72f, 0.76f, 0.82f);
            if (_cam != null)
            {
                _cam.clearFlags = CameraClearFlags.SolidColor;
                _cam.backgroundColor = night ? UiKit.Hex("2C3550") : UiKit.Hex("C9DCE8");
            }
        }
    }

    /// <summary>A building with one model per restoration stage; only the current stage is active.</summary>
    public class RestorableBuilding : MonoBehaviour
    {
        public string Id { get; private set; }
        List<GameObject> _stages;
        int _current = -1;

        public void Init(string id, List<GameObject> stages)
        {
            Id = id;
            _stages = stages;
        }

        public void SetStage(int stage, bool animate)
        {
            if (_stages.Count == 0) return;
            stage = Mathf.Clamp(stage, 0, _stages.Count - 1);
            if (stage == _current) return;
            _current = stage;
            for (int i = 0; i < _stages.Count; i++) _stages[i].SetActive(i == stage);
            if (animate) StartCoroutine(Pop(_stages[stage].transform));
        }

        static IEnumerator Pop(Transform t)
        {
            for (float k = 0; k < 1f; k += Time.deltaTime * 3f)
            {
                float s = 1f + Mathf.Sin(k * Mathf.PI) * 0.12f;
                t.localScale = new Vector3(s, s, s);
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }
}
