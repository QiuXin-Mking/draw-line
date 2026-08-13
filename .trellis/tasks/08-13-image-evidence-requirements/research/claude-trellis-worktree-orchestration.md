# Claude Code + Trellis Worktree 并行编排

> 目标：让 UI 演示版按模块并行开发，同时保持主分支可构建、可演示；不以并行速度交换架构一致性。

## 结论

可以并行，但不能把现有 T00--T12 直接各开一个 worktree 后同时开工。T00 当前同时拥有 Shell、主题、Demo 数据和模块注册；T01/T02 又分别绕开了这条边界。这样并行必然在 `AppShellViewModel`、`DemoScenario`、导入状态和 `MainWindow` 上产生冲突。

采用两阶段模型：

```text
阶段 0：基础契约与 Shell 重构（5 个小任务，受控合入）
        ↓ 形成 ui-foundation 基线提交
阶段 1：12 个模块 worktree（文件夹完全隔离，可分批 4--6 个并行）
        ↓
阶段 2：单一集成 worktree（按顺序 cherry-pick、全量验收、演示脚本）
```

现有 T00、T01、T02 的代码不回退；阶段 0 将它们收束到一致的依赖方向。

## 必须先固定的架构契约

| 契约 | 职责 | 禁止事项 |
| --- | --- | --- |
| `Workspace/IWorkspaceSession` | 唯一的当前项目、选中对象、导入/校验/排样摘要状态来源 | 模块自行保存另一份“当前项目”静态数据 |
| `Workspace/IWorkspaceCommands` | Shell 或模块发起的跨页意图，如导航、打开对象、显示 TODO | View 之间直接 `new` 对方 View/ViewModel |
| `Modules/Contracts/IDesktopModule` | 模块 ID、标题、导航分组、排序、创建页面 | 每个模块修改 `AppShellViewModel` 注册表 |
| `Demo/` 分模块样例提供者 | 演示数据按模块拆分，公共摘要只来自 Workspace snapshot | 继续无限扩张单一 `DemoScenario` |
| Composition Root | 唯一创建 Infrastructure adapter、Application use case、模块依赖的地点 | `ImportView` 等 UI 层直接 `new AsciiDxfReader` / `ZipProjectStore` |

模块注册采用“模块本地定义 + Shell 发现”的模式：每个模块在自己的目录新增 `*ModuleDefinition` 实现 `IDesktopModule`，模块目录外不需要登记。Shell 只负责发现、排序和显示；启动测试验证恰有 M01--M12，且 ID 唯一。这样 12 个模块不会争抢中央注册表。

## 阶段 0：基础任务拆分

这些任务从当前 `main` 创建短分支，在**基础集成 worktree** 内依次合入。它们的 PRD 必须以本文的接口边界为准。

| 编号 | 任务 / 推荐分支 | 独占路径 | 依赖 | 交付与验收 |
| --- | --- | --- | --- | --- |
| F01 | Workspace 契约 `agent/ui-workspace-contracts` | `src/LeatherNesting.Desktop/Workspace/**`、对应 tests | 无 | `IWorkspaceSession`、不可变 snapshot、跨页命令；同一项目状态可被两个模块观察 |
| F02 | Module 契约与发现 `agent/ui-module-contracts` | `src/LeatherNesting.Desktop/Modules/Contracts/**`、对应 tests | 无 | `IDesktopModule`、唯一 ID 校验、稳定排序；不引用具体模块 |
| F03 | Demo 数据拆分 `agent/ui-demo-contracts` | `src/LeatherNesting.Desktop/Demo/**`、对应 tests | F01 | 模块样例 provider 和公共摘要；不修改任一页面 |
| F04 | 导入流程解耦 `agent/ui-import-workflow` | `src/LeatherNesting.Desktop/Modules/Import/**`、`Desktop/Adapters/Import/**`、对应 tests | F01 | UI 不直接构造读写器；一次导入得到同一个 CAD 文档与 Workspace 状态 |
| F05 | Shell 集成重构 `agent/ui-shell-integration` | `src/LeatherNesting.Desktop/Shell/**`、`Views/MainWindow.cs`、`Composition/**`、对应 tests | F01--F04 | 页面实例稳定、Shell 状态来自 Workspace、模块自动发现；不改模块内部 UI |

