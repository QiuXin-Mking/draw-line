# 崩溃恢复（快照）实现计划

1. 新增 `ProjectSnapshotStore`（Infrastructure）：`SaveSnapshot` / `LoadSnapshot` / `ClearSnapshot`，快照路径 = 项目路径 + `.autosave`，复用 zip 序列化。
2. 测试：
   - 快照 round-trip（含几何多态，无损）。
   - `ClearSnapshot` 后 `LoadSnapshot` 返回空（无待恢复）。
3. 全量测试：`dotnet test`，确认无回归。

**验证**：`dotnet test tests/LeatherNesting.Infrastructure.Tests`

## 回滚点

- 纯新增，回滚删除 `ProjectSnapshotStore` 即可。
