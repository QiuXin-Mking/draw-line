# CAD 内核 + 交互式工艺工作台 — 实施计划

> 状态：规划中，等待最终审阅后 `task.py start`。

## 0. 验证命令

```bash
export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"
dotnet build LeatherNesting.sln -c Release
dotnet test LeatherNesting.sln -c Release
```

期望：0 警告 0 错误；现有 77 个测试不回归，新增测试全过。

## 1. 实施清单（有序）

1. **R1 圆弧面积**：`Loop2D.ComputeSignedArea` 对 `CircularArc2D` 精确计算；`FaceCandidate.ComputeArea` 同步。新增圆弧面积单测。
2. **R2 曲线求交**：新增 `Geometry/Intersection/`（线-弧、弧-弧）；接入 `FaceCandidate`/`PlacementValidator` 自交检测。新增求交单测。
3. **R5 裁片变换**：`Transform2D.Apply(Point2D)` + `Apply(Loop2D)`；新增 `TransformCommand`。新增变换单测（矩形移动/旋转/镜像，圆弧旋转）。
4. **R3 参数输入**：`CadWorkbenchViewModel` 增加参数状态与 `SelectPiece/MoveSelected/RotateSelected`；`CadWorkbenchView` 增加工具参数面板。
5. **R4/R6 画布交互**：`CanvasView` 增加 `ToModel`、点选/框选、拖动、旋转手柄、选中高亮。
6. **收尾**：全解 build + test，`git diff --check`，手动（`!` 启动）验证「导入凉鞋.dxf → 选择/移动/旋转/平移 + 修复工具」。

## 2. 高风险文件 / 回退点

- `Transform2D.cs`（加 Apply）——纯新增，回退 = 删方法。
- `Loop2D.cs`（圆弧面积）——只改 `ComputeSignedArea`，`Area` 语义不变（仍为绝对值）。
- `CanvasView.cs`（加交互）——选择/拖拽只改视口与选中态，不碰几何；几何与命令层可独立回退。
- `CadWorkbenchView.cs`（加参数面板）——回退 = 删面板块。

## 3. 完成后检查

- [ ] 全解 build 0 警告 0 错误、test 全过。
- [ ] 含圆弧轮廓面积精确、线-弧/弧-弧求交有单测。
- [ ] 裁片变换（移动/旋转/镜像）几何正确、可撤销。
- [ ] 工作台参数输入 + 画布点选/拖拽/旋转可用（手动验证）。
