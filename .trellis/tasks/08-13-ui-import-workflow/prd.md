# UI 导入流程解耦

## Goal

将 M02 导入页从直接构造 `AsciiDxfReader`、`ZipProjectStore`、几何 reader 与工作台 ViewModel 的方式，改成由 Desktop Composition/Workspace 注入的导入协调流程；一个导入结果必须成为唯一 Workspace 当前文档。

## Requirements

- 只拥有 `src/LeatherNesting.Desktop/Modules/Import/**`、`src/LeatherNesting.Desktop/Adapters/Import/**`、对应 Import tests 与本任务文件。
- 使用 F01 的 Workspace 契约；不修改 Shell、Composition、Application/Infrastructure 既有实现。
- UI 不得直接 `new` ASCII reader、project store 或其他页面 ViewModel。
- 保留真实 DXF 检查、毫米确认、取消、保存与工艺工作台入口；未接入能力照旧标 TODO。

## Acceptance Criteria

- [ ] Import View 的依赖通过构造参数/接口获得。
- [ ] 同一次导入的诊断、几何与 Workspace 当前文档可追溯到同一协调结果。
- [ ] 相关测试、Desktop 测试和完整 build 通过。
- [ ] diff 不含授权范围外的生产代码。

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
