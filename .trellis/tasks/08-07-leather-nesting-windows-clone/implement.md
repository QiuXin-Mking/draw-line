# 跨平台皮革排样软件实施计划

> 状态：规划完成，等待最终审阅后由用户启动 Trellis  
> 依据：`prd.md`、`design.md`、`research/ui-evidence-and-functional-model.md`  
> 交付策略：7 个顺序阶段；第 6 阶段形成双平台内部 MVP，第 7 阶段形成商业测试版本。

## 0. 执行规则

### 0.1 Trellis 门禁

1. 用户最终审阅前保持 `task.json.status=planning`，不得执行 `task.py start`。
2. 启动后一次只执行当前阶段；上一阶段未过阶段门，不得宣布或默认其完成。
3. 每个测试使用固定 ID，并在代码中用 `[Trait("Stage", "N")]` 和 `[Trait("TestId", "P... ")]` 标记。
4. 阶段验收记录必须包含：提交/标签、安装包或构建物哈希、命令、原始结果、失败清单、人工签字和回退版本。
5. 黄金文件不得由待测实现自动重生成后直接批准；更新要有差异报告和人工复核。
6. 不修改 `02-竞品数据分析/02-银象科技/语音原版文字/` 的受保护原始转写。
7. 发现银象未知语义时更新研究/PRD，不用猜测补齐；超出 MVP 的设备、视觉、真皮能力另建任务。

### 0.2 每阶段通用检查

```bash
dotnet restore LeatherNesting.sln --locked-mode
dotnet build LeatherNesting.sln -c Release --no-restore
dotnet test LeatherNesting.sln -c Release --no-build
dotnet format LeatherNesting.sln --verify-no-changes
git diff --check
```

期望：所有命令退出码为 0；没有跳过的当前阶段测试；任何 warning-as-error、快照差异或 schema 迁移失败都会阻断阶段门。

### 0.3 时间与人员假设

- 估算基于 1 名熟悉 C#/.NET 的全职开发者，并能及时获得产品验收和 Windows 测试机。
- 双平台内部 MVP：约 14–17 周；商业测试版本累计约 20–26 周。
- 如果只有兼职、缺少现场样本或 Windows 测试机，按等待时间额外顺延。
- 真皮缺陷、视觉、投影和切割机直连不包含在该估算内。

### 0.4 阶段概览

| 阶段 | 工期 | 独立可验收成果 | 累计结果 |
|---|---:|---|---|
| 1 | 1.5–2 周 | 双平台壳、项目文件、DXF 导入与诊断 | 可从真实 DXF 建立可保存项目 |
| 2 | 2.5–3 周 | CAD 轮廓修复、offset、节点、剪断、普通剪口 | 可把问题裁片修成可验证工艺裁片 |
| 3 | 2–3 周 | 订单、尺码、码齿版本库、材料与约束 | 可形成完整且可复现的排样输入 |
| 4 | 3–4 周 | 自动排样、进度、停止/取消、指标 | 可稳定产生最佳完整结果 |
| 5 | 2–2.5 周 | 人工微调、统一校验、多格式导出 | 可形成生产交接文件包 |
| 6 | 2–2.5 周 | Mac/Windows 安装包、老机兼容、内部 MVP | 可在正式支持平台内部使用 |
| 7 | 6–9 周 | 商业授权、签名发布、升级回退、试点 beta | 可交付指定工厂商业测试 |

---

## 阶段 1：项目、跨平台壳与 DXF 导入

### 1.1 目标和独立成果

用户可在 macOS 和 Windows 打开程序，新建项目，导入真实 DXF，查看裁片/图层/单位和问题报告，保存、关闭并重新打开项目。该阶段不需要排样，也能作为独立的“DXF 检查器”验收。

### 1.2 实施清单

- [ ] 创建 `global.json`、中央包管理、`.editorconfig`、`LeatherNesting.sln` 和设计规定的项目/测试结构。
- [ ] 锁定 .NET 10、Avalonia 12 和所有 NuGet 补丁版本，启用 nullable、warnings as errors、deterministic build 和 compiled bindings。
- [ ] 建立 `ProjectDocument` v1、schema version、原子保存、自动恢复和迁移入口。
- [ ] 建立 `IDxfReader/IDxfWriter`、`IProjectStore`、`IClock`、`IFileDialogService` 和平台适配边界。
- [ ] 用 `凉鞋.dxf`、38–45.DXF 做 DXF 库选型 spike，记录 ADR、许可证、支持实体、误差和跨平台结果。
- [ ] 实现单位判定、实体清单、曲线规范化、外环/孔候选、开放/重复/零长/自交诊断和 ImportReport。
- [ ] 实现 U1 项目中心、U2 四步导入向导、U3 只读裁片/尺码列表和基础画布。
- [ ] 建立结构化日志、崩溃捕获和脱敏诊断 ID；默认不记录完整客户几何。
- [ ] 建立 Windows/macOS CI：restore、build、unit、headless UI 和发布 dry-run。
- [ ] 建立 `fixtures/manifest.json`，记录样本来源、哈希、期望实体/裁片/诊断，不复制无授权竞品素材到发布包。

