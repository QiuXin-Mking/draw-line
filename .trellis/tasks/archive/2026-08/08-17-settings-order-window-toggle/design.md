# 设计：设置>订单窗口 切换左侧栏显隐（取代◀细条）

## 目标形态

```
设置 ▸ 订单窗口 [✓]   ← ToggleType=CheckBox 的可勾选菜单项
```

- 勾选 = 左侧栏显示；取消 = 左侧栏缩回左缘、中央画布变宽。
- 移除 `◀` 细条及其 Auto 列，外层 Grid 还原单列 `*`。

## 数据流

```
点击「设置>订单窗口」菜单项
  → TopCommandArea._activateMenu(command)         [ShellMenuCommand.Launch == ToggleOrderWindow]
  → AppShellViewModel.ActivateMenuCommand(command)
       → 抛 OrderWindowToggleRequested 事件（不导航、不写 TODO）
  → AppShellView 订阅回调
       → ToggleLeftRail()                          [现有方法，去掉 glyph 更新]
            ├ 翻转 _leftRailCollapsed
            ├ LeftRail.IsVisible = !collapsed
            ├ LeftRailColumn.Width = 0 | 13*       [BodyGrid 第 0 列显式清零/恢复]
            └ TopCommands.OrderWindowToggle.IsChecked = !collapsed   [同步勾选]
```

## 改动点

### 1. `ShellTopCommands.cs`
- `ShellCommandLaunch` 枚举新增 `ToggleOrderWindow`。
- `SettingsMenu` 中「订单窗口」命令：
  ```csharp
  new ShellMenuCommand("订单窗口", "M01", false, NavigateToModule: false, Launch: ShellCommandLaunch.ToggleOrderWindow),
  ```
  （`IsPlaceholderAction=false`，`NavigateToModule=false`，`TargetModuleId` 保留 "M01" 便于溯源但不会被导航。）

### 2. `TopCommandArea.cs`
- `CreateCommandItem` 中：当 `command.Launch == ShellCommandLaunch.ToggleOrderWindow` 时，
  - `item.ToggleType = ToggleType.CheckBox;`
  - `item.IsChecked = true;`
  - 捕获引用：`OrderWindowToggle = item;`
- 新增属性 `public MenuItem? OrderWindowToggle { get; private set; }`。
- 点击仍走统一 `_activateMenu(command)`，不新增分支。

### 3. `AppShellViewModel.cs`
- 新增 `public event EventHandler? OrderWindowToggleRequested;`
- `ActivateMenuCommand` 在 `NewBoardSettings` 分支之后新增：
  ```csharp
  if (command.Launch == ShellCommandLaunch.ToggleOrderWindow)
  {
      OrderWindowToggleRequested?.Invoke(this, EventArgs.Empty);
      return;
  }
  ```
- 与既有 `BoardSettingsRequested` 模式一致（ViewModel 抛事件、View 订阅）。

### 4. `AppShellView.cs`
- 删除：`_leftRailGlyph` 字段、`LeftRailStrip`/`LeftRailToggle` 属性、`BuildLeftStrip()`、glyph 更新逻辑。
- `BuildLayout()`：外层 Grid 还原 `ColumnDefinitions.Parse("*")`，行 `Auto,*,Auto`；
  移除 `LeftRailStrip`、`Grid.SetColumnSpan(TopCommands, 2)`、`Grid.SetColumnSpan(StatusBar, 2)`；
  children = TopCommands / bodyLayer / StatusBar。
- `ToggleLeftRail()` 保留折叠逻辑，去掉 glyph 更新，末尾同步勾选：
  ```csharp
  if (TopCommands.OrderWindowToggle is { } item)
      item.IsChecked = !_leftRailCollapsed;
  ```
- 构造函数订阅：`_viewModel.OrderWindowToggleRequested += (_, _) => ToggleLeftRail();`
- `LeftRailColumn`、`IsLeftRailCollapsed`、`LeftRail.IsVisible` 逻辑不变。

## 测试调整

| 测试 | 现状 | 调整 |
|------|------|------|
| TOP-005 | 断言外层 2 列 Auto,Star | 改断言单列 Star、children 3 |
| FRAME-006 | 断言细条/LeftRailToggle/glyph | 改为断言「订单窗口」菜单项 checkable、初始勾选 |
| FRAME-007 | 断言折叠/恢复 + glyph | 改为断言折叠/恢复时 LeftRailColumn 与菜单项勾选一致 |
| FRAME-008 | 断言 TopCommands/StatusBar ColumnSpan=2 | 改为断言外层单列、无 Auto 细条 |
| 新增 TOP | — | 断言激活「订单窗口」抛 OrderWindowToggleRequested、不导航、不写 TODO |

## 兼容性

- `MenuBase`/`TopCommandArea` 其他菜单项不受影响（仅 `ToggleOrderWindow` 命令变 checkable）。
- CAD 右键菜单（`ActivateContextCommand` → `ActivateMenuCommand`）不含该命令，无冲突。
- 无持久化、无二进制兼容问题；纯 UI 层改动。
