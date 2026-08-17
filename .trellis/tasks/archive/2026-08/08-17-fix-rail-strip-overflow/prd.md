# 修复折叠细条挤占顶栏/状态栏布局

## Goal

修复 `9ca5edd`（左缘细条一键折叠/展开左侧栏）引入的窗口布局回归：外层 Grid 从单列 `*` 改为 `Auto,*` 两列后，`TopCommands`（顶栏）与 `StatusBar`（状态栏）未显式设置跨列，默认落入第 0 列 Auto 细条，被压缩成 14px 细长条；同时顶栏的期望宽度把 Auto 列撑满，`*` 列（bodyLayer / 中央画布）几乎没有剩余空间，窗口显示异常。

## Background and Evidence

- 折叠改动提交：`9ca5edd`，`src/LeatherNesting.Desktop/Shell/AppShellView.cs` 的 `BuildLayout()`。
- 外层 Grid 改为 `ColumnDefinitions.Parse("Auto,*")`，行 `Auto,*,Auto`。
- `TopCommands` / `StatusBar` 添加时未调用 `Grid.SetColumnSpan`，默认 `Column=0, ColumnSpan=1`，与折叠细条（`LeftRailStrip`，`Width=14`）同列。
- 用户 2026-08-17 实测：窗口异常，其他视图（中央画布等）没有位置。

## Requirements

### R1 Column span fix

- `TopCommands` 与 `StatusBar` 必须横跨外层 Grid 两列：`Grid.SetColumnSpan(..., 2)`。
- `LeftRailStrip` 仍居第 0 列第 1 行，`bodyLayer` 仍居第 1 列第 1 行，几何不变。
- 折叠/展开行为（`ToggleLeftRail`）不受影响：折叠后顶栏与状态栏仍占满窗口宽度，只有左侧栏列宽清零。

### R2 Tests

- 更新或新增测试，断言外层 Grid 中 `TopCommands` 与 `StatusBar` 的 `ColumnSpan == 2`，防止回归。
- 现有测试（FRAME-006/007、TOP-005 等）必须保持通过。

## Acceptance Criteria

- 运行后窗口布局恢复正常：顶栏、状态栏横跨整个窗口宽度，中央画布占据剩余空间。
- 折叠左侧栏后，中央画布变宽，顶栏/状态栏不变形。
- 全解决方案测试通过。