### 1.3 主要交付文件

```text
global.json
Directory.Build.props
Directory.Packages.props
LeatherNesting.sln
src/LeatherNesting.{Domain,Geometry,Application,Infrastructure,Desktop}/
tests/LeatherNesting.{Domain,Infrastructure,Desktop,EndToEnd}.Tests/
fixtures/manifest.json
docs/decisions/ADR-001-dxf-adapter.md
docs/acceptance/stage-1.md
```

### 1.4 固定测试用例

| ID | 类型 | 用例与期望 |
|---|---|---|
| P1-BLD-001 | 构建 | Windows/macOS 均能 restore/build；架构测试确认 Domain 不引用 UI/Infrastructure |
| P1-DXF-001 | 正常 | 导入 `凉鞋.dxf`，得到 9 个闭合裁片和可定位文字/图层信息 |
| P1-DXF-002 | 真实异常 | 导入 38–45.DXF，识别旧式开放 `POLYLINE` 并列出诊断，不显示为空、不擅自封闭 |
| P1-DXF-003 | 校验 | 单位缺失/头信息冲突时阻止静默提交，用户确认后内部统一为 mm 并记录决定 |
| P1-DXF-004 | 失败 | 损坏/不支持 DXF 返回文件、实体和原因；程序及已有项目保持可用 |
| P1-DXF-005 | 回归 | 旧式 POLYLINE 的每条顶点数量被保留，不能在清点阶段丢失实体信息 |
| P1-DXF-006 | 单位 | `$INSUNITS` 被记录为提示，但在用户确认前仍不能作为毫米项目提交 |
| P1-DXF-007 | 工作流 | 检查 DXF 仅产生待确认结果；确认毫米后才增加项目导入记录和 SHA-256 来源 |
| P1-PRJ-001 | 一致性 | 保存→关闭→重开后项目 ID、输入哈希、裁片、单位决定和报告一致 |
| P1-PRJ-002 | 故障 | 模拟写盘失败/中断，只保留上一个完整项目并提供恢复副本 |
| P1-PRJ-003 | 正常 | 新建项目具有 schema v1、非空 ID、revision 0 和干净状态 |
| P1-UI-001 | UI | 导入向导前进/返回/取消；取消不改变项目；脏项目关闭出现保存/放弃/取消 |
| P1-UI-002 | 兼容 | 1366×768、100/125/150% DPI 无关键按钮截断；键盘可完成主流程 |
| P1-PLT-001 | 平台 | macOS arm64/x64 与 Windows x64 启动、导入、保存、重开冒烟通过 |
| P1-E2E-001 | 跨层主路径 | 用真实 `凉鞋.dxf` 完成检查→确认毫米→保存→重开，并保留导入来源哈希 |

### 1.5 阶段命令

```bash
dotnet test LeatherNesting.sln -c Release --filter "Stage=1"
dotnet publish src/LeatherNesting.Desktop -c Release -r osx-arm64 --self-contained true
dotnet publish src/LeatherNesting.Desktop -c Release -r osx-x64 --self-contained true
dotnet publish src/LeatherNesting.Desktop -c Release -r win-x64 --self-contained true
```

期望：P1 全部通过；三个 RID 产生可启动构建；DXF ADR 明确选用/拒绝理由和替换边界。

### 1.6 人工验收与阶段门

- [ ] 从空项目到导入并保存不需要手工改 DXF。
- [ ] 用户能从画布或问题表互相定位错误实体。
- [ ] 原文件、单位决定和诊断在项目中可追溯。
- [ ] Mac 和至少一台正式支持 Windows 环境完成演示。
- [ ] 产品负责人批准 DXF adapter ADR 和 Stage 1 记录。

**回退点**：只回退 Stage 1 安装包和项目 schema v1；如果 DXF 库失败，保留接口/领域模型并替换 adapter，不重写 UI。

---

## 阶段 2：CAD 诊断、修复与普通工艺

### 2.1 目标和独立成果

用户能对问题 CAD 执行可预览、可撤销的轮廓闭合/连接/生成、offset、节点编辑、剪断和普通剪口，并导出/重载验证。该阶段可独立作为“裁片修版工具”验收。

### 2.2 实施清单

- [ ] 实现统一 `ToleranceProfile`，清除散落魔法公差并检查整数几何溢出。
- [ ] 实现端点索引、相交拆分、平面半边图、候选面、containment tree 和拓扑验证。
- [ ] 分别实现 Close、Join/Gap Repair、Boundary Generation；桥接段显示 Extend/Trim/Add 来源。
- [ ] 实现 offset adapter：材料内/外、join style、1→0/1→N 预览和源轮廓保留。
- [ ] 实现节点显示/插入/移动/删除、单点剪断、两点去段及 feature anchor 重映射。
- [ ] 实现 `NotchFeature`：V/方/U/半圆/Mark，宽深、材料侧、Cut/Mark 和图层/刀具语义。
- [ ] 实现操作预览 session、Command transaction、Undo/Redo 和崩溃恢复日志。
- [ ] 实现 U4 工艺工作台、问题定位、拓扑差异提示和提交/取消。
- [ ] 建立 geometry property/fuzz tests 和黄金 DXF round-trip。

