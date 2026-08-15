# Implement: CAD 坐标轴固定在模型原点

## Order

1. **自绘轴控件**：新增 `src/LeatherNesting.Desktop/Views/CadOriginAxes.cs`。
   - 构造：`CadOriginAxes(CanvasView source)`，订阅 `source.ViewChanged += (_,_) => InvalidateVisual()`。
   - 公开只读：`OriginPixel`（`Point`，由 `ViewScale`/`ViewOriginModel` 换算）便于测试。
   - `Render`：原点出屏 → 透明；否则 X 轴右向水平线 + `→` 箭头 + `+X`，Y 轴垂直线 + `↑` 箭头 + `+Y`，色 `AppTheme.MaterialBoundary`。
   - 标注文字用 `FormattedText`（仿 `CadRuler.DrawText`），`IsHitTestVisible=false` 由宿主设置。
2. **替换静态轴**：`CadWorkspaceHost.cs`：
   - `BuildAxes()` 改为非 static，返回 `CadOriginAxes(_state-持有-Drawing)`；`Canvas` Grid 层序 `{ Drawing, axes, _coordinates, _status }`，轴 `IsHitTestVisible=false`。
   - 移除原 `TextBlock` 版本。
3. **测试**：新增 `tests/LeatherNesting.Desktop.Tests/Shell/CadOriginAxesTests.cs`。

## Test coverage（CadOriginAxesTests.cs）

- `AXIS-001` 原点像素：构造 `CanvasView` + `CadOriginAxes`，断言 `OriginPixel` 与 `ViewOriginModel`/`ViewScale` 换算一致（`(-origin.X*scale, origin.Y*scale)`）。
- `AXIS-002` 随视图更新：`SetData(refit)` / 滚轮后 `ViewChanged` 触发且 `OriginPixel` 变化。
- `AXIS-003` 非静态 TextBlock：`CadWorkspaceHost` 的轴为 `CadOriginAxes` 实例，色语义 `MaterialBoundary`；`Canvas` Grid 中不含原 `+X\n│\n└── +Y` TextBlock。
- `AXIS-004` 出屏隐藏：手动设视图使原点像素为负，`CadOriginAxes` 暴露 `IsOriginVisible`（或等价只读），断言隐藏分支。

## Validation commands

- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug --filter "FullyQualifiedName~CadOriginAxes"`
- `dotnet test tests/LeatherNesting.Desktop.Tests -c Debug`（全量回归，重点 RUL/CAD-HOST/FRAME）

## Risky files / rollback points

- `CadWorkspaceHost.cs`：`BuildAxes` 由 static 改实例；若层序/引用出错，回滚 = 恢复原 TextBlock 版本。
- `CadOriginAxes.cs`：新增文件，无既有引用依赖。
- `CanvasView` 不改动（复用已暴露的 `ViewScale`/`ViewOriginModel`/`ViewChanged`）。

## Follow-up checks before task.py start

- [ ] `prd.md` 已过收敛检查，无未决阻断问题。
- [ ] 用户已批准本实现计划。
- [ ] 验证命令全绿后再 `task.py start`。
