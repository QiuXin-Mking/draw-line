# 设计：版型设置弹窗（新建排版入口）

本切片实现 PRD R2（版型设置）+ 入口映射（新建排版）。R3/R4 归后续切片。

## 现状

- 菜单已定义 `新建排版`（`ShellMenuCommand("新建排版", "M01", true)`，占位、导航到 M01）。
- 点击走 `AppShellViewModel.ActivateMenuCommand` → `Select(M01)` + `ShowTodo`。
- 弹窗先例 `PiecePropertiesWindow`：`Window` 子类，view 层用 `TopLevel.GetTopLevel(this) is Window owner` 获取 owner 后 `await ShowDialog(owner)`。
- 状态栏含 `StatusDemoText`（`"DEMO · 骨架数据仅用于界面对照"`），可作配置摘要显示位。

## 目标行为

- `文件 > 新建排版` 与工具栏同名按钮 → 模态打开 `版型设置`（标题「版型设置」，~570×335，经典白灰、细边框、小字号、右下确定/取消）。
- **确定**：校验通过 → 写入共享内存配置 `LayoutSetupStore` + 状态栏更新配置摘要；**取消**：不改状态；**非法输入**：保留弹窗、字段旁中文错误、不写状态。
- 不导航离开当前模块（PRD R1：关闭后保留五区原状态）。

## 结构

新增 `src/LeatherNesting.Desktop/Modules/LayoutSetup/`：

| 文件 | 职责 |
|---|---|
| `LayoutSetupConfig.cs` | `LayoutDirection` 枚举（横向/纵向）、`LayoutSetupConfig` record（8 字段 + `Summary`）、`LayoutSetupStore`（共享内存配置：`Current`/`IsConfirmed`/`Confirm`/`Reset`，`Default` 单例） |
| `LayoutSetupViewModel.cs` | 表单模型：输入字符串、默认值、方向/余片选项、`TryConfirm()` 校验（字段级错误）、`ToConfig()` |
| `LayoutSetupView.cs` | 表单 `UserControl`：字段 + 单选 + 下拉 + 错误文案 + 确定/取消；暴露关键控件属性供测试 |
| `LayoutSetupWindow.cs` | `Window` 子类：标题/尺寸/样式，确定→`TryConfirm`+`Close(true)`，取消→`Close(false)`；`Config` 暴露确认结果 |

修改：

| 文件 | 改动 |
|---|---|
| `Shell/ShellTopMenu.cs` | 增加 `NewLayoutLabel = "新建排版"` 常量；`FileMenu` 与 `ShellToolbar` 的「新建排版」改为 `NavigateToModule:false, IsPlaceholderAction:false`（已实现动作，不再占位/导航） |
| `Shell/AppShellView.cs` | `BuildTopBar` 两个回调先经 `TryOpenNewLayout` 拦截；命中则 `OpenLayoutSetupDialogAsync()`，否则走原有 `_viewModel.ActivateXxxCommand` |

## 数据流

```
菜单/工具栏点击
  → AppShellView.BuildTopBar 回调
  → TryOpenNewLayout(label)  命中「新建排版」？
     是 → OpenLayoutSetupDialogAsync()
          owner = TopLevel.GetTopLevel(this) as Window
          window = new LayoutSetupWindow()
          ok = await window.ShowDialog<bool?>(owner)
          if ok == true && window.Config is var config:
              LayoutSetupStore.Default.Confirm(config)
              StatusDemoText.Text = config.Summary
              _viewModel.ShowDemoHint($"版型「{config.Name}」已确认 · 仅存内存")
     否 → _viewModel.ActivateMenuCommand / ActivateToolbarCommand（原逻辑）
```

## 校验规则（PRD R5）

- 宽度/长度/边缘/间距：可解析、非负小数（`材料长度 0` 合法 = 无限长卷料，不推导公式）。
- 层数：正整数。
- 名称：可空（默认空）。
- 任一非法 → `TryConfirm` 返回 false，对应字段旁显示中文错误（如「宽度必须是大于等于 0 的数值。」），不写 Store。

## 关键契约

- 多层余片下拉仅提供截图证实的 `补齐`，不新增猜测选项。
- 确定仅写内存配置，不启动排样、不持久化、不生成版型结果（PRD R5 诚实性）。
- 复用 `AppTheme` 经典白灰面板与既有设计令牌，不自行定义新色板。

## 测试

- 新增 `tests/LeatherNesting.Desktop.Tests/Modules/LayoutSetup/LayoutSetupViewModelTests.cs`：
  默认值（1360.00/0.00/6/补齐/0.00/2.00/纵向）、方向选项、余片选项、合法提交产生正确 `LayoutSetupConfig`、宽度/长度/层数/边缘/间距非法输入错误、长度 0 合法、取消不改状态、Store `Confirm` 写入。
- 更新 `tests/LeatherNesting.Desktop.Tests/Shell/TopCommandAreaTests.cs` TOP-008：`新建排版` 不再导航到 M01、不再产生 TODO；断言 `NavigateToModule=false, IsPlaceholderAction=false`。
- 表单 UI 断言（构造控件不渲染）：字段标签/默认值/单选选中/下拉选项/按钮文案。
- 不做真实 `ShowDialog` 的 UI 自动化（测试工程无 headless Avalonia），模态打开由 AppShellView 拦截逻辑 + 契约测试覆盖。
