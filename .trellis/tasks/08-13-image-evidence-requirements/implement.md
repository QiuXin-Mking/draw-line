# UI 优先演示版：多 Agent 可执行开发计划

## 执行前提

- 先完成 T00 并合入主线，再并行执行 12 个模块任务。
- 每个 Agent 的第一行上下文必须是：`Active task: .trellis/tasks/08-13-image-evidence-requirements`。
- 每个 Agent 只拥有指定目录和指定测试文件；不得重写别人模块、不得撤销已有用户修改。
- TODO 合同不可省略：无真实业务逻辑的可操作控件必须直观标记 `TODO · 演示占位，未接入实际逻辑`。
- 每个子任务完成时必须运行自己的测试、`dotnet test` 相关项目、`dotnet build`，并附截图/演示证据路径。

## T00：共享演示 Shell（阻塞所有模块）

**所有权**：`src/LeatherNesting.Desktop/Shell/`、`DesignSystem/`、`Demo/`、`Views/MainWindow.cs`、`tests/LeatherNesting.Desktop.Tests/Shell/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 目标 | 把现有单页 `MainWindow` 升级为可承载 12 模块的专业 CAD 应用外壳，不删除已存在导入与工艺工作台入口。 |
| 新文件 | `Shell/AppShellView.cs`、`Shell/AppShellViewModel.cs`、`Shell/ModuleDescriptor.cs`、`DesignSystem/TodoBadge.cs`、`DesignSystem/AppTheme.cs`、`Demo/DemoScenario.cs`、`Demo/DemoScenarioFactory.cs`。 |
| 修改 | `Views/MainWindow.cs` 只负责创建 AppShell；`App.cs` 注册主题。 |
| UI | 左导航、顶部命令栏、中央内容、右检查器、底状态栏；深色 CAD 工作区+原创浅色管理面板；1366×768 可用。 |
| 真实能力 | 保留并可打开现有导入页和 `CadWorkbenchView`。 |
| TODO | 所有非已接入命令点击显示统一 TODO 面板；状态栏显示 `DEMO`，直到连接真实项目状态。 |
| 测试 | 模块清单恰有 12 项；初始模块可见；切换导航更换内容；TODO 徽章含文字；导入/工作台入口仍在。 |
| 完成判据 | Shell 成为唯一导航宿主，其他 Agent 不再改 MainWindow；截图可呈现完整专业软件骨架。 |

## T01：M01 项目与订单中心

**所有权**：`Modules/Projects/`、`tests/LeatherNesting.Desktop.Tests/Modules/Projects/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 项目摘要卡、订单信息、版本时间线、最近变更、导出历史、状态轨迹。 |
| 字段 | 项目名、订单号、客户、款号、交期、优先级、创建人、版本、状态、备注。 |
| 演示交互 | 点击版本查看只读差异摘要；状态可视化，不更改 `ProjectDocument`。 |
| TODO | 新建、复制、审批、恢复、编辑订单信息均为 TODO，显示尚未持久化。 |
| 测试/验收 | 所有字段来自 `DemoScenario`；状态时间线完整；TODO 操作不会修改数据且有文本提示。 |

## T02：M02 DXF 导入与诊断

**所有权**：`Modules/Import/`、`tests/LeatherNesting.Desktop.Tests/Modules/Import/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 复用/嵌入现有真实导入向导入口；增加步骤轨迹、单位确认卡、实体/图层统计、诊断表、定位按钮和问题级别图例。 |
| 真实能力 | 调用现有 `ProjectWorkflowViewModel.InspectAsync`、毫米确认和取消。 |
| TODO | 自动修复、批量图层映射、拖放多文件、非 DXF 格式入口均标 TODO。 |
| 测试/验收 | DXF 选择/检查/确认仍可执行；未确认单位不可进入可排样状态；诊断按 severity 呈现。 |

## T03：M03 CAD 画布、选择与显示

**所有权**：`Modules/CadCanvas/`、`Views/CanvasView.cs`（只允许扩展显示，不改几何算法）、`tests/LeatherNesting.Desktop.Tests/Modules/CadCanvas/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 深色画布宿主、对象树、图层可见性、缩放/全图/缩放到选择、坐标/标尺状态、图例。 |
| 真实能力 | 复用现有 Loop 画布缩放/平移与工艺工作台数据。 |
| TODO | 真实命中测试、框选、多选、图层持久化、复杂曲线编辑未接入则标 TODO。 |
| 测试/验收 | 隐藏/显示演示类别会改变画布图层；全图动作可见；每项未接入工具显示 TODO。 |

## T04：M04 轮廓诊断与几何修复

