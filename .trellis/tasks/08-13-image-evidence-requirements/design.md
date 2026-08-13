# UI 优先演示版设计

## 决策

本轮先交付可导航、可演示、可截图的 Avalonia 桌面 UI 框架；只复用已存在的 DXF 导入、项目工作流和 CAD 画布能力。其余业务逻辑不伪造：没有真实实现或没有接入真实领域服务的功能，必须在对应控件旁、详情面板或操作结果中出现显式 `TODO` 标识。

## 共享架构

```text
src/LeatherNesting.Desktop/
  Shell/                 # 主窗口、工作区导航、命令栏、状态栏
  DesignSystem/          # 颜色、字体、间距、图标替代、TODO 徽章
  Demo/                  # 只读 DemoScenario、演示数据工厂、导航状态
  Modules/
    Projects/            # M01
    Import/              # M02
    CadCanvas/           # M03
    GeometryRepair/      # M04
    ProcessFeatures/     # M05
    Pieces/              # M06
    Materials/           # M07
    NestingRun/          # M08
    NestingReview/       # M09
    Validation/          # M10
    Export/              # M11
    Administration/      # M12
```

每个模块拥有自己的 `*View.cs`、`*ViewModel.cs` 和 `*DemoState.cs`；模块不得直接依赖其他模块的 View 或 ViewModel。跨模块数据只通过 `DemoScenario`（演示阶段）或既有 Application/Domain use case（真实逻辑阶段）传递。

## Shell 合同

| 区域 | 必须内容 | 演示阶段行为 |
| --- | --- | --- |
| 左导航 | 12 个模块、分组、当前状态点 | 点击切换页面；无真实逻辑的入口带 TODO 徽章。 |
| 顶部命令栏 | 新建、打开、保存、导入、运行、停止、取消、导出 | 已接真实 use case 的功能可执行；其他命令必须显示 `TODO · 尚未接入业务逻辑`。 |
| 中央工作区 | 当前模块页 | 1366×768 下不截断；模块可各自滚动。 |
| 右侧检查器 | 当前选择、字段、校验摘要、操作提示 | 使用静态演示数据；“编辑”控件若不持久化必须标 TODO。 |
| 底部状态栏 | 项目、版本、单位、坐标、缩放、运行/校验状态 | 显示真实或演示来源；演示来源需写 `DEMO`。 |

## TODO 视觉与行为合同

1. TODO 必须是文字，不只靠颜色。标准文本：`TODO · 演示占位，未接入实际逻辑`。
2. 尚未实现的提交类操作不得默默成功：按钮点击后打开说明面板或通知，列出未来接入模块和当前不写入的数据。
3. 可以在演示层切换静态状态（例如“运行中”），但必须显示 `TODO · 模拟状态`，且不会产生/声称生产结果。
4. 已有真实功能（当前 DXF 选择、检查、毫米确认、项目保存、只读 CAD 画布）不能加 TODO；其余除非有自动测试证明，否则默认标 TODO。

## 数据流与兼容性

`DemoScenario` 提供同一份只读的项目、裁片、材料、放置、校验、导出记录样例，保证 12 个页面在演示时数字一致。真实服务接入后，每个模块替换自己的 demo provider，不可让 View 直接访问基础设施层。

现有 `ProjectWorkflowViewModel` 和 `CadWorkbenchViewModel` 保留。Shell 只适配其公开状态，不能将 import、几何修复或持久化逻辑搬入 UI 事件处理器。

## 并行边界和集成顺序

1. **先合入共享壳（T00）**：任何模块页面都依赖其导航、主题、DemoScenario 和 TODO 组件。
2. T01/T02/T03/T04/T05/T06/T07/T08/T09/T10/T11/T12 可在 T00 合入后并行；每个只修改自己目录及其导航注册项。
3. T04、T05 可展示已有 CAD 领域能力，但其提交、导出或规则库未接入时仍标 TODO。
4. T08/T09/T10/T11 共用 `DemoScenario` 的排样数据；不得各自复制一套数字。
5. 最后由 T13 集成验证：导航、视口、TODO 标识、视觉一致性、测试与演示流程。

## 风险与回退

- 多 Agent 同改 `MainWindow.cs` 容易冲突：仅 T00 拥有 Shell，模块 agent 通过一个约定的注册清单文件追加条目。
- 演示 UI 会被误认为成品：所有未接入区域有 TODO 徽章、状态栏 `DEMO`、操作提示。
- UI 直接耦合演示对象会阻碍真实逻辑：Demo state 只能经接口提供；真实接入时用 adapter 替换。
- 回退方式：UI 壳和模块按独立提交；移除模块注册即可撤回其页面，不触及 Domain/Geometry。
