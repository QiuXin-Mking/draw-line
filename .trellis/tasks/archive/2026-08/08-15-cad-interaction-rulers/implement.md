# Implement: CAD 交互功能（动态标尺 + 坐标提示 + 缩放联动）

## Order

1. **CanvasView 视图状态**：`src/LeatherNesting.Desktop/Views/CanvasView.cs` 新增 `ViewScale`、`ViewOriginModel`、`ViewChanged` 事件；在缩放/平移/FitToView/SetData/ArrangeOverride 末尾触发。不动交互行为。
2. **自绘标尺控件**：新增 `CadRuler`（如放 `src/LeatherNesting.Desktop/Views/CadRuler.cs`），方向 Horizontal/Vertical，订阅 `ViewChanged`，Render 绘制 `RulerChrome` 背景 + `RulerTick` 刻度/标签，刻度间距自适应。
3. **接入 shell**：`AppShellView.cs:235-260` 用 `CadRuler` 替换静态 `Border`，把 `CadWorkspaceHost.Drawing` 的视图接给标尺（构造时传 `CanvasView` 或让标尺订阅其 `ViewChanged`）。保留 `VerticalRuler`/`HorizontalRuler` 公开属性名。
4. **坐标提示**：`CadWorkspaceHost.cs` 画布 Grid 顶层加红色 `TextBlock`；`Drawing.PointerMoved` → `ToModel` → 更新文本；`PointerExited` 清空。
5. **测试**：新增 `tests/LeatherNesting.Desktop.Tests/Shell/CadInteractionRulerTests.cs`。

## Test coverage（CadInteractionRulerTests.cs）

- `RUL-001` 视图状态：`CanvasView` 暴露 `ViewScale`/`ViewOriginModel`，初始 `ViewScale == 10`；`SetData` 后 refit 触发 `ViewChanged`。
- `RUL-002` 缩放联动：模拟滚轮后 `ViewChanged` 触发且 `ViewScale` 变化；标尺控件收到通知并重绘（断言 `IsMeasureValid`/`InvalidateVisual` 副作用或状态字段）。
- `RUL-003` 标尺非死文字：`AppShellView.VerticalRuler`/`HorizontalRuler` 为自绘 `CadRuler`（非静态 TextBlock 内容），背景语义 `RulerChrome`。
- `RUL-004` 坐标提示：构造 `CadWorkspaceHost` 后左上角坐标 TextBlock 存在、初始为空；触发 `PointerMoved`（或调用内部处理）后文本为 `X … mm · Y … mm`；模拟 PointerExited 后清空。
- `RUL-005` 回归：`ShellFrameTests` FRAME-003 与 `CloneSurfaceColorTests` 标尺断言在新控件下仍通过（必要时同步更新断言至自绘控件语义）。

## Validation commands

- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug --filter "FullyQualifiedName~CadInteractionRuler"`
- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug`（全量回归，重点 ShellFrameTests/CloneSurfaceColorTests/CadHostEvidenceTests）

## Risky files / rollback points

- `CanvasView.cs`：新增为增量成员；若事件通知导致既有测试断言视图不变失败，回滚点 = 移除 `ViewChanged` 触发但保留属性。
- `AppShellView.cs`：标尺替换需同步更新 `ShellFrameTests` 断言；`CadRuler` 无法在无 UI 线程测试里构建时改用 `ItemsSource` 式安全构建（参考上一任务 `ContextMenu.ItemsSource` 教训）。
- 无数据/持久化改动。

## Follow-up checks before task.py start

- [ ] `prd.md` 已过收敛检查，无重复事实、无未决阻断问题。
- [ ] 用户已明确批准本实现计划。
- [ ] 全部验证命令通过后再 `task.py start`。
