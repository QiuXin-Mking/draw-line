# 架构全景与治理方向

> 定位：本文件是「皮革划线排样软件」的**架构基线 + 治理路线图**。它回答三个问题：现在是什么架构、为什么这么分、接下来怎么把它管住。
>
> 适用范围：后续所有架构决策、分层违规判断、模块改造，都以本文为基准；如与本文冲突，先改本文（走 ADR），再改代码。

---

## 1. 一句话结论

本程序是一套 **Clean Architecture（六边形 / 端口-适配器）** 分层架构，配 **手工组合根（无 DI 容器）** 与 **MVVM 表现层**。核心价值是「排样（nesting）」，计算几何被刻意抽成独立项目，所有 I/O 收敛在 `Infrastructure`，依赖方向严格单向（内层不认外层）。

**治理判断：架构本身已经干净，重点不在重构，而在「把已存在的边界用工具钉死」和「管住 Desktop 91 个文件的成长」。**

---

## 2. 分层全景

```
┌─────────────────────────────────────────────────────────────────┐
│  Desktop (Avalonia UI + MVVM + 组合根)           91 个源文件      │
│    App / Shell / 12 个 M01~M12 模块 / Views+ViewModels            │
│    依赖：Application + Infrastructure                             │
└──────────────┬──────────────────────────────────────────────────┘
               │ 实现端口
┌──────────────▼──────────────────────────────────────────────────┐
│  Infrastructure (适配器：唯一懂外部格式的地方)   6 个源文件        │
│    AsciiDxfReader/Writer · AsciiNestingDxfWriter · ZipProjectStore│
│    依赖：Domain + Application + Geometry                          │
└──────────────┬──────────────────────────────────────────────────┘
               │ 实现端口
┌──────────────▼──────────────────────────────────────────────────┐
│  Application (用例编排 + 端口定义)               8 个源文件        │
│    ImportDxfUseCase · ExportNestingDxfUseCase · CadCommands        │
│    Ports.cs (IProjectStore/IClock/IFileDialogService)             │
│    依赖：Domain + Geometry                                        │
└──────┬───────────────────────────────┬──────────────────────────┘
       │                               │
┌──────▼───────────────┐      ┌────────▼──────────────────────────┐
│  Geometry (纯计算几何) │      │  Domain (纯实体/值对象)  1 个源文件│
│   30 个源文件          │      │   ProjectDocument · ImportReport   │
│   Loop2D · NFP · 碰撞  │      │   ImportDiagnostic · UnitDecision  │
│   · 修复 · 拓扑 · 排样  │      │   不可变 record + with            │
│   依赖：Domain+Clipper2 │      │   依赖：无                        │
└───────────────────────┘      └───────────────────────────────────┘
```

### 依赖方向表

| 项目 | 源文件数 | 职责 | 允许依赖 | 禁止依赖 |
|---|---|---|---|---|
| `Domain` | 1（内含 4 类型） | 纯实体 / 值对象 / 诊断 | 无 | 一切 |
| `Geometry` | 30 | 计算几何（纯函数） | Domain + Clipper2 | Application / Infrastructure / Desktop |
| `Application` | 8 | 用例编排 + 端口接口 | Domain + Geometry | Infrastructure / Desktop |
| `Infrastructure` | 6 | 适配器（DXF 读写 / 项目存储） | Domain + Application + Geometry | Desktop |
| `Desktop` | 91 | UI + MVVM + 组合根 | Application + Infrastructure | —（叶子层） |

### 测试镜像（`tests/`，与 `src/` 1:1）

| 测试项目 | 源文件数 | 对应 |
|---|---|---|
| `Domain.Tests` | 1 | Domain |
| `Geometry.Tests` | 14 | Geometry |
| `Application.Tests` | 3 | Application |
| `Infrastructure.Tests` | 5 | Infrastructure |
| `Desktop.Tests` | 30 | Desktop |
| `EndToEnd.Tests` | 1 | 跨层集成 |

---

## 3. 各层职责与关键抽象

### 3.1 Domain —— 纯实体，极薄但边界清晰

单文件 `ProjectDocument.cs`，四种类型：

- `UnitDecision`：单位裁决枚举（`Unresolved` / `ConfirmedMillimetres`）。
- `ImportDiagnostic`：导入诊断（Code / Severity / Message / EntityId）。
- `ImportReport`：导入报告（源路径、SHA-256 指纹、单位裁决、诊断列表）。
- `ProjectDocument`：项目聚合根，不可变 record。方法均为纯函数：
  - `CommitImport(report)` → `with` 表达式返回新实例，`Revision+1`、`IsDirty=true`；
  - `MarkSaved()` → `with { IsDirty = false }`。