**所有权**：`Modules/GeometryRepair/`、`tests/LeatherNesting.Desktop.Tests/Modules/GeometryRepair/`；不得修改 `Geometry/` 和既有 Application 命令实现。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 轮廓问题表、选中对象预览、闭合/连接/生成轮廓/偏移/节点/剪断工具分组、预览差异、提交/取消/撤销区域。 |
| 真实能力 | 接线当前 `CadWorkbenchViewModel` 已有预览、提交、取消、撤销/重做状态。 |
| TODO | 任何当前 ViewModel 未实际接线的工具手势、批量修复、项目持久化提交必须标 TODO。 |
| 测试/验收 | 工具组与状态机可视；预览/取消状态改变可见；所有 TODO 不修改项目数据。 |

## T05：M05 工艺特征与码齿规则

**所有权**：`Modules/ProcessFeatures/`、`tests/LeatherNesting.Desktop.Tests/Modules/ProcessFeatures/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 工艺特征列表、内线/冲孔/剪口/文本/Mark 分类、选中裁片预览、普通剪口字段、码齿库/版本/尺码表/预览。 |
| 真实能力 | 仅可读取并展示当前 `NotchFeature`、`NotchValidator` 可支撑的演示验证结果。 |
| TODO | 创建/编辑/写入剪口，码齿生成、库持久化、刀具映射均标 TODO。 |
| 测试/验收 | 普通工艺和码齿在 UI/数据模型上明确分离；所有未接入提交操作显著 TODO。 |

## T06：M06 裁片、尺码与订单数量

**所有权**：`Modules/Pieces/`、`tests/LeatherNesting.Desktop.Tests/Modules/Pieces/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 以截图信息密度为目标的缩略图/表格双视图；数量、优先级、角度、镜像、间距、尺码、左右件、计划/已放/未放。 |
| 演示交互 | 搜索、排序、未完成筛选、选中行联动右检查器；可编辑控件只改变内存 demo 并在顶部显示 TODO。 |
| TODO | 保存订单、批量应用、真实统计与真实排样回写均标 TODO，除非接入真实领域模型。 |
| 测试/验收 | `未放=max(需求-已放,0)` 的显示正确；批量勾选不会影响未勾选演示项；所有编辑带 TODO。 |

## T07：M07 材料、料单与约束

**所有权**：`Modules/Materials/`、`tests/LeatherNesting.Desktop.Tests/Modules/Materials/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 片料/卷料切换卡，材料清单、宽/长/层数/边缘/间距/方向/可用区字段，多材料顺序、面积与用长摘要。 |
| 演示交互 | 切换材料展示不同 DemoScenario 指标；参数编辑只在内存中更新并出现 TODO。 |
| TODO | 材料持久化、真实可用区编辑、真实面积计算、真皮边界/瑕疵均标 TODO。 |
| 测试/验收 | 空/负数输入显示字段错误；片料和卷料字段差异清晰；演示数值标 DEMO。 |

## T08：M08 排样策略与运行控制

**所有权**：`Modules/NestingRun/`、`tests/LeatherNesting.Desktop.Tests/Modules/NestingRun/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 策略预设、时间预算、允许角度、排放顺序、种子、小件填空；运行控制和状态机时间线；当前最佳指标/日志。 |
| 演示交互 | 可以演示 `准备→运行→发现更优→完成/停止/取消`，但所有状态旁显示 `TODO · 模拟状态`。 |
| TODO | 自动排样算法、真实计时、真实取消、真实进度、方案写入均标 TODO。 |
| 测试/验收 | 状态转换合法；停止与取消展示不同语义；不能声称产生真实生产方案。 |

## T09：M09 结果画布与人工微调

**所有权**：`Modules/NestingReview/`、`tests/LeatherNesting.Desktop.Tests/Modules/NestingReview/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 材料画布、材料分页、实例选择、空余区、未放清单、利用率/完成率/用长面板、版本对比。 |
| 演示交互 | 选中实例、切换材料/版本、显示碰撞示例覆盖层。 |
| TODO | 拖动、旋转、镜像、锁定、局部重排、真实碰撞验证均标 TODO，除非真实接入统一验证器。 |
| 测试/验收 | 数字与 DemoScenario 一致；低利用率有原因卡；所有手调入口明确 TODO。 |

## T10：M10 校验、审批与质量报告

**所有权**：`Modules/Validation/`、`tests/LeatherNesting.Desktop.Tests/Modules/Validation/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 阻断/警告/提示汇总、问题表、对象定位占位、校验规则说明、审批面板、质量报告预览。 |
| 演示交互 | 切换有效/含错误的 DemoScenario；跳转结果画布的导航消息。 |
| TODO | 真实全量校验、实际豁免签名、审批持久化、PDF 生成均标 TODO。 |
| 测试/验收 | 有阻断项时批准/导出入口为禁用或 TODO；问题有对象、规则和建议；报告明确 DEMO。 |

## T11：M11 导出与生产交接

