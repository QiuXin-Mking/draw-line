# 设计：版型设置对话框 + 「新建排版」路由

## 边界

- 新增文件：`src/LeatherNesting.Desktop/Modules/BoardSettings/BoardSettingsView.cs`，含两个类：
  - `BoardSettingsWindow : Window` — 模态对话框壳。
  - `BoardSettingsView : UserControl` — 表单内容。
- 修改文件：
  - `Shell/ShellTopCommands.cs` — 新增 `ShellCommandLaunch` 枚举，给两条「新建排版」命令加判别位。
  - `Shell/AppShellViewModel.cs` — 新增 `BoardSettingsRequested` 事件，在命令激活时对「新建排版」提前拦截。
  - `Shell/AppShellView.cs` — 订阅事件，弹对话框。
- 测试：
  - 更新 `Shell/TopCommandAreaTests.cs`（`TOP-008`）。
  - 新增 `Modules/BoardSettings/BoardSettingsViewTests.cs`。

## 契约

```csharp
public enum ShellCommandLaunch
{
    Module,           // 默认：导航到 TargetModuleId 对应模块
    NewBoardSettings, // 打开「版型设置」对话框
}
```

- `ShellMenuCommand` 与 `ShellToolbarCommand` 追加可选参数 `ShellCommandLaunch Launch = ShellCommandLaunch.Module`（在参数表末尾，带默认值），因此既有构造调用不受影响。
- 「新建排版」两条命令改为 `Launch: ShellCommandLaunch.NewBoardSettings`，并把 `IsPlaceholderAction` 置为 `false`（已非占位）。

```csharp
public sealed class AppShellViewModel
{
    public event EventHandler? BoardSettingsRequested;
    // ActivateToolbarCommand / ActivateMenuCommand 顶部：
    // if (command.Launch == ShellCommandLaunch.NewBoardSettings)
    // { BoardSettingsRequested?.Invoke(this, EventArgs.Empty); return; }
}
```

## 数据流

```
点击「新建排版」(toolbar/menu)
  → TopCommandArea 回调 → AppShellViewModel.ActivateToolbarCommand / ActivateMenuCommand
  → 判别 Launch == NewBoardSettings → 触发 BoardSettingsRequested（不导航、不写 TodoHint）
  → AppShellView 订阅者 OpenBoardSettings()
  → new BoardSettingsWindow().ShowDialog(TopLevel.GetTopLevel(this) as Window)
```

## 权衡

- **判别位用枚举而非字符串比较**：命令标签是用户可见文案，用 `label == "新建排版"` 判断脆弱；枚举语义清晰、可扩展（后续更多对话框复用）。
- **事件而非 View 内直接判断**：`AppShellViewModel` 是无 UI 的纯控制器，弹窗需要 `Window` owner，属 UI 层职责。ViewModel 只声明意图（事件），View 负责渲染，与代码库把 `ActivateContextCommand` 委托传入 `CadWorkspaceHost` 的做法一致。
- **`ShowDialog` 返回 `Task`**：本任务确定按钮仅关闭窗口，不需要返回值；`async void` 入口 + `await` 与 `OrderPiecePanels` 一致，`async void` 用于事件订阅是既有约定。

## 兼容性 / 回滚

- 记录追加默认参数，编译期向后兼容；无新增依赖、不改 csproj（SDK 项目自动包含新 `.cs`）。
- 若需回滚：删除新文件、还原 `ShellTopCommands.cs` / `AppShellViewModel.cs` / `AppShellView.cs` 及两处测试即可，无数据/迁移副作用。
