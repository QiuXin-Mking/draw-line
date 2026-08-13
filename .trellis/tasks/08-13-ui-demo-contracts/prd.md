# UI Demo 数据拆分

## Goal

把当前扁平的 Desktop `DemoScenario` 拆为可由独立 UI 模块消费的只读样例提供者，同时保留现有 Shell 演示摘要兼容性。

## Requirements

- 只拥有 `src/LeatherNesting.Desktop/Demo/**`、`tests/LeatherNesting.Desktop.Tests/Demo/**` 与本任务文件。
- 新的模块样例提供者不得依赖具体 View、Shell 或 Infrastructure。
- 保持当前已完成页面的编译兼容；破坏性 API 必须在此任务内通过向后兼容 facade 消化。
- 用测试锁定 provider 的确定性、数据不可由页面修改、公共摘要与现有展示字段一致。

## Acceptance Criteria

- [ ] Desktop 全量 build 与测试通过。
- [ ] diff 不含 Demo 与测试、任务文档以外的代码。
- [ ] 不再需要让每个新模块修改单一 mutable 全局样例对象。

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
