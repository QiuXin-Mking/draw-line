# 跨平台皮革排样软件技术设计

> 状态：已批准，供 `implement.md` 执行  
> 批准日期：2026-08-10  
> 目标：在 macOS 与工厂 Windows 环境交付可验证、可逐阶段回退的桌面排样软件。

## 1. 已批准决策

| 决策 | 结论 |
|---|---|
| 交付方式 | 工作流优先的 7 个纵向阶段，每阶段有可运行成果和独立测试门 |
| 桌面技术 | .NET 10 LTS + Avalonia 12，MVVM，使用编译绑定 |
| 排样控制 | “停止”安全结束并保留当前最佳完整方案；“取消”丢弃临时结果并恢复运行前快照 |
| 竞品边界 | clean-room 功能对标，不复制品牌、素材、视觉排布、文案、源代码或未知算法 |
| MVP 材料 | 矩形片材与卷材；不规则真皮、缺陷/质量区、视觉与切割机直连后置 |
| 生产交接 | MVP 输出可验证文件包，不假设银象设备协议 |
| Windows | 正式支持仍在微软支持周期内并经发布矩阵验证的 Windows 11 x64、Windows 10 Enterprise/IoT LTSC 2019/2021 x64；Home/Pro 22H2 尽力兼容；Windows 7/8.1/XP/Vista 不支持 |

选择 .NET 10 的依据是当前官方支持矩阵和 Avalonia 12 的推荐目标。实现时锁定具体 SDK/包补丁版本，升级补丁必须经过全量回归。

## 2. 质量目标

1. **可复现**：同一项目锁定输入指纹、规则版本和随机种子，可重现同一基线结果。
2. **不损坏数据**：原始 DXF 永久保留；导入修复、CAD 编辑、排样和导出均使用事务式提交。
3. **单一验证规则**：自动排样、手工编辑和导出前检查共用同一个验证器。
4. **UI 始终可响应**：几何分析和排样在后台任务运行，支持进度、停止、取消和故障恢复。
5. **老机器可用**：1366×768、100%/125%/150% 缩放；无硬件加速时可切换软件渲染；普通任务不依赖网络。
6. **故障可解释**：错误包含问题对象、原因、建议操作和诊断 ID，不以无提示崩溃或静默忽略结束。
7. **跨平台核心一致**：领域、几何、排样和项目文件不包含操作系统分支；平台差异封装在适配器中。

## 3. 系统边界

### 3.1 MVP 内部

- 项目/订单、尺码和数量管理。
- DXF 导入、诊断、规范化和可审计修复。
- 外环、孔、节点、剪断、offset、普通剪口和码齿规则。
- 矩形片材/卷材、间距、边距、角度、镜像和纹向约束。
- 自动排样、人工微调、统一校验和结果对比。
- 项目文件、DXF、PNG、PDF、CSV/JSON 导出。
- macOS/Windows 自包含发布、诊断和商业授权。

### 3.2 MVP 外部

- 相机、投影校准、皮张扫描、缺陷/质量区。
- 刀具、运动控制、送料、切割队列、报警和补切。
- ERP/MES/PDM、云排样、移动端和 Linux。
- 未经接收方验证的银象原生文件或设备通信协议。

这些能力以后通过适配器和独立 Trellis 任务接入，不在桌面核心中预留无功能按钮。

## 4. 总体架构

采用模块化单体。桌面程序本地完成核心业务；只有激活需要服务器。

```text
LeatherNesting.Desktop (Avalonia Views / ViewModels / platform adapters)
                    │
                    ▼
LeatherNesting.Application (use cases, commands, transactions, job control)
          │                    │
          ▼                    ▼
LeatherNesting.Domain     LeatherNesting.Geometry
(orders/rules/projects)   (curves/topology/offset/collision/nesting)
          ▲                    ▲
          └──────────┬─────────┘
                     │ interfaces
                     ▼
LeatherNesting.Infrastructure (DXF/project/export/logging/settings)
LeatherNesting.Licensing      (signed license and machine binding)
LeatherNesting.ActivationServer (ASP.NET Core, Stage 7 only)
```

### 4.1 解决方案目录

