# UI 演示集成验收

## Goal

集成 12 个 UI 模块，生成演示脚本、TODO 台账并完成视觉和自动化质量门。

## Requirements

- 仅在全部模块就绪后执行。
- 拥有 `docs/demo-ui-walkthrough.md`、`docs/ui-todo-inventory.md`、集成测试；只修复 Shell 集成问题，不重写模块内部。
- 验证所有模块可导航、TODO 标识完整、DemoScenario 数据跨页一致、现有真实入口不回归。

## Acceptance Criteria

- [ ] 12 页均可进入并在 1366×768/100%/125%/150% 下可用。
- [ ] 所有未实现交互均有 TODO，且无伪造成功。
- [ ] `dotnet test`、构建、手动启动与演示脚本通过。

## Reference

父任务 `implement.md` T13、`design.md` 和子 Agent 矩阵。
