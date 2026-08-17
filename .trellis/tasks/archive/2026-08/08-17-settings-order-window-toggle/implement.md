# 实施：设置>订单窗口 切换左侧栏显隐（取代◀细条）

## 顺序

1. **`src/LeatherNesting.Desktop/Shell/ShellTopCommands.cs`**
   - `ShellCommandLaunch` 枚举新增 `ToggleOrderWindow`。
   - `SettingsMenu` 的「订单窗口」命令改为 toggle 命令（见 design.md）。

2. **`src/LeatherNesting.Desktop/Shell/TopCommandArea.cs`**
   - `CreateCommandItem`：`ToggleOrderWindow` 命令构建 checkable `MenuItem`（`ToggleType = ToggleType.CheckBox`、`IsChecked = true`），并捕获到 `OrderWindowToggle`。
   - 新增 `public MenuItem? OrderWindowToggle { get; private set; }`。

3. **`src/LeatherNesting.Desktop/Shell/AppShellViewModel.cs`**
   - 新增 `public event EventHandler? OrderWindowToggleRequested;`
   - `ActivateMenuCommand` 增加 `ToggleOrderWindow` 分支：抛事件并返回。

4. **`src/LeatherNesting.Desktop/Shell/AppShellView.cs`**
   - 删除细条相关：`_leftRailGlyph`、`LeftRailStrip`、`LeftRailToggle`、`BuildLeftStrip()`、glyph 更新。
   - `BuildLayout()`：外层 Grid 还原单列 `*`，移除 ColumnSpan 与细条列。
   - `ToggleLeftRail()`：去掉 glyph，末尾同步 `OrderWindowToggle.IsChecked`。
   - 构造函数订阅 `OrderWindowToggleRequested` → `ToggleLeftRail()`。

5. **测试** `tests/LeatherNesting.Desktop.Tests/Shell/ShellFrameTests.cs` + `TopCommandAreaTests.cs`
   - TOP-005：外层单列 Star。
   - FRAME-006/007/008：按 design.md 表格重写。
   - 新增 TOP：激活「订单窗口」→ 事件触发、不导航、不写 TODO。

6. **spec 更新** `.trellis/spec/frontend/component-guidelines.md`
   - 「Collapsible side rails」段落改写：触发改为「设置>订单窗口」checkable 菜单命令，删除◀细条方案描述；保留「显式清零列宽」要点与「外层 Grid 加列会重新归位未定位子元素」的 pitfall。

## 验证命令

```bash
dotnet test tests/LeatherNesting.Desktop.Tests/LeatherNesting.Desktop.Tests.csproj --filter "TestId=TOP-005|TestId=FRAME-006|TestId=FRAME-007|TestId=FRAME-008"
dotnet test LeatherNesting.sln
```

## 验收

- 全解决方案测试通过。
- 「设置>订单窗口」默认勾选；取消勾选 → 左侧栏缩回、画布变宽、菜单项取消勾选；再勾选恢复。
- 无 `◀` 细条残留。

## 回滚点

- 单文件独立提交；若中途失败可 `git checkout -- <file>` 回滚到上一文件。