### 2.3 主要交付文件

```text
src/LeatherNesting.Geometry/{Topology,Repair,Offset,Validation}/
src/LeatherNesting.Application/CadEditing/
src/LeatherNesting.Desktop/{Views,ViewModels}/CadWorkbench*
tests/LeatherNesting.Geometry.Tests/
fixtures/golden/cad-repair/
docs/acceptance/stage-2.md
```

### 2.4 固定测试用例

| ID | 类型 | 用例与期望 |
|---|---|---|
| P2-BND-001 | 正常/边界 | 闭合矩形不变；gap=.05、tol=.1 可预览补桥；gap=.11 被拒绝 |
| P2-BND-002 | 歧义 | 多个候选环要求选择；T 支路不进入边界；不默认选最大面积 |
| P2-BND-003 | 失败 | bow-tie、自交双环、不同 Z/OCS 返回结构化错误且原几何不变 |
| P2-OFF-001 | 正常 | 100×50 矩形向内 1 得 98×48；外环缩、孔扩大 |
| P2-OFF-002 | 不变量 | 反转 winding 或曲线顺序后，材料空间 offset 结果在公差内等价 |
| P2-OFF-003 | 拓扑 | 细颈 1→2、小岛 1→0 均预警并要求确认；取消不提交 |
| P2-NOD-001 | 一致性 | 在线/圆弧插点前后 Hausdorff 误差≤约定值；单点剪断总长度守恒 |
| P2-NOD-002 | 校验 | 删除导致少于 3 点或移动产生自交时阻断可切割状态 |
| P2-NOT-001 | 正常 | 直/竖/弧边、镜像、反 winding 下剪口材料侧和宽深一致 |
| P2-NOT-002 | 失败 | 零/负/NaN/过大、重叠、穿孔、局部材料不足得到明确错误 |
| P2-UND-001 | 撤销 | 每次手势一条命令；撤销/重做后业务模型和 feature anchor 一致 |
| P2-RT-001 | 往返 | 导入→修复→offset→剪口→DXF/sidecar→重载，环/面积/图层/feature 满足公差 |
| P2-UI-001 | UI | 各工具模式互斥；Preview/Commit/Cancel 状态正确；颜色外有文字/图标提示 |

### 2.5 阶段命令

```bash
dotnet test tests/LeatherNesting.Geometry.Tests -c Release --filter "Stage=2"
dotnet test tests/LeatherNesting.Application.Tests -c Release --filter "Stage=2"
dotnet test tests/LeatherNesting.Desktop.Tests -c Release --filter "Stage=2"
```

### 2.6 人工验收与阶段门

- [ ] 对一份开放/断裂真实样本完成分析、预览、提交、撤销和重新提交。
- [ ] 新增桥接、offset 拓扑变化和 orphan feature 都能定位。
- [ ] 原始 DXF 始终保留；导出重载后人工叠加检查通过。
- [ ] 几何黄金文件由产品/CAD 人员批准，不由程序自动自批。

**回退点**：项目中的 CAD 操作为版本化 operation log；回退应用时仍可读取源 DXF 和已提交 v1/v2 项目，不可识别的新操作只读并提示升级。

---

## 阶段 3：订单、码齿版本库、材料与排样输入

### 3.1 目标和独立成果

用户可建立订单、尺码/左右数量、裁片约束、矩形/卷材，并用版本化码齿规则生成派生裁片，最终得到一份完整、可复现、可交给排样引擎的不可变输入包。

### 3.2 实施清单

- [ ] 实现 Order/OrderLine、精确 SizeKey、左右数量、款号、材料、交期、优先级和 revision。
- [ ] 实现 PatternPiece revision、派生裁片和订单数量一致性；批量操作显示影响范围。
- [ ] 实现普通剪口与 `SizeToothRuleSet`/`SizeToothEntry` 两套模型，禁止共享含糊 DTO。
- [ ] 实现码齿库 copy-on-write 版本、显式有序 token、尺码体系、锚点、方向、宽深、间距和旧任务快照。
- [ ] 实现“当前参数/版本库/逐点编辑”三种本产品模式；银象对应关系保留未验证标记，不复制冲突默认值。
- [ ] 实现矩形片材/卷材、margin、gap、角度、镜像、纹向、成组、锁定等约束。
- [ ] 实现 U3 可编辑订单、U5 码齿库/全尺码叠加预览、U6 材料与排样设置。
- [ ] 实现 `CreateNestingRequest`，生成 immutable snapshot、输入哈希、规则版本和 seed。
- [ ] 使用现场访谈表收集码数、序列、方向和 `替换反` 证据；只把已验证映射加入生产模板。

### 3.3 主要交付文件

