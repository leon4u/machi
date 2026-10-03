# 《温泉小镇复兴》2D 美术资产清单与 ChatGPT Prompt
### V0.1（对应 Vertical Slice + MVP：Ch1–Ch3）

> 用法：先做第 1 节"风格锚点"，你满意并锁定后，后面每张图都把锚点图作为参考图一起发给 ChatGPT，并在 prompt 末尾附上对应的风格块：角色、背景、CG 用第 1.2 节「插画风格块」，物品图标、生产器、UI 用第 1.2b 节「休闲图标风格块」。
> 优先级：**P0 = Vertical Slice（First Hour）必须**，P1 = MVP（Ch2–Ch3）需要，P2 = 可延后。
> 角色外观细节（发型、服装颜色）是我根据 Character Bible 补的建议，你可以改。

---

## 0. 生成与导出规范

| 类型 | 生成尺寸 | 背景 | 导入 Unity 尺寸 | 备注 |
|---|---|---|---|---|
| 角色立绘（半身） | 1024×1536 竖图 | 透明 | 原尺寸 | 所有角色同一构图：头顶留 5% 空白，腰部以下截断 |
| 表情差分 | 同上 | 透明 | 原尺寸 | 用"编辑图片"在锚点立绘上只改表情 |
| Merge 物品图标 | 1024×1024 | 透明 | 256×256 | 物品居中，占画面 80%，同一光源（左上） |
| UI 图标 | 1024×1024 | 透明 | 128×128 | |
| 剧情背景 / CG | 1536×1024 横图 | 不透明 | 1920×1280（放大）或原尺寸 | 竖屏游戏里会裁切中间，主体放中央 |
| UI 面板 | 1024×1024 | 透明 | 九宫格切图 | 边角装饰放四角，中间留纯色 |

文件命名：`类别_英文名_编号.png`，例如 `char_mio_neutral.png`、`bg_office_messy.png`。**Merge 棋盘上的物品和生产器例外：文件名必须等于游戏里的物品 id**（`tool_03.png`、`plank.png`、`gen_toolbox.png`），放进 `Assets/Machi/Resources/Items/`，否则游戏找不到图。

---

## 1. 风格锚点（P0，最先做）

### 1.1 色板（2D 与 3D 共用）

| 用途 | 颜色 |
|---|---|
| 木头（暖） | #B9835A / #8A5A3B |
| 瓦屋顶（灰蓝） | #5D6B7A |
| 灰泥墙（米白） | #EFE6D6 |
| 暖灯 | #FFC56B |
| 樱花粉 | #F2B8C6 |
| 青苔绿 | #7FA36B |
| 雾蓝（冷清、未开放区域） | #AFC3CF |
| 夜空 | #2C3550 |
| UI 主色（和纸米色） | #F7F0E3 |
| UI 强调（朱红） | #D9614C |

### 1.2 插画风格块（角色立绘、剧情背景、CG）

```
Style: modern Japanese slice-of-life illustration, soft clean lineart, gentle cel shading with subtle watercolor texture, warm and calm mood, muted natural palette (warm wood #B9835A, off-white plaster #EFE6D6, slate blue roof #5D6B7A, warm lamp light #FFC56B, moss green #7FA36B), cozy healing atmosphere, not idol-like, not overly glamorous, realistic body proportions, natural hair colors.
```

### 1.2b 休闲图标风格块（Merge 物品、生产器、UI）

参考日式休闲 / 合成手游的道具图标：圆润鼓鼓、Q 版比例、细节少、深棕粗描边、1～2 层平涂阴影、左上角糖果般的白色高光，缩到 64px 也一眼认得出。2026-10-03 由 Ang 试玩后决定：棋盘上不用写实手绘风，那种细节缩小后会糊。

