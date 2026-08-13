# UI Workspace 契约

## Goal

建立 Desktop 层唯一的跨模块工作区状态契约，使项目摘要、选中对象、页面导航意图和演示提示不再由 Shell、Import、Projects 各自保留副本。

## Requirements

- 只拥有 `src/LeatherNesting.Desktop/Workspace/**` 与 `tests/LeatherNesting.Desktop.Tests/Workspace/**`。
- 提供不可变 `WorkspaceSnapshot`、`IWorkspaceSession` 和 `IWorkspaceCommands`；状态变化可订阅。
- Snapshot 至少表达当前项目摘要、选中对象 ID、活动模块、演示/TODO 提示；不得引用具体 Avalonia View。
- 提供内存实现和单元测试，验证状态变更、快照不可变、通知顺序。
- 不修改 Shell、模块页面、Composition、Demo、Domain/Application/Infrastructure。

## Acceptance Criteria

- [ ] Desktop build 通过且 0 warning / 0 error。
- [ ] Workspace 测试覆盖快照、状态通知和跨页命令意图。
- [ ] diff 不含授权目录外的生产代码。

## Goal

TBD.

## Requirements

- TBD

## Acceptance Criteria

- [ ] TBD

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
