# Design: CAD 坐标轴固定在模型原点

## Architecture

仿 `CadRuler` 范式：新增自绘轴控件 `CadOriginAxes`，订阅 `CanvasView.ViewChanged` 重绘，随视图平移/缩放把轴画在模型原点 (0,0) 的像素投影处。替换 `CadWorkspaceHost.BuildAxes()` 的静态 TextBlock。

```
CanvasView（唯一视图状态；ViewChanged 事件）
   │ 订阅 ViewChanged → InvalidateVisual
   ▼
CadOriginAxes（自绘：十字轴 + 箭头 + +X/+Y 标注）
   │ 挂进 CadWorkspaceHost.Canvas 的 Grid（IsHitTestVisible=false）
```

## Files

| 文件 | 变更 |
| --- | --- |
| `src/LeatherNesting.Desktop/Views/CadOriginAxes.cs` | 新增自绘控件：构造接收 `CanvasView`，订阅 `ViewChanged`，`Render` 绘制 |
| `src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs` | `BuildAxes()` 改为返回 `CadOriginAxes`（挂到 `Canvas` Grid，替换静态 TextBlock）；保留 `+X/+Y` 语义 |
| `tests/LeatherNesting.Desktop.Tests/Shell/CadOriginAxesTests.cs` | 新测试 |

## Pixel math

原点 (0,0) 在像素坐标：`CanvasView.ToModel`（`CanvasView.cs:56`）为
`x=(px-OX)/s, y=(OY-py)/s`。反推 `ToPixel((0,0))`：

- `px = -ViewOriginModel.X * ViewScale`
- `py =  ViewOriginModel.Y * ViewScale`

其中 `ViewOriginModel = new(-OX/s, OY/s)`。自绘控件不调用私有 `ToPixel`，直接用上述公开换算。

## Render behavior

- 计算原点像素 `(ox, oy)`。
- 若 `ox < 0 || oy < 0 || ox > Bounds.Width || oy > Bounds.Height` → 原点出屏，不绘制（整控件透明）。
- X 轴：从原点向右到 `Bounds.Width` 画水平线，端点画 `→` 箭头，`+X` 文字标注（`AppTheme.MaterialBoundary`）。
- Y 轴：从原点向上到 `Bounds.Height` 画垂直线（Y-up），端点画 `↑` 箭头，`+Y` 标注。
- 轴朝可视区延伸：以原点为起点朝向可视方向绘制（X 正向向右、Y 正向向上；若原点在右侧/下方，轴反向）。MVP 按「原点在可视区内时向正向绘制到边缘」实现，出屏隐藏。

## Compatibility & Rollback

- `CadWorkspaceHost.Canvas` 的 Grid 层序：`Drawing, axes, _coordinates, _status` —— 轴在 Drawing 之上、坐标提示之下。
- `BuildAxes` 现为 `static`；改后需实例字段访问 `Drawing`（同 `BuildContextMenu` 模式）。
- 回滚 = 撤销 `CadOriginAxes.cs` 与 `CadWorkspaceHost.cs` 改动；`CanvasView` 无需改动。
- 无数据/持久化影响。

## Verification commands

- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug --filter "FullyQualifiedName~CadOriginAxes"`
- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug`（全量回归）
