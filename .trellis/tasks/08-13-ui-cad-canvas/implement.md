# 执行计划：CAD 工具栏八阶段闭环

## 总体运行规则

采用“一个阶段一个可验收增量”，而不是 27 个按钮各自开工。阶段之间通过稳定合同衔接；只有通过本阶段 Gate 才进入下一阶段。每个实现 worktree 均从最新已合入阶段分支创建，不从其他未合并 worktree 分叉。

统一分支建议：`feat/cad-toolbar-s01-contract` 至 `feat/cad-toolbar-s08-closure`。

## 阶段 1：序号、控件 ID 与语义合同冻结

**目标**：先回答“画面上到底有多少按钮、按什么顺序、叫什么、哪些是事实、哪些是推测”。

**输入**：`05-图片/03-.md`、连续 tooltip 截图、现有 5 按钮测试。

**产物**：

- `CadToolDefinition` 和 `CadToolCatalog`。
- 27 项 Order / ControlId / CommandKey / Label / Tooltip / Group / Confidence / TODO 状态。
- Catalog 单元测试。

**Gate S1**：恰有 27 项；序号 1–27 连续；ID `CAD-04…CAD-30` 连续且唯一；前五项与截图明示一致；低置信项带 `TODO待实机确认`。

**禁止**：本阶段不画图标、不改布局、不接业务逻辑。

## 阶段 2：27 个原创矢量图标

**目标**：解决“每个按钮看起来是什么”，不混入状态与业务。

**拆分**：可并行分给 3 个 worktree，前提是只写各自文件。

| 子任务 | 图标范围 | 文件所有权 |
| --- | --- | --- |
| S2-A | 01–10，订单/选择/基础图元 | `CadToolIconGroupA.cs`、`CadToolIconGroupB.cs` |
| S2-B | 11–20，工艺/轮廓/曲线 | `CadToolIconGroupC.cs`、`CadToolIconGroupD.cs` |
| S2-C | 21–27，对象/历史/设置 | `CadToolIconGroupE.cs`、图标工厂聚合测试 |

**Gate S2**：27 个 IconKey 均能创建非空矢量图；图标没有文字 emoji、截图裁切或外部文件依赖；在 100%/125%/150% 下仍清楚。

**合并顺序**：A → B → C；聚合文件由 S2-C 最后适配，禁止其他 worktree 重写。

## 阶段 3：静态外观与布局骨架

**目标**：先达到截图中的密度和层级，不做复杂交互。

**产物**：

- “填充”复选框、红色标签、`255` 输入框。
- 27 个 24×24 方形按钮、五组分隔、黑色画布上沿右对齐。
- Tooltip 和 Automation Name。
- 1366×768 单行布局。

**Gate S3**：控件数量、顺序、组间分隔、尺寸、颜色和 Tooltip 自动测试通过；人工截图与参考图按区域对照，不出现换行、遮挡、大圆角或 emoji。

## 阶段 4：按钮状态与上下文模式

**目标**：让工具栏“像软件”，而不是 27 张静态图。

**产物**：

- `CadToolbarState`。
- Normal / Hover / Active / Disabled / KeyboardFocus 五种视觉状态。
- `CadEdit` 27 项与 `NestingReview` 6 项投影。
- 工具互斥选择。

**Gate S4**：任意时刻最多一个 active tool；切换模式后按钮数分别为 27/6；公共按钮 ID 和相对顺序不变；无历史/无选择时相关按钮禁用。

## 阶段 5：最小真实交互与统一 TODO

**目标**：把能做的做真，不能做的明确告诉用户。

**真实接线**：范围缩放调用现有 `Refit()`；选择按钮切换模式；取消返回选择；关闭/清空沿用现有行为。

**TODO 接线**：绘图、文字、标注、孔位、马牙齿、轮廓、UV、擦除、面域和变换调用 `ReportUnsupported(Label)`，不修改几何。

**Gate S5**：遍历所有 TODO 命令，点击后状态文字含命令名与 `TodoBadge.StandardText`，同时几何引用/数量、文件名和工作区快照不变；真实范围缩放测试通过。