**特点**：无 I/O、无框架依赖、不可变。业务规则（如「必须确认毫米单位后才能提交」）内聚在 Application 用例层，Domain 只提供不可变状态转换。

### 3.2 Geometry —— 领域计算核心，纯函数

30 个源文件，是真正的「领域服务库」。按子命名空间分块：

| 子目录 | 职责 |
|---|---|
| `Loop2D` / `Curve2D` / `Point2D` / `Transform2D` | 二维曲线 / 点 / 位姿基础模型 |
| `Nesting/` | 排样引擎（`NestEngine`、`NestOptimizer`、`NfpCalculator`、`PlacementCandidateGenerator`、`ClipperCollisionDetector`、`NestModels`） |
| `Repair/` | 几何修复（`ContourCloser`、`GapRepair`、`BoundaryGenerator`） |
| `Topology/` | 拓扑（`PlanarGraph`、`ContainmentTree`、`EndpointIndex`、`FaceCandidate`） |
| `NodeEditing/` | 节点编辑（`NodeOperations`、`BreakOperations`、`FeatureAnchorRemap`） |
| `Offset/` | 偏移（`OffsetAdapter`、`OffsetResult`） |
| `Features/` | 工艺特征（`NotchFeature`、`NotchValidator`） |
| `Intersection/`、`Validation/` | 求交、放置校验（`PlacementValidator`） |
| `ClipperPathAdapter` | 唯一对接第三方库 `Clipper2` 的适配点 |

**特点**：所有运算确定性（排样引擎带固定 `seed`），无副作用，可被大量单元测试覆盖（现有 14 个测试文件）。

### 3.3 Application —— 用例编排 + 端口

8 个源文件：

- **端口（接口）**：`Ports.cs` 里 `IProjectStore` / `IClock` / `IFileDialogService`；`DxfImport.cs` 里 `IDxfReader`；`NestingExport.cs` 里 `INestingDxfWriter`。
- **用例**：`ImportDxfUseCase`（先 `InspectAsync` 检查+算 SHA-256，再经 `ImportDxfPreparation.CommitTo` 提交）、`ExportNestingDxfUseCase`（组装三图层 DXF 文档）。
- **CAD 编辑命令模式**：`CadCommand` / `CadCommandTransaction` / `CadCommands` / `CadOperationSession` / `CrashRecoveryLog` —— 带崩溃恢复日志的命令栈（撤销/重做基础）。

**特点**：用例 = 单方法编排，不碰具体格式；端口抽象了「什么能力需要」，实现在 Infrastructure。

### 3.4 Infrastructure —— 适配器，唯一懂外部格式的地方

6 个源文件：

- `Dxf/AsciiDxfReader.cs`：无依赖的 DXF 解析器，清点实体、识别闭合 `LWPOLYLINE` 与旧式 `POLYLINE`，产出阻塞式诊断（见 ADR-01）。
- `Dxf/AsciiDxfGeometryReader.cs`：把 DXF 实体转成 `Loop2D` 几何。
- `Dxf/AsciiDxfWriter.cs` + `AsciiNestingDxfWriter.cs`：实现 `IDxfWriter` / `INestingDxfWriter`。
- `Projects/ZipProjectStore.cs`：实现 `IProjectStore`，项目文档落盘（zip）。

**特点**：换库只动这一层（ADR-01 拒了 `netDxf`，自研 reader）。Domain / UI 契约不变。

### 3.5 Desktop —— 表现层 + 组合根

91 个源文件，唯一膨胀点，也是治理重点：

- `App.cs` / `Program.cs`：Avalonia 启动。
- `Composition/DesktopComposition.cs`：**唯一组合根**，手工装配（无 DI 容器），把所有用例、适配器、模块工厂接线。
- `Shell/`：五栏工作站外壳（`AppShellView/ViewModel`、`CadWorkspaceHost`、`TopCommandArea` 等）。
- `Modules/`：12 个模块 `M01~M12`，通过 `DesktopModuleDiscovery` 反射发现 `IDesktopModule`，用 `HasRealLogic` 标志区分「有实逻辑」与「占位」。
- `Views/` + `ViewModels/`：MVVM。
- `DesignSystem/`：`AppTheme`、`ToolbarIcon` 等组件规范。
- `Workspace/`：`IWorkspaceSession` / `InMemoryWorkspaceSession`（内存会话）。