```
Style: Japanese casual mobile merge game item icon, slightly chunky and rounded "puffy" proportions, simplified shapes with very few details, bold dark-brown outline (thick, even width), flat cel shading with only 1-2 shade tones, a soft glossy white highlight on the top-left, bright warm pastel colors with good saturation, cute and friendly kawaii feel, readable at 64px.
```

### 1.3 锚点图（先生成这 3 张，反复改到满意再继续）

**A1 主角立绘锚点 `char_mio_neutral.png`**
```
Half-body character portrait of Morikawa Mio, a 27-year-old Japanese woman who left a Tokyo event-planning job and just arrived in a small mountain hot-spring town. Shoulder-length dark brown hair loosely tucked behind one ear, calm observant eyes, light natural makeup. Wearing a simple oatmeal knit cardigan over a white shirt, dark navy wide trousers, a canvas tote bag strap on her shoulder. Neutral, slightly tired but curious expression, facing slightly to the left, three-quarter view. Transparent background, cropped at the waist, vertical 2:3 composition with 5% headroom.
[插画风格块]
```

**A2 Merge 物品锚点 `tool_01.png`（抹布）**
```
Game item icon for a Japanese casual merge game: a folded light-blue cotton cleaning cloth with a small stitched edge. Single object centered, occupying 80% of the frame, no text, no background objects, transparent background.
[休闲图标风格块]
```

**A3 场景背景锚点 `bg_station_day.png`**
```
Background illustration of an old, half-abandoned rural Japanese train station in a misty mountain valley, morning. A faded wooden station sign with peeling paper, a closed waiting room with boards over the windows, a small bus stop bench, a few weeds through the asphalt, thin morning mist, distant forested mountains, a river glimpse. No people, no text on signs. Wide 3:2 composition with the main subject in the center third.
[插画风格块]
```

锁定后：把这 3 张保存为"锚点"，后面每次生成都上传对应类型的锚点图，并加一句：
```
Use the attached image as the exact style reference (line weight, shading, palette). Keep the same art style.
```

---

## 2. 角色立绘

所有角色先做 neutral（标准表情），再用"编辑图片"做表情差分：
```
Edit this image: keep the same character, outfit, pose, framing and art style. Only change the facial expression to [表情]. Transparent background.
```

表情（每人 5 个）：`neutral` 平静 / `smile` 微笑 / `surprised` 惊讶 / `troubled` 为难 / `sad` 低落沉思。

| 优先级 | 文件 | 角色 | Prompt 主体（后接风格块） |
|---|---|---|---|
| P0 | `char_mio_*` | 森川澪 | 见 A1 |
| P0 | `char_harue_*` | 北川春江 62 岁 | `Half-body portrait of Kitagawa Harue, a 62-year-old Japanese woman who runs the town's general store and knows everyone's story. Short permed graying hair, round friendly face, lively eyes, a beige apron with a pen in the pocket over a mustard-yellow blouse, sleeves rolled up. Warm talkative expression, three-quarter view facing right. Transparent background, cropped at the waist, vertical 2:3.` |
| P0 | `char_chizuru_*` | 小野寺千鹤 74 岁 | `Half-body portrait of Onodera Chizuru, a 74-year-old Japanese wagashi confectioner. Neat white hair in a low bun, gentle but principled eyes, small and slightly stooped, wearing a plain indigo samue work outfit with a white apron, a cloth wrist support on her right wrist. Calm, reserved expression, three-quarter view facing left. Transparent background, cropped at the waist, vertical 2:3.` |
| P0 | `char_ren_*` | 藤原莲 29 岁 | `Half-body portrait of Fujiwara Ren, a 29-year-old Japanese chef who worked in Osaka and is temporarily back in his hometown. Short slightly messy black hair, sharp confident eyes, light stubble, wearing a dark gray t-shirt under an open olive overshirt, sleeves pushed up, forearms of a cook. Skeptical half-smile, arms loosely crossed, three-quarter view facing right. Transparent background, cropped at the waist, vertical 2:3. Not idol-like, realistic.` |
| P1 | `char_seiichi_*` | 高桥征一 68 岁 | `Half-body portrait of Takahashi Seiichi, a 68-year-old owner of an old public bathhouse. Short cropped gray hair, thick eyebrows, weathered face, stocky build, wearing a navy happi-style work jacket over a white undershirt, a towel around his neck. Stubborn frowning expression, three-quarter view facing left. Transparent background, cropped at the waist, vertical 2:3.` |
| P1 | `char_kamiya_avatar` | 神谷真琴 32 岁（MVP 只需电话头像） | `Bust portrait (head and shoulders) of Kamiya Makoto, a 32-year-old Japanese prefectural government officer in charge of regional revitalization. Neat short black hair, thin silver glasses, calm unreadable expression, white shirt with a dark gray suit jacket, lanyard ID card. Transparent background, square 1:1.` |
| P2 | `char_hiyori_*` / `char_ryoko_*` / `char_yuto_*` / `char_aoi_*` / `char_kuroda_*` | Ch5 以后角色 | MVP 之后再做，按同一格式写 |

