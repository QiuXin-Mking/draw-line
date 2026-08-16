# 设计：版型设置弹窗（新建排版入口）

本切片实现 PRD R2（版型设置）+ 入口映射（新建排版）。R3/R4 归后续切片。

## 实现基线（2026-08-16 并行合并）

仓库已存在并行实现的 `Modules/BoardSettings`（提交 `feat(shell): 新增版型设置对话框模块` + 未提交样式微调），
采用 `ShellCommandLaunch.NewBoardSettings` 标记 + `BoardSettingsRequested` 事件 + `AppShellView` 订阅弹窗。
按用户确认：**并入 BoardSettings**（保留该架构），把表单升级到确认规格，删除我建的 LayoutSetup 对照模块。

## 现状

- `ShellTopCommands`：`ShellMenuCommand`/`ShellToolbarCommand` 增加 `Launch` 落点（默认 Module，新建排版=NewBoardSettings）。
- `AppShellViewModel.ActivateMenuCommand/ActivateToolbarCommand`：Launch==NewBoardSettings → 触发 `BoardSettingsRequested` 事件，不导航、不发 TODO。
- `AppShellView`：订阅事件 → `OpenBoardSettings()` → `new BoardSettingsWindow().ShowDialog<bool?>(owner)`。

## 结构（`Modules/BoardSettings/`）

| 文件 | 职责 |
|---|---|
| `BoardSettingsConfig.cs` | `BoardDirection` 枚举、`BoardSettingsConfig` record（8 字段 + Summary + Default）、`BoardSettingsStore`（共享内存配置：Confirm/Reset/事件） |
| `BoardSettingsViewModel.cs` | 表单模型：默认值、方向/余片选项（补齐/丢弃）、`TryConfirm` 校验（字段级错误）、`Cancel`、`ConfirmedConfig` |
| `BoardSettingsView.cs` | `BoardSettingsWindow`（标题「版型设置」，500×400，CenterOwner，Light 主题，确定/取消）+ `BoardSettingsView` 表单 + `IsArabicDigitText` 层数过滤 |
| `AppShellView.cs`（改） | `OpenBoardSettings`：确定 → `BoardSettingsStore.Default.Confirm(config)` + 状态栏 `StatusDemoText` 更新摘要 |

## 表单（2026-08-16 用户确认布局）

1. 版型名称（空）
2. 版型方向（横向/纵向单选，默认纵向）
3. 材料宽度(mm) 1360.00 + 材料长度(mm) 0.00（0=无限长卷料）
4. 材料层数 6 + 多层余片下拉（补齐/丢弃，默认补齐）
5. 材料边缘(mm) 0.00 + 裁片间距(mm) 2.00
右下角：确定 / 取消。

## 校验规则（PRD R5）

- 宽度/长度/边缘/间距：可解析、非负小数（长度 0 合法，不推导公式）。
- 层数：正整数，输入层仅阿拉伯数字（Tunnel TextInput 过滤 + VM 校验双保险）。
- 名称：可空。
- 任一非法 → `TryConfirm` false、字段旁中文错误、不写 Store、弹窗保留。

## 数据流

```
菜单/工具栏「新建排版」点击
  → AppShellViewModel：Launch==NewBoardSettings → BoardSettingsRequested 事件
  → AppShellView.OpenBoardSettings：
      owner = TopLevel.GetTopLevel(this) as Window
      ok = await new BoardSettingsWindow().ShowDialog<bool?>(owner)
      确定 → BoardSettingsStore.Default.Confirm(config) + StatusDemoText.Text = config.Summary
      取消/非法 → 不改变已确认状态
```

## 关键契约

- 选项（用户确认）：方向 `横向`/`纵向`；多层余片下拉 `补齐`/`丢弃`。
- 确定仅写内存配置，不持久化、不启动排样、不生成版型结果（PRD R5 诚实性）。
- 复用 `AppTheme` 经典白灰面板与既有设计令牌。
- 层数过滤用 Tunnel 策略（先于 TextBox 自身处理）。

## 测试

- `BoardSettingsViewModelTests`（新增）：默认值、方向/余片选项、合法提交、长度 0、非法输入、取消不改状态、Store Confirm。
- `BoardSettingsViewTests`（更新）：确认规格默认值、方向单选、确定/取消按钮、多层余片下拉选项、层数阿拉伯数字过滤谓词。
- `TopCommandAreaTests` TOP-008（并行已更新）：新建排版触发 BoardSettingsRequested、不导航、不发 TODO。
