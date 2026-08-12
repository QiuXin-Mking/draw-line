# 2D CAD 视口与几何交互 —— 背景知识

> 写于 2026-08-08，为 Windows 皮革排样软件的技术选型提供背景知识。

## 你需要的是"视口"而不是"CAD 引擎"

听到"CAD 软件"，很容易想到 AutoCAD——几千万行代码、DWG 解析器、约束求解器、三维引擎。

但一个排样软件的需要的东西少得多：**在屏幕上显示裁片，让用户用鼠标拖拽旋转**。这只需要一个 **2D 视口（Viewport）** + **几何交互层**，几百行代码就能搞定。

---

## 1. 核心概念：两个坐标系

2D 图形交互的全部秘密在于**两个坐标系之间的转换**：

```
世界坐标 (World Space)              屏幕坐标 (Screen Space)
─────────────────────              ─────────────────────
  单位：毫米 (mm)                     单位：像素 (px)
  描述裁片的真实物理尺寸              描述在显示器上的位置
  (500, 300) = 距原点 500mm           (200, 150) = 距窗口左上角
                300mm                            200px, 150px
```

### 为什么需要两个坐标系？

- **世界坐标**是数据的"真相"——裁片在现实中多大就是多大，和屏幕分辨率无关
- **屏幕坐标**是显示的"投影"——取决于你缩放到多大、平移到哪个位置

这就像地图和屏幕的关系：地球上的经纬度不会变（世界坐标），但手机屏幕上显示的位置会随你的缩放和拖动而变化（屏幕坐标）。

---

## 2. 坐标变换

### 2.1 变换公式

两者之间的转换通过一个 **2D 仿射变换**（平移 + 缩放）实现：

```
屏幕 → 世界：
  worldX = (screenX - offsetX) / scale
  worldY = (screenY - offsetY) / scale

世界 → 屏幕：
  screenX = worldX × scale + offsetX
  screenY = worldY × scale + offsetY
```

只有两个参数：
- `scale`：缩放比例（1.0 = 1mm 对应 1px）
- `(offsetX, offsetY)`：世界原点在屏幕上的偏移量

### 2.2 注意 Y 轴方向

数学坐标系 Y 轴向上，屏幕坐标系 Y 轴向下。大多数 2D 图形库（Skia、GDI+、Canvas）默认 Y 轴向下，所以通常不需要翻转——直接让世界坐标也 Y 向下即可。但如果你的数据 Y 向上（常见于某些 CAD 导出），需要额外处理：

```
screenY = canvasHeight - (worldY × scale + offsetY)
```

### 2.3 变换矩阵表示

用 3×3 齐次矩阵统一表示（实际代码里不一定要用矩阵类，直接运算更快）：

```
[ scale      0      offsetX ]   [ worldX ]   [ screenX ]
[   0      scale    offsetY ] × [ worldY ] = [ screenY ]
[   0        0         1    ]   [   1    ]   [    1    ]
```

Avalonia 自带 `Matrix` 结构体可以做这些运算，SkiaSharp 有 `SKMatrix`。

---

## 3. 视口状态

视口只需要维护这几个变量：

```csharp
public class Viewport
{
    public double Scale { get; set; } = 1.0;     // 缩放
    public double OffsetX { get; set; } = 0;      // 平移
    public double OffsetY { get; set; } = 0;
    public double CanvasWidth { get; set; }       // 画布尺寸（像素）
    public double CanvasHeight { get; set; }
}
```

### 常用操作

| 操作 | 实现 |
|------|------|
| 滚轮缩放 | `scale *= 1.1` 或 `scale /= 1.1` |
| 以鼠标为中心缩放 | 缩放后调整 offset，使鼠标下的世界坐标不动 |
| 中键拖拽平移 | 修改 `offsetX`, `offsetY` |
| 自适应（Fit All） | 自动算 scale 和 offset 使所有内容可见 |
| 框选放大（Zoom to Rect） | 调整视口使指定矩形区域填满窗口 |

### 以鼠标为中心缩放（关键体验细节）

这是好 CAD 软件和烂 CAD 软件的分水岭。如果只是简单乘以缩放系数，鼠标下的内容会"漂走"：

```csharp
void ZoomAtPoint(double mouseScreenX, double mouseScreenY, double factor)
{
    // 缩放前，鼠标指向的世界坐标
    var worldX = (mouseScreenX - OffsetX) / Scale;
    var worldY = (mouseScreenY - OffsetY) / Scale;

    // 执行缩放
    Scale *= factor;

    // 调整偏移，使同一个世界坐标仍然在鼠标位置下
    OffsetX = mouseScreenX - worldX * Scale;
    OffsetY = mouseScreenY - worldY * Scale;

    Invalidate(); // 触发重绘
}
```

用公式表达就是：保持 `ScreenToWorld(mousePos) = mouseWorld` 这个等式在缩放前后都成立。

---

## 4. 渲染管线

