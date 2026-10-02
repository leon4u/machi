# 雾见町 3D 资产（第 1 批：Vertical Slice + 全图剪影）

所有模型都由 `scripts/` 里的 Blender Python 脚本生成，可以重复生成、改参数后重跑。

## 目录

| 路径 | 内容 |
|---|---|
| `fbx/assets/` | 每个资产一个 FBX（Unity 直接拖入） |
| `fbx/town/town_ground.fbx` | 地形、河流、道路 |
| `fbx/town/town_full_layout.fbx` | 整张小镇（地形 + 所有摆放好的资产），用于快速预览 |
| `town_layout.json` | 每个资产实例的位置、旋转、所属区域（Unity 坐标），运行时摆放用 |
| `textures/T_TownPalette.png` | 所有模型共用的调色板贴图（32×20 像素） |
| `kirimi_town.blend` | Blender 源文件 |
| `renders/` | 预览图 |
| `scripts/` | 生成脚本：`lowpoly_lib.py`（工具函数和色板）、`assets.py`（每个资产）、`build_town.py`（布局、导出、渲染） |

## 已做的资产

**第 1 批正式模型（区域 ① 旧车站区、② 商店街前段）**
- 站牌 `station_sign_s0/s1`（破旧 / 修复亮灯）
- 候车室 `station_waiting_room_s0/s1`（木板封窗 / 开放亮灯）
- 项目办公室小屋 `office_hut_s0/s1`（藤蔓覆盖 / 清理后）
- 千鹤堂 `chizurudo_s0~s3`（关闭褪色 / 门面修复 / 每周两天营业 / 正式营业）
- 春江杂货店、空铺 A（卷帘门）、空铺 B（木板封窗）、普通民居
- 铁轨、月台、巴士站
- 道具：路灯（亮 / 灭）、电线杆、花箱、盆栽、长椅、自动贩卖机、货箱、木栅栏、杉树、圆树、樱花树、灌木、杂草、石头、纸灯笼

**剪影（Blockout，雾色，未开放区域占位）**
公共浴场、商店街后段 4 家店、河岸仓库、石桥、月见庄、鸟居、参道石阶、神社本殿、夏祭舞台、温泉源头小屋。做到对应章节时再替换成正式模型。

## 导入 Unity

machi 仓库里已经接好了：FBX、调色板和 `town_layout.json` 放在 `Assets/Machi/Resources/`，`TownView.cs` 在运行时摆放模型并替换材质（`M_TownPalette` / `M_Glow`），导入设置由 `MachiImportRules.cs` 自动处理（调色板 Point 过滤、不压缩、无 Mipmap）。

坐标约定：`town_layout.json` 里以 `blender` 字段为准（Blender 坐标，Z 朝上）。`TownView` 把小镇根节点绕 Y 轴转 180°，子物体位置取 `(-x, z, -y)`、旋转取 `-rot_z`，最终世界坐标是 `(x, 高度, y)`，建筑正面朝 -Z，相机在南边朝 +Z 看。`unity` 字段已弃用，不要使用。

## 修复阶段怎么切换

同一栋建筑的不同阶段是独立模型（如 `chizurudo_s0` ~ `chizurudo_s3`），位置和朝向完全一样。machi 仓库的 `RestorableBuilding` 组件会加载所有阶段模型，按存档里的阶段只激活其中一个。`town_layout.json` 中标了 `restorable:xxx` 的实例就是需要切换阶段的建筑。

## 重新生成

需要 Python 3.11 和 `bpy==4.2.0`（`pip install bpy==4.2.0`），或者直接用 Blender 4.2：

```
python3 scripts/build_town.py <输出目录>
blender -b -P scripts/build_town.py -- <输出目录>
```

加 `--no-render` 可以跳过渲染，几秒就能导出。

## 局限

- 这是程序化生成的低多边形模型，风格统一、面数很低，适合手机端，但细节不如手工建模。原型和 Vertical Slice 够用，正式上线前可以按需精修关键建筑（千鹤堂、浴场、月见庄）。
- 角色 / NPC 小人没有做，目前计划在 3D 里用 2D 立绘做成的公告板小人，或者之后再单独做。