**所有权**：`Modules/Export/`、`tests/LeatherNesting.Desktop.Tests/Modules/Export/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | DXF、JSON/CSV、PNG/PDF 的输出选择；命名模板、目录、单位、原点、旋转、图层映射、导出清单/manifest 预览。 |
| 真实能力 | 可链接到已有 `AsciiDxfWriter` 的未来 adapter，但本任务不接入实际导出，避免生产误导。 |
| TODO | 所有实际文件写入、外部程序启动、PLT/DWG、设备发送均标 TODO。 |
| 测试/验收 | 有阻断校验时生产导出展示禁用解释；manifest 字段完整；外部程序路径永不执行。 |

## T12：M12 规则库、审计、权限与系统设置

**所有权**：`Modules/Administration/`、`tests/LeatherNesting.Desktop.Tests/Modules/Administration/`。

| 项目 | 具体执行内容 |
| --- | --- |
| 页面 | 预设库（材料/策略/导出/图层/工艺）、版本列表、审计时间线、角色矩阵、单位/容差/自动保存/主题/日志设置。 |
| 演示交互 | 切换预设版本、筛选审计、切换角色显示允许/禁用状态；均源自 DemoScenario。 |
| TODO | 规则写入、权限认证、日志落盘、外部适配器注册、全局配置持久化均标 TODO。 |
| 测试/验收 | 旧项目快照与最新预设可区分；权限不足的动作明确禁用；所有未持久化编辑有 TODO。 |

## T13：集成、演示脚本与质量门

**所有权**：`docs/demo-ui-walkthrough.md`、`docs/ui-todo-inventory.md`、`tests/LeatherNesting.Desktop.Tests/UiDemoIntegrationTests.cs`；允许修复 Shell 集成问题，不重写模块内部。

| 项目 | 具体执行内容 |
| --- | --- |
| 集成 | 验证 12 模块全部注册、可导航、标题一致、右检查器/状态栏一致、演示数据跨页一致。 |
| 演示脚本 | 12–15 分钟：项目→导入→画布→裁片→材料→模拟运行→结果→校验→导出→审计；每一步说清真实能力或 TODO。 |
| TODO 台账 | 列出每个 TODO 控件、当前限制、后续依赖模块、去除 TODO 的验收条件。 |
| 视觉门 | macOS 与 Windows 启动；1366×768 和 100/125/150% 缩放截图；深色画布、可读表格、不重叠关键控件。 |
| 命令 | `dotnet test tests/LeatherNesting.Desktop.Tests`；`dotnet test`；`dotnet build LeatherNesting.sln`；手动 `dotnet run --project src/LeatherNesting.Desktop`。 |
| 完成判据 | 12 页全可导航；任何未实现交互都有 TODO；现有真实导入/保存/工艺工作台不回归；演示文档可由另一人照读完成。 |

## 建议并行批次

| 批次 | 可并行任务 | 前置 | 目的 |
| --- | --- | --- | --- |
| A | T00 | 无 | 搭共享边界，避免 12 人改 MainWindow。 |
| B | T01、T02、T03、T04 | T00 | 项目、导入、CAD 和几何工作台。 |
| C | T05、T06、T07 | T00 | 工艺、裁片订单、材料配置。 |
| D | T08、T09、T10、T11、T12 | T00 | 运行、复核、校验、导出、管理。 |
| E | T13 | B/C/D 全部完成 | 集成、视觉回归、TODO 审计和演示。 |

## Claude Code + Trellis 派发模板

对每个子任务创建 child task，并在调度 prompt 中使用下列格式：

```text
Active task: .trellis/tasks/<child-task-path>

你负责 <Txx / 模块名>，文件所有权仅限：<paths>。
你不在代码库中独自工作：保留他人修改，遇到共享接口变化时适配而非回退。

目标：<从 Txx 表复制>
必须实现：<UI/状态/测试>
TODO 合同：所有未接真实逻辑的可操作控件显示“TODO · 演示占位，未接入实际逻辑”，点击不得伪造成功。
禁止修改：<others' paths>。
验证：运行 <tests/build commands>，并在任务内记录结果与截图路径。
```

## 子任务创建建议

```bash
python3 ./.trellis/scripts/task.py create "UI 演示 Shell" --slug ui-demo-shell --parent .trellis/tasks/08-13-image-evidence-requirements
python3 ./.trellis/scripts/task.py create "UI M01 项目订单" --slug ui-projects --parent .trellis/tasks/08-13-image-evidence-requirements
# 其余模块按 ui-import、ui-cad-canvas、ui-geometry-repair、ui-process-features、ui-pieces、ui-materials、ui-nesting-run、ui-nesting-review、ui-validation、ui-export、ui-administration 创建。
```

创建完成后，在每个 child 的 PRD 中复制相应 Txx 表及验收，待 T00 通过后再将可并行批次派发给 Claude Code 的 Trellis Agent。
