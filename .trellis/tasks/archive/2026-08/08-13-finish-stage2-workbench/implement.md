# 阶段 2 剩余 — 实施计划

> 状态：规划中，等待最终审阅后 `task.py start`。

## 0. 验证命令

```bash
export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"
dotnet build LeatherNesting.sln -c Release
dotnet test LeatherNesting.sln -c Release
```

期望：0 警告 0 错误；现有 70 个测试不回归，新增测试全过。

## 1. 实施清单（有序）

1. **R1 具体命令**：在 `src/LeatherNesting.Application/CadEditing/Commands/` 下实现快照式撤销的 8 类命令（Close/GapRepair/BoundaryGenerate/Offset/MoveNode/InsertNode/DeleteNode/Break/RemoveSegment/Notch），每个带 `Execute`/`Undo`/`Redo` 与 `CadCommandResult`。
   - 验收：新增 `CadCommandTests` 用例，每个命令 Execute→Undo 后 loops 复原、Redo 后恢复。
2. **修 `CadOperationSession` 双重执行**：`Commit` 改为只记录 pending 命令不重跑；`CadCommandTransaction` 增加 `Record(command)`。
   - 验收：`Preview→Commit` 后 loops 只被应用一次；`Undo` 复原。
3. **R2 ViewModel 串通**：`CadWorkbenchViewModel` 各 `Preview*` 构造命令并走 `_session.Preview`；`Commit/Cancel/Undo/Redo` 委托 session。
   - 验收：`CadWorkbenchViewModelTests` 新增「PreviewClose 后 CurrentLoops 变化、Commit 后 Undo 复原」用例；现有 4 个状态机用例不回归。
4. **R3 应用导航**：`MainWindow` 增加「进入工艺工作台」入口，导入成功后切换 `Content` 到 `CadWorkbenchView`。
   - 验收：手动（`!` 启动）能导入后进入工作台；headless 测试覆盖 ViewModel 加载。
5. **R4 DXF writer**：`IDxfWriter` + `AsciiDxfWriter`（LWPOLYLINE/LINE 最小子集）。
   - 验收：新增 writer 单测（矩形导出 → `AsciiDxfReader` 读回，环数/顶点一致）。
6. **R5 黄金文件 + round-trip**：`fixtures/golden/cad-repair/` 样本 + `GoldenRoundTripTests` 真实往返（导入→修复→offset→剪口→导出→重载→校验）。
   - 验收：往返测试通过；黄金文件哈希记录在 `manifest.json`。
7. **R6 验收记录**：写 `docs/acceptance/stage-2.md`，逐条对照 P2-* 记录通过状态。
8. **收尾**：`dotnet format --verify-no-changes`、`git diff --check`、全解 build+test 复跑。

## 2. 高风险文件 / 回退点

- `CadOperationSession.cs`（改 Commit 语义）——回退点：保留 `Preview` 原样，仅改 `Commit`/`CadCommandTransaction`。
- `MainWindow.cs`（加导航）——回退点：只新增入口，不动既有导入流程。
- 新增 DXF writer 不触碰既有 `AsciiDxfReader`，往返失败只影响新增测试。

## 3. 完成后检查

- [ ] 全解 build 0 警告 0 错误、test 全过。
- [ ] `docs/acceptance/stage-2.md` 存在且无跳过用例。
- [ ] 黄金文件不自动自批（标记待 CAD 人员复核）。
- [ ] `implement.jsonl` / `check.jsonl` 已含真实条目（若走子代理）。
