# 左侧栏折叠细条 —— 实施清单

## 前置检查（`task.py start` 前）

- [ ] 无阻塞开放问题；prd.md 已收敛；用户已确认最终规划摘要。

## 实施顺序

1. **改 `src/LeatherNesting.Desktop/Shell/AppShellView.cs`**
   - 加字段：`_leftRailToggle`、`_leftRailGlyph`、`_leftRailCollapsed`。
   - `BuildBody()`：暴露 `LeftRailColumn => body.ColumnDefinitions[0]`（三列结构不变）。
   - `BuildLayout()`：外层 Grid 改 `Columns="Auto,*"`；把细条（新方法 `BuildLeftStrip()`）放列 0/行 1，`bodyLayer` 放列 1。
   - `BuildLeftStrip()`：宽 14 的 `Border` + 内层透明 `Button`，字形 `◀`，Click → `ToggleLeftRail()`。
   - `ToggleLeftRail()`：切换 `_leftRailCollapsed`、`LeftRail.IsVisible`、`LeftRailColumn.Width`、字形。
   - 公开成员：`Button LeftRailToggle`、`bool IsLeftRailCollapsed`、`ColumnDefinition LeftRailColumn`、`void ToggleLeftRail()`。
   - 视觉复用 `AppTheme.HeaderSurface` / `ClassicBorderNeutral` / `PrimaryText`。

2. **新增测试** `tests/LeatherNesting.Desktop.Tests/Shell/`（加在 `ShellFrameTests.cs` 或独立 `LeftRailCollapseTests.cs`，挂 `[Collection("Avalonia UI")]`）
   - 初始展开态：`IsLeftRailCollapsed == false`、`LeftRail.IsVisible == true`、`LeftRailColumn.Width` 为 `13*`、字形 `◀`、细条在 Grid 列 0/行 1。
   - 折叠一次：`IsLeftRailCollapsed == true`、`LeftRail.IsVisible == false`、列宽 `0`、字形 `▶`。
   - 再触发恢复：全部回到初始值（含 `13*`、可见、`◀`）。
   - 断言 BodyGrid 三列仍为 `13*,74*,13*`（回归 FRAME-001 结构不变）。

3. **验证**
   - 构建：`dotnet build LeatherNesting.sln`
   - 测试：`dotnet test tests/LeatherNesting.Desktop.Tests --filter "FullyQualifiedName~Shell"`（先 shell），再跑全量。

## 风险文件 / 回滚点

- `src/LeatherNesting.Desktop/Shell/AppShellView.cs`（唯一生产改动文件）。
- 若 FRAME-001 意外变红：还原 BodyGrid 相关改动，改用显式列宽方案排查（设计已规避）。

## `task.py start` 前复核

- [ ] prd/design/implement 三件套齐备；测试命令已列明。