### 4.1 渲染流程

每一帧的渲染步骤：

```
1. 拿到所有几何体的世界坐标数据
2. 对每个顶点：世界 → 屏幕变换
3. 用图形库画多边形（填充 + 描边）
4. 画辅助元素（选中高亮、旋转手柄、网格、尺寸标注）
```

### 4.2 裁片渲染

每个裁片是一个多边形（`List<Point>`，世界坐标 mm）。渲染伪代码：

```
function RenderPiece(piece):
    screenPath = []
    for each vertex in piece.Vertices:
        sx = vertex.X * scale + offsetX
        sy = vertex.Y * scale + offsetY
        screenPath.Add(sx, sy)

    canvas.FillPath(screenPath, fillBrush)    // 半透明填充
    canvas.DrawPath(screenPath, strokePen)    // 轮廓线
    canvas.DrawText(piece.Name, piece.Center) // 标签
```

### 4.3 渲染性能考虑

- 50 个以下裁片：逐多边形绘制，SkiaSharp 轻松胜任
- 50-200 个裁片：仍可直接绘制，考虑脏矩形优化
- 200+ 个裁片：考虑使用 `SKPicture` 缓存不变的裁片

### 4.4 分层渲染

```
层级（从底到顶）：
  1. 背景网格（淡灰小方格，像图纸）
  2. 皮革底板矩形（填充色）
  3. 已放置的裁片（填充 + 描边）
  4. 未放置的裁片（不同颜色）
  5. 选中裁片高亮（加粗描边 + 旋转手柄）
  6. 正在拖拽的裁片（半透明 + 碰撞预览）
  7. 标尺 / 信息栏
```

### 4.5 抗锯齿

SkiaSharp 默认抗锯齿。让裁片边缘看起来平滑，不需要额外工作。

---

## 5. 鼠标交互

### 5.1 交互生命周期

```
MouseDown  → HitTest → 找到目标 → 记住初始状态
MouseMove  → 计算 Delta → 更新目标 → 重绘
MouseUp    → 固定最终状态 → 清理拖拽状态
```

### 5.2 命中检测（Hit Test）

**目标**：给定鼠标的屏幕坐标，判断它点击了哪个裁片。

**步骤**：

```
1. 屏幕坐标 → 世界坐标（逆变换）
2. 对每个裁片：判断世界坐标是否在多边形内部
3. 如果有多个命中，选最上层（最后绘制）的那个
4. 如果没命中任何裁片 → 可能是点击空白区域（开始框选或取消选择）
```

**点包容测试 — 射线法（Ray Casting Algorithm）**：

从被测点向右发出一条水平射线，数它与多边形边相交的次数。奇数次 = 内部，偶数次 = 外部。

```csharp
bool IsPointInPolygon(double px, double py, List<Point> polygon)
{
    bool inside = false;
    int n = polygon.Count;
    for (int i = 0, j = n - 1; i < n; j = i++)
    {
        double xi = polygon[i].X, yi = polygon[i].Y;
        double xj = polygon[j].X, yj = polygon[j].Y;

        // 判断水平射线是否穿过这条边
        if ((yi > py) != (yj > py) &&
            px < (xj - xi) * (py - yi) / (yj - yi) + xi)
        {
            inside = !inside;
        }
    }
    return inside;
}
```

时间复杂度 O(n)，对鞋面裁片（通常 10-100 个顶点）完全够用。

**优化**：先用包围盒（Bounding Box）做粗略筛选，通过后再做精确射线检测。

### 5.3 拖拽移动

```
MouseDown → 命中裁片A → 记录：
  - dragTarget = A
  - lastWorldPos = ScreenToWorld(mousePos)
  - 记录 A 的初始顶点位置（用于撤销）

MouseMove → 
  currentWorld = ScreenToWorld(mousePos)
  delta = currentWorld - lastWorldPos
  A.Translate(delta.X, delta.Y)     // 平移所有顶点
  lastWorldPos = currentWorld
  InvalidateVisual()                // 重绘

MouseUp →
  dragTarget = null
  // （可选）触发碰撞检测或自动吸附
```

### 5.4 旋转

通常通过**旋转手柄（Rotation Handle）**交互——裁片被选中时，角落显示一个小圆点：

```
拖拽旋转手柄 →
  1. 计算：angle = atan2(mouseY - centerY, mouseX - centerX)
  2. 计算：deltaAngle = angle - lastAngle
  3. 绕裁片中心旋转所有顶点 deltaAngle 弧度
  4. 更新 lastAngle
```

旋转公式（绕中心点）：

```
newX = centerX + (oldX - centerX) * cos(θ) - (oldY - centerY) * sin(θ)
newY = centerY + (oldX - centerX) * sin(θ) + (oldY - centerY) * cos(θ)
```

### 5.5 吸附（Snapping）

