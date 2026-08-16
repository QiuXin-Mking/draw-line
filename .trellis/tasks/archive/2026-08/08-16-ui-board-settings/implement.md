# 执行清单

## 1. 新建对话框

- 创建 `src/LeatherNesting.Desktop/Modules/BoardSettings/BoardSettingsView.cs`：
  - `BoardSettingsWindow`：`Title="版型设置"`、`Width=380`、`Height=460`、`WindowStartupLocation=CenterOwner`、`CanResize=false`、`Background=PanelSurface`、`Content=new BoardSettingsView()`。
  - `BoardSettingsView`：8 个字段 + 「确定」按钮，控件以 public 属性暴露。

## 2. Shell 命令判别位

- `ShellTopCommands.cs`：新增 `ShellCommandLaunch` 枚举。
- `ShellMenuCommand` / `ShellToolbarCommand` 追加 `Launch` 参数（默认 `Module`）。
- `FileMenu[0]` 与 `ShellToolbar.Commands[0]` 的「新建排版」改为 `IsPlaceholderAction=false` + `Launch: NewBoardSettings`。

## 3. ViewModel 事件

- `AppShellViewModel.cs`：新增 `BoardSettingsRequested` 事件；在 `ActivateToolbarCommand` / `ActivateMenuCommand` 顶部拦截 `NewBoardSettings`。

## 4. View 接线

- `AppShellView.cs`：构造函数订阅 `BoardSettingsRequested` → `OpenBoardSettings()`（`ShowDialog`）；加 `using ...Modules.BoardSettings`。

## 5. 测试

- 更新 `Shell/TopCommandAreaTests.cs` `TOP-008`：断言点击「新建排版」触发 `BoardSettingsRequested`，且不再写入 TodoHint / 不导航到 M01。
- 新增 `Modules/BoardSettings/BoardSettingsViewTests.cs`：字段默认值、方向默认纵向、确定按钮 `IsDefault` + `ClassicFocus` 边框。

## 验证

```bash
dotnet build src/LeatherNesting.Desktop/LeatherNesting.Desktop.csproj
dotnet test tests/LeatherNesting.Desktop.Tests/LeatherNesting.Desktop.Tests.csproj
```

## 回滚点

- 每步独立、可 `git diff` 检视；新文件删除即回滚，记录参数为追加无迁移。