```text
LeatherNesting.sln
Directory.Build.props
Directory.Packages.props
global.json
src/
  LeatherNesting.Domain/
  LeatherNesting.Geometry/
  LeatherNesting.Application/
  LeatherNesting.Infrastructure/
  LeatherNesting.Desktop/
  LeatherNesting.Licensing/
  LeatherNesting.ActivationServer/
tests/
  LeatherNesting.Domain.Tests/
  LeatherNesting.Geometry.Tests/
  LeatherNesting.Application.Tests/
  LeatherNesting.Infrastructure.Tests/
  LeatherNesting.Desktop.Tests/
  LeatherNesting.EndToEnd.Tests/
  LeatherNesting.Compatibility.Tests/
fixtures/
  dxf/
  projects/
  golden/
  benchmarks/
docs/
  compatibility/
  operations/
```

### 4.2 依赖规则

- Domain 不引用 Avalonia、DXF 库、文件系统、网络或操作系统 API。
- Geometry 只依赖 Domain 的稳定标识和值对象；不得引用 ViewModel。
- Application 通过接口调用项目存储、DXF、导出、授权、时钟和后台任务。
- Desktop 只编排用例和展示状态，不重新实现几何/业务校验。
- Infrastructure 实现适配器；第三方库类型不得越过其边界。
- ActivationServer 不引用 Desktop；客户端只持公钥，私钥只在服务器。
- 依赖方向由架构测试强制验证。

## 5. 技术选择与适配门

| 能力 | 选择 | 约束 |
|---|---|---|
| Runtime | .NET 10 LTS | `global.json` 固定 SDK；自包含发布 |
| UI | Avalonia 12 | 编译绑定；MVVM；不在 code-behind 放业务逻辑 |
| 测试 | xUnit + Avalonia.Headless | 单元、性质、黄金文件、UI 状态机、平台 E2E |
| 多边形布尔/offset | Clipper2 候选 | 通过 adapter；量化缩放和公差集中管理 |
| 拓扑校验 | NetTopologySuite 候选 | 不与 Clipper2 各自定义不同容差 |
| DXF | `IDxfReader/IDxfWriter` 适配层 | Stage 1 用真实样本比较候选库；不将领域永久绑定到已归档的 netDxf |
| 项目格式 | 版本化 ZIP 容器 + JSON 清单 | 原子保存、迁移、输入嵌入和校验和 |
| 日志 | 结构化本地日志 | 默认不记录客户几何；支持导出脱敏诊断包 |
| PDF/图片 | 通过 exporter adapter | 字体随包或显式回退，保证中文报告 |

### 5.1 DXF 库选型门

Stage 1 必须用 `凉鞋.dxf` 与 38–45.DXF 做候选库 spike，至少验证：

- ASCII DXF 版本、闭合 `LWPOLYLINE`、旧式 `POLYLINE`、LINE、ARC、bulge、SPLINE、TEXT、图层和单位。
- 导入→领域模型→导出→重载后的实体数、包围盒、面积和最大误差。
- .NET 10、Windows x64、macOS x64/arm64 的构建与运行。
- 许可证、维护状态、漏洞和二进制体积。

未通过时只替换 Infrastructure 适配器，不修改领域和 UI 合同。

## 6. 领域模型

### 6.1 几何

```text
Curve2D = LineSegment | CircularArc | Polyline | SplineReference
Loop2D  = stableId + role(Outer|Hole) + normalizedWinding + curves
PatternPiece = stableId + revision + outer + holes + internalCurves + features
Transform2D = translation + rotation + mirror
ToleranceProfile = importSnap + flattenChord + topology + export
```

规则：

- 业务内部统一毫米；原始单位和换算记录保存在 ImportReport。
- 不以颜色判断语义；使用 role、feature type 和 layer mapping。
- 所有几何值拒绝 NaN/Infinity；尺码键使用字符串或 decimal，不使用 double。
- 每个派生轮廓保存来源 ID、操作参数和版本。
- winding 在领域入口规范化，但“材料内/外”由拓扑决定，不能由符号猜测。

### 6.2 工艺特征

```text
NotchFeature
  contourId, anchorArcLength, shape, width, depth,
  materialSide, outputMode(Cut|Mark), layerOrTool

SizeToothRuleSet
  id, version, sizeSystem, orderedEntries[], createdAt

SizeToothEntry
  exactSizeKey, orderedTokens[], width, depth, spacing,
  halfSizeSpacing, anchorRule, materialSide
```

普通剪口和码齿是两个模型。编辑轮廓后通过弧长和局部几何重映射锚点；无法唯一映射时 feature 变为 `Orphaned`，必须人工修复才能导出。

