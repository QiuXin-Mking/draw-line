# 崩溃恢复（快照方案）

## Goal

崩溃后能恢复未保存的工作——自动快照 `NestingProject`，启动时检测上次是否异常退出并恢复。

## 现状（confirmed facts）

- `NestingProject` 持久化已就绪（`ZipNestingProjectStore`）。
- 保存是手动的（`ImportCoordinator.SaveAsync`），无自动保存、无快照、无崩溃检测。
- 原 `CrashRecoveryLog`（命令重放方向）是死代码，本方案**改用快照**，不做命令重放。

## Requirements

1. **快照保存**：把当前 `NestingProject` 写到快照文件（项目路径 + `.autosave`）。
2. **崩溃检测**：正常保存/退出时清除快照；启动时快照文件存在 = 上次异常退出。
3. **恢复**：检测到快照后，加载快照恢复（提示用户，不静默覆盖当前状态）。

## Acceptance Criteria

- [ ] 保存快照后，能从快照文件加载出与保存时一致的 `NestingProject`（round-trip）。
- [ ] 清除快照后，加载返回空（无待恢复快照）。
- [ ] 快照 round-trip 含几何多态（圆弧/直线），无损。

## Out of Scope

- 命令重放（原 `CrashRecoveryLog` 方向）。
- 多项目快照管理、快照历史/版本。
- 触发接线（操作后自动触发、启动恢复提示）的 UI/编排——本轮交付核心快照读写能力，接线后续。

## 决策

- **快照触发时机**：已确认**操作后自动**（每次 CAD 操作 Commit 后异步写快照）。
