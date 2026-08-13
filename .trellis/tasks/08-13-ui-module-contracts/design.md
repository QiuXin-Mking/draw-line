# 技术设计

模块定义是纯 Desktop 契约：元数据包含 ID、标题、分组、排序值；`IDesktopModule` 暴露元数据和由未来 Composition 注入的页面工厂。校验器作为独立纯函数，Shell 未来负责扫描和调用它。

本任务不实现扫描，也不修改现有 `ModuleDescriptor`。
