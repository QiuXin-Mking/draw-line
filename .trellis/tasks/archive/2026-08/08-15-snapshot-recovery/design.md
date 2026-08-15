# 崩溃恢复（快照）技术设计

## 组件

| 组件 | 层 | 职责 |
|---|---|---|
| `ProjectSnapshotStore` | Infrastructure | 快照文件读写（`.autosave`） |
| `ISnapshotRecovery`（端口） | Application | 保存 / 检测 / 恢复快照的编排边界 |

## 快照文件

- 位置：项目路径 + `.autosave`（与项目文件同目录）。
- 内容：完整 `NestingProject`（聚合根，含 `Pieces`/`Materials`/`NestingResults`）。
- 序列化：复用 `ZipNestingProjectStore` 的 zip + `manifest.json`（几何多态已支持）。

## 崩溃检测协议

- `SaveSnapshot` 写 `.autosave`（异步 fire-and-forget）。
- 正常保存/退出时 `ClearSnapshot` 删除 `.autosave`。
- 启动时 `.autosave` 存在 = 上次异常退出 → 提供 `LoadSnapshot` 恢复。

## 关键决策

1. **快照 = 完整 `NestingProject`**，不是增量 diff——简单、可独立恢复。
2. **触发时机**：操作后自动（`CadOperationSession.Commit` 后调用 `SaveSnapshot`）；本轮交付 store，接线后续。
3. **复用持久化**：不新写序列化，复用 `ZipNestingProjectStore` 的 zip 逻辑（几何多态、旧项目兼容都继承）。

## 兼容与回滚

- 纯新增 `ProjectSnapshotStore`，不影响现有 `ZipNestingProjectStore` / `ZipProjectStore`。
- 回滚：删除新增类型即可。
