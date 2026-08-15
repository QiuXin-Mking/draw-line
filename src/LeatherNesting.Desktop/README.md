# LeatherNesting.Desktop

**桌面端（Avalonia UI）**：基于 **Avalonia** 的可执行程序（`WinExe`）。它是整个软件的壳——负责窗口、导航、画布渲染、各功能页面，并把应用层用例与基础设施实现装配起来（依赖注入）。

它采用 **模块化 Shell 架构**：一个固定五区的经典工作站外壳（`Shell/`），通过 `IDesktopModule` 接口发现并装载 12 个功能模块（`Modules/`，编号 M01–M12），每个模块自带 View + ViewModel + Module 声明。

```
LeatherNesting.Desktop
   ├─ 依赖：Application（用例）、Infrastructure（端口实现）
   └─ 间接：Geometry（画布渲染用几何）
```

---

## 入口与装配

| 文件 | 职责 |
| --- | --- |
| `Program.cs` / `App.cs` | Avalonia 程序入口。 |
| `MainWindow.cs` | 主窗口，承载 Shell。 |
| `Composition/DesktopComposition.cs` | **组合根**：集中装配桌面服务与各模块工厂（依赖注入中心）。 |

---

## `Shell/` — 工作站外壳（框架）

| 文件 | 职责 |
| --- | --- |
| `AppShellView.cs` / `AppShellViewModel.cs` | 五区经典工作站框架，负责 12 个模块的导航与当前选中模块状态。 |
| `ModuleDescriptor.cs` | 描述 12 个可导航模块之一。 |
| `DesktopModuleDiscovery.cs` | 从桌面程序集发现各模块定义。 |
| `ShellTopCommands.cs` | 顶层菜单契约（`ShellTopMenu`）与图标工具栏顺序（`ShellToolbar`）。 |
| `TopCommandArea.cs` | 传统两级菜单 + 图标命令区（含 `ShellToolbarButton`）。 |
| `CadWorkspaceHost.cs` | 贡献给外壳中心区的 CAD 控件宿主。 |
| `CadPropertyPane.cs` | 外壳右侧的高密度 CAD 属性面板。 |

---

## `Modules/` — 12 个功能模块

每个模块遵循 `XxxModule : IDesktopModule`（声明元数据 + 视图工厂）+ `XxxView`（UI）+ `XxxViewModel`（状态）的结构。模块声明与视图放在同一目录，供 Shell 自动发现。

| 编号 | 模块目录 | 职责 |
| --- | --- | --- |
| M01 | `Projects/` | 项目 & 订单中心（演示页）。 |
| M02 | `Import/` | **真实 DXF 导入检查器**：检查 → 确认 → 持久化，复用项目/导入工作流。 |
| M03 | `CadCanvas/` | CAD 浏览（含演示几何、工具栏 `Toolbar/`）。 |
| M04 | `GeometryRepair/` | 轮廓诊断与几何修复工作台（问题列表、修复工具、前后差异预览）。 |
| M05 | `ProcessFeatures/` | 工艺特征与放码规则库（演示）。 |
| M06 | `Pieces/` | 裁片、尺寸、订单数量、放置约束（高密度演示页）。 |
| M07 | `Materials/` | 材料清单与排样约束（演示）。 |
| M08 | `NestingRun/` | 排样策略与运行控制（演示，不启动真实排样）。 |
| M09 | `NestingReview/` | 排样结果画布与人工审阅（演示）。 |
| M10 | `Validation/` | 校验、审批与质检报告（演示）。 |
| M11 | `Export/` | 导出包配置与清单（演示）。 |
| M12 | `Administration/` | 规则库、审计、权限、系统设置（演示）。 |

> 注意：标注“演示”的模块（M01/M05/M06/M07/M08/M09/M10/M11/M12）目前多为只读占位数据，`ViewModel` 注释里明确写着 `TODO`，尚未接入真实持久化或排样逻辑。真正接入业务的是 **M02（导入）** 和 **U4 CAD 工作台**。

`Modules/Contracts/` 定义模块契约：`IDesktopModule`、`DesktopModuleMetadata`（导航元数据）、`DesktopModuleCatalog`（按导航顺序呈现）。

---

## `ViewModels/` + `Views/` — 核心工作台

| 文件 | 职责 |
| --- | --- |
| `Views/CanvasView.cs` | **CAD 画布**：把毫米坐标（Y 向上）绘制到像素（Y 向下），带缩放/平移、点选/拖选。 |
| `Views/CadWorkbenchView.cs` | **U4 CAD 修复与工艺工作台**：单画布 + 互斥工具模式。 |
| `ViewModels/CadWorkbenchViewModel.cs` | 工作台状态机（就绪/预览/已提交），工具模式：选择、边界修复、偏移、节点编辑、剪断、刀口。 |
| `Views/CadEvidenceCanvas.cs` | 黑色取证对齐的只读投影，展示已确认导入的几何。 |
| `ViewModels/ImportWizardViewModel.cs` | 导入向导会话状态（选文件 → 单位复核 → 识别 → 修复决策 → 提交）。 |
| `ViewModels/ProjectWorkflowViewModel.cs` | 协调 Stage 1 项目/导入工作流，不把业务规则写进视图。 |

---

## `Workspace/` — 跨模块工作区

| 文件 | 职责 |
| --- | --- |
| `IWorkspaceCommands.cs` | 跨模块工作区意图接口。 |
| `IWorkspaceSession.cs` | 当前工作区状态的只读订阅点。 |
| `InMemoryWorkspaceSession.cs` | 内存实现（同时实现 Commands + Session）。 |
| `WorkspaceSnapshot.cs` / `WorkspaceProjectSummary.cs` | 工作区快照与项目摘要。 |

---

## `DesignSystem/` — 设计系统

| 文件 | 职责 |
| --- | --- |
| `AppTheme.cs` | 固定经典工作站配色（截图取证用）。 |
| `ClassicPaneHost.cs` | 稳定的紧凑面板宿主。 |
| `ToolbarIcon.cs` | 自包含矢量图标（不依赖字体/位图）。 |
| `TodoBadge.cs` | 文本 TODO 标记（占位未接逻辑）。 |
| `CadTools/` | CAD 工具栏图标的矢量绘制（A–E 组）。 |

## `Demo/`、`Adapters/`

| 目录 | 职责 |
| --- | --- |
| `Demo/DemoScenario.cs` + `DemoScenarioFactory.cs` | 只读演示数据（版本时间线、历史、项目摘要）。 |
| `Adapters/Import/DefaultImportCoordinatorFactory.cs` | 桌面端接线（导入几何读取、工作台工厂）的临时实现。 |

---

## 依赖与构建产物

- **项目引用**：`LeatherNesting.Application`、`LeatherNesting.Infrastructure`。
- **NuGet**：`Avalonia`、`Avalonia.Desktop`、`Avalonia.Themes.Fluent`。
- `bin/`、`obj/`：构建产物与中间文件，**无需关注**。
