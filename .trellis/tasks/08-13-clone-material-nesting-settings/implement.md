# 实施：版型设置弹窗（新建排版入口）

## 顺序与检查点

1. **纯逻辑层**：新增 `Modules/LayoutSetup/LayoutSetupConfig.cs`（`LayoutDirection`、`LayoutSetupConfig`、`LayoutSetupStore`）。
   - 检查点：编译通过。
2. **表单模型 + 单测**：新增 `LayoutSetupViewModel.cs` 与 `LayoutSetupViewModelTests.cs`。
   - 检查点：`dotnet test tests/LeatherNesting.Desktop.Tests` 绿。
3. **视图 + 窗口**：新增 `LayoutSetupView.cs`、`LayoutSetupWindow.cs`（含表单 UI 断言测试）。
   - 检查点：编译通过，表单断言测试绿。
4. **菜单契约**：改 `ShellTopMenu.cs`（`NewLayoutLabel` 常量；「新建排版」`NavigateToModule:false, IsPlaceholderAction:false`），同步更新 `TopCommandAreaTests` TOP-008 并新增契约断言。
   - 检查点：`dotnet test tests/LeatherNesting.Desktop.Tests` 绿。
5. **Shell 拦截**：改 `AppShellView.cs`（`TryOpenNewLayout` + `OpenLayoutSetupDialogAsync` + 确认回写 Store/状态栏）。
   - 检查点：编译通过。
6. **全量验证**：桌面测试、全解决方案测试、`dotnet build LeatherNesting.sln`、`git diff --check`。
7. **收尾**：更新 spec（如需要）、提交（workflow 3.4）。

## 验证命令

```bash
dotnet build LeatherNesting.sln
dotnet test tests/LeatherNesting.Desktop.Tests
dotnet test LeatherNesting.sln
git diff --check
```

## 回滚点

- 步骤 4 的菜单契约改动若与既有测试冲突：先回退 `ShellTopMenu.cs` 改动，仅保留拦截层，确认契约测试再改回。
- 每步结束编译/测试通过后才进入下一步；若某步失败，修复后从该步重跑。

## 评审门

- 实现前须经本文件与 design.md 评审（workflow 1.4），`task.py start` 后进入 Phase 2。