```text
src/LeatherNesting.Domain/{Orders,Materials,Rules}/
src/LeatherNesting.Application/{Orders,RuleLibraries,NestingRequests}/
src/LeatherNesting.Desktop/{Views,ViewModels}/{Orders,SizeTeeth,Materials}/
tests/LeatherNesting.Domain.Tests/
fixtures/golden/size-tooth-rules/
docs/field-research/silver-elephant-validation-form.md
docs/acceptance/stage-3.md
```

### 3.4 固定测试用例

| ID | 类型 | 用例与期望 |
|---|---|---|
| P3-ORD-001 | 正常 | 订单尺码/左右数量生成准确需求；画布状态不反向修改订单 |
| P3-ORD-002 | 校验 | 负数、溢出、重复裁片 ID、缺材料、左右关系不完整被阻断 |
| P3-SIZ-001 | 精度 | 31、31.5、32 等半码精确排序，无二进制浮点漂移 |
| P3-TOO-001 | 正常 | 显式有序齿序列按锚点生成稳定黄金几何和语义 feature |
| P3-TOO-002 | 失败 | 缺码、重复码、非法计数、负宽深、间距不足、超出可用边长返回可理解错误 |
| P3-TOO-003 | 版本 | 复制修改库后产生新版本；旧项目重开仍使用旧版本并复现输出 |
| P3-TOO-004 | 取消 | 多尺码预览取消后不生成派生裁片、不改变库版本 |
| P3-MAT-001 | 正常 | 片材/卷材、margin/gap、角度/镜像/纹向形成正确约束快照 |
| P3-MAT-002 | 校验 | 零/负尺寸、边距吞没材料、空角度集合、矛盾约束阻断请求 |
| P3-REQ-001 | 一致性 | 同一项目 revision、规则版本、seed 产生相同请求哈希 |
| P3-UI-001 | UI | 批量应用先显示影响裁片/尺码；缺失和冲突可定位；1366×768 可操作 |
| P3-FLD-001 | 证据 | 所有生产码齿模板均有录屏/截图/DXF 输入输出证据 ID；未知值没有伪默认 |

### 3.5 阶段命令

```bash
dotnet test tests/LeatherNesting.Domain.Tests -c Release --filter "Stage=3"
dotnet test tests/LeatherNesting.Application.Tests -c Release --filter "Stage=3"
dotnet test tests/LeatherNesting.Desktop.Tests -c Release --filter "Stage=3"
```

### 3.6 人工验收与阶段门

- [ ] 从一个款号建立 31–37 的示例订单、左右数量、材料和约束。
- [ ] 应用码齿库前能看到所有目标裁片/尺码及最终序列。
- [ ] 修改规则库不会改变已保存旧订单。
- [ ] 输出的 NestingRequest 报告可由非开发人员读懂并确认。
- [ ] 未取得现场证据时，通用码齿能力可以技术验收，但不得把银象映射标为“已验证生产模板”。

**回退点**：规则和订单按 revision 保存；回退版本只读打开未知新规则并允许导出诊断，不自动降级或覆盖。

---

## 阶段 4：自动排样与运行控制

### 4.1 目标和独立成果

用户可选择质量档启动自动排样，查看进度和当前最佳指标，完成、停止并保留最佳，或取消并恢复运行前状态。结果满足统一几何和订单验证。

### 4.2 实施清单

- [ ] 定义 `INestingEngine`、NestingRequest/Progress/Result/RunOutcome、algorithm ID/version。
- [ ] 将 Python Demo 的确定性货架算法移植为基线，固定输入和 seed 时可重复。
- [ ] 加入 0/90/180、镜像、gap、margin、纹向、成组和锁定约束。
- [ ] 实现共享 `PlacementValidator`：碰撞、越界、间距、角度、镜像、数量和轮廓有效性。
- [ ] 实现候选位置/NFP 与启发式搜索；只发布已通过验证的完整最佳快照。
- [ ] 实现快速/均衡/精细 quality preset、时间预算、内存保护和 progress throttling。
- [ ] 实现 StopSignal 与 CancellationToken 分离、幂等取消、旧 job 进度隔离和失败恢复。
- [ ] 实现 U7 状态机、耗时、利用率、已放/未放、材料数和错误摘要。
- [ ] 建立 Python Demo 对照、确定性基准、无解和低性能压力测试。

### 4.3 主要交付文件

```text
src/LeatherNesting.Geometry/{Collision,Nesting}/
src/LeatherNesting.Application/NestingJobs/
src/LeatherNesting.Desktop/{Views,ViewModels}/NestingRun*
tests/LeatherNesting.{Geometry,Application,Desktop}.Tests/
fixtures/benchmarks/nesting-baseline.json
docs/acceptance/stage-4.md
```

### 4.4 固定测试用例

