# 崩溃恢复（快照节流触发）实现计划

1. 新增 `SnapshotThrottle`（Application）：每 N 次 `ShouldFlush()` 返回一次 true。
2. 新增 `NestingProjectFactory.FromLoops`（Application）：Loop2D → `NestingProject`。
3. 新增 `ISnapshotStore` 端口（Application），`ProjectSnapshotStore` 实现它。
4. 新增 `OperationSnapshotCoordinator`（Application）：`RecordOperation` 节流触发快照。
5. 测试：
   - `SnapshotThrottle`：第 10 次触发、非 10 的倍数不触发。
   - `NestingProjectFactory`：Loop2D → Piece（`Id = StableId`）。
   - `OperationSnapshotCoordinator`：10 次操作后产生快照，少于 10 次不产生。
6. 全量测试：`dotnet test`，确认无回归。

**验证**：`dotnet test tests/LeatherNesting.Application.Tests tests/LeatherNesting.Infrastructure.Tests`

## 回滚点

- 纯新增，回滚删除新增类型即可。
