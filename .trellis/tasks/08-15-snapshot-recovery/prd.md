# 崩溃恢复（快照方案）— 操作后节流触发

## Goal

完成崩溃恢复闭环：CAD 操作后**节流**自动快照（每 10 个操作写一次，避免每次写文件卡顿），启动时检测并恢复。

## 现状（confirmed facts）

- 快照读写能力已就绪：`ProjectSnapshotStore`（`.autosave`，复用 zip 序列化）。
- 工作台 `CadWorkbenchViewModel` 持有 `CadOperationSession`，管理 Loop2D；`Commit`/`Undo`/`Redo` 是状态变更入口。
- **衔接缺口**：工作台 Loop2D 与 `NestingProject.Pieces` 未连接——
  - 导入流程 `ImportCoordinator` 用 `ProjectDocument`，不是 `NestingProject`。
  - Loop2D 的 `StableId` 是 `loop-N`，不是 `Piece.Id`。
- 无节流、无自动触发。

## Requirements

1. **节流触发**：每次 `Commit`/`Undo`/`Redo` 累计计数，**每 10 个操作**触发一次快照（异步 fire-and-forget，不阻塞 UI）。
2. **衔接**：把工作台当前 Loop2D 构建成 `NestingProject`（Pieces），供快照序列化。
3. **快照**：到达阈值时调用 `ProjectSnapshotStore.SaveSnapshotAsync`。
4. **正常退出**：清除快照（`ClearSnapshot`），启动时快照存活 = 上次崩溃 → 恢复。

## Acceptance Criteria

- [ ] 累计 10 个操作后触发一次快照（不每操作都写）。
- [ ] 快照内容含当前 Loop2D 几何（圆弧/直线无损）。
- [ ] 快照写入异步，不阻塞 UI 线程。
- [ ] 正常退出清除快照，启动时无快照不恢复。

## Out of Scope

- 命令重放（原 `CrashRecoveryLog` 方向）。
- 启动恢复提示的 UI（本轮交付触发 + 快照，恢复提示接线后续）。

## 决策

- **衔接范围**：已确认 **StableId 占位**——触发时 `Piece.Id = Loop2D.StableId`，`Name`/`Size` 留空（当前无真实元数据源）。