| ID | 类型 | 用例与期望 |
|---|---|---|
| P4-ALG-001 | 基线 | Demo 固定样本得到确定性合法结果，指标与迁移基线在批准容差内 |
| P4-ALG-002 | 约束 | 所有 placement 不重叠、不越界、满足 gap/margin/角度/镜像/纹向 |
| P4-ALG-003 | 一致性 | 相同输入哈希、算法版本和 seed 产生同一基线结果 |
| P4-ALG-004 | 无解 | 不可放件进入 unplaced 并给原因；不伪造成功、不丢订单数量 |
| P4-RUN-001 | 停止 | Running→Stopping→StoppedWithBest，返回最后一个已验证完整最佳结果 |
| P4-RUN-002 | 取消 | Running→Cancelling→Cancelled，恢复运行前快照且临时结果不可见 |
| P4-RUN-003 | 失败 | 算法异常/内存压力保留上次接受结果，项目不半写，显示诊断 ID |
| P4-RUN-004 | 并发 | 旧 job 的延迟 progress 不能覆盖新 job；重复 Stop/Cancel 幂等 |
| P4-UI-001 | UI | 运行中冲突字段禁用，UI 可平移缩放；进度节流且不静默卡死 |
| P4-PRF-001 | 性能 | 约定低配 Windows 基准机上 UI 响应达预算，停止请求在规定时间内生效 |
| P4-CMP-001 | 竞品基线 | 固定样本/约束/硬件/时间下生成对比报告；目标利用率≥约定竞品结果 90% |

### 4.5 阶段命令

```bash
dotnet test tests/LeatherNesting.Geometry.Tests -c Release --filter "Stage=4"
dotnet test tests/LeatherNesting.Application.Tests -c Release --filter "Stage=4"
dotnet test tests/LeatherNesting.Desktop.Tests -c Release --filter "Stage=4"
dotnet run --project tools/LeatherNesting.Benchmarks -- --manifest fixtures/benchmarks/nesting-baseline.json
```

### 4.6 人工验收与阶段门

- [ ] 同一项目分别演示完成、停止和取消，三个结果清楚不同。
- [ ] 运行时窗口可操作，停止后结果完整，取消后恢复原状态。
- [ ] 对比报告记录双方输入、约束、时间、硬件、seed 和测量方法。
- [ ] 未达到 90% 时阶段不通过；可调整算法或经用户书面修改基线，不得删测试。

**回退点**：排样结果保存 algorithm ID/version；旧版本可显示新结果，不能编辑不认识的算法参数。回退不删除已有接受结果。

---

## 阶段 5：人工微调、统一校验与生产文件包

### 5.1 目标和独立成果

用户能在自动结果上拖动、旋转、镜像、锁定、成组、排剩余和局部重排；只有通过统一校验的结果才能输出可追溯 DXF、PNG、PDF、CSV/JSON 文件包。

### 5.2 实施清单

- [ ] 实现 U8 中央画布、未放件、属性/问题面板和状态栏。
- [ ] 实现选择/框选、拖动、定角旋转、镜像、贴边、锁定、成组和批量 Undo/Redo。
- [ ] 实现排剩余和局部重排；锁定件不能被移动，旧自动结果标注变更来源。
- [ ] 轻量预览与权威 `PlacementValidator` 使用同一规则来源。
- [ ] 实现 U9 ResultSummary 和导出前阻断检查。
- [ ] 实现版本化 ExportProfile、DXF adapter、PNG/PDF/CSV/JSON exporter 和 sidecar 业务语义。
- [ ] 实现临时目录、全部成功后原子改名、取消/失败清理和导出历史。
- [ ] 用约定接收 CAD 做 DXF 打开、叠加、测量和回读；记录 profile 与黄金输出。

### 5.3 主要交付文件

```text
src/LeatherNesting.Application/{ManualPlacement,Validation,Export}/
src/LeatherNesting.Infrastructure/Export/
src/LeatherNesting.Desktop/{Views,ViewModels}/{NestingWorkspace,ResultReview,ExportWizard}/
tests/LeatherNesting.{Application,Infrastructure,Desktop,EndToEnd}.Tests/
fixtures/golden/exports/
docs/export-profiles/
docs/acceptance/stage-5.md
```

### 5.4 固定测试用例

| ID | 类型 | 用例与期望 |
|---|---|---|
| P5-MAN-001 | 正常 | 拖动/旋转/镜像/贴边后合法落点提交，指标同步更新 |
| P5-MAN-002 | 校验 | 重叠、越界、gap、禁角度/镜像时落点拒绝并解释规则 |
| P5-MAN-003 | 锁定 | 局部重排不移动锁定件；排剩余只消费 unplaced |
| P5-MAN-004 | 撤销 | 单次/多选操作的撤销重做恢复 placement、指标和选择状态 |
| P5-VAL-001 | 一致性 | 自动、手工和导出前对同一非法 placement 返回相同问题代码 |
| P5-EXP-001 | 正常 | DXF/PNG/PDF/CSV/JSON 全部生成，订单数量和 ResultSummary 指标一致 |
| P5-EXP-002 | 往返 | DXF 导出→重载后环、位置、角度、图层、单位和最大误差满足 profile |
| P5-EXP-003 | 失败/取消 | 不可写目录、磁盘不足、用户取消时无最终半文件，临时文件被清理 |
| P5-EXP-004 | 路径 | 中文、空格和长路径按正式平台规则成功或给明确限制 |
| P5-EXP-005 | 阻断 | 未放件、开放轮廓、orphan feature、数量不符时禁止生产导出 |
| P5-UI-001 | UI | 高缩放下命中准确；颜色外有文本；键盘完成校验和导出 |
| P5-E2E-001 | 全链路 | 导入→修复→码齿→排样→微调→导出→重载通过黄金验收 |

