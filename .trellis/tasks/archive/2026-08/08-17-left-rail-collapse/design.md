# 左侧栏折叠细条 —— 技术设计

## 架构与边界

- 折叠触发器（细条）是 **shell 级 chrome**：放在外层 Grid（`BuildLayout`）的新列中，不属于 BodyGrid 三列几何。这样 BodyGrid 的 `13*,74*,13*` 结构与 FRAME-001 保持不动。
- 折叠逻辑是 **纯视图状态**，放在 `AppShellView` 内（`_leftRailCollapsed` 字段），与 `OrderCardView._isExpanded` 同一模式，不进 ViewModel、不做持久化。

## 布局数据流

```
外层 Grid（BuildLayout）       改前：Columns="*"
                               改后：Columns="Auto,*"   ← 列 0 细条，列 1 bodyLayer
                                   细条：Grid.SetColumn=0, Grid.SetRow=1（只占 body 行）
                                   bodyLayer：Grid.SetColumn=1
```

```
BodyGrid（BuildBody）          Columns="13*,74*,13*"（不变）
                               LeftRail=列0, center=列1, RightRail=列2（不变）
                               新增公开属性 LeftRailColumn => body.ColumnDefinitions[0]
```

折叠切换（`ToggleLeftRail()`）：
```csharp
_leftRailCollapsed = !_leftRailCollapsed;
LeftRail.IsVisible = !_leftRailCollapsed;
LeftRailColumn.Width = _leftRailCollapsed ? new GridLength(0) : new GridLength(13, GridUnitType.Star);
_leftRailGlyph.Text = _leftRailCollapsed ? "▶" : "◀";
```

- 折叠后左栏列宽 0 → `center` 的 `74*` 吸收释放的空间，中央画布变宽；细条列（Auto）仍占位。
- 显式置宽 0 而非仅靠 `IsVisible`：避免依赖 Avalonia 对「内容全隐藏的 star 列是否收缩为 0」的实现细节，行为确定。

## 新增公开成员（供测试）

- `Button LeftRailToggle`（细条内的点击按钮）
- `bool IsLeftRailCollapsed`
- `ColumnDefinition LeftRailColumn`
- `void ToggleLeftRail()`

## 外观

- 细条：宽 14px，背景 `AppTheme.HeaderSurface`、右缘 `AppTheme.ClassicBorderNeutral` 1px 边，与左栏视觉同源；内部按钮透明背景、文本 `AppTheme.PrimaryText`，字形 `◀`/`▶`，字号约 10。

## 兼容性与测试影响

- BodyGrid 三列、左栏三行、五个 host 位置均不变 → FRAME-001 / FRAME-002 保持绿色。
- 外层 Grid 由单列变双列：无既有测试断言外层 Grid 列数（已核对），新增测试将覆盖。
- 折叠时左栏 `IsVisible=false` 后，OrderGroupHost 等仍在 Grid 中（位置不变），恢复时无需重建内容，实例稳定。

## 关键权衡

- 细条放外层 Grid（列 0）而非并入 BodyGrid：保住「五区域几何」测试、改动面最小；代价是细条在概念上与左栏「贴边但独立」。
- 不做动画：本次只有状态切换，最小满足需求；动画留作后续任务。

## 回滚

- 改动集中在 `src/LeatherNesting.Desktop/Shell/AppShellView.cs` + 新增测试；回滚 = 还原该文件并移除新增测试，不影响其他模块。
