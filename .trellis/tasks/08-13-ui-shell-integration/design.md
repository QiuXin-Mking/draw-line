# 技术设计

Composition root 显式构造 Workspace、Demo provider、Import coordinator 与 module catalog。Shell 缓存每个模块的 Control，订阅 Workspace 更新并渲染 snapshot；发现目录只处理元数据和 module factory。旧模块通过短期兼容定义注册，后续各模块 worktree 在本目录自身替换为本地 `IDesktopModule`。