### 5.5 阶段命令

```bash
dotnet test LeatherNesting.sln -c Release --filter "Stage=5"
dotnet test tests/LeatherNesting.EndToEnd.Tests -c Release --filter "TestId=P5-E2E-001"
```

### 5.6 人工验收与阶段门

- [ ] 业务人员能优化自动结果并理解每个阻断错误。
- [ ] DXF 在约定 CAD 中打开、叠加和测量通过；不能只用本软件回读自证。
- [ ] PDF/CSV/JSON 的数量和利用率逐项一致。
- [ ] 一次失败导出和一次用户取消均验证目录无残留最终文件。

**回退点**：ExportProfile 带版本；回退时保留项目和历史结果，禁用未知 profile，不覆盖已有导出文件。

---

## 阶段 6：双平台兼容、安装与内部 MVP

### 6.1 目标和独立成果

在 macOS 与正式支持 Windows 环境提供可安装、自包含、可诊断的内部 MVP；在不支持系统上给出可执行升级指引；完成一台非生产工厂 Windows 试点机的全链路演练。

### 6.2 实施清单

- [ ] 生成并锁定 `osx-arm64`、`osx-x64`、`win-x64` 自包含发布；依赖和许可证清单随构建归档。
- [ ] Windows 安装器检测 OS、架构、磁盘空间和安装权限；不支持时显示原因和离线升级指南路径。
- [ ] macOS 完成 x64/arm64 签名/公证 dry-run；最终证书在 Stage 7 配置。
- [ ] 实现 renderer 诊断、安全图形模式和软件渲染回退；几何结果不因渲染路径改变。
- [ ] 优化 1366×768、DPI、字体、中文输入/路径、低内存和慢磁盘体验。
- [ ] 建立 Windows 11、LTSC 2019、LTSC 2021、Home/Pro 22H2 尽力兼容以及 macOS 14/15/26 测试矩阵。
- [ ] 建立可打印设备盘点表、冒烟脚本、诊断包、安装/卸载/升级/回退手册。
- [ ] 在非生产试点机按 `windows-upgrade-guide.md` 演练备份、升级、驱动恢复和业务验收。
- [ ] 做 8 小时 soak、异常关闭恢复、100 个中等裁片基准和安装包病毒误报检查。

### 6.3 主要交付文件

```text
packaging/windows/
packaging/macos/
scripts/publish.ps1
scripts/publish.sh
scripts/smoke-test.ps1
scripts/smoke-test.sh
docs/compatibility/{matrix,inventory,smoke-results}.md
docs/operations/{install,diagnostics,rollback}.md
artifacts/internal-mvp/<version>/
docs/acceptance/stage-6.md
```

### 6.4 固定测试用例

| ID | 类型 | 用例与期望 |
|---|---|---|
| P6-PKG-001 | 发布 | 三个 RID 自包含构建；干净机器无需预装 .NET 即可启动 |
| P6-WIN-001 | 正式平台 | Windows 11 x64 安装→导入→修复→排样→微调→导出→卸载通过 |
| P6-WIN-002 | 正式平台 | Windows 10 Enterprise/IoT LTSC 2019 x64 完整冒烟通过 |
| P6-WIN-003 | 正式平台 | Windows 10 Enterprise/IoT LTSC 2021 x64 完整冒烟通过 |
| P6-WIN-004 | 尽力兼容 | Windows 10 Home/Pro 22H2 冒烟并显示支持等级，不承诺长期维护 |
| P6-WIN-005 | 不支持 | Win7/8.1/XP/Vista 安装器/启动器拒绝且提供清晰升级/替换步骤，不崩溃 |
| P6-MAC-001 | 平台 | macOS arm64 完整冒烟、项目文件与 Windows 互换 |
| P6-MAC-002 | 平台 | macOS x64 完整冒烟、项目文件与 arm64/Windows 互换 |
| P6-GFX-001 | 回退 | 模拟硬件渲染失败后安全图形模式可启动，几何/导出哈希指标不变 |
| P6-DPI-001 | UI | 1366×768 与 100/125/150% 缩放完成核心工作流，无阻断控件不可达 |
| P6-LOC-001 | 本地化 | 中文用户名、路径、项目/裁片名、IME 输入和报告字体正确 |
| P6-UPG-001 | 升级 | 旧 schema/旧安装升级前备份，迁移成功；失败可恢复旧程序和原项目 |
| P6-SOAK-001 | 稳定性 | 8 小时循环导入/排样/保存无未处理异常，内存增长在批准阈值内 |
| P6-PERF-001 | 老机 | 指定低配基准机满足 UI/导入预算，停止/取消可在约定时间内响应 |

### 6.5 阶段命令

