# 阶段 2 剩余 — 技术设计

## 1. 命令与撤销模型

### 已有基础设施
`CadCommand`（抽象，含 `Execute`/`Undo`/`Redo`）、`CadCommandContext`（`CurrentLoops`）、`CadCommandResult`（`ResultLoops`/`Diagnostics`/`Success`）、`CadCommandTransaction`（undo/redo 栈）、`CadOperationSession`（preview/commit/cancel）。

### 具体命令（R1）
每个工具一个具体命令，采用**快照式撤销**：
- 构造时保存目标参数（loop id、node index、新位置、offset 距离等）。
- `Execute(context)`：把 `context.CurrentLoops` 存为私有 `_before`，应用几何操作，返回新 loops。
- `Undo(context)`：返回 `_before`。
- `Redo`：返回缓存的 after（或重新 Execute，取决于命令是否依赖 context）。

| 命令 | 几何操作 | 关键参数 |
|---|---|---|
| `CloseContourCommand` | `ContourCloser.Close` | loop id |
| `GapRepairCommand` | `GapRepair.Repair` | 曲线集合 |
| `BoundaryGenerateCommand` | `BoundaryGenerator.Generate` | 曲线集合 |
| `OffsetCommand` | `OffsetAdapter.Offset` | 距离/方向/join |
| `MoveNodeCommand` / `InsertNodeCommand` / `DeleteNodeCommand` | `NodeOperations.*` | loop id / node index / 新位置 |
| `BreakAtPointCommand` / `RemoveSegmentCommand` | `BreakOperations.*` | 断点/两点 |
| `NotchCommand` | `NotchFeature` + `NotchValidator` | 轮廓 id / 锚点弧长 / 形状 / 宽深 / 材料侧 |

### CadOperationSession 的 preview/commit 缺陷（必须修）
当前流程会**双重执行**：`Preview(command)` 里 `command.Execute` 一次并把结果写入 `_previewLoops`；`Commit()` 又调 `_transaction.Commit(pending, context)` 再次 `command.Execute`（且此时 context 已是 after 状态）。

修复方向（择一，推荐前者）：
- **A（推荐）**：`CadOperationSession.Commit()` 不重跑命令，只把 pending 命令压入 undo 栈、清空 pending；`CadCommandTransaction` 增加一个「只记录、不执行」的入口（如 `Record(command)`）。
- B：让命令幂等（Preview 与 Commit 各跑一次结果一致）——对 offset/节点移动等非幂等操作不可靠，弃用。

## 2. ViewModel 串通（R2）

`CadWorkbenchViewModel`：
- 每个 `Preview*` 构造对应命令 → `_session.Preview(cmd)`；`Success` 则 `_state=Previewing`，否则把 `Diagnostics` 灌入 `_problemMessages`。
- `Commit()`/`Cancel()`/`Undo()`/`Redo()` 直接委托 `_session`，并同步 `_state`。
- `CurrentLoops` 暴露 `_session.PreviewLoops`，供测试与（未来）画布读取。
- `LoadLoops` 保留（现有），`SelectTool` 保留（互斥模式，已有测试覆盖）。

## 3. 应用导航（R3）

`MainWindow`（导入检查器）在导入/确认毫米后，显示「进入工艺工作台」入口；点击后把窗口 `Content` 切换为 `CadWorkbenchView`（传入同一 `ProjectWorkflowViewModel` 的已确认轮廓）。阶段 2 用最简的窗口内切换，不引入路由框架。

## 4. DXF writer 与黄金 round-trip（R4/R5）

- 新增 `src/LeatherNesting.Infrastructure/Dxf/IDxfWriter.cs`（端口）与 `AsciiDxfWriter.cs`（实现），输出与 `AsciiDxfReader` 对称的最小 ASCII DXF：`LWPOLYLINE`（闭合标志）、`LINE`，保留图层。
- 黄金文件 `fixtures/golden/cad-repair/`：至少 1 个矩形 + 1 个需修复样本，`manifest.json` 记录来源与 SHA-256。
- 往返测试（`GoldenRoundTripTests` 或新增）：导入 → 修复 → offset → 剪口 → 导出 → 重载 → 校验环数/面积/图层在容差内。

## 5. 验收记录（R6）

`docs/acceptance/stage-2.md` 逐条对照 P2-BND/OFF/NOD/NOT/UND/RT/UI 记录通过状态与命令。

## 6. 风险与回退

- DXF writer 与 reader 格式不对称 → 往返失败。用最小 LWPOLYLINE/LINE 子集并与 reader 对齐，单测覆盖。
- 命令快照内存 → 阶段 2 规模小（单轮廓几 KB），可接受；深度限制沿用 `maxUndoDepth`。
- 导航改动影响 Stage 1 已验收的导入流程 → 保留 MainWindow 原有导入路径，只新增入口，不重构。