F01/F02 可并行；F03/F04 在 F01 后并行；F05 最后合入。`T00` 的剩余责任归 F02/F03/F05，`T02` 的架构修正归 F04；已实现的 `T01` 作为阶段 1 的 M01 worktree 基线。

## 阶段 1：12 个模块 worktree

每个模块只允许改自己的模块目录、自己的测试目录和自己的 child task 文件。除 F05 集成者外，禁止改 `Shell/`、`Workspace/`、`Composition/`、`Demo/`、`Views/MainWindow.cs`、共享设计系统。

| Worktree | 保留任务 | 文件所有权 | 前置基础 | 说明 |
| --- | --- | --- | --- | --- |
| `ln-wt-m01` | T01 项目与订单 | `Modules/Projects/**`、`tests/**/Modules/Projects/**` | F01--F05 | 把现有静态演示改为 workspace/provider 读取 |
| `ln-wt-m02` | T02 DXF 导入 | `Modules/Import/**`、`tests/**/Modules/Import/**` | F04/F05 | F04 成果的页面验收与视觉完善 |
| `ln-wt-m03` | T03 CAD 画布 | `Modules/CadCanvas/**`、对应 tests | F01/F02/F05 | 不改既有几何算法；画布适配放模块内 |
| `ln-wt-m04` | T04 几何修复 | `Modules/GeometryRepair/**`、对应 tests | F01/F02/F05 | 只消费 Workspace/CAD 工作台契约 |
| `ln-wt-m05` | T05 工艺特征 | `Modules/ProcessFeatures/**`、对应 tests | F01/F02/F05 | 未接线动作必须带 TODO |
| `ln-wt-m06` | T06 裁片与尺码 | `Modules/Pieces/**`、对应 tests | F01/F02/F05 | 内存演示状态只留在模块内 |
| `ln-wt-m07` | T07 材料约束 | `Modules/Materials/**`、对应 tests | F01/F02/F05 | 参数校验不越过模块边界 |
| `ln-wt-m08` | T08 排样运行 | `Modules/NestingRun/**`、对应 tests | F01/F02/F05 | 模拟运行不可宣称真实排样 |
| `ln-wt-m09` | T09 结果复核 | `Modules/NestingReview/**`、对应 tests | F01/F02/F05 | 跨页定位走 `IWorkspaceCommands` |
| `ln-wt-m10` | T10 校验审批 | `Modules/Validation/**`、对应 tests | F01/F02/F05 | 出口阻断只读 Workspace 校验摘要 |
| `ln-wt-m11` | T11 导出交接 | `Modules/Export/**`、对应 tests | F01/F02/F05 | 不实际写生产文件；全部明确 TODO |
| `ln-wt-m12` | T12 管理设置 | `Modules/Administration/**`、对应 tests | F01/F02/F05 | 演示设置不修改全局运行时配置 |

每个模块在自有目录内提供 `ModuleDefinition`。注册、导航、主题、公共 Demo 和应用组合均由基础层处理，模块不得因“方便”越界修改。

## Git worktree 与合入策略

### 分支职责

| 分支 | 唯一写入者 | 用途 |
| --- | --- | --- |
| `main` | 集成者 | 始终可构建的已验收版本 |
| `integration/ui-foundation` | 基础集成者 | 合入 F01--F05，完成后作为模块共同基线 |
| `agent/ui-*` | 对应一个 Claude Code worker | 一个任务、一组独占目录、一个可审查提交 |
| `integration/ui-demo` | 集成者 | 依序吸收 12 个模块并执行 T13 |

Worktree 放在仓库同级目录（例如 `../leathernesting-worktrees/`），不放在项目根目录；因此不需要改 `.gitignore`，也不会把构建文件混入仓库。

