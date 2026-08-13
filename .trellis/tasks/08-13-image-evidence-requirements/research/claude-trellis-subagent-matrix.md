# Claude Code + Trellis：12 个 UI 模块子 Agent 派发矩阵

## 0. 总体执行顺序

先创建并完成 **UI Demo Shell** 子任务。Shell 合入后，才可以同时派发下面的 12 个模块。最后创建/执行集成验收任务。

| 波次 | 子任务 | 模块 | 独占目录 | 依赖 |
| --- | --- | --- | --- | --- |
| A | `ui-demo-shell` | 共享框架 | `Shell/`、`DesignSystem/`、`Demo/` | 无 |
| B | `ui-projects` | M01 项目与订单 | `Modules/Projects/` | Shell |
| B | `ui-import` | M02 导入与诊断 | `Modules/Import/` | Shell |
| B | `ui-cad-canvas` | M03 CAD 画布 | `Modules/CadCanvas/` | Shell |
| B | `ui-geometry-repair` | M04 几何修复 | `Modules/GeometryRepair/` | Shell |
| C | `ui-process-features` | M05 工艺与码齿 | `Modules/ProcessFeatures/` | Shell |
| C | `ui-pieces` | M06 裁片订单 | `Modules/Pieces/` | Shell |
| C | `ui-materials` | M07 材料约束 | `Modules/Materials/` | Shell |
| D | `ui-nesting-run` | M08 排样控制 | `Modules/NestingRun/` | Shell |
| D | `ui-nesting-review` | M09 结果复核 | `Modules/NestingReview/` | Shell |
| D | `ui-validation` | M10 校验审批 | `Modules/Validation/` | Shell |
| D | `ui-export` | M11 导出交接 | `Modules/Export/` | Shell |
| D | `ui-administration` | M12 管理设置 | `Modules/Administration/` | Shell |
| E | `ui-demo-integration` | 集成/演示 | `docs/`、集成测试 | 全部模块 |

## 1. 每个 Agent 的不可省略前缀

```text
Active task: .trellis/tasks/<当前 child task>

你并不独自工作。保留其他 Agent 的更改；仅修改任务分配给你的目录及测试。若共享接口与预期不同，适配它，不要回退其他人的代码。

所有未接真实业务逻辑的交互控件必须显示：TODO · 演示占位，未接入实际逻辑。
点击 TODO 控件只能显示说明，不得伪造保存、排样、校验通过或导出成功。
```

## 2. 12 个模块的最短派发指令

| 子任务 | Agent 完成目标 | 必须展示的 UI | TODO 最小范围 | 验收命令 |
| --- | --- | --- | --- | --- |
| ui-projects | 创建项目与订单中心演示页 | 摘要、订单、状态时间线、版本、历史 | 编辑、新建、恢复、批准 | `dotnet test tests/LeatherNesting.Desktop.Tests` |
| ui-import | 包装真实 DXF 导入流程并显示诊断 | 步骤、单位、图层、实体、诊断 | 自动修复、批量映射、非 DXF | 同上 |
| ui-cad-canvas | 建 CAD 浏览与显示控制页 | 深色画布、对象树、图层、标尺、图例 | 多选命中、持久图层编辑 | 同上 |
| ui-geometry-repair | 建修复操作与预览工作台 | 问题表、工具组、差异预览、提交取消撤销 | 未接线手势、批量修复、持久化 | 同上 |
| ui-process-features | 建工艺特征/码齿规则页 | 工艺列表、剪口字段、码齿库/尺码表 | 创建/保存特征、码齿生成、刀具映射 | 同上 |
| ui-pieces | 建裁片、尺码、数量约束页 | 缩略图、表格、筛选、检查器 | 持久化、真实排样回写、批量写入 | 同上 |
| ui-materials | 建片料/卷料材料管理页 | 类型切换、字段、材料清单、统计 | 持久化、真实面积、真皮边界 | 同上 |
| ui-nesting-run | 建策略与运行状态页 | 预设、参数、运行状态、日志、指标 | 算法、真实进度/取消/结果 | 同上 |
| ui-nesting-review | 建排样结果和手调页 | 材料画布、实例、未放、指标、版本比较 | 拖动旋转镜像、真实验证/局部重排 | 同上 |
| ui-validation | 建校验审批报告页 | 问题级别、规则、定位、审批、报告 | 全量校验、签名、PDF | 同上 |
| ui-export | 建输出和交接页 | 输出选择、命名/目录/映射、manifest | 文件写入、外部程序、PLT/DWG/设备 | 同上 |
| ui-administration | 建规则库、审计、权限、设置页 | 版本库、时间线、角色、偏好 | 持久化、认证、日志落盘、适配器注册 | 同上 |

## 3. 模块交付格式

每个子 Agent 在自己的 Trellis 子任务中必须写入：

1. 实际改动文件清单；
2. 哪些控件为真实能力，哪些明确为 TODO；
3. 测试命令和结果；
4. 一张 1366×768 截图或可复现截图步骤；
5. 与 Shell/`DemoScenario` 的接口需求（若存在）。

不得以“页面已显示”取代这些证据。
