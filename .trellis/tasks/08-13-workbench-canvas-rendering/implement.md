# 画布渲染 — 实施计划

> 状态：规划中，等待最终审阅后 `task.py start`。

## 0. 验证命令

```bash
export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"
dotnet build LeatherNesting.sln -c Release
dotnet test LeatherNesting.sln -c Release
```

期望：0 警告 0 错误；现有 77 个测试不回归。

## 1. 实施清单（有序）

1. **新建 `CanvasView`**（`src/LeatherNesting.Desktop/Views/CanvasView.cs`）：`Control` 重写 `Render`，实现毫米→像素坐标变换 + fit-to-view + 轮廓/孔描边 + 节点点。
   - 验收：加载已知矩形能在 headless 下渲染（无异常）、坐标变换单测正确（Y 翻转、等比缩放）。
2. **曲线展平工具**：`LineSegment2D`/`Polyline2D`/`CircularArc2D` → 线段列表（可复用到渲染与将来导出）。
   - 验收：单测覆盖三类曲线的展平顶点数/首尾点。
3. **接入 `CadWorkbenchView`**：替换占位 `Border` 为 `CanvasView`，在工具操作后 `SetData(vm.CurrentLoops)`。
   - 验收：手动（`!` 启动）导入 DXF 进入工作台能看到轮廓；headless 下 `CadWorkbenchViewModelTests` 不回归。
4. **缩放平移**：滚轮缩放（光标为中心）+ 拖拽平移；数据变化 fit-to-view。
   - 验收：缩放平移不改变几何数据，只改视口。
5. **坐标变换 golden 测试**：用 `fixtures/golden/cad-repair/rectangle.dxf` 做「已知点→像素」断言，锁死 Y 翻转与比例。
6. **收尾**：全解 build + test，`git diff --check`。

## 2. 高风险文件 / 回退点

- 新增 `CanvasView.cs`（纯新增，不影响既有代码）。
- 改 `CadWorkbenchView.cs` 只替换画布 `Border`，不动工具栏/状态栏/命令逻辑。
- 坐标变换错误只影响显示，不影响几何数据（回退 = 撤销渲染改动）。

## 3. 完成后检查

- [ ] 全解 build 0 警告 0 错误、test 全过。
- [ ] 工作台画布显示轮廓，非占位。
- [ ] 旋转角度 + 完整交互的后续已记录到 `docs/todo/02-画布渲染旋转交互待办.md`。