```bash
# 基础集成 worktree（先建立；完成 F01--F05 后得到共同基线）
git worktree add ../leathernesting-worktrees/foundation -b integration/ui-foundation main

# 阶段 1：从已冻结的共同基线创建模块 worktree
git worktree add ../leathernesting-worktrees/m01 -b agent/ui-m01 integration/ui-foundation
git worktree add ../leathernesting-worktrees/m02 -b agent/ui-m02 integration/ui-foundation
# ... m03 至 m12 同理；每个目录只启动其所属任务。

# 集成 worktree（只由一个人/agent 操作）
git worktree add ../leathernesting-worktrees/integration -b integration/ui-demo integration/ui-foundation
```

不要让 12 个 worker 直接向 `main` 合并，也不要在 worker 中运行 `git merge`。模块完成后，集成者依次检查范围、cherry-pick、运行模块测试及全量 build；冲突必须回到模块 owner 修复，不能由集成者猜测业务意图。

## Claude Code + Trellis 的执行规则

1. 一个 worktree 启动一个 Claude Code 会话，显式指定 child task；不要复用主工作区会话。
2. 每个 child task 的 `implement.jsonl` / `check.jsonl` 仅注入：该模块 PRD、本文的相关契约段、对应截图 Markdown、相关 package spec。不要把父任务所有图片与 12 页需求注入每个 worker。
3. Trellis channel 只用于进度、问题、验收结论的耐久日志；它不替代 worktree 隔离。每个 worker 用独立 channel，例如 `ui-m06`，并以 `done/error` 事件作为完成信号。
4. 当前 `.claude/agents/trellis-implement.md` 明确禁止 `git commit`。为了让 worktree 真正自闭环，需要新增一个**仅用于隔离 worktree** 的 `trellis-worktree-implement` 角色：允许 `git add` 与一次 scoped commit，但仍禁止 `push`、`merge`、`rebase` 和修改所有权外文件。若不增加该角色，则由集成者在每个 worktree 代为提交，吞吐量会明显下降。
5. `trellis-check` 可以在同一 worker worktree 中自修复，但检查前必须执行范围断言：`git diff --name-only` 不得含非授权路径。检查后才允许提交。
6. `.trellis/config.yaml` 当前 `max_live_workers: 6` 合理。推荐实际同时运行 4--6 个，而不是 12 个；12 个 worktree 是任务池，不是并发数。机器资源充裕并且没有阻塞式 build 时，才提高到 6。

### 每个模块 worker 的完成门

```text
1. task 处于本模块 child task，读取最小上下文。
2. 只修改授权路径；新增 ModuleDefinition 也必须位于模块目录。
3. 运行模块测试、Desktop 测试和 dotnet build。
4. trellis-check 检查并修复；再次验证。
5. 断言 diff 无越权文件、无未标识的演示伪逻辑。
6. 生成一个提交，向 Trellis channel 发送 done（含 commit、测试、TODO 清单）。
```

## 集成顺序与验收

1. F01--F05 全部通过后，建立 `integration/ui-foundation` 的基线提交。
2. 以 4--6 个为一批执行 M01--M12；推荐首批 M01/M03/M05/M06/M07/M12，次批 M02/M04/M08/M09/M10/M11。
3. `integration/ui-demo` 依提交逐个吸收模块；每吸收一个至少运行模块测试和 `dotnet build`，每批运行 `dotnet test`。
4. 最后执行 T13：12 页导航探测、1366×768 视觉演示、TODO 台账、真实导入/保存/工艺工作台回归。
5. 只有 T13 通过才把 `integration/ui-demo` 合入 `main`。

## 本次落地前的决策

建议先实施“基础任务重编排 + worktree 专用 Claude agent + 最小上下文清单”，然后创建 `foundation` worktree。这样模块 worker 启动后不需要等待反复改共享 Shell，也不会覆盖当前主工作区的已完成 T00--T02 工作。
