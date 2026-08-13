# UI 模块契约与发现

## Goal

建立模块本地定义、集中发现的 UI 注册契约，让 M01--M12 各自在自己的目录注册而无需并发修改 Shell 注册表。

## Requirements

- 只拥有 `src/LeatherNesting.Desktop/Modules/Contracts/**` 与 `tests/LeatherNesting.Desktop.Tests/Modules/Contracts/**`。
- 定义 `IDesktopModule`、不可变模块元数据、稳定排序和 ID 唯一性校验；不得引用具体模块或 Shell。
- 支持未来由 Shell 扫描 Desktop assembly 的模块定义；本任务不改 Shell、不添加具体 M01--M12 实现。
- 编写单元测试：重复 ID 必须失败，排序稳定，元数据不可变。

## Acceptance Criteria

- [ ] Desktop build 通过且 0 warning / 0 error。
- [ ] 所有新增文件位于授权路径内。
- [ ] 契约不依赖 Avalonia View 创建以外的 Shell 实现细节。

## Goal

TBD.

## Requirements

- TBD

## Acceptance Criteria

- [ ] TBD

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
