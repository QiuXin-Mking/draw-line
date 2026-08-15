# Design: CAD 交互功能（动态标尺 + 坐标提示 + 缩放联动）

## Architecture

复用既有「CanvasView 为唯一视图状态所有者」的模式：让 `CanvasView` 暴露视图状态并发出变更通知，标尺与坐标提示订阅该通知，不再让 `AppShellView` 维护死文字标尺。

```
CanvasView（唯一视图状态：_scale/_offset；已处理滚轮缩放/平移）
   │ 公开：ViewScale, ViewOrigin(ModelAtPixel0 或 PixelToModel)、ViewChanged 事件
   │ 以及现有 ToModel(Point)
   ▼
ViewObserver（新增）：订阅 ViewChanged + 用 Render 重绘刻度
   ├─ CadRuler（垂直/水平自绘 Control，黑底灰刻度，随视图更新）
   └─ 坐标提示（CadWorkspaceHost 内 TextBlock overlay，PointerMoved → ToModel）
```

## Files

| 文件 | 变更 |
| --- | --- |
| `src/LeatherNesting.Desktop/Views/CanvasView.cs` | 暴露 `ViewScale`、`ViewOffsetYOrigin`（或等价），新增 `ViewChanged` 事件；在缩放/平移/FitToView/SetData 后触发；不改变交互行为 |
| `src/LeatherNesting.Desktop/Shell/AppShellView.cs` | 用自绘标尺替换 `BuildVerticalRuler`/`BuildHorizontalRuler`（`AppShellView.cs:235-260`）；把 `CadWorkspaceHost.Drawing` 的视图状态接给标尺 |
| `src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs` | 画布左上角新增红色坐标 TextBlock overlay；`Drawing.PointerMoved` → `ToModel` → 更新文本；离开画布清空 |
| `tests/LeatherNesting.Desktop.Tests/Shell/CadInteractionRulerTests.cs` | 新测试（CTX-.. / RUL-..） |

## CanvasView view state contract

当前私有字段：`_scale`（px/mm，默认 10）、`_offset`（像素偏移，Y 下正）。要支持标尺在画布像素坐标里标注模型坐标：

- `public double ViewScale => _scale;`
- `public Point2D ViewOriginModel => new(-_offset.X / _scale, _offset.Y / _scale);` —— 画布像素 (0,0) 对应的模型坐标（Y-up 镜像注意）。
- `public event EventHandler? ViewChanged;` —— 在 `OnPointerWheelChanged`、`OnPointerMoved`(平移)、`FitToView`、`SetData`、`ArrangeOverride`（尺寸变化触发 FitToView）末尾触发。

标尺绘制：垂直标尺（宽 22）取画布当前可视模型 Y 范围；水平标尺（高 20）取可视 X 范围；刻度选择走「取整毫米 → 稀疏化到可读间隔」的自适应逻辑。不需要向 CanvasView 回写，只读即可。

## Ruler control design

新增自绘 `Control`（如 `CadRuler`，方向枚举 Horizontal/Vertical）：
- 订阅 `CanvasView.ViewChanged`，重绘 `InvalidateVisual`。
- `Render(DrawingContext)`：背景 `AppTheme.RulerChrome`；按 `ViewScale` 与 `ViewOriginModel` 计算每个像素对应的模型坐标，画出刻度线 + 数字标签；文字用 `AppTheme.RulerTick`。
- 刻度自适应：计算当前每 px 对应 mm，选取「目标刻度间距」为 1/5/10/50/100/500/1000 等整步长，使屏幕上刻度间隔 ≥ ~60px，可读不拥挤。

## Coordinate overlay

- `CadWorkspaceHost` 内画布 `Canvas` Border 的 Grid 顶层叠加一个 `TextBlock`（红色，左上角，`IsHitTestVisible=false`）。
- `Drawing.PointerMoved +=` → `Drawing.ToModel(e.GetPosition(Drawing))` → `X {x:F2} mm · Y {y:F2} mm`。
- `PointerExited`（或 `PointerLeave`）→ 文本清空，避免陈旧坐标残留。

## Compatibility & Rollback

- `ShellFrameTests` FRAME-003 断言标尺为 `Border` 且尺寸 22/20、行列位置。若改用自绘 `Control`，需同步更新该断言（仍暴露 `VerticalRuler`/`HorizontalRuler` 属性，类型改为自绘控件但保留 Width/Height 与 Grid 位置语义）。`CloneSurfaceColorTests:38-39` 断言 `RulerChrome` 背景——自绘控件在 Render 里填 `RulerChrome` 仍满足；若断言读 `Background` 属性需同步调整。
- 回滚 = 撤销对 4 个文件的改动；`CanvasView` 新增成员为增量，不影响既有调用方。

## Verification commands

- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug --filter "FullyQualifiedName~CadInteractionRuler"`
- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug`（全量回归，重点 ShellFrameTests/CloneSurfaceColorTests/CadHostEvidenceTests）