```bash
dotnet test LeatherNesting.sln -c Release --filter "Stage=6"
./scripts/publish.sh --version <version> --rids osx-arm64,osx-x64
powershell -File scripts/publish.ps1 -Version <version> -Rids win-x64
powershell -File scripts/smoke-test.ps1 -Manifest fixtures/compatibility-smoke.json
```

平台 E2E 命令必须在对应平台执行并归档机器清单和日志；不能用 Mac headless 测试代替 Windows 安装验收。

### 6.6 人工验收与阶段门

- [ ] 一台 Mac 与 Windows 11、LTSC 2019、LTSC 2021 各有带版本/硬件信息的通过记录。
- [ ] 同一项目在 Mac/Windows 互开，规则版本、结果和导出指标一致。
- [ ] 在低配或老显卡试点机演示安全图形模式。
- [ ] 在不支持 Windows 环境看到升级指南，而不是运行时崩溃。
- [ ] 在一台非生产设备演练 Windows 升级、驱动恢复和业务验收；生产机尚不批量升级。
- [ ] 内部用户完成至少 3 个真实订单样本的可用性验收。

**阶段 6 通过即形成双平台内部 MVP。**

**回退点**：保留上一内部安装包、项目 schema 备份和发布哈希；卸载默认不删项目；失败升级恢复旧二进制但不对新项目做破坏性降级。

---

## 阶段 7：商业授权、签名发布与试点 Beta

### 7.1 目标和独立成果

提供可销售测试的签名桌面版本：首次在线激活、硬件容错绑定、周期刷新、离线宽限和分级限制；具备安装升级、审计、支持诊断和指定工厂 beta 回退能力。

### 7.2 实施清单

- [ ] 实现 `LeatherNesting.Licensing`：许可证 claim、Ed25519 验签、硬件多因子容错、状态机和时钟回拨策略。
- [ ] 实现 ASP.NET Core 10 ActivationServer：激活码、设备席位、签发、刷新、撤销、幂等、限流和审计。
- [ ] 私钥只部署到服务器密钥管理；仓库、客户端、日志、CI artifact 不得包含私钥。
- [ ] 实现首次激活、后台刷新、网络失败、60 天限制、180 天重新激活及恢复。
- [ ] 明确只读限制：可打开/查看/导出诊断；不能新建、修改、排样或生产导出。
- [ ] 完成 Windows 代码签名和 macOS 签名/公证；生成 SBOM、第三方许可证和构建 provenance。
- [ ] 建立版本检查和受控更新：下载/校验/安装前备份/失败回退；不在排样运行时强制更新。
- [ ] 实现支持诊断包、激活审计查询和不含客户几何的基础遥测；遥测默认关闭或显式同意。
- [ ] 实现 FilePackage 生产交接 profile；切割机直连仍不进入本阶段。
- [ ] 选择 1 家工厂、1–3 台非关键生产电脑开展 beta，定义问题分级、暂停条件和每日回退窗口。

### 7.3 主要交付文件

```text
src/LeatherNesting.Licensing/
src/LeatherNesting.ActivationServer/
tests/LeatherNesting.Licensing.Tests/
tests/LeatherNesting.ActivationServer.Tests/
packaging/signing/
docs/security/{threat-model,key-rotation,incident-response}.md
docs/licensing/{operations,offline-grace,support}.md
docs/beta/{site-plan,acceptance,rollback}.md
artifacts/commercial-beta/<version>/
docs/acceptance/stage-7.md
```

### 7.4 固定测试用例

| ID | 类型 | 用例与期望 |
|---|---|---|
| P7-LIC-001 | 正常 | 有效激活码首次联网签发，本机验签后离线重启仍为 Active |
| P7-LIC-002 | 席位 | 同码超出设备数被拒绝或消耗明确席位，重复请求幂等 |
| P7-LIC-003 | 容错 | 网卡/非关键硬件变化不误伤；超过硬件阈值要求重新激活并提供支持码 |
| P7-LIC-004 | 宽限 | 刷新失败进入 OfflineGrace；60/180 天边界和功能权限准确 |
| P7-LIC-005 | 恢复 | 宽限或限制状态联网刷新成功后恢复，不损坏项目和已保存结果 |
| P7-LIC-006 | 时间 | 时区/DST 不误报；明显回拨触发可恢复流程并产生审计，不删除数据 |
| P7-SEC-001 | 签名 | 篡改许可证、响应或安装包均被拒绝；客户端不存在私钥 |
| P7-SEC-002 | API | 激活/刷新限流、输入校验、幂等、撤销和审计测试通过 |
| P7-PKG-001 | 发布 | Windows 签名验证、macOS Gatekeeper/公证验证、SBOM 和许可证清单齐全 |
| P7-UPD-001 | 更新 | 正常升级保留项目/许可证；损坏包、断网、安装失败自动回退 |
| P7-OFF-001 | 离线 | 工厂断网期间核心功能在宽限内完整可用，无后台阻塞或长时间卡顿 |
| P7-BETA-001 | 现场 | 指定工厂 3 个真实订单从导入到文件交接通过，并可在回退窗口恢复旧版 |
| P7-PRV-001 | 隐私 | 默认日志/诊断不含完整客户几何、激活码、原始硬件序列号或私钥 |

