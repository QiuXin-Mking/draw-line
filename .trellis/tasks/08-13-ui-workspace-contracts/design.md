# 技术设计

`Workspace` 是 Desktop 内部边界。Session 持有当前不可变 snapshot，并通过一个单一事件公布替换后的快照；Commands 只表达意图，避免 View 之间直接引用。实现为线程安全要求之外的单线程 UI 内存实现，后续 Composition 注入。

禁止此任务引用 Modules、Shell 或 Avalonia 控件。
