# UI 演示共享 Shell

## Goal

实现 T00：为 12 个 UI 模块提供唯一的 Avalonia Shell、原创视觉主题、共享演示数据与显式 TODO 合同。

## Requirements

- 独占 `Shell/`、`DesignSystem/`、`Demo/` 和 `MainWindow.cs` 的 Shell 集成。
- 导航必须恰好注册 M01–M12；保留现有真实导入和工艺工作台入口。
- 不接真实逻辑的命令显示 `TODO · 演示占位，未接入实际逻辑`，且不伪造成功。
- 1366×768 下提供左导航、顶部命令、中心工作区、右检查器、底状态栏。

## Acceptance Criteria

- [ ] 有 12 个可注册模块并可切换内容。
- [ ] TODO 徽章包含可读文字；点击仅显示限制说明。
- [ ] 现有导入、项目保存、CAD 工作台入口不回归。
- [ ] Shell 测试和桌面测试项目通过。

## Reference

父任务 `08-13-image-evidence-requirements` 的 `design.md` §共享架构和 `implement.md` T00 是本任务完整合同。