### 6.3 订单、材料与排样

```text
Order = orderNo, customer, styleNo, dueDate, priority, lines[], revision
OrderLine = pieceRevisionId, sizeKey, leftQty, rightQty, materialId, constraints
Material = Sheet(width,height) | Roll(width,maxLength)
PieceConstraint = angles, mirrorAllowed, grainDirection, groupId, locked
NestingRequest = immutable snapshot + seed + qualityPreset + limits
NestingResult = placements + unplaced + metrics + diagnostics + versions
```

数量来自 OrderLine；排样器只消费不可变请求。结果不能反向改变订单，只能生成“已放/未放”派生统计。

### 6.4 项目聚合

`ProjectDocument` 是事务边界，包含：

- 项目元数据和 schema 版本。
- 原始 DXF 的嵌入副本/哈希及 ImportReport。
- 裁片 revisions 和 CAD 操作日志。
- 订单、材料、规则库版本快照。
- 已接受的排样结果和导出历史。
- 当前 dirty revision、最后成功保存 revision。

保存流程为：写临时文件 → 校验清单和哈希 → fsync/关闭 → 原子替换。崩溃恢复只加载最后完整版本和自动恢复日志。

## 7. UI 信息架构

主窗口采用工作流导航，不复刻竞品密集工具栏：

```text
顶部：项目名 / 保存状态 / 撤销重做 / 全局诊断 / 帮助
左侧：1 项目  2 导入  3 裁片  4 工艺  5 材料  6 排样  7 结果
中央：当前工作台画布/表格
右侧：当前对象属性、问题和操作
底部：坐标、缩放、选择数、校验状态、后台任务状态
```

在 1366×768 下右侧面板可折叠；主动作始终可见。颜色之外同时提供图标、文字和问题编号。

### 7.1 U1 项目/订单中心

- ViewModel：`ProjectCenterViewModel`、`OrderEditorViewModel`。
- 命令：New/Open/Save/SaveAs/Recover/Clone、订单增删改。
- 禁用规则：没有可写项目时禁用保存；有脏数据关闭时必须保存/放弃/取消。
- 错误：损坏、schema 太新、迁移失败、目录不可写、外部修改冲突。

### 7.2 U2 CAD 导入向导

- ViewModel：`ImportWizardViewModel`，步骤不可跳过。
- 状态：`SelectFile → UnitReview → Recognition → RepairDecision → Committed`。
- 识别报告按问题定位对象；阻断错误和警告分级。
- Cancel 丢弃本次 session，不改变项目。

### 7.3 U3 裁片/尺码明细

- ViewModel：`PieceCatalogViewModel`、`OrderLinesViewModel`。
- 缩略图提供名称、尺码、左右需求、已放/未放和有效性。
- 批量编辑必须先显示影响件数；所有约束变更使旧排样结果标记 `Stale`。

### 7.4 U4 CAD 修复和工艺

单一画布，工具模式互斥：Select、BoundaryRepair、Offset、NodeEdit、Break、Notch。

关键状态机：

```text
Boundary: SelectEdges → PickInside/Candidate → Analyze → Preview → Commit|Cancel
Offset: SelectLoop → Parameters → Preview → TopologyWarning? → Commit|Cancel
Node: SelectLoop → EditGesture → Validate → CommitCommand|Reject
Break: PickPoint(s) → PreviewSegments → RemapFeatures → Commit|Cancel
Notch: PickAnchor → Shape/Side/Size → Preview → Validate → Commit|Cancel
```

- 预览不修改 ProjectDocument。
- Commit 形成一个 Undo command；Cancel 不进入历史。
- 轮廓生成不得使用凸包代替真实边界。
- offset 的 1→0、1→N 必须单独确认。

### 7.5 U5 码齿库

- ViewModel：`SizeToothLibraryViewModel`、`SizeToothApplyViewModel`。
- 左侧版本，中间精确尺码行，右侧有序 token 和多尺码叠加预览。
- 应用流程锁定库版本；库编辑通过 copy-on-write 创建新版本。
- 未确认的银象默认值不预填为“行业标准”，只在实验模板中显示来源和未验证标记。

### 7.6 U6 材料和排样设置

