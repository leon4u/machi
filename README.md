# Tomoru Machi《温泉小镇复兴》

女性向 Cozy 手游：Merge + 剧情 + 小镇修复。Unity 6，移动端（iOS / Android），竖屏。

## 目录

| 路径 | 内容 |
|---|---|
| `Assets/Machi/Core/` | 纯 C# 游戏规则（合成棋盘、订单、体力、建筑修复、存档数据）。不依赖 UnityEngine，可单独测试 |
| `Assets/Machi/Runtime/` | Unity 层：启动、UI、3D 小镇、相机、存档读写 |
| `Assets/Machi/Editor/` | 导入规则、菜单 `Machi/...` |
| `Assets/Machi/Resources/Config/` | `game_config.json`（物品、生产器、订单、建筑、区域），`town_layout.json`（3D 摆放） |
| `Assets/Machi/Resources/Town/` | 低多边形 FBX 和调色板贴图 |
| `Assets/Machi/Resources/Items/` | 2D 物品图标（文件名 = 物品 id，例如 `tool_01.png`；没有图时显示色块占位） |
| `Design/` | 设计文档和美术 prompt |
| `Tools/Blender/` | 生成 3D 模型的 Blender 脚本 |
| `Tests/CoreTests/` | Core 的测试（`dotnet run`） |

## 第一次打开

1. Unity Hub → Add → 选这个文件夹。版本选任意 **Unity 6（6000.0.x LTS）**，提示升级 / 切换版本时直接确认。
2. 打开后会自动生成 `Assets/Machi/Scenes/Main.unity` 并打开（也可以用菜单 `Machi/Create Or Open Main Scene`）。
3. Game 视图分辨率选竖屏（例如 1080×1920），点 Play。

## 现在能玩到什么（M0 技术原型 = First Hour 的玩法骨架）

- 3D 小镇：拖动平移、滚轮 / 双指缩放、45° 俯视不旋转；点击建筑弹出修复面板
- 合成棋盘：点工具箱产出物品（消耗体力），拖动相同物品合成，拖到别的物品上交换
- 委托：神谷 → 千鹤 → 春江的 6 个委托，交付后获得修复材料
- 修复：项目办公室、站牌、候车室、千鹤堂（4 个阶段），修复后模型切换 + 复兴度上升
- 白天 / 夜晚切换（夜里灯会亮）
- 自动存档（每 10 秒、切后台、退出时），底部"重置存档"可以从头再玩

还没有：剧情对话、小游戏、2D 美术（用色块占位）、音效、商业化。

## 改数值 / 加内容

都在 `Assets/Machi/Resources/Config/game_config.json`，不需要改代码。改完后跑一次测试，确认配置没有写错、第一章还能通关：

```
cd Tests/CoreTests && dotnet run
```

## 渲染管线

M0 用内置渲染管线（Built-in），保证第一次打开就能跑。之后切 URP：Package Manager 安装 Universal RP → `Window > Rendering > Render Pipeline Converter` 一键转换。`TownView` 会自动识别当前管线并使用对应 Shader。