路人 NPC 剪影（P1）：`npc_silhouette_elder_01..03`
```
Simple flat silhouette-style illustration of an elderly Japanese townsperson (variant: man with cane / woman with shopping bag / man carrying a wooden bucket), soft single-tone fill with subtle shading, no facial details, transparent background.
[插画风格块]
```

---

## 3. Merge 物品图标

通用模板（替换"物品描述"）：
```
Game item icon for a Japanese casual merge game: [物品描述]. Single object centered, occupying 80% of the frame, slightly chunky and rounded "puffy" proportions, simplified shapes with very few details, bold dark-brown outline (thick, even width), flat cel shading with only 1-2 shade tones, a soft glossy white highlight on the top-left, bright warm pastel colors with good saturation, cute and friendly kawaii feel, readable at 64px, no text, no background objects, transparent background. Style similar to popular Japanese mobile puzzle / merge game item icons. Use the attached image as the exact style reference.
```

- 先只生成抹布（`tool_01`）一张，满意后把它当参考图附在后面每一张里，整套风格才统一。
- 同一条链里，等级越高的物品要"更大、更完整、颜色更丰富"，让玩家一眼看出等级。
- 文件名就是下表"文件"一列加 `.png`，直接覆盖旧图。

### 3.1 工具链（P0）
| 文件 | 等级 | 物品描述 |
|---|---|---|
| tool_01 | Lv1 | 见 A2，抹布 |
| tool_02 | Lv2 | `a cleaning kit: a small wooden bucket holding a cloth, a hand brush and a spray bottle` |
| tool_03 | Lv3 | `a basic repair kit: a hammer, a roll of tape and a few nails on a small wooden tray` |
| tool_04 | Lv4 | `a carpenter's toolset: a Japanese pull saw, a wood plane and a chisel bundled with a cloth wrap` |
| tool_05 | Lv5 | `a premium repair toolbox, polished wooden box with brass corners, lid open showing neatly arranged tools` |

### 3.2 食材链（P1）
| 文件 | 等级 | 物品描述 |
|---|---|---|
| food_01 | Lv1 | `a small cloth sack of white rice tied with string` |
| food_02 | Lv2 | `two onigiri rice balls wrapped with nori on a bamboo leaf` |
| food_03 | Lv3 | `a simple wooden bento box with rice, tamagoyaki and pickles` |
| food_04 | Lv4 | `a Japanese teishoku set meal on a tray: rice, miso soup, grilled fish, small side dishes` |
| food_05 | Lv5 | `a festive banquet lacquer box (jubako) with colorful seasonal dishes` |

### 3.3 旅馆用品链（P1，Ch3 浴场用品段）
| 文件 | 等级 | 物品描述 |
|---|---|---|
| inn_01 | Lv1 | `a folded white hand towel with a thin blue stripe` |
| inn_02 | Lv2 | `a neatly folded blue-and-white yukata with an obi belt` |
| inn_03 | Lv3 | `a set of ryokan room amenities on a tray: yukata, towel, tea cup, small soap` |
| inn_04 | Lv4 | `a premium stay set: folded futon, yukata, wooden bath bucket and a small flower vase` |