## 阶段 6：Shell、画布与右参数栏集成

**目标**：把新工具栏接回唯一 CAD 宿主，不产生第二套画布或第二套状态。

**产物**：

- `CadWorkspaceHost` 使用新 Catalog/View/State。
- M02 确认导入仍把真实几何发布到同一 CAD Host。
- M03、顶部“CAD工具”和右侧参数栏仍指向同一会话。
- 兼容现有 `DrawingToolButtons` 测试访问，或一次性迁移到新测试 API。

**Gate S6**：导入真实 DXF → 确认毫米 → 中央画布出现几何；27 工具栏可见；右面板未被遮挡；切换模块再返回保持状态。

## 阶段 7：自动化、视觉与人工逐项验收

**目标**：证明“数量对、样式对、行为诚实、旧功能没坏”。

**自动命令**：

```bash
dotnet test tests/LeatherNesting.Desktop.Tests --filter "FullyQualifiedName~CadToolbar|FullyQualifiedName~CadHost"
dotnet test tests/LeatherNesting.Desktop.Tests
dotnet test
dotnet build LeatherNesting.sln
```

**人工检查**：在 `docs/manual-ui-acceptance.md` 为 01–27 每项填写：可见、顺序、图标、Tooltip、状态、点击结果、TODO 是否诚实、截图路径、Pass/Fail。

**视觉矩阵**：至少检查 1366×768@100%、1920×1080@125%、1920×1080@150%，以及 CAD 编辑/结果浏览两种模式。

**Gate S7**：所有自动命令通过；27 行人工记录完整；不存在未标 TODO 的假功能；高严重度视觉问题为 0。

## 阶段 8：合并、回归、文档和关闭

**目标**：把多个 worktree 的结果安全归并到主线并留下后续可继续开发的台账。

**顺序**：

1. 确认每个阶段 worktree 干净、各自提交且 Gate 通过。
2. 按 S1 → S2 → S3 → S4 → S5 → S6 → S7 顺序合并，冲突由拥有目标文件的阶段 Agent 解决。
3. 在主目录重新运行 S7 全量命令，不接受“在 worktree 里通过”代替主线验证。
4. 更新 TODO 台账：每个未实现工具的后续依赖、移除 TODO 的条件、建议业务任务。
5. 执行 Trellis check、必要的 spec update、最终 commit 和 finish-work。

**Gate S8 / Definition of Done**：主线构建和测试通过；27 项均有 ID、图标、Tooltip、状态和验收记录；真实导入/范围缩放未回归；所有未实现算法明确 TODO；工作树不存在本任务遗留的未提交文件。

## Agent / Worktree 调度矩阵

| 阶段 | 是否可并行 | 前置依赖 | 推荐角色 | 主要冲突点 |
| --- | --- | --- | --- | --- |
| S1 合同 | 否 | 无 | 1 个 implement + 1 个 check | Catalog 是后续唯一基线 |
| S2 图标 | 是，3 路 | S1 已合入 | 3 个 implement | 必须按组分文件 |
| S3 外观 | 可与 S4 状态并行 | S1；使用稳定 API | 1 个 UI implement | 不修改 Catalog |
| S4 状态 | 可与 S3 外观并行 | S1 | 1 个 state implement | 不修改 View 样式 |
| S5 接线 | 否 | S3、S4 已合入 | 1 个 implement | 命令路由与 TODO 边界 |
| S6 集成 | 否 | S5 | 1 个 integration implement | `CadWorkspaceHost` 由此阶段独占 |
| S7 验收 | 自动测试与人工截图可并行 | S6 | check Agent + 人工 | 不顺手重构产品代码 |
| S8 闭环 | 否 | S7 | 主 Agent | 主线合并、全量验证、提交 |

## 每阶段通用回报模板

```text
阶段：Sx
分支 / worktree：
拥有文件：
完成的 AC：
未完成 / TODO：
运行的测试命令与结果：
截图或证据路径：
是否允许进入下一 Gate：Yes / No
遗留风险：
```