- ViewModel：`MaterialEditorViewModel`、`NestingSetupViewModel`。
- 普通设置：材料尺寸、边距、间距、角度、镜像、纹向、质量档。
- 高级设置：时间上限、固定 seed、是否保留锁定件；算法内部参数默认隐藏。
- 请求提交后输入快照只读，修改需先取消当前任务。

### 7.7 U7 排样运行控制

```text
Ready ─Start→ Running ─finished→ Completed
                    ├─Stop→ Stopping → StoppedWithBest
                    ├─Cancel→ Cancelling → Cancelled(previous snapshot restored)
                    └─error→ Failed(previous accepted result retained)
```

- `Stop` 只接受已完成验证的最佳候选，不接受正在写入的半成品。
- `Cancel` 幂等；连续点击不产生多个恢复操作。
- 关闭窗口时若 Running，必须选择停止并保存、取消并退出或返回。
- UI 每 100–250ms 节流更新，不为每个算法迭代重绘。

### 7.8 U8 人工微调

- ViewModel：`NestingWorkspaceViewModel`。
- 选择/框选、拖动、定角旋转、镜像、锁定、成组、贴边、撤销/重做、排剩余、局部重排。
- 拖动期间允许轻量预览，落点时运行权威验证；非法落点恢复并解释规则。
- 一次拖拽/批量操作是一条 Undo command。

### 7.9 U9 结果和导出

- ViewModel：`ResultReviewViewModel`、`ExportWizardViewModel`。
- 先运行阻断校验，再显示指标、未放件、输入/规则/算法版本。
- 输出项目、DXF、PNG、PDF、CSV/JSON；每种输出由独立 exporter 实现。
- 导出到同目录临时文件，全部成功后原子改名；失败或取消删除临时文件。

## 8. 几何处理管线

### 8.1 导入与规范化

```text
DXF entities
 → preserve source metadata
 → unit decision
 → curve normalization
 → endpoint index / intersection split
 → planar graph
 → candidate face loops
 → containment tree (outer/hole)
 → semantic feature recognition
 → validation + ImportReport
```

自动行为仅限确定且可逆的规范化。连接间隙、多个候选闭环和可能改变拓扑的动作必须预览。

### 8.2 统一公差

`ToleranceProfile` 是项目级版本化对象，至少包含：

- `ImportSnapToleranceMm`
- `TopologyToleranceMm`
- `FlattenChordToleranceMm`
- `CollisionToleranceMm`
- `ExportRoundTripToleranceMm`

不得在 UI、DXF adapter、Clipper2 adapter 和测试中分别写魔法数。整数几何缩放必须检查溢出。

### 8.3 offset

- UI 接收正距离和材料方向，Application 转换为拓扑操作。
- 输入先检查 simple/closed；相交输入阻断，不静默 union。
- 外环/孔洞整体 offset，保持 containment；结果可能为空或拆分。
- 第一版允许离散化，报告最大误差；若真实切割不满足公差，再增加曲线感知 offset/arc refit。

### 8.4 碰撞和有效性

同一个 `PlacementValidator` 返回结构化问题：Overlap、OutOfBounds、GapViolation、AngleViolation、MirrorViolation、QuantityMismatch、OpenContour、OrphanedFeature。

验证器服务于：

- 排样候选接受。
- 人工拖放落点。
- 项目健康检查。
- 导出阻断检查。

## 9. 排样引擎

### 9.1 接口

```text
INestingEngine.RunAsync(
  NestingRequest request,
  IProgress<NestingProgress> progress,
  CancellationToken cancel,
  StopSignal stop)
  -> NestingRunOutcome
```

`NestingRunOutcome` 明确区分 Completed、StoppedWithBest、Cancelled、NoFeasibleSolution、Failed。

### 9.2 路线

1. 先把 Python Demo 的确定性货架算法移植为可重复基线。
2. 加入 0/90/180、镜像、gap/margin 和约束验证。
3. 用 NFP/候选位置减少空隙。
4. 在相同验证器之上增加启发式搜索；始终保留最后一个已验证最佳候选。

算法升级不得改变项目格式和 UI 状态机，只增加 `algorithmId/version`。验收比较必须固定输入、约束、时间预算、硬件和 seed。

### 9.3 性能预算

- 普通 UI 操作反馈目标 <100ms；超过 250ms 的工作进入后台。
- 100 个中等复杂裁片的导入/诊断目标 <5s（基准机另行记录）。
- 画布平移/缩放目标 30 FPS；低配/软件渲染最低可用目标 15 FPS。
- 排样无硬性统一秒数；使用质量档和时间预算，任何时刻可停止。
- 内存压力超过安全阈值时停止生成新候选，保留最佳结果并报告。

