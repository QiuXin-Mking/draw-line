# 完成阶段 2 剩余：工艺工作台 + 黄金 round-trip + 验收

## Goal

把主任务 `08-07-leather-nesting-windows-clone` 阶段 2（CAD 诊断、修复与普通工艺）中尚未完成的部分做完：让 U4 工艺工作台真正可用（工具预览/提交/撤销/重做端到端串起来），建立黄金 DXF round-trip，并补齐阶段 2 验收记录，使阶段 2 达到「可独立验收的裁片修版工具」。

## Background / Confirmed Facts

- 几何层已完成并通过：`ContourCloser`/`GapRepair`/`BoundaryGenerator`/`OffsetAdapter`/`NodeOperations`/`BreakOperations`/`NotchFeature`/`NotchValidator`/`ContainmentTree`/`EndpointIndex`/`ToleranceProfile`。`Geometry.Tests` 38/38 通过（上一任务 `08-13-fix-stage2-geometry-tests` 已归档）。
- CadEditing 基础设施已就位：`CadCommand`（抽象基类）、`CadCommandContext`、`CadCommandResult`、`CadCommandTransaction`（undo/redo 栈）、`CadOperationSession`（preview/commit/cancel）、`CrashRecoveryLog`。
- 现有测试：`CadCommandTests`（P2-UND-001，4 个用例，仅用测试内的 `AddLoopCommand`）；`CadWorkbenchViewModelTests`（P2-UI-001，4 个用例，只测状态机，不测实际轮廓变更）。

### 明确缺口

1. **无生产代码的具体命令**：`CadCommand` 只有测试里的 `AddLoopCommand` 一个子类；Close/Offset/MoveNode/Break/Notch 等真实命令均未实现。
2. **ViewModel 的 preview→commit 是空壳**：`PreviewClose()` 等只计算本地结果、设 `_state=Previewing`，从不调用 `_session.Preview(command)`；`Commit()` 调用 `_session.Commit()` 但没有任何 pending command，因此**实际不改变任何轮廓**。
3. **工作台未接入应用**：`App.cs` 只创建 `MainWindow`（导入检查器），没有「导入 → 工艺工作台」的导航入口。
4. **无 DXF writer**：`src/LeatherNesting.Infrastructure/Dxf/` 只有 `IDxfReader`/`AsciiDxfReader`，无 writer，黄金 round-trip（P2-RT-001）无法做真实 DXF 往返。
5. **`docs/acceptance/stage-2.md` 不存在**。
6. `CadWorkbenchView` 注释明确「real canvas rendering in later iteration」，当前画布是占位 `Border` + 文字。

## Requirements

- **R1 具体命令**：在 `LeatherNesting.Application/CadEditing/` 下实现各工具的具体 `CadCommand` 子类，每个都有 `Execute`/`Undo`/`Redo`：闭合（Close）、gap 修复（GapRepair）、边界生成（BoundaryGenerate）、offset、移动/插入/删除节点、单点剪断/去段、剪口（Notch）。
- **R2 ViewModel 串通**：让 `CadWorkbenchViewModel` 的每个 Preview 方法真正构造对应命令、调用 `_session.Preview(command)`，使 `PreviewLoops` 实际变化；`Commit`/`Cancel`/`Undo`/`Redo` 通过 `CadOperationSession` + `CadCommandTransaction` 真正改变并恢复轮廓。
- **R3 应用导航**：在导入完成后提供进入工艺工作台的入口，工作台能加载当前项目的轮廓。
- **R4 最小 DXF writer + 黄金 round-trip**：实现与 `AsciiDxfReader` 对应的 ASCII DXF writer；建立「导入→修复→offset→剪口→导出→重载」的往返测试，验证环/面积/图层/剪口在容差内一致。
- **R5 黄金文件**：建立 `fixtures/golden/cad-repair/` 黄金样本，标注来源、哈希与期望结果。
- **R6 验收记录**：写 `docs/acceptance/stage-2.md`，逐条对照 P2-BND/OFF/NOD/NOT/UND/RT/UI 记录通过状态。

## Acceptance Criteria

- [ ] `dotnet build LeatherNesting.sln -c Release` 0 warning / 0 error。
- [ ] `dotnet test LeatherNesting.sln -c Release` 全部通过（现有 70 + 新增命令/round-trip/UI 测试）。
- [ ] 每个工具能 preview→commit→undo→redo，且轮廓实际改变并恢复一致。
- [ ] 黄金 round-trip：矩形 + 至少一个修复样本，导出 DXF → 重载后环/面积/图层满足容差。
- [ ] `stage-2.md` 逐条记录 P2-* 用例，无跳过。

## Out of Scope

- 画布真实渲染与鼠标拾取/拖动（按 `CadWorkbenchView` 注释留后续迭代）。
- 阶段 3+（订单/码齿/材料/排样）。
- 完整 ExportProfile / PNG/PDF 导出（阶段 5）。
- 真机/平台验收（阶段 6）。

## Open Questions

无阻塞问题。Q1（画布范围）已由用户确认：**本轮做功能性界面，画布渲染留后续迭代**。