---

## 4. 核心原理：排样算法链与数据流

### 4.1 业务边界（ADR-02 固化）

本软件**只负责排样**，不生成切割刀路（刀路由下游切片软件完成）。输出契约：**DXF 与 JSON 两种都要，先 DXF、JSON 暂缓**。位姿范围：**任意角度（自由旋转）**——`NestRequest.AllowedRotationsDegrees` 接收任意角度列表，毛向/纹路等约束在任意角度基础上叠加。

### 4.2 算法链

```
DXF 读入
  AsciiDxfReader：清点实体 → 识别闭合 LWPOLYLINE → 单位裁决门（未确认毫米即阻塞）
        │
        ▼
Geometry 修复：ContourCloser / GapRepair / BoundaryGenerator → 闭合 Loop2D
        │
        ▼
排样引擎 NestEngine
  ├─ PlacementCandidateGenerator   生成任意角度候选位姿（ADR-02）
  ├─ NfpCalculator                NFP = Minkowski 和，求无碰撞贴靠点
  ├─ ClipperCollisionDetector     Clipper2 布尔运算做碰撞 / 间隙校验
  └─ NestOptimizer.Optimize       局部搜索（iterations=50, seed=2026 → 确定性）
        │
        ▼
ExportNestingDxfUseCase 组装三图层 DXF（裁片 / 标注 / 利用率标题）→ AsciiNestingDxfWriter 落盘
```

### 4.3 数据流方向

- **纯计算层**：Domain → Geometry → Application 均为无副作用计算。
- **I/O 只在两端**：读 DXF 进（Infrastructure 入口），写 DXF/JSON 出（Infrastructure 出口）。
- **唯一核心质量指标**：材料利用率（`NestResult.Utilization`）。

---

## 5. 已就位的治理基础设施（保持，勿动）

| 机制 | 位置 | 作用 |
|---|---|---|
| 分层 + 单向依赖 | 五个项目 + 端口-适配器 | 隔离外部格式 / UI 变动 |
| 组合根 | `DesktopComposition.cs` | 唯一装配点，杜绝散落 `new` |
| 模块发现 + `HasRealLogic` | `DesktopModuleDiscovery` / `IDesktopModule` | 12 个面板可渐进填实 |
| 测试镜像 | `tests/` 六个项目 | 每层可独立验证 |
| ADR 目录约定 | `docs/adr/` | 每个「为什么」留痕 |
| Todo 目录约定 | `docs/todo/` | 待办按数字前缀归档 |
| 构建红线 | `Directory.Build.props` | `Nullable`、`TreatWarningsAsErrors`、中央包管理 + lock 文件 + Deterministic |

---

## 6. 治理方向（优先级路线图）

### P0 —— 钉死依赖方向（成本最低，收益最高）

**行动**：新增 `NetArchTest.Rules` 架构守卫测试，断言：

1. `Domain` / `Geometry` 不得引用 `Application` / `Infrastructure` / `Desktop` / `Avalonia`。
2. `Application` 不得引用 `Infrastructure` / `Desktop`。
3. 只有 `Infrastructure` 允许出现 DXF 格式细节类型。
4. `Domain` / `Geometry` / `Application` 不得引用 `Clipper2`（除 `Geometry.ClipperPathAdapter` 一个适配点）。

**收益**：把「干净」从「靠自觉」变成 CI 报错。现有 xunit + `TreatWarningsAsErrors` 基建现成，可挂进 `EndToEnd.Tests` 或新建 `Architecture.Tests`。

### P1 —— 固化两个「非教科书」决定（写 ADR，避免误判违规）

1. **端口放 Application 而非 Domain** → 因此 `Infrastructure → Application` 依赖是**合法的**。不记录的话，将来会有人以为是违规而「纠正」，反而破坏结构。→ 建议 `docs/adr/03-端口位置与几何边界.md`。
2. **Geometry 的定位** → 明确 Domain = 纯实体（薄）、Geometry = 纯计算（厚），禁止在 Domain 里写几何；二者之间不得互相污染。→ 同一 ADR 一并固化。

### P2 —— 管住 Desktop 91 个文件的成长（模块成长规则）

现状：`M01~M12` 里仅少数 `HasRealLogic=true`，其余是占位。风险是占位模块变成复制粘贴泥球。