性能数值是工程预算，不是竞品营销承诺；Stage 1 建立实际基准机后固化。

## 10. 并发、事务和恢复

- 每个后台任务拥有 immutable input snapshot、job ID 和 CancellationToken。
- 进度消息带 job ID；旧任务消息不得更新新任务 UI。
- StopSignal 与 CancellationToken 分离：Stop 要结果，Cancel 不要结果。
- 所有项目修改在 UI 线程通过 command transaction 提交；后台线程不得直接改 ObservableCollection。
- 自动恢复日志只记录已提交命令，不记录预览帧。
- 应用崩溃后提示恢复；恢复副本绝不覆盖原项目，直到用户确认保存。

## 11. 导出与生产交接

### 11.1 DXF

- ExportProfile 明确 DXF 版本、单位、图层映射、曲线策略、文字和精度。
- 标准几何保证接收方可读；业务语义同时写入 sidecar JSON，受控环境可选 XDATA。
- 每个目标 CAD/切割软件建立命名 profile 和黄金样本，不能只声称“兼容 AutoCAD”。

### 11.2 报告

报告包含订单、材料、数量、利用率、未放件、耗时、输入哈希、规则版本、算法版本和导出时间。PDF、CSV 和 JSON 的指标来自同一个 ResultSummary。

### 11.3 未来设备适配器

设备直连使用 `IProductionHandoffAdapter`：Preflight、Package、Submit、QueryStatus、Cancel。MVP 只实现 FilePackage adapter；网络/串口/厂商 SDK 另行授权和验收。

## 12. 平台兼容和发布

### 12.1 发布矩阵

| 平台 | 等级 | 架构/说明 |
|---|---|---|
| Windows 11（微软仍支持且发布矩阵已验证的版本） | 正式支持 | win-x64，自包含；记录实际版本号 |
| Windows 10 Enterprise/IoT LTSC 2019 | 正式支持 | win-x64，真实机或 VM 全链路验收 |
| Windows 10 Enterprise/IoT LTSC 2021 | 正式支持 | win-x64，真实机或 VM 全链路验收 |
| Windows 10 Home/Pro 22H2 | 尽力兼容 | win-x64，明确 OS 已结束免费支持 |
| Windows 7/8.1/XP/Vista | 不支持 | 启动器/安装器解释原因并指向离线升级指南 |
| macOS 14/15/26 | 正式支持 | osx-x64、osx-arm64；与 .NET 10 官方矩阵同步 |

Windows x86 不默认发布；只有现场清单证明价值后另立评审。不得绕过 Windows 11 TPM/CPU/Secure Boot 条件。

### 12.2 图形回退

- 默认使用 Avalonia/Skia 正常渲染。
- 启动失败或诊断发现不兼容驱动时允许受控软件渲染配置。
- “安全图形模式”只降低抗锯齿、阴影和预览刷新频率，不改变几何计算与导出。
- 记录 renderer、驱动、DPI 和显示器信息，但导出诊断包前允许用户检查和脱敏。

### 12.3 安装与升级

- Windows 使用签名安装器，安装前检测版本/架构/磁盘空间；不兼容时不写入半安装状态。
- macOS 提供签名、公证的 arm64/x64 或 universal 分发方案，由体积测试决定。
- 配置、项目和许可证分目录；卸载程序默认不删除客户项目。
- 升级先备份 schema；新版本首次打开旧项目时生成副本并迁移。
- 工厂 OS 升级按 `windows-upgrade-guide.md` 先试点后批量。

## 13. 授权与安全

### 13.1 客户端授权状态

```text
Unactivated → Activating → Active
Active → RefreshDue → Active | OfflineGrace
OfflineGrace → Active | ReadOnlyRestricted → ReactivationRequired
```

- 客户端只保存签名许可证和公钥；本地加密不是信任根。
- 硬件指纹采用多因子和容错阈值，网卡变化不能单独使许可证失效。
- 30 天尝试刷新；60 天后限制新建/排样/导出，仍可查看；180 天后要求重新激活。具体商业周期可配置但要版本化。
- 服务器时间、单调计时和已签名时间证据共同检测明显回拨；不能因夏令时或时区变化误伤。

