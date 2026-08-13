# UI M01 项目与订单中心

## Goal

实现 M01 项目与订单中心的演示页：项目摘要卡、订单信息、版本时间线、最近变更、导出历史、状态轨迹。所有数据来自共享 `DemoScenario`，写入类操作标 TODO、不更改真实项目数据。

## Background / Confirmed Facts

- 父任务 `implement.md` T01 与 `research/product-function-specification-table.md` M01 定义了完整合同。
- 当前 `DemoScenario` 字段不足：缺项目编号、创建人、客户、款号、交期、优先级、状态、备注，以及版本时间线/最近变更/导出历史三个列表。
- 当前 M01 在 Shell 中是占位页（`ModulePlaceholderView`）。
- 真实 `ProjectDocument` 已存在（Domain），但本任务只做演示页，不接真实写入/审批/恢复。

## Requirements

- **R1 扩展 DemoScenario**：新增项目编号、创建人、客户、款号、交期、优先级、状态、备注；新增版本时间线、最近变更、导出历史三个只读列表（子记录）。
- **R2 M01 页面**（`Modules/Projects/`）：项目摘要卡（项目名/编号/创建人/状态）、订单信息（订单号/客户/款号/交期/优先级/备注）、版本时间线、最近变更、导出历史、状态轨迹。
- **R3 演示交互**：点击版本条目显示只读差异摘要；状态可视化（状态机文字），不更改 `ProjectDocument`。
- **R4 TODO 行为**：新建、复制、审批、恢复、编辑订单信息均标 TODO，点击显示文本说明，不写入。
- **R5 接入 Shell**：M01 用 `ProjectsView` 替换占位页。

## Acceptance Criteria

- [ ] M01 页面所有字段来自 `DemoScenario`，非硬编码散落。
- [ ] 版本时间线、变更/导出历史完整可读。
- [ ] TODO 操作不写入数据且显示文本说明（`TodoBadge.StandardText`）。
- [ ] `dotnet build` 0 警告 0 错误，Desktop 测试不回归；新增 M01 测试通过。

## Out of Scope

- 真实项目持久化、审批、恢复、复制版本逻辑（后续任务）。
- 其它模块页面（T02–T12）。

## Open Questions

无阻塞问题。