**规则**：占位模块 → 实逻辑模块的转换，**必须**以「有对应用例（Application）+ 有测试」为前提；无实逻辑的模块不得引入私有业务分支。

### P3 —— 资产与「外研」目录分离（整洁度）

根目录混着二进制（`凉鞋.dxf` / `划线.rar` / `凉鞋.png`）与研竞目录（`01-加密方案` ~ `05-图片`）。

**建议**：
- 二进制样本 → `fixtures/`（已存在，收纳之）或 git-lfs；
- 研竞资料 → 独立仓库或 `docs/research/` 类目录；
- 根目录只留代码与构建文件。

### P4 —— Python 原型独立为展示 Demo（已完成）

早期确定性货架 Demo（`leather_nesting_demo.py`、`render_*.py`、`view_dxf.py`）已独立到 [`python-demo/`](../python-demo/)，定位为**对外展示**（读 DXF → 排样 → 输出 DXF/PNG/利用率），不再作为算法参照演进。一键展示：`cd python-demo && ./run.sh`。根目录 `凉鞋.dxf` 仍保留，供 C# 测试（`RepoFixture.Path`）使用。

### P5 —— 冻结 JSON 契约再动手（进行中）

`docs/todo/01-json输出契约待办.md` 已标字段待定。**坚持**「先与切片软件确认 schema 再实现」，避免实现完才发现字段对不上（ADR-02 已明确此序）。

---

## 7. 治理红线（不可逾越）

1. **依赖方向单向**：内层永不引用外层。新增代码先对照 §2 依赖表自查。
2. **组合根唯一**：业务对象装配只发生在 `DesktopComposition`；View / ViewModel 内禁止 `new` 出用例或适配器。
3. **端口在 Application，实现在 Infrastructure**：外部格式知识不得上渗到 Application / Geometry / Domain。
4. **确定性**：排样引擎保持固定 `seed` 可复现；随机性不得未经声明引入。
5. **模块实逻辑需配套用例 + 测试**：占位模块不得长出私有业务逻辑。

---

## 8. 后续行动清单（可执行）

| # | 行动 | 产出 | 优先级 |
|---|---|---|---|
| 1 | 新建架构守卫测试 | `NetArchTest` 项目 / 测试用例 | P0 |
| 2 | 起草 ADR-03 | `docs/adr/03-端口位置与几何边界.md` | P1 |
| 3 | 制定模块成长规则并写进本文档 §7 | 规范条目 | P2 |
| 4 | 资产目录迁移方案 | `fixtures/` 收纳二进制 | P3 |
| 5 | Python spike 声明 | README / ADR 标注 | P4 |
| 6 | 冻结 JSON 契约 | 与切片软件确认 schema | P5 |

---

## 附录：关键文件索引

| 文件 | 作用 |
|---|---|
| `LeatherNesting.sln` | 解决方案（5 src + 6 tests） |
| `Directory.Build.props` | 构建红线（nullable / warnings-as-errors / deterministic） |
| `Directory.Packages.props` | 中央包版本管理 |
| `src/LeatherNesting.Domain/ProjectDocument.cs` | 领域聚合根（不可变） |
| `src/LeatherNesting.Geometry/Nesting/NestEngine.cs` | 排样引擎入口 |
| `src/LeatherNesting.Geometry/Nesting/NestOptimizer.cs` | 局部搜索优化（确定性） |
| `src/LeatherNesting.Geometry/Nesting/NfpCalculator.cs` | NFP 计算 |
| `src/LeatherNesting.Application/Ports.cs` | 端口接口集 |
| `src/LeatherNesting.Application/DxfImport.cs` | DXF 导入用例（含 SHA-256 指纹门） |
| `src/LeatherNesting.Application/NestingExport.cs` | 排样导出用例 |
| `src/LeatherNesting.Infrastructure/Dxf/AsciiDxfReader.cs` | 自研 DXF 解析器（ADR-01） |
| `src/LeatherNesting.Desktop/Composition/DesktopComposition.cs` | 唯一组合根 |
| `src/LeatherNesting.Desktop/Shell/DesktopModuleDiscovery.cs` | 模块反射发现 |
| `docs/adr/01-dxf-adapter.md` | DXF 适配决策 |
| `docs/adr/02-职责.md` | 职责边界与输出契约 |
| `docs/todo/01-json输出契约待办.md` | JSON 契约待办 |