### 3.4 生产器（P0 ×1，P1 ×2）
| 文件 | 描述 |
|---|---|
| gen_toolbox (P0) | `a weathered old wooden toolbox with the lid closed, rusty iron latch, faded paper label, darker aged wood with chipped edges, a small sparkle hinting it can be tapped` (must look clearly older and closed, unlike the open brass-cornered tool_05) |
| gen_ricebin (P1) | `a traditional wooden rice storage bin with a scoop on top` |
| gen_linen (P1) | `a small wooden linen shelf stacked with towels` |

### 3.5 剧情 / 订单道具（P0）
| 文件 | 描述 |
|---|---|
| plank | `a few stacked light wooden planks` |
| prop_old_map | `an old folded paper town map, slightly torn at the corners, faded colors` |
| prop_charger | `a white phone charger with cable coiled` |
| prop_project_docs | `a stack of project documents in a clear folder with a prefecture seal` |
| prop_wagashi_mold | `an old carved wooden wagashi mold with a cherry blossom pattern` |
| prop_nerikiri_sakura | `a single pink cherry-blossom nerikiri wagashi on a small ceramic plate` |
| prop_bento | `a simple homemade bento wrapped in a furoshiki cloth` |

---

## 4. 剧情背景与 CG

模板：
```
Background illustration of [场景描述]. No people, no readable text. Wide 3:2 composition with the main subject in the center third so it can be cropped to a vertical phone screen. Use the attached image as the exact style reference.
[插画风格块]
```

| 优先级 | 文件 | 场景描述 |
|---|---|---|
| P0 | bg_station_day | 见 A3 |
| P0 | bg_office_messy | `the interior of a small abandoned project office hut: dusty desk, cardboard boxes, a chair with a coat, vines creeping in through a window, dust floating in sunlight` |
| P0 | bg_office_clean | 同上场景（用编辑功能）：`same room, now cleaned and tidy, boxes stacked, window open, a laptop and the old map on the desk` |
| P0 | bg_office_night | 同上场景：`same tidy room at night, desk lamp on, warm light, the old map spread on the desk, dark blue window` |
| P0 | bg_shop_street_day | `the entrance of a quiet old Japanese shopping street (shotengai) in a mountain town: closed shutters, a small wagashi shop with a faded wooden signboard and a paper notice on the door, potted plants, a general store with goods outside` |
| P0 | bg_chizuru_front | `close view of a closed traditional wagashi shop front: wooden lattice door, faded noren rolled up, a paper notice on the glass, potted plants by the door` |
| P0 | bg_chizuru_front_fixed | 同上（编辑）：`same shop front, cleaned, signboard repaired, potted plants arranged, a small entrance lamp glowing, still closed` |
| P0 | cg_map_reveal | `close-up of an old town map spread on a desk at night under a lamp; a faded festival poster printed on it reading only a number "47"; on the back a handwritten surname in ink (leave the name area blank to add in-engine)` |
| P1 | bg_chizuru_inside | `inside a small traditional wagashi workshop: wooden workbench, old molds on shelves, cloth-covered trays, soft light from a window` |
| P1 | bg_bath_inside_old | `the interior of an old Japanese public bathhouse (sento): tiled bath, a faded mountain mural on the wall, wooden buckets, slightly worn, little steam` |
| P1 | bg_bath_inside_fixed | 同上（编辑）：`same bathhouse restored, clean tiles, steady steam rising, warm lighting` |

> 文字（"第47回 雾见町夏祭""森川"）不要让 ChatGPT 画，在 Unity 里用字体叠加，避免错字。

---

## 5. UI