### 13.2 隐私和密钥

- 硬件标识本地归一化后哈希；服务器只保存必要标识和审计信息。
- Ed25519 私钥只在服务器密钥管理中；日志、仓库和安装包不得包含私钥。
- 激活 API 限流、幂等并有审计；离线加密狗另立供应链和驱动兼容任务。

## 14. 测试架构

| 层 | 目的 | 代表测试 |
|---|---|---|
| Domain unit | 值对象、订单、版本和状态机 | 非法尺码/数量、规则版本、Stop/Cancel outcome |
| Geometry unit/property | 拓扑、offset、碰撞和不变量 | winding 反转、面积、长度、1→N、随机有效多边形 |
| Infrastructure integration | 真文件和 adapter | DXF round-trip、项目原子保存、schema migration、export cleanup |
| Application integration | 完整用例和事务 | 导入→修复→排样→编辑→校验→导出 |
| Avalonia headless | ViewModel、绑定和交互 | 命令启用、向导、错误、Stop/Cancel、DPI 布局 |
| Golden/visual | 防止几何和渲染漂移 | DXF 指标、PNG 关键区域、PDF/CSV/JSON 指标一致 |
| Platform E2E | 安装和平台差异 | LTSC 2019/2021、Win11、macOS x64/arm64 |
| Machine acceptance | 最终工艺能力 | 最小齿、刀缝、公差、图层/刀具映射；设备任务执行 |

测试禁止仅断言“无异常”。黄金文件更新必须附原因、差异摘要和人工批准；不能把当前实现重新生成的结果自动当答案。

## 15. 可观测性

- 日志字段：timestamp、level、eventId、jobId、projectRevision、duration、outcome、diagnosticId。
- 不默认记录完整路径、客户名或几何坐标；必要时使用哈希和计数。
- 诊断包包含版本、平台、渲染器、配置、脱敏日志和用户明确选择的项目报告。
- 排样基准记录算法版本、seed、硬件、时间预算和输入哈希，保证利用率比较可信。

## 16. 推出与回退

每个阶段形成可运行标签和 migration boundary：

1. 新 schema 只能向前添加或提供显式迁移。
2. 阶段未通过时回退代码/安装包，不自动回写旧项目。
3. Stage 6 先在一台非生产 Windows 试点机和一台 Mac 验收。
4. Stage 7 先发内部/指定工厂 beta；授权服务器和桌面功能开关可独立回退。
5. 设备直连永远先 dry-run/preflight，再在非生产材料上试切。

## 17. 仍需现场证据的门

以下不阻塞 Stage 1–2 的通用 CAD 基础，但阻塞相关业务功能的生产签收：

- 码数 19.50、0.0–6.5、31–37 的业务关系。
- 方形/半圆/尖角/半码的有序排列、锚点和方向。
- “常规/使用库/自定义”准确语义。
- `替换反` 完整标签和行为。
- 银象设备接收格式、图层/刀具映射、最小齿/刀缝限制和通信协议。

已批准的 Stop/Cancel 语义不再等待竞品确认；这是本产品自己的可测合同。

## 18. ADR 摘要

### ADR-001：.NET 10 + Avalonia 12

- **采用**：处于 LTS 支持期，Avalonia 12 官方推荐 .NET 10，支持目标 Windows LTSC 和当前 macOS。
- **代价**：必须验证所有 DXF/几何/打包依赖；不能沿用 .NET Framework 时代控件。

### ADR-002：工作流优先而非像素克隆

- **采用**：每阶段交付完整任务流，能早期验证工厂操作和数据语义。
- **代价**：早期截图不会覆盖全部竞品窗口，但不会积累无逻辑空壳。

### ADR-003：Stop 与 Cancel 分离

- **采用**：Stop 保留最后已验证最佳结果；Cancel 回滚。
- **代价**：算法必须维护不可变最佳快照，UI 和测试状态更多。

### ADR-004：DXF adapter 与版本化项目容器

- **采用**：隔离第三方库和不稳定 DXF 变体，同时保存可复现业务语义。
- **代价**：需要维护 adapter 合同、schema migration 和 sidecar 数据。

### ADR-005：统一 PlacementValidator

- **采用**：自动、人工、导出使用相同规则，避免结果不一致。
- **代价**：验证器必须足够快并提供预览/权威两种运行模式，但规则来源相同。
