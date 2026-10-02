using Machi.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Machi
{
    /// <summary>
    /// Entry point. Spawns itself in any scene when you press Play, then builds camera, light,
    /// UI, the 3D town and the merge board from Resources. No scene setup is required.
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }
        public Game Game { get; private set; }

        TownView _town;
        TownCamera _townCam;
        RectTransform _boardScreen;
        RectTransform _buildingPanel;
        Text _coins, _materials, _energy, _revival, _toast, _bpTitle, _bpBody;
        Button _switchBtn, _bpRestore;
        RestorableBuilding _selected;
        float _toastUntil, _saveTimer;
        bool _boardOpen;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (FindAnyObjectByType<GameRoot>() == null) new GameObject("GameRoot").AddComponent<GameRoot>();
        }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            Application.targetFrameRate = 60;

            var json = Resources.Load<TextAsset>("Config/game_config");
            var cfg = new ConfigIndex(JsonUtility.FromJson<GameConfig>(json.text));
            foreach (var err in cfg.Validate()) Debug.LogError("[Machi] config: " + err);
            Game = new Game(cfg, new SystemRandom());
            var save = SaveStore.Load();
            if (save != null) Game.Load(save, SaveStore.Now); else Game.NewGame(SaveStore.Now);
        }

        void Start()
        {
            if (Instance != this) return;
            var cam = Camera.main;
            if (cam == null) cam = new GameObject("Main Camera", typeof(Camera)) { tag = "MainCamera" }.GetComponent<Camera>();
            _townCam = cam.gameObject.AddComponent<TownCamera>();

            var sun = FindAnyObjectByType<Light>();
            if (sun == null) sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            _town = new GameObject("Town").AddComponent<TownView>();
            _town.Build(Game, cam, sun);
            _town.BuildingTapped += OpenBuilding;
            _townCam.Tapped += _town.HandleTap;

            BuildUi();
            Game.WalletChanged += RefreshHud;
            RefreshHud();
            ShowBoard(false);
        }

        void OnDestroy()
        {
            if (Instance == this && Game != null) Game.WalletChanged -= RefreshHud;
        }

        void Update()
        {
            Game.TickEnergy(SaveStore.Now);
            if (_toast != null && _toast.gameObject.activeSelf && Time.time > _toastUntil) _toast.gameObject.SetActive(false);
            _saveTimer += Time.unscaledDeltaTime;
            if (_saveTimer > 10f) { _saveTimer = 0; Save(); }
        }

        void OnApplicationPause(bool paused) { if (paused) Save(); }
        void OnApplicationQuit() => Save();

        void Save()
        {
            if (Game != null) SaveStore.Save(Game.ToSave());
        }

        // ---------------------------------------------------------------- UI
        void BuildUi()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasGo = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            var root = (RectTransform)canvasGo.transform;

            // Board screen (covers the town while open)
            _boardScreen = UiKit.Panel("BoardScreen", root, UiKit.Cream).rectTransform.Anchor(0, 0, 1, 0.9f);
            var ordersArea = UiKit.Rect("OrdersArea", _boardScreen).Anchor(0.02f, 0.76f, 0.98f, 0.99f);
            var boardArea = UiKit.Rect("BoardArea", _boardScreen).Anchor(0, 0.1f, 1, 0.76f);
            var dragLayer = UiKit.Rect("DragLayer", root).Anchor(0, 0, 1, 1);
            _boardScreen.gameObject.AddComponent<OrdersView>().Build(Game, new ItemVisuals(Game.Cfg), ordersArea);
            _boardScreen.gameObject.AddComponent<MergeBoardView>().Build(Game, new ItemVisuals(Game.Cfg), boardArea, dragLayer);

            // HUD
            var hud = UiKit.Panel("Hud", root, new Color(0.23f, 0.2f, 0.17f, 0.85f)).rectTransform.Anchor(0, 0.93f, 1, 1);
            _coins = HudLabel(hud, 0.00f, 0.25f);
            _materials = HudLabel(hud, 0.25f, 0.50f);
            _energy = HudLabel(hud, 0.50f, 0.75f);
            _revival = HudLabel(hud, 0.75f, 1.00f);

            var bar = UiKit.Rect("BottomBar", root).Anchor(0, 0, 1, 0.07f);
            _switchBtn = UiKit.Button("Switch", bar, "", UiKit.Vermilion, () => ShowBoard(!_boardOpen), 38);
            ((RectTransform)_switchBtn.transform).Anchor(0.03f, 0.1f, 0.55f, 0.9f);
            var nightBtn = UiKit.Button("Night", bar, "白天 / 夜晚", UiKit.WoodDark, () => _town.SetNight(!_town.Night), 30);
            ((RectTransform)nightBtn.transform).Anchor(0.57f, 0.1f, 0.8f, 0.9f);
            var resetBtn = UiKit.Button("Reset", bar, "重置存档", Color.gray, ResetSave, 28);
            ((RectTransform)resetBtn.transform).Anchor(0.82f, 0.1f, 0.97f, 0.9f);

            // Building panel
            _buildingPanel = UiKit.Panel("BuildingPanel", root, UiKit.Cream).rectTransform.Anchor(0.08f, 0.1f, 0.92f, 0.32f);
            _bpTitle = UiKit.Label("Title", _buildingPanel, "", 44, UiKit.Ink);
            ((RectTransform)_bpTitle.transform).Anchor(0.05f, 0.72f, 0.95f, 0.95f);
            _bpBody = UiKit.Label("Body", _buildingPanel, "", 32, UiKit.WoodDark);
            ((RectTransform)_bpBody.transform).Anchor(0.05f, 0.35f, 0.95f, 0.72f);
            _bpRestore = UiKit.Button("Restore", _buildingPanel, "修复", UiKit.Vermilion, RestoreSelected, 36);
            ((RectTransform)_bpRestore.transform).Anchor(0.05f, 0.06f, 0.6f, 0.3f);
            var close = UiKit.Button("Close", _buildingPanel, "关闭", Color.gray, () => _buildingPanel.gameObject.SetActive(false), 32);
            ((RectTransform)close.transform).Anchor(0.65f, 0.06f, 0.95f, 0.3f);
            _buildingPanel.gameObject.SetActive(false);

            _toast = UiKit.Label("Toast", root, "", 36, Color.white);
            var tbg = _toast.gameObject.AddComponent<Outline>();
            tbg.effectColor = new Color(0, 0, 0, 0.8f);
            ((RectTransform)_toast.transform).Anchor(0.05f, 0.84f, 0.95f, 0.92f);
            _toast.gameObject.SetActive(false);
        }

        Text HudLabel(RectTransform hud, float x0, float x1)
        {
            var t = UiKit.Label("Stat", hud, "", 34, Color.white);
            ((RectTransform)t.transform).Anchor(x0, 0, x1, 1);
            return t;
        }

        void RefreshHud()
        {
            _coins.text = "金币 " + Game.Coins;
            _materials.text = "修复材料 " + Game.Materials;
            _energy.text = $"体力 {Game.Energy}/{Game.Cfg.Config.maxEnergy}";
            _revival.text = "复兴度 " + Game.Revival;
            if (_buildingPanel != null && _buildingPanel.gameObject.activeSelf && _selected != null) FillBuilding(_selected);
        }

        void ShowBoard(bool open)
        {
            _boardOpen = open;
            _boardScreen.gameObject.SetActive(open);
            _townCam.InputEnabled = !open;
            if (open) _buildingPanel.gameObject.SetActive(false);
            UiKit.SetButtonText(_switchBtn, open ? "回到小镇" : "去合成");
        }

        public void Toast(string msg, float seconds = 2.5f)
        {
            _toast.text = msg;
            _toast.gameObject.SetActive(true);
            _toastUntil = Time.time + seconds;
        }

        // ---------------------------------------------------------------- buildings
        void OpenBuilding(RestorableBuilding b)
        {
            _selected = b;
            _buildingPanel.gameObject.SetActive(true);
            FillBuilding(b);
        }

        void FillBuilding(RestorableBuilding b)
        {
            var def = Game.Cfg.Buildings.TryGetValue(b.Id, out var d) ? d : null;
            int stage = Game.GetStage(b.Id), max = Game.MaxStage(b.Id);
            _bpTitle.text = def != null ? def.name : b.Id;
            if (def == null) { _bpBody.text = "（配置里没有这栋建筑）"; _bpRestore.interactable = false; return; }
            if (stage >= max)
            {
                _bpBody.text = $"已完成全部修复（{stage}/{max}）";
                _bpRestore.interactable = false;
                return;
            }
            var next = def.stages[stage];
            bool locked = !Game.IsRegionUnlocked(def.region);
            _bpBody.text = locked ? "这个区域还没有开放"
                : $"下一阶段：{next.note}（{stage}/{max}）\n需要修复材料 {next.cost}，现有 {Game.Materials}";
            _bpRestore.interactable = !locked && Game.Materials >= next.cost;
        }

        void RestoreSelected()
        {
            if (_selected == null) return;
            var r = Game.Restore(_selected.Id);
            if (r == RestoreResult.Restored) { Toast("雾见町复兴度提升了！"); Save(); }
            else if (r == RestoreResult.NotEnoughMaterials) Toast("修复材料不够，去完成委托吧");
            FillBuilding(_selected);
        }

        void ResetSave()
        {
            SaveStore.Delete();
            Game.NewGame(SaveStore.Now);
            _buildingPanel.gameObject.SetActive(false);
            Toast("存档已重置");
        }
    }
}
