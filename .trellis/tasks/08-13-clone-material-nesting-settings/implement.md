# 实施：版型设置弹窗（新建排版入口）— 已执行记录

## 结果（2026-08-16 完成，并入 BoardSettings）

并行已存在 `Modules/BoardSettings` + `Launch` 机制，本切片在其上完成规格升级：

1. **新增** `BoardSettingsConfig.cs` — `BoardDirection`、`BoardSettingsConfig`（Default=1360.00/0.00/6/补齐/0.00/2.00）、`BoardSettingsStore`。
2. **新增** `BoardSettingsViewModel.cs` — 表单模型 + 校验（宽度/长度/边缘/间距非负、层数正整数阿拉伯数字、补齐/丢弃选项）。
3. **重写** `BoardSettingsView.cs` — 5 行布局（名称 / 方向 / 宽+长 / 层数+余片下拉 / 边缘+间距）、确定/取消、字段级错误、层数 Tunnel 数字过滤；`BoardSettingsWindow` 确定→TryConfirm+Close(true)、取消→Close(false)。
4. **改** `AppShellView.cs` — `OpenBoardSettings` 确定后 `BoardSettingsStore.Default.Confirm(config)` + 状态栏 `StatusDemoText` 更新摘要。
5. **删** 我建的 `Modules/LayoutSetup/` 对照模块；撤掉多余 `NewLayoutLabel` 常量。
6. **测试** — 新增 `BoardSettingsViewModelTests`（13 例）；更新 `BoardSettingsViewTests`（BOARD-001 默认值改为确认规格、BOARD-004 下拉、BOARD-005 取消按钮、BOARD-006 数字过滤）；`TopCommandAreaTests` TOP-008 并行已更新。

## 验证命令（全部通过）

```bash
dotnet build LeatherNesting.sln                       # 0 警告 0 错误
dotnet test LeatherNesting.sln                        # 全部通过（Desktop 290）
git diff --check                                      # 通过
```

## 说明

- 层数数字过滤原用 `RaiseEvent(TextInputEvent)` 测试，因 TextBox 自身消费事件不可靠，改为 Tunnel 订阅 + 谓词 `IsArabicDigitText` 直接测试。
- 未提交（并行用户侧在工作区动态提交，提交由用户侧负责）。
