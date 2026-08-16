# 新建排版弹出「版型设置」对话框

## Goal

点击顶部图标工具栏或「文件」菜单里的「新建排版」，弹出「版型设置」模态对话框，采集新版型的版型参数（名称、方向、材料尺寸、排样参数）。

## Requirements

### 入口

- 「新建排版」同时存在于：
  - 顶部图标工具栏 `ShellToolbar.Commands[0]`（`ToolbarIconKey.NewLayout`）。
  - 「文件」菜单 `ShellTopMenu.FileMenu[0]`。
- 点击后弹出「版型设置」模态对话框（`Window` + `ShowDialog(owner)`），**不再**走占位 TODO 提示。

### 对话框字段（自上而下）

| 字段 | 控件 | 默认值 |
|------|------|--------|
| 版型名称 | 文本框 | `"a"` |
| 版型方向 | 单选框（横向 / 纵向） | 纵向 |
| 材料宽度(mm) | 数值文本框 | `"1380.00"` |
| 材料长度(mm) | 数值文本框 | 空 |
| 材料层数 | 数值文本框 | `"1"` |
| 多层余片 | 数值文本框 | 空 |
| 材料边缘(mm) | 数值文本框 | `"0.00"` |
| 裁片间距(mm) | 数值文本框 | 空 |

### 按钮

- 底部右侧一个「确定」按钮。
- 处于默认焦点状态：`IsDefault = true`，蓝色边框（`AppTheme.ClassicFocus` + 2px 边框）。
- 点击「确定」关闭对话框。

## Constraints

- 遵循本项目 C# 代码构建 UI 的约定（无 XAML），参考 `PiecePropertiesWindow` / `PiecePropertiesView`。
- 仅使用 `AppTheme` 语义画笔，禁止局部近似色；焦点态必须用 `AppTheme.ClassicFocus`（见 `frontend/component-guidelines`）。
- 文本显式设置语义前景色（`AppTheme.PrimaryText`），防止 macOS 深色模式产生白字。
- 数值输入沿用现有 `TextBox` 约定（代码库未用 `NumericUpDown`）。

## Acceptance Criteria

- [ ] 点击工具栏「新建排版」弹出「版型设置」对话框。
- [ ] 点击「文件 → 新建排版」同样弹出该对话框。
- [ ] 对话框包含上述 8 个字段，默认值与上表一致。
- [ ] 「版型方向」默认选中「纵向」，「横向」未选中。
- [ ] 「确定」按钮位于底部右侧，`IsDefault=true` 且边框为 `AppTheme.ClassicFocus`（2px）。
- [ ] 点击「确定」关闭对话框。
- [ ] 更新受影响测试（`TOP-008`），并新增视图测试；`dotnet test` 全部通过。

## Notes

- 保持 `prd.md` 聚焦于需求、约束与验收标准。
- 版型参数目前仅为 UI 采集（DEMO），不落库、不参与排样计算；控件以 public 属性暴露，作为后续「接线位」。