### 7.5 阶段命令

```bash
dotnet test LeatherNesting.sln -c Release --filter "Stage=7"
dotnet test tests/LeatherNesting.ActivationServer.Tests -c Release
./scripts/build-commercial-release.sh --version <version> --signed
./scripts/verify-release.sh artifacts/commercial-beta/<version>
```

密钥相关测试使用临时测试密钥和隔离环境；正式私钥不进入本地测试命令。

### 7.6 人工验收与阶段门

- [ ] 新装、激活、断网、60/180 天模拟、恢复和设备变化完成演练。
- [ ] Windows/macOS 安装包签名、公证、SBOM 和哈希由发布负责人复核。
- [ ] 激活服务器备份、密钥轮换、撤销和故障回退演练通过。
- [ ] 指定工厂 beta 完成至少 3 个真实订单，未出现 P0/P1 数据损坏或不可回退问题。
- [ ] 生产交接仅使用已批准 ExportProfile；没有暗含未验证的设备直连。

**阶段 7 通过即形成商业测试版本，不等于成熟全功能竞品替代。**

**回退点**：桌面版本、授权服务和功能开关可独立回退；已签许可证在兼容期内继续验签；服务器回退不撤销合法离线许可证。发生数据损坏、密钥泄露或不可恢复激活故障时立即暂停 beta。

---

## 8. 跨阶段依赖和并行边界

```text
Stage 1 项目/DXF
   ↓
Stage 2 CAD/工艺
   ↓
Stage 3 订单/规则/材料
   ↓
Stage 4 自动排样
   ↓
Stage 5 微调/导出
   ↓
Stage 6 平台 MVP
   ↓
Stage 7 商业发布
```

- 同一阶段内，纯领域/几何、UI、测试夹具和平台脚本可并行，但接口必须先冻结。
- Stage 7 的服务器基础可在 Stage 4 后做隔离 spike，但不能提前改变桌面阶段门。
- 现场银象验证可从 Stage 1 持续进行；新证据通过 PRD/design 变更审查进入后续阶段。
- 切割机、视觉和真皮任务不得借“顺便预留”扩大本任务范围。

## 9. 风险触发器

| 触发器 | 处理 |
|---|---|
| DXF 候选库不能处理真实样本 | 停留 Stage 1，替换/组合 adapter；不绕过诊断 |
| offset/碰撞精度无法达到真实切割公差 | 停留 Stage 2/4，建立曲线感知算法或缩小支持实体范围并重新批准 |
| 银象码齿语义仍无证据 | 交付通用可配置库，生产模板标记未验证；不猜公式 |
| 利用率达不到固定基线 90% | 停留 Stage 4，优化算法或由用户修改可测基线；不只调报告数字 |
| LTSC 2019 图形/依赖不兼容 | 优先软件渲染/依赖替换；仍失败则提交兼容边界变更，不维护 Win7 式旧客户端 |
| Windows 升级影响外设 | 停止批量升级，恢复试点机，补齐驱动/供应商方案 |
| 激活影响离线生产 | 停止商业 beta，回退到不阻断核心工作的上一合法策略并审计 |
| 设备直连需求提前进入 | 新建独立 Trellis 任务，要求协议、硬件和非生产试切环境 |

## 10. 最终完成定义

只有同时满足以下条件，整个 Trellis 任务才可归档：

- [ ] 7 个阶段验收记录全部存在，固定测试 ID 无跳过且结果已归档。
- [ ] PRD 九个 UI 模块均有实现、测试和用户可见失败路径。
- [ ] Windows 11、LTSC 2019、LTSC 2021、macOS x64/arm64 的商业构建完成全链路验证。
- [ ] Windows 不支持版本提供清晰升级/替换指引，升级指南在非生产机演练。
- [ ] DXF 黄金样本经外部 CAD 打开/叠加验证，不依赖自读自证。
- [ ] 自动结果无重叠/越界，利用率达到批准基线；Stop/Cancel 合同通过并发测试。
- [ ] 项目、规则、算法和 ExportProfile 可版本化复现；升级/崩溃/取消不损坏数据。
- [ ] 商业授权、签名、公证、SBOM、隐私和回退演练通过。
- [ ] 未确认竞品语义仍明确标记，没有被伪装为已完成；超范围能力已有独立任务建议。

## 11. Trellis 启动和逐阶段执行

最终审阅批准后，由用户或执行代理运行：

```bash
python3 ./.trellis/scripts/task.py validate 08-07-leather-nesting-windows-clone
python3 ./.trellis/scripts/task.py start 08-07-leather-nesting-windows-clone
```

启动后先只执行“阶段 1”。每个阶段完成时更新 `docs/acceptance/stage-N.md`，运行该阶段过滤测试和通用检查，经过用户验收后再进入下一阶段。不要在规划阶段提前运行 `task.py start`。
