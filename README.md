# 皮革划线排样软件（Leather Nesting / draw-line）

面向皮革裁切生产的**不规则形状排样（Nesting）**桌面软件。核心职责是：读入裁片轮廓（DXF）→ 在给定皮革幅面上自动计算裁片的最优摆放位置与角度（**排样**）→ 输出排样结果图（DXF），供下游排版/裁切使用。

> 本软件**负责排样**，**不负责生成切割刀路**（toolpath / G-code）——刀路由下游切片软件完成。职责边界详见 [ADR 02-职责](docs/adr/02-职责.md)。

---

## 目录

1. [项目背景](#1-项目背景)
2. [核心职责与输出契约](#2-核心职责与输出契约)
3. [功能特性](#3-功能特性)
4. [系统架构](#4-系统架构)
5. [桌面 Shell 工作台](#5-桌面-shell-工作台)
6. [桌面模块 M01~M12](#6-桌面模块-m01m12)
7. [排样算法链](#7-排样算法链)
8. [DXF 输入 / 输出](#8-dxf-输入--输出)
9. [技术栈](#9-技术栈)
10. [仓库结构](#10-仓库结构)
11. [构建 · 运行 · 测试](#11-构建--运行--测试)
12. [文档组织](#12-文档组织)
13. [Python 演示 Demo](#13-python-演示-demo)
14. [开发状态与诚实标注](#14-开发状态与诚实标注)
15. [参考资料](#15-参考资料)

---

## 1. 项目背景

在皮革制品（鞋面、皮包、皮衣等）生产中，需要从大张天然皮革上裁切出各种不规则形状的裁片。皮革形状不规则、每张裁片形状各异，如何在幅面上尽可能紧密地排列裁片、找到最优摆放方案，是降低材料浪费、控制生产成本的关键环节。

本项目以一款市售皮革裁切划线软件为参照，**1:1 复刻其工作台界面与核心功能**，并自研排样算法，最终输出可供通用软件（AutoCAD、Illustrator 等）使用的排样图（DXF 格式）。

**现状一句话**：桌面端是参照软件的单窗口 CAD 工作台复刻（含 12 个业务模块入口、五栏工作台、菜单/工具栏、订单与裁片数据面板）；排样与几何核心（Nesting / DXF / 几何修复）已在底层四层以 C# 实现并通过测试。大量界面仍以 DEMO 数据驱动，生产级接线与持久化在持续推进（见 [第 14 节](#14-开发状态与诚实标注)）。

---

## 2. 核心职责与输出契约

```
┌─────────────────────────────────────────────┐
│  本软件负责（核心职责：排样）                  │
│   1. 读入裁片轮廓（DXF）                      │
│   2. 排样 nesting（任意角度 / 自由旋转）       │
│   3. 输出排样结果文件（DXF 先行，JSON 暂缓）   │
└─────────────────────────────────────────────┘
                        │
    不负责（交给下游切片软件）│
                        ▼
┌─────────────────────────────────────────────┐
│  切割刀路（toolpath / G-code）生成            │
│  切片软件的厂商兼容适配（标准格式解耦）        │
└─────────────────────────────────────────────┘
```

### DXF 输出颜色约定（ACI 62）

排样结果导出 DXF 时，用颜色码区分线角色，与参照/远程排样系统保持一致：

| ACI | 角色 | 含义 |
|-----|------|------|
| `0` | 外轮廓（Bound） | 裁片外边界 |
| `3` | 切割线（Cutoff） | 切穿线 |
| `5` | 标记线 / 刀口（Cutoff） | 半切 / 记号 |

输出仅含几何线（绝对坐标 + 闭合位），不含刀路。

---

## 3. 功能特性

### 已实现的真实逻辑（有测试支撑）

| 能力 | 位置 | 说明 |
|------|------|------|
| DXF 读取 | `Infrastructure/Dxf/AsciiDxfReader` | 自研 ASCII DXF 解析（弃用 netDxf，见 ADR 01）；识别闭合 LWPOLYLINE/POLYLINE；**单位裁决门**——未确认毫米单位即阻塞，防止误用非毫米数据 |
| 几何修复 | `Geometry/Repair` | 轮廓闭合（ContourCloser）、缝隙修复（GapRepair）、边界生成（BoundaryGenerator） |
| 排样引擎 | `Geometry/Nesting` | 贪心 bottom-left-fill 排样，**确定性**（同输入必同输出）；支持任意角度候选、NFP 无碰撞贴靠、Clipper2 碰撞校验、局部搜索优化 |
| DXF 输出 | `Infrastructure/Dxf/AsciiNestingDxfWriter` | 组装三图层 DXF（裁片 / 标注 / 利用率标题），按 ACI 62 颜色契约落盘 |
| 用例编排 | `Application` | DxfImport（导入用例）、NestingExport（导出用例）、NestingProjectFactory、CadEditing/CadCommands 命令栈、Ports 端口集 |
| 项目存取 | `Infrastructure/Projects` | ZipProjectStore、ZipNestingProjectStore、ProjectSnapshotStore |
| 排样算法语言选型 | 决策记录 | 排样算法采用 C# 托管实现（vs C++ 下沉，见 ADR 05） |

### 桌面 UI 能力（界面已复刻，多为 DEMO 数据驱动）

| 能力 | 说明 |
|------|------|
| 单窗口 CAD 工作台 | 复刻参照软件的五栏高密度工作台，非「左导航 + 卡片页」现代风格 |
| 菜单栏（8 组） | 文件 / 编辑 / 操作 / 绘制 / 数据库 / 工具 / 设置 / 帮助，命令路由到各模块 |
| 图标工具栏（10 项） | 新建排版、订单管理、CAD工具、开始排版、停止排版、取消排版、范围缩放、设置窗口、等宽长条、发送切割 |
| 「新建排版 → 版型设置」 | 弹出版型设置模态框，确定后写入共享配置并更新状态栏摘要 |
| 「设置 → 订单窗口」 | **可勾选开关**：勾选=显示左侧栏（订单组 / 裁片列表 / 进度汇总），取消=左侧栏缩回左缘、中央画布变宽 |
| 订单组多订单折叠卡片 | 每订单一张卡片，点击展开/收起订单明细；选中订单切换下方裁片列表 |
| 裁片卡片列表 | 高密度裁片卡片（缩略图 / 尺码 / 包围尺寸 / 旋转 / 完成度 / 单套 / 套数 / 余量 / 总量），数量可编辑 |
| CAD 画布 | 黑色画布 + 左侧/底部标尺，默认模块 M03 常驻中央；导入 DXF 后自动切换到画布 |
| 进度汇总 / 排版输出信息 / CAD 参数 | 左侧、右侧停靠面板，展示 DEMO 统计与参数 |
| 快捷键目录 | 复刻参照软件 §8.3 快捷键表（Ctrl+Z / Ctrl+C / Ctrl+T 导到订单等），映射到菜单标签与路由 |

> ⚠️ **诚实标注**：上表桌面 UI 均以**骨架/演示数据**呈现，界面元素上的「· DEMO」字样即此含义；真实排样数据回写、订单持久化等生产接线仍为 TODO（见 [第 14 节](#14-开发状态与诚实标注)）。

---

## 4. 系统架构

### 分层架构（Clean Architecture / 端口-适配器）

五层自外向内单向依赖，内层永不引用外层；I/O 收敛在两端（读 DXF 进、写 DXF 出）。

```
┌───────────────────────────────────────────────────────────────────────────────┐
│  Desktop（表现层 · Avalonia + MVVM + 组合根）            111 个源文件            │
│    Program/App │ MainWindow │ Shell 五栏工作台 │ 12 个模块 M01~M12             │
└──────────────▲──────────────────────────────────────────────▲────────────────┘
               │ 依赖（调用 Application 用例）                  │ 依赖
┌──────────────┴───────────────┬──────────────────────────────┴────────────────┐
│  Infrastructure（适配器层 · 唯一懂外部格式的地方）         14 个源文件           │
│    Dxf/AsciiDxfReader · AsciiDxfWriter · AsciiNestingDxfWriter                │
│    Projects/ZipProjectStore · ZipNestingProjectStore · ProjectSnapshotStore   │
└──────────────▲──────────────────────────────┬────────────────────────────────┘
               │ 实现端口                       │ 依赖
┌──────────────┴───────────────┬──────────────┴────────────────────────────────┐
│  Application（用例编排 + 端口定义）          20 个源文件                       │
│    DxfImport(导入用例) · NestingExport(导出用例) · NestingProjectFactory      │
│    CadEditing/CadCommands 命令栈 · Ports.cs 端口接口集                        │
└──────────────┬───────────────┬────────────────────────────────────────────────┘
               │ 依赖           │ 依赖
┌──────────────┴───────┐   ┌────┴──────────────────────────────────────────────┐
│  Geometry（纯计算几何） │   │  Domain（纯实体 / 值对象）         7 个源文件        │
│    38 个源文件         │   │    ProjectDocument · ImportReport                │
│    Loop2D/Curve2D/    │   │    ImportDiagnostic · UnitDecision               │
│    Point2D/Transform  │   │    不可变 record + with 表达式                    │
│    Nesting/（排样引擎） │   │    零依赖                                        │
│    Repair · Topology  │   │                                                  │
│    Offset · Features  │   │                                                  │
│    NodeEditing        │   │                                                  │
│    ClipperPathAdapter │   │                                                  │
│      └─→ Clipper2     │   │                                                  │
└───────────────────────┘   └──────────────────────────────────────────────────┘
```

> 注意：`Infrastructure → Application` 不是违规，而是「端口在 Application、实现在 Infrastructure」的自然结果（见 [ADR 02-职责](docs/adr/02-职责.md) 与治理文档 P1）。

### 工程 / 测试依赖图

```
LeatherNesting.sln
│
├── src/                            依赖方向（→ 表示「引用」）
│   ├── LeatherNesting.Domain          （零依赖）
│   ├── LeatherNesting.Geometry    → Domain + Clipper2(NuGet)
│   ├── LeatherNesting.Application → Domain + Geometry
│   ├── LeatherNesting.Infrastructure → Domain + Application + Geometry
│   └── LeatherNesting.Desktop     → Application + Infrastructure + Avalonia(NuGet)
│
└── tests/                          与 src/ 1:1 镜像
    ├── LeatherNesting.Domain.Tests
    ├── LeatherNesting.Geometry.Tests
    ├── LeatherNesting.Application.Tests
    ├── LeatherNesting.Infrastructure.Tests
    ├── LeatherNesting.Desktop.Tests
    └── LeatherNesting.EndToEnd.Tests   （跨层集成）
```

### 运行时数据流

```
   ┌────────────┐        ┌────────────────────┐        ┌─────────────────────┐
   │  输入 DXF   │        │  Desktop 模块       │        │  用例 + 适配器        │
   │ 凉鞋.dxf 等 │        │  M02 导入 → M03 画布 │        │                      │
   └─────┬──────┘        └─────────┬──────────┘        └──────────┬──────────┘
         │                          │                              │
         ▼                          ▼                              ▼
   ┌──────────────────────────────────────────────────────────────────────┐
   │  Infrastructure  Dxf/AsciiDxfReader                                  │
   │    清点实体 → 识别闭合 LWPOLYLINE/POLYLINE → 单位裁决门（未确认毫米即阻塞）│
   └───────────────────────────────────┬──────────────────────────────────┘
                                        │ Loop2D 原始轮廓
                                        ▼
   ┌──────────────────────────────────────────────────────────────────────┐
   │  Geometry  Repair  ContourCloser / GapRepair / BoundaryGenerator     │
   │     修复并闭合 → 合法 Loop2D                                          │
   └───────────────────────────────────┬──────────────────────────────────┘
                                        │
                                        ▼
   ┌──────────────────────────────────────────────────────────────────────┐
   │  Geometry  Nesting/  排样引擎 NestEngine                              │
   │   ├─ PlacementCandidateGenerator    生成任意角度候选位姿               │
   │   ├─ NfpCalculator                  NFP = Minkowski 和，求无碰撞贴靠点  │
   │   ├─ ClipperCollisionDetector       Clipper2 布尔做碰撞/间隙校验        │
   │   └─ NestOptimizer.Optimize         局部搜索（确定性 seed）             │
   └───────────────────────────────────┬──────────────────────────────────┘
                                        │ NestResult（利用率 Utilization）
                                        ▼
   ┌──────────────────────────────────────────────────────────────────────┐
   │  Application  NestingExport 用例 → Infrastructure AsciiNestingDxfWriter│
   │     组装三图层 DXF（裁片 / 标注 / 利用率标题）→ 落盘                      │
   └──────────────────────────────────────────────────────────────────────┘
```

> 完整图形化架构（ASCII + Mermaid 双版本，含模块导航图、业务边界图）见 [docs/architecture.md](docs/architecture.md)；文字版架构基线见 [docs/architecture-and-governance.md](docs/architecture-and-governance.md)。

---

## 5. 桌面 Shell 工作台

应用以**单窗口高密度 CAD 工作台**呈现（参照软件复刻，非左导航卡片式）。窗口固定经典浅色主题（`ThemeVariant.Light`），默认视口 **1366×768**（最小 1024×640）。

```
┌───────────────────────────────────────────────────────────────────────┐
│ 菜单：文件 编辑 操作 绘制 数据库 工具 设置 帮助                          │
│ 工具栏：新建排版 订单管理 CAD工具 开始排版 停止排版 取消排版 范围缩放      │
│         设置窗口 等宽长条 发送切割                                      │
├──────────────┬─────────────────────────────────────────────────────────┤
│ 左栏(13%)     │  中央 CAD 画布 (74%)                右栏(13%)           │
│ ┌ 订单组 ──┐ │  ┌─────────────┬──────────────────┐  ┌ CAD 参数 ────┐   │
│ │ ▸贴皮测试 │ │  │ 竖标尺       │ 黑色画布          │  │ (属性面板)    │   │
│ │ ▸鞋面-39 │ │  │              │  + CAD 工作台     │  ├ 排版输出信息─┤   │
│ ├ 裁片列表─┤ │  │              │                  │  │ 利用率/面积/片数│ │
│ │ □◉ 40卡 │ │  ├─────────────┴──────────────────┤  │ 耗时等       │   │
│ │ □◉ 40卡 │ │  │ 横标尺                          │  └──────────────┘   │
│ ├ 进度汇总─┤ │  └────────────────────────────────┘                     │
│ │ 组进度▓  │ │                                                         │
│ └─────────┘ │                                                         │
├──────────────┴─────────────────────────────────────────────────────────┤
│ 状态栏：就绪 │ 项目：… 状态：… │ DEMO · 骨架数据仅用于界面对照 │ 版本    │
└─────────────────────────────────────────────────────────────────────────┘
```

### 五栏结构

| 区域 | 组成 | 比例 |
|------|------|------|
| 左栏 | 订单组（多订单折叠卡片）、裁片列表、进度汇总 | `20*,60*,20*` 行 |
| 中央 | 竖标尺(22px) + 黑色 CAD 画布 + 横标尺(20px) | 固定标尺 + 画布 |
| 右栏 | CAD 参数（属性面板）、排版输出信息 | `62*,38*` 行 |
| 顶栏 | 菜单栏 + 图标工具栏 | 固定高度 |
| 状态栏 | 状态 / 项目 / DEMO 提示 / 版本 | 固定高度 |

三栏列宽 `13* : 74* : 13*`，与参照软件（258 : 1387 : 307）比例一致。

### 「设置 → 订单窗口」左侧栏显隐

- 该菜单项是**可勾选开关**：勾选 = 显示左侧栏，取消勾选 = 左侧栏（订单组 + 裁片列表 + 进度汇总）整体缩回左缘，**中央画布随之变宽**；再次勾选恢复。
- 实现：`AppShellViewModel` 抛 `OrderWindowToggleRequested` 事件 → `AppShellView.ToggleLeftRail()` 显式清零左栏列宽（`13* → 0`，不依赖 star 列自动收缩）并同步菜单勾选状态。
- 约束：三栏 `13*,74*,13*` 几何与左栏三行比例始终保持不变，折叠只改变列宽。

### 顶栏命令模型

- **菜单栏**：8 组（文件/编辑/操作/绘制/数据库/工具/设置/帮助），每组含命令、分隔符与子菜单（如「设置 → 语言 → 中文/英文」）。菜单命令通过 `AppShellViewModel.Select` 路由到目标模块；未实现动作发布标准 TODO 提示而非伪造成功。
- **工具栏**：10 项图标 + 文字矢量命令，快捷键与目标模块见 [ShellTopCommands.cs](src/LeatherNesting.Desktop/Shell/ShellTopCommands.cs)。
- **特例**：「新建排版」是唯一会弹模态框的命令（`ShellCommandLaunch.NewBoardSettings`）——由 View 层 `ShowDialog` 弹出版型设置窗口，确认后写入 `BoardSettingsStore.Default`。
- 图标均为 Avalonia 矢量图形，非字体字形/网络位图（跨平台渲染稳定）。

---

## 6. 桌面模块 M01~M12

Shell 通过 `DesktopModuleDiscovery` 反射发现 `IDesktopModule`，`DesktopComposition`（唯一组合根）装配各模块工厂；12 个模块按固定顺序注册（`UiDemoIntegrationTests` 断言 M01~M12 稳定有序）。

| ID | 模块 | 分组 | 模块类 | 当前状态 |
|----|------|------|--------|----------|
| M01 | 项目与订单 | 项目 | `ProjectsModule` | 界面 + 订单信息展示 |
| M02 | DXF 导入 | 项目 | `ImportModule` | **真实导入模块**（读 DXF → 投影到 CAD 画布） |
| M03 | CAD 画布 | CAD 工作台 | `CadCanvasModule` | **默认模块**，常驻中央画布 |
| M04 | 几何修复 | CAD 工作台 | `GeometryRepairModule` | 界面 |
| M05 | 工艺特征 | CAD 工作台 | `ProcessFeaturesModule` | 界面 |
| M06 | 裁片 | 数据 | `PiecesModule` | 裁片/尺码/订单数量（DEMO 数据） |
| M07 | 材料 | 数据 | `MaterialsModule` | 界面 |
| M08 | 排样运行 | 排样 | `NestingRunModule` | 界面 |
| M09 | 排样复核 | 排样 | `NestingReviewModule` | 界面 |
| M10 | 校验 | 排样 | `ValidationModule` | 界面 |
| M11 | 导出 | 输出 | `ExportModule` | 界面 |
| M12 | 管理 | 管理 | `AdministrationModule` | 界面 |

> 模块的「真实逻辑」集中在底层（Geometry/Infrastructure/Application）；M01~M12 的桌面 View 大多以 DEMO 数据驱动界面对照。

---

## 7. 排样算法链

排样是经典 NP-hard 组合优化问题。本项目采用**确定性、可复现**的引擎设计（同输入必同输出，便于测试与审计）：

| 组件 | 文件 | 职责 |
|------|------|------|
| 排样引擎 | `Geometry/Nesting/NestEngine.cs` | 贪心 bottom-left-fill：按面积降序（稳定 ID 破平）逐个放入左下可放位置；`NestInOrder` 供优化器探索重排 |
| 候选位姿 | `PlacementCandidateGenerator.cs` | 生成任意角度候选位姿（ADR-02） |
| 无碰撞贴靠 | `NfpCalculator.cs` | NFP = Minkowski 和，求无碰撞贴靠点 |
| 碰撞/间隙校验 | `ClipperCollisionDetector.cs` | Clipper2 布尔运算做碰撞与间隙校验（支持间隙 mm 约束） |
| 局部搜索 | `NestOptimizer.cs` | 确定性 seed 的局部搜索优化利用率 |
| 模型 | `NestModels.cs` | NestRequest / NestResult（Placements / Unplaced / Utilization） |

- 支持任意角度（自由旋转），毛向/纹路等约束在任意角度基础上叠加。
- 间隙参数为负时抛异常（防御性校验）；允许旋转集合为空时报错。
- 相关决策：逆向参照算法 [ADR 04](docs/adr/04-逆向axenester算法.md)；C# 托管 vs C++ 下沉选型 [ADR 05](docs/adr/05-排样算法-csharp-vs-cpp.md)。

---

## 8. DXF 输入 / 输出

### 读取（`Infrastructure/Dxf/AsciiDxfReader.cs`）

- 自研 ASCII DXF 解析（**弃用 netDxf**，理由见 [ADR 01](docs/adr/01-dxf-adapter.md)）。
- 清点实体 → 识别闭合 `LWPOLYLINE` / `POLYLINE` → 提取 `Loop2D` 原始轮廓。
- **单位裁决门（UnitDecision）**：`DXF-UNIT-REVIEW` 阻塞级诊断，未由用户确认为毫米前禁止提交为毫米项目，防止误用非毫米单位数据。
- 返回 `ImportReport` / `ImportDiagnostic`（Domain 模型），供 UI 呈现。

### 写出（`Infrastructure/Dxf/AsciiNestingDxfWriter.cs`）

- 组装三图层 DXF：**裁片 / 标注 / 利用率标题**。
- 颜色契约（ACI 62）：`0`=外轮廓 Bound、`3`=切割线 Cutoff（切穿）、`5`=标记线/刀口（半切/记号）。
- 输出仅含几何线（绝对坐标 + 闭合位），**不含刀路**。

---

## 9. 技术栈

| 类别 | 选型 | 版本 |
|------|------|------|
| 运行时 / 语言 | .NET（C#，net10.0） | SDK 10.0.400（global.json 固定） |
| UI 框架 | Avalonia（跨平台 XAML/C# 桌面） | 12.1.0 |
| UI 主题 | Avalonia.Themes.Fluent | 12.1.0 |
| 几何布尔运算 | Clipper2 | 2.0.0 |
| 测试框架 | xUnit | 2.9.3（runner 3.0.2，Test.Sdk 17.12.0） |
| 依赖管理 | 中央包管理（Directory.Packages.props） | — |

> 技术选型完整论证见 [ADR 03-技术栈选择](docs/adr/03-技术栈选择.md)。

---

## 10. 仓库结构

```
划线软件-叔叔/
├── LeatherNesting.sln                 解决方案（5 个 src 工程 + 6 个测试工程）
├── Directory.Build.props / Directory.Packages.props   构建与中央包管理
├── global.json                        .NET SDK 版本固定
├── README.md / AGENTS.md / com.md    说明与协作/沟通约定
│
├── src/                              正式产品（C#）
│   ├── LeatherNesting.Domain          纯实体/值对象（零依赖，7 文件）
│   ├── LeatherNesting.Geometry        纯计算几何 + 排样引擎（38 文件）
│   ├── LeatherNesting.Application     用例编排 + 端口定义（20 文件）
│   ├── LeatherNesting.Infrastructure  DXF / 项目存储适配器（14 文件）
│   └── LeatherNesting.Desktop         Avalonia 表现层：Shell + 12 模块（111 文件）
│
├── tests/                            与 src/ 1:1 镜像的测试工程
│   ├── LeatherNesting.Domain.Tests / Geometry.Tests / Application.Tests
│   ├── LeatherNesting.Infrastructure.Tests / Desktop.Tests
│   └── LeatherNesting.EndToEnd.Tests  跨层集成
│
├── docs/                             项目文档
│   ├── adr/                          决策记录（01-dxf-adapter … 05-排样算法语言）
│   ├── todo/                         任务/待办（数字前缀命名）
│   ├── architecture.md               系统架构图（ASCII + Mermaid）
│   └── architecture-and-governance.md 架构基线 + 治理方向
│
├── python-demo/                      Python 排样展示 Demo（对外演示）
├── demo_output/                      排样 Demo 输出（DXF / PNG / summary.json）
├── fixtures/                         golden 基准等测试夹具
├── 凉鞋.dxf                         凉鞋裁片输入样例（C# 与 Python 共用）
│
├── .trellis/                         开发流程管理（工作流、spec、任务、日志）
└── 07-逆向软件/ 01-加密方案/ …       参照软件逆向与竞品分析资料
```

> 文档规范：决策记录放 `docs/adr/`（数字前缀 `NN-名字.md`）；任务待办放 `docs/todo/`（同样数字前缀）。`docs/decisions/` 已废弃不再使用。

---

## 11. 构建 · 运行 · 测试

```bash
# 构建整个解决方案
dotnet build LeatherNesting.sln

# 运行桌面应用（Avalonia）
dotnet run --project src/LeatherNesting.Desktop/LeatherNesting.Desktop.csproj

# 运行全部测试
dotnet test LeatherNesting.sln

# 只跑某测试工程
dotnet test tests/LeatherNesting.Desktop.Tests

# 只跑某测试类（示例：Shell 框架布局测试）
dotnet test tests/LeatherNesting.Desktop.Tests --filter "FullyQualifiedName~ShellFrameTests"
```

> 当前验证基线：**全解决方案 6 个测试工程、411 项测试全部通过**（2 Domain + 12 Application + 64 Geometry + 35 Infrastructure + 1 EndToEnd + 297 Desktop）。UI 相关测试属于 `"Avalonia UI"` 非并行集合（`DisableParallelization = true`），构造控件断言结构状态，不依赖窗口挂载。

---

## 12. 文档组织

| 文档 | 内容 |
|------|------|
| [docs/architecture.md](docs/architecture.md) | 系统架构图：分层 / 依赖 / 数据流 / 模块导航 / 业务边界（ASCII + Mermaid） |
| [docs/architecture-and-governance.md](docs/architecture-and-governance.md) | 架构基线 + 治理方向（文字版） |
| [docs/adr/01-dxf-adapter.md](docs/adr/01-dxf-adapter.md) | DXF 适配决策（自研 reader，弃 netDxf） |
| [docs/adr/02-职责.md](docs/adr/02-职责.md) | 职责边界与输出契约 |
| [docs/adr/03-技术栈选择.md](docs/adr/03-技术栈选择.md) | 技术栈选择 |
| [docs/adr/04-逆向axenester算法.md](docs/adr/04-逆向axenester算法.md) | 逆向排样算法 |
| [docs/adr/05-排样算法-csharp-vs-cpp.md](docs/adr/05-排样算法-csharp-vs-cpp.md) | 排样算法语言选择（C# 托管 vs C++ 下沉） |
| [docs/todo/](docs/todo/) | 待办：JSON 输出契约 / 画布渲染旋转交互 / 工艺工作台嵌入 / Piece 模型统一 / 菜单工具栏接线等 |

---

## 13. Python 演示 Demo

[`python-demo/`](python-demo/) 是独立的 **Python 排样展示 Demo**，用于对外演示算法效果（读 DXF → 排样 → 输出 DXF/PNG/利用率），与 C# 正式产品解耦。

```bash
cd python-demo && ./run.sh
```

- 一键生成三种皮革尺寸的排样 DXF/PNG 与利用率汇总，并展示自由角度（0° + 175°）排样效果。
- 输出落盘到 `demo_output/`（`2000x1000.dxf/png`、`2000x4000.*`、`2000x9000.*`、`free_angle_175deg_showcase.png`、`summary.json`）。
- 详见 [`python-demo/README.md`](python-demo/README.md)。

> ⚠️ Python Demo 是确定性货架填充演示，**不保证全局最优**，仅作展示、不再作为算法参照演进。正式产品为 C# 排样引擎（`src/LeatherNesting.Geometry/Nesting/`，NFP + 局部搜索，支持任意角度）。

---

## 14. 开发状态与诚实标注

- ✅ **已落地（有测试）**：分层架构、DXF 读取（含单位裁决门）、几何修复、排样引擎（确定性 NFP + 局部搜索）、DXF 三图层输出、桌面 Shell 五栏工作台、12 模块发现与导航、菜单/工具栏命令路由、版型设置模态、订单组折叠卡片、「设置 → 订单窗口」左侧栏显隐。
- 🟡 **DEMO 数据驱动（界面已复刻、逻辑待接线）**：订单组 / 裁片列表 / 进度汇总 / 排版输出信息等停靠面板；M01~M12 多数模块 View；订单数量编辑、批量优先级等动作。
- 🔵 **待办（见 `docs/todo/`）**：JSON 输出契约、画布渲染旋转交互、工艺工作台嵌入主界面、统一 Piece 模型、菜单栏工具栏命令接线、真实排样回写与订单持久化。
- 界面上的「· DEMO」字样是**诚实标注**，表示该区域仅用骨架数据对照参照软件，未伪造真实结果。

---

## 15. 参考资料

- DXF 格式规范：[Autodesk DXF Reference](https://www.autodesk.com/developer-network/platform-technologies/autocad-dxf)
- 排样问题综述：*Irregular Packing Problems: A Review of Mathematical Models*
- 相关开源项目：Deepnest、SVGNest
- 参照软件逆向资料：`07-逆向软件/`、`05-视频上面梳理的信息.md`、`06-首次远程-收集的信息/`
