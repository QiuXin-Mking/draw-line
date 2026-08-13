# UI Shell 集成重构

## Goal

把基础 Workspace、模块发现、Demo provider 和 Import coordinator 集成为唯一的 Desktop 应用组合入口，使导航切换保留模块实例，并让 Shell 的检查器/状态栏消费 Workspace snapshot。

## Requirements

- 只拥有 `src/LeatherNesting.Desktop/Shell/**`、`src/LeatherNesting.Desktop/Composition/**`、`src/LeatherNesting.Desktop/Views/MainWindow.cs`、`tests/LeatherNesting.Desktop.Tests/Shell/**` 与本任务文件。
- Shell 不再为每次导航创建新 View；模块 View/VM 由 Module 契约和 Composition 获取。
- Shell 发现本程序集 `IDesktopModule`，验证模块 ID，保持目前 12 个页面可导航；当前未迁移模块可经兼容 adapter 存在。
- 状态栏、右检查器和 TODO 命令只从 `IWorkspaceSession` / `IWorkspaceCommands` 读取或写入，不再直接读取静态 `DemoScenarioFactory.Default`。
- 所有 Infrastructure adapter（包括 F04 Import adapter）仅由 Composition 构造；不得改 Application/Infrastructure 实现、不得重写模块内部。

## Acceptance Criteria

- [ ] 同一模块切走再切回保持同一 Control 实例和可见状态。
- [ ] 12 个模块均可被发现、ID 唯一、排序稳定。
- [ ] Shell 从 Workspace 更新状态栏/检查器。
- [ ] Import 仍真实可进入检查、确认、保存与工作台。
- [ ] Desktop 全量测试与 build 通过，变更仅在授权路径。

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
