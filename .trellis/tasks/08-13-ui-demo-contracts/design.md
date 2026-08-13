# 技术设计

Demo 层是只读数据边界：公共项目摘要与各模块场景记录分开；工厂返回稳定值对象或只读集合。兼容 facade 暂时支持既有 Shell/Projects UI，具体模块以后只取所需 provider。

## 已落实的契约

- `IDemoProjectSummaryProvider.Summary` 是 Shell 和跨模块共用的纯值投影；它不依赖 View、Shell 或 Infrastructure。
- `IProjectsDemoProvider` 在公共摘要上增加项目页所需的版本、变更和导出记录。
- `DemoScenarioFactory.Projects` 是新模块入口；列表由只读集合公开，页面不能通过 `IList<T>` 修改记录。
- `DemoScenarioFactory.Default` 与 `DemoScenario` 保留为旧页面的兼容 facade，字段和值集合投影必须与 provider 一致。新增模块不得扩展该 facade。
