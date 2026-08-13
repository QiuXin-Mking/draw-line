# 技术设计

Demo 层是只读数据边界：公共项目摘要与各模块场景记录分开；工厂返回稳定值对象或只读集合。兼容 facade 暂时支持既有 Shell/Projects UI，具体模块以后只取所需 provider。
