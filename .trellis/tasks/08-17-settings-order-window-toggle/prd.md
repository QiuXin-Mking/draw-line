# 设置>订单窗口 切换左侧栏显隐（取代◀细条）

## Goal

按用户澄清的需求（2026-08-17）：**不要左缘 `◀` 折叠细条**。改为在菜单栏「设置」下拉的「订单窗口」可选项上，勾选/取消来控制左侧栏（订单组/裁片列表/进度汇总）的显示与隐藏。取消勾选后左侧栏缩回左缘、中央画布变宽；再勾选恢复。

## Background and Evidence

- `9ca5edd` 实现了左缘 `◀` 细条折叠，用户实测后确认需求不符：不是要细条，而是「设置 > 订单窗口」菜单开关。
- `bc63e50` 修复了细条把顶栏/状态栏挤到 Auto 列的问题；本次移除细条后外层 Grid 还原为单列，该修复相关断言（FRAME-008、TOP-005 的列数）需同步调整。
- 「设置」菜单已有「订单窗口」项（`ShellTopCommands.cs:94`），当前行为是导航到 M01，需改为勾选开关。
- Avalonia 12.1.0 的 `MenuItem` 支持 `ToggleType = ToggleType.CheckBox` 与 `IsChecked`，可用于勾选式菜单项。
- 命令路由现状：`TopCommandArea` 点击 → `AppShellView.BuildTopBar` 回调 → `AppShellViewModel.ActivateMenuCommand` → 导航/占位。既有 `BoardSettingsRequested` 事件模式（ViewModel 抛事件、View 订阅）可复用。

## Requirements

### R1 移除左缘 ◀ 细条

- 删除 `BuildLeftStrip()`、`LeftRailStrip`、`LeftRailToggle`、`_leftRailGlyph` 及其公开属性。
- 外层布局 Grid 从 `Auto,*` 还原为单列 `*`（行 `Auto,*,Auto` 不变）。
- `ToggleLeftRail()` 保留（继续清零/恢复 BodyGrid 第 0 列宽），但不再更新 glyph，改由菜单命令触发。

### R2 「设置 > 订单窗口」可勾选菜单开关

- `ShellTopCommands.SettingsMenu` 中「订单窗口」命令从「导航 M01」改为开关：`NavigateToModule: false`、`Launch: ShellCommandLaunch.ToggleOrderWindow`、`IsPlaceholderAction: false`。
- `ShellCommandLaunch` 枚举新增 `ToggleOrderWindow`。
- `TopCommandArea` 对该命令构建 `ToggleType = CheckBox` 的 `MenuItem`，`IsChecked = true`（左侧栏默认显示），并暴露该项供 shell 同步勾选状态。
- `AppShellViewModel.ActivateMenuCommand` 对 `ToggleOrderWindow` 抛 `OrderWindowToggleRequested` 事件（不导航、不显示 TODO）。
- `AppShellView` 订阅该事件 → 调用 `ToggleLeftRail()`，并把 `TopCommands` 中「订单窗口」菜单项的 `IsChecked` 同步为左侧栏可见状态。

### R3 测试与防回归

- 更新 TOP-005：外层 Grid 断言从「2 列 Auto,Star」改为单列 Star。
- 重写 FRAME-006/007/008：改为断言「订单窗口」菜单项 checkable、初始勾选、`ToggleLeftRail` 折叠/恢复时勾选状态与左侧栏一致；移除对细条/ glyph 的断言。
- 新增测试：激活「订单窗口」命令触发 `OrderWindowToggleRequested`、不导航、不写 TODO。
- 全解决方案测试通过。

## Acceptance Criteria

- 运行后菜单栏「设置 > 订单窗口」为可勾选项，默认勾选，左侧栏显示。
- 取消勾选 → 左侧栏（订单组/裁片列表/进度汇总）缩回左缘，中央画布变宽，菜单项取消勾选。
- 再次勾选 → 左侧栏恢复，菜单项勾选。
- 无 `◀` 细条残留；顶栏/状态栏横跨整窗、不变形。
- 全解决方案测试通过。
