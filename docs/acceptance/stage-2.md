# 阶段 2 验收记录

实施范围：CAD 诊断、修复与普通工艺——轮廓闭合/gap 修复/边界生成、offset、节点编辑、剪断、普通剪口，以及可预览/可撤销的工艺工作台与黄金 DXF round-trip。

## 自动化用例状态（2026-08-13 本机 .NET 10，全量 Release）

| 用例 | 自动化资产 | 状态 |
|---|---|---|
| P2-BND-001/002/003 | `TopologyTests`、`RepairTests`（闭合矩形不变 / gap 0.05 桥接 / gap 0.11 拒绝 / T 支路 / bow-tie） | 通过 |
| P2-OFF-001/002/003 | `OffsetTests`（100×50 内缩 98×48 / 反转 winding 等价 / 细颈预警 / 自交阻断） | 通过 |
| P2-NOD-001/002 | `NodeOperationTests`（插入/移动/删除节点、剪断守恒、自交阻断） | 通过 |
| P2-NOT-001/002 | `NotchTests`（剪口校验：零宽/负深/NaN/重叠） | 通过 |
| P2-UND-001 | `CadCommandTests`（undo/redo 恢复业务模型与 feature anchor） | 通过 |
| P2-RT-001 | `DxfRoundTripTests`（矩形往返 / offset 往返 / 黄金文件重读） | 通过 |
| P2-UI-001 | `CadWorkbenchViewModelTests`（工具互斥 / preview-commit-cancel / 撤销恢复） | 通过 |

## 命令与工作台（新增）

- 具体命令：`CloseContourCommand`/`GapRepairCommand`/`BoundaryGenerateCommand`/`OffsetCommand`/`MoveNodeCommand`/`InsertNodeCommand`/`DeleteNodeCommand`/`BreakAtPointCommand`/`RemoveSegmentCommand`（快照式撤销）。
- `CadOperationSession` 修复 preview/commit 双重执行缺陷；`CadCommandTransaction` 新增 `Record`。
- `CadWorkbenchViewModel` 的预览/提交/撤销真正通过 session 改变轮廓。
- 应用内新增「进入工艺工作台」入口，导入 DXF 后读取闭合轮廓进入工作台。
- DXF：`AsciiDxfWriter`（最小 LWPOLYLINE）与 `AsciiDxfGeometryReader`（几何提取），黄金文件 `fixtures/golden/cad-repair/rectangle.dxf`。

## 验证命令

```bash
dotnet build LeatherNesting.sln -c Release
dotnet test LeatherNesting.sln -c Release
```

结果：0 警告 0 错误；75/75 测试通过。

## 阶段门结论

几何层 + 命令层 + 工作台状态机 + 黄金往返的自动化验收**全部通过**，阶段 2 的自动化部分关闭。

**仍需人工/后续项（不阻断自动化，但记入未完成清单）**：
- 画布真实渲染与鼠标拾取/拖动（按 `CadWorkbenchView` 注释留后续迭代）。
- 剪口的存储/导出模型（`NotchFeature` 校验与几何生成已实现，但其附着到项目与 DXF 表达待后续）。
- 黄金文件需产品/CAD 人员复核批准（当前为程序生成的临时基线，非人工批准）。
- 真机/平台/DPI 验收属阶段 6。