UI 面板、按钮、HUD 图标都用第 1.2b 节「休闲图标风格块」，和棋盘物品保持一致。

| 优先级 | 文件 | Prompt 主体 |
|---|---|---|
| P0 | ui_dialog_box | `A mobile game dialogue box panel, soft washi-paper texture in cream #F7F0E3, thin warm-brown border with a small cherry blossom ornament in one corner, rounded corners, empty center, transparent background, flat game UI asset` |
| P0 | ui_nameplate | `A small horizontal name tag for a game dialogue, vermilion #D9614C with a subtle washi texture, rounded ends, empty, transparent background` |
| P0 | ui_order_card | `A mobile game order card panel, cream paper with a wooden top strip and a small pin, empty slots area, transparent background` |
| P0 | ui_board_bg | `A merge game board background: a 7x9 grid of soft tatami-like square tiles in warm beige, subtle wooden frame, top-down flat, no items` |
| P0 | ui_board_frame | `A chunky rounded wooden frame for a mobile merge game board, warm wood #B9835A with a darker inner edge, empty transparent center, flat game UI asset, 9-slice friendly` |
| P0 | ui_tile | `A single square tile for a merge game board, soft cream-beige with a slightly darker rounded inner border, subtle tatami weave, flat, no items` |
| P0 | ui_buttons | `A set of rounded mobile game buttons in vermilion, warm wood and cream, each blank, soft shadow, transparent background, laid out in a grid` |
| P0 | ui_icons_hud | `A set of flat mobile game HUD icons in the same style: town map, task list, merge board, collection book, settings gear, energy (small onsen steam drop), coin (old brass coin), gem (sakura crystal), restoration material (wooden plank with nails), town revival meter (paper lantern). Each icon in its own cell, transparent background` |
| P0 | ui_chapter_banner | `A horizontal celebratory banner for "chapter complete": paper lantern garland, soft gold and vermilion ribbon, blank center for text, transparent background` |
| P1 | ui_collection_frame | `A frame for a collection book entry: cream card with a thin wooden border and a small pressed-flower decoration, blank center` |
| P2 | app_icon | `App icon: a glowing paper lantern hanging in front of a misty Japanese hot spring town at dusk, simple bold shapes, readable at small size, square` |
| P2 | logo | 最后做，标题文字在设计软件里排，不让 AI 画字 |

---

## 6. 小游戏素材

| 优先级 | 小游戏 | 文件 | 描述 |
|---|---|---|---|
| P0 | 清扫 | mg_dust_overlay | `A seamless semi-transparent layer of gray dust and cobweb smudges, transparent background, for a swipe-to-clean minigame` |
| P0 | 归位 | mg_office_items | `A set of separate office clutter objects: cardboard box, stack of papers, desk lamp, potted plant, folder, each isolated, transparent background` |
| P0 | 料理制作（和菓子） | mg_wagashi_set | `A set of separate wagashi-making items: colored nerikiri dough balls (pink, white, green, yellow), a wooden sakura mold open, a small ceramic plate, a bamboo spatula, each isolated, transparent background` |
| P1 | 调温 | mg_temp_gauge | `A vertical hot spring temperature gauge with a wooden frame, blue at the bottom to warm orange at the top, a small marker, transparent background` |

---

## 7. 数量汇总

| | P0（Vertical Slice） | P1（MVP 追加） |
|---|---|---|
| 角色立绘（含 5 表情） | 4 人 × 5 = 20 | 1 人 × 5 + 1 头像 = 6 |
| Merge 物品 + 生产器 | 6 | 11 |
| 剧情道具 | 7 | 0 |
| 背景 / CG | 7 | 3 |
| UI | 7 | 1 |
| 小游戏 | 3 | 1 |
| 合计 | 约 50 张 | 约 22 张 |

> 3D 小镇（建筑、地形、道路、路灯等）由我用 Blender 做，见 `art/3d/`。2D 和 3D 共用第 1.1 节色板。