排样软件中拖拽裁片时，可以自动吸附到：
- 皮革边缘
- 相邻裁片边缘（保持间隙）
- 网格对齐
- 角度吸附（0°/90°/180° 附近吸附到精确角度）

实现方式：在 `MouseMove` 中，计算出目标位置后，检查是否在吸附距离内（如 5mm），如果在就修正到吸附目标位置。

---

## 6. 平移和缩放

### 6.1 滚轮缩放

```csharp
void OnMouseWheel(MouseWheelEventArgs e)
{
    double factor = e.Delta.Y > 0 ? 1.1 : 1/1.1;
    ZoomAtPoint(e.GetPosition(canvas), factor);
}
```

### 6.2 中键拖拽平移（Pan）

```csharp
void OnMiddleMouseDown(e)  → isPanning = true, 记住 panStart
void OnMiddleMouseMove(e)  → Offset += mouseDelta, Invalidate
void OnMiddleMouseUp(e)    → isPanning = false
```

### 6.3 自适应（Fit All）

```csharp
void FitAll(List<Piece> pieces, Rect leatherBounds)
{
    // 1. 计算所有几何体的包围盒（世界坐标）
    var bounds = ComputeBoundingBox(pieces, leatherBounds);

    // 2. 算缩放：取宽高比中较小的
    double scaleX = CanvasWidth  / bounds.Width  * 0.9;  // 留 10% 边距
    double scaleY = CanvasHeight / bounds.Height * 0.9;
    Scale = Math.Min(scaleX, scaleY);

    // 3. 居中
    OffsetX = (CanvasWidth  - bounds.Width  * Scale) / 2 - bounds.Left * Scale;
    OffsetY = (CanvasHeight - bounds.Height * Scale) / 2 - bounds.Top  * Scale;
}
```

### 6.4 右键拖拽平移（Windows CAD 软件惯例）

许多 Windows CAD 软件支持右键拖拽来平移，我们也可以支持：

```
右键按下 + 拖动 → 平移视口
```

这样可以单手操作（不需要按中键）。

---

## 7. 撤销/重做（Undo/Redo）

CAD 交互中撤销很重要。用 **Command Pattern**：

```csharp
interface ICommand
{
    void Execute();
    void Undo();
}

class MoveCommand : ICommand
{
    Piece Target;
    double DeltaX, DeltaY;

    void Execute() => Target.Translate(+DeltaX, +DeltaY);
    void Undo()    => Target.Translate(-DeltaX, -DeltaY);
}

class RotateCommand : ICommand
{
    Piece Target;
    double CenterX, CenterY, Angle;

    void Execute() => Target.Rotate(CenterX, CenterY, +Angle);
    void Undo()    => Target.Rotate(CenterX, CenterY, -Angle);
}
```

维护两个栈：`undoStack` 和 `redoStack`，Ctrl+Z / Ctrl+Y 触发。

---

## 8. 网格背景

许多 CAD 软件的背景是小方格，帮助视觉判断尺寸和位置：

```
世界坐标中网格间距 = 固定值（如 10mm）
屏幕中网格间距 = 10 * scale（像素）
当间距 < 5px 时 → 隐藏细格，只画粗格（如 100mm）
当间距 > 50px 时 → 可以标注数字

有 效策略：根据缩放级别自适应切换网格层级
```

没必要自己一行行画线——SkiaSharp 的 `SKShader` 可以高效绘制。

---

## 9. 与 AutoCAD 的关系

### 我们不需要什么

| 不需要 | 原因 |
|--------|------|
| DWG 格式解析 | 我们用 DXF（文本格式，已有 netDxf 库） |
| 图层管理系统 | 排样软件就一层 |
| 约束求解器 | 裁片不需要几何约束 |
| 块/参照/属性 | 不需要 |
| 三维引擎 | 纯 2D |
| 命令行界面 | GUI 操作即可 |
| 尺寸标注引擎 | 对排样而言非核心 |
| 打印布局 | 导出 DXF 就行 |

### 我们需要什么

| 需要 | 复杂度 |
|------|--------|
| World ↔ Screen 坐标变换 | 简单（约 30 行） |
| 多边形命中检测 | 简单（约 20 行） |
| 拖拽移动 | 简单（约 50 行） |
| 旋转手柄 | 中等（约 80 行） |
| 平移 + 缩放 | 简单（约 60 行） |
| 撤销/重做 | 中等（约 100 行） |
| SkiaSharp 渲染 | 中等（约 150 行） |
| **总计** | **约 500 行** |

---

## 10. 延伸阅读

- **SkiaSharp 文档**：https://learn.microsoft.com/en-us/dotnet/api/skiasharp
- **射线法点包容**：https://en.wikipedia.org/wiki/Point_in_polygon
- **Avalonia 自定义渲染**：`DrawingContext` + `CustomControl`
- **NFP（No Fit Polygon）**：排样优化的核心算法，见 `docs/research/2026-07-27-leather-nesting-market-and-algorithm.md`
