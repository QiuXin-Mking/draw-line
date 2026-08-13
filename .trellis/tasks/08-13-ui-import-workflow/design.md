# 技术设计

Import 模块定义 UI-facing coordinator interface；其 adapter 封装现有 Application/Infrastructure 调用并把结果写入 `IWorkspaceSession`。Composition 注入实际 adapter 的责任保留给 F05；该任务可提供可注入默认实现/测试 fake，但不改 Composition。
