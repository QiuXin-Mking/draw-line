# Trellis 完整使用攻略与原理解析

> 本指南详细解析 Trellis 的核心哲学、全流程使用攻略，以及如何判断 PRD 全面性和 Task 自动化能力。

---

## 一、Trellis 是什么？

Trellis 是一个**结构化的 AI 辅助开发框架**，核心思想是：

> 通过标准化的工作流程（workflow）、领域规范（spec）和任务管理（task），让 AI 和人类开发者能够高效协作，避免上下文丢失和重复性错误。

---

## 二、核心架构：三大支柱

| 支柱 | 目录 | 作用 |
|------|------|------|
| **Workflow** | `.trellis/workflow.md` | 定义开发的生命周期和阶段 |
| **Spec** | `.trellis/spec/` | 存储编码规范和领域知识 |
| **Task** | `.trellis/tasks/` | 管理具体的功能需求和技术任务 |

### 辅助目录

| 目录 | 作用 |
|------|------|
| **Workspace** | `.trellis/workspace/` | 开发者日志和会话追踪 |
| **Shared** | `.trellis/shared/` | 跨任务共享的参考资料 |

---

## 三、Workflow：标准化开发流程

`.trellis/workflow.md` 定义项目开发的完整生命周期，典型阶段包括：

```
1. Discover（发现）   → 理解需求，创建任务
2. Design（设计）     → 技术方案，架构设计
3. Implement（实现）  → 编码实现
4. Review（审查）     → 代码审查，质量检查
5. Test（测试）       → 测试验证
6. Deploy（部署）     → 部署上线
```

### 关键概念

- **Phase Gate（阶段门）**：每个阶段结束时有明确的完成标准
- **Context Carryover（上下文传递）**：确保跨会话的上下文不丢失
- **Checkpoint（检查点）**：关键决策点的记录

---

## 四、Spec：领域规范与编码标准

`.trellis/spec/` 是 Trellis 的灵魂，让 AI 理解项目的**领域知识**和**编码约定**。

### 典型文件结构

```
.trellis/spec/
├── index.md              # 规范索引和入口
├── coding-style.md       # 编码风格规范
├── architecture.md       # 架构设计规范
├── api-conventions.md    # API 设计约定
├── database.md           # 数据库规范
├── frontend/             # 前端专项规范
│   ├── component.md
│   └── state-management.md
└── backend/              # 后端专项规范
    ├── authentication.md
    └── error-handling.md
```

### Spec 的核心作用

1. **知识沉淀**：将项目特定的知识固化下来，避免每次都要重新解释
2. **一致性保证**：确保 AI 生成的代码符合项目规范
3. **上下文压缩**：通过引用 spec，减少重复性的上下文占用

### Spec 的编写原则

- **具体而非抽象**：不要写"使用良好的命名"，要写"组件名使用 PascalCase，hooks 使用 camelCase"
- **示例驱动**：每个规范都配代码示例
- **可验证**：规范应该能被工具检查（如 lint、format）

---

## 五、Task：结构化任务管理

`.trellis/tasks/` 是需求到实现的桥梁。

### 任务文件结构

```
.trellis/tasks/
├── TASK-001-user-auth/           # 任务目录
│   ├── prd.md                    # 产品需求文档
│   ├── tech-spec.md              # 技术规格说明
│   ├── notes.md                  # 开发笔记和思考
│   └── context.jsonl             # 上下文快照
├── TASK-002-payment-integration/
└── ...
```

### 关键文件说明

| 文件 | 用途 |
|------|------|
| `prd.md` | 产品需求，从用户视角描述功能 |
| `tech-spec.md` | 技术实现方案，包含接口设计、数据模型等 |
| `notes.md` | 开发过程中的思考、决策、问题记录 |
| `context.jsonl` | 机器可读的上下文快照，用于恢复会话 |

### Task 的生命周期

```
创建 → 分析 → 设计 → 实现 → 测试 → 完成
```

每个阶段都有明确的产出物和检查标准。

---

## 六、Workspace：会话与日志管理

`.trellis/workspace/` 记录开发者的活动和 AI 的会话历史。

### 典型结构

```
.trellis/workspace/
├── journal.md          # 开发日志，按日期记录
├── sessions/           # 会话记录
│   ├── 2024-01-15-session-001.md
│   └── 2024-01-15-session-002.md
└── decisions/          # 重要决策记录
    └── DECISION-001-why-we-chose-postgresql.md
```

### Journal 的作用

- **上下文恢复**：快速了解之前做了什么
- **知识追溯**：查找之前的决策原因
- **进度追踪**：了解项目当前状态

---

## 七、核心哲学

### 1. 上下文管理（Context Management）

AI 的上下文窗口有限，Trellis 通过以下方式管理上下文：
- **分层加载**：先加载通用规范（spec），再加载具体任务（task）
- **增量更新**：只传递变化的部分，避免重复
- **快照机制**：通过 `context.jsonl` 保存和恢复会话状态

### 2. 知识持久化（Knowledge Persistence）

传统开发中，知识存在于：
- 人的大脑中（易遗忘、难传递）
- 代码注释中（易过时、难查找）
- 文档中（易分散、难同步）

Trellis 将知识结构化存储在 `.trellis/` 中，确保：
- **可查找**：按主题组织，快速定位
- **可验证**：与代码同步更新
- **可传承**：新成员快速上手

### 3. 人机协作（Human-AI Collaboration）

Trellis 定义了清晰的协作边界：
- **人类负责**：需求定义、架构决策、代码审查
- **AI 负责**：代码生成、文档编写、测试用例、知识整理
- **共同维护**：Spec 的更新、Task 的管理

---

## 八、完整使用流程

### 阶段一：项目初始化

```bash
# 1. 初始化 Trellis 结构
trellis init

# 2. 编写基础规范
.trellis/spec/index.md
.trellis/spec/coding-style.md

# 3. 定义工作流程
.trellis/workflow.md
```

### 阶段二：需求开发

```bash
# 1. 创建任务
trellis task create "用户认证系统"

# 2. 编写 PRD（产品需求文档）
# .trellis/tasks/TASK-XXX/prd.md

# 3. AI 辅助编写技术规格
# .trellis/tasks/TASK-XXX/tech-spec.md
```

### 阶段三：编码实现

```bash
# 1. AI 读取相关 Spec
# 2. AI 读取 Task 上下文
# 3. AI 生成代码
# 4. 人类审查和修改
# 5. 更新 Task 笔记
```

### 阶段四：质量保证

```bash
# 1. 运行测试
# 2. AI 进行代码审查（基于 Spec）
# 3. 更新 Spec（如果发现不一致）
# 4. 完成任务
```

### 阶段五：知识沉淀

```bash
# 1. 将新学到的知识写入 Spec
# 2. 记录决策到 Workspace
# 3. 归档 Task
```

---

## 九、PRD 文档全面性检查清单

### 核心判断标准：5W2H 覆盖度

| 维度 | 检查点 |
|------|--------|
| **Why** | 为什么做这个功能？业务价值是什么？ |
| **What** | 具体要做什么？功能边界在哪里？ |
| **Who** | 谁是目标用户？有哪些角色？ |
| **When** | 什么时间触发？有没有时间约束？ |
| **Where** | 在什么场景下使用？ |
| **How** | 如何实现？交互流程是什么？ |
| **How much** | 性能指标？数据量预估？ |

### PRD 全面性自查表

```markdown
## 必须包含的内容（Must Have）

### 1. 背景与目标
- [ ] 业务背景说明
- [ ] 用户痛点描述
- [ ] 成功指标（KPI/OKR）

### 2. 用户故事
- [ ] 作为 [角色]，我想要 [功能]，以便 [价值]
- [ ] 至少包含 3-5 个核心用户故事

### 3. 功能需求
- [ ] 功能清单（Feature List）
- [ ] 功能优先级（MoSCoW 法则）
- [ ] 功能边界（In Scope / Out of Scope）

### 4. 非功能需求
- [ ] 性能要求（响应时间、并发量）
- [ ] 安全要求（认证、授权、数据保护）
- [ ] 兼容性要求（浏览器、设备、版本）
- [ ] 可用性要求（SLA、容错）

### 5. 交互设计
- [ ] 用户流程图（User Flow）
- [ ] 关键页面原型（Wireframe / Mockup）
- [ ] 异常流程处理（错误、空状态、 loading）

### 6. 数据需求
- [ ] 数据模型（ER 图 / 数据字典）
- [ ] API 接口定义（请求/响应格式）
- [ ] 数据流转图

### 7. 验收标准
- [ ] 可测试的验收条件（Given-When-Then）
- [ ] 边界条件说明
- [ ] 测试用例概述

### 8. 风险与依赖
- [ ] 技术风险
- [ ] 业务风险
- [ ] 外部依赖（第三方服务、团队依赖）
```

### PRD 质量评估矩阵

| 质量维度 | 低质量（1-3分） | 中质量（4-6分） | 高质量（7-10分） |
|----------|----------------|----------------|-----------------|
| **完整性** | 缺失关键章节 | 基本覆盖，但不够深入 | 全面覆盖，无遗漏 |
| **清晰性** | 模糊、歧义多 | 基本清晰，有小歧义 | 精确、无歧义 |
| **可测试性** | 无法直接测试 | 部分可测试 | 每个需求都可测试 |
| **可追踪性** | 无法追踪到代码 | 部分可追踪 | 每个需求有唯一 ID |
| **一致性** | 自相矛盾 | 基本一致 | 完全自洽 |

---

## 十、Task Handleless 运行指南

### Handleless 的定义

**Handleless = 无需人工干预，自动完成全流程**

### Handleless 运行检查清单

```markdown
## 前置条件（Pre-conditions）

### 1. 输入完备性
- [ ] 所有输入参数都有明确的定义
- [ ] 参数有默认值或可选值
- [ ] 输入验证规则清晰

### 2. 环境确定性
- [ ] 依赖的服务已明确列出
- [ ] 环境变量已配置
- [ ] 权限和访问控制已设置

### 3. 决策自动化
- [ ] 所有决策点都有明确的规则
- [ ] 没有"根据情况判断"的模糊描述
- [ ] 有默认的 fallback 策略

## 执行过程（Execution）

### 4. 流程自动化
- [ ] 每个步骤都有明确的输入输出
- [ ] 步骤之间有明确的依赖关系
- [ ] 可以并行执行的步骤已标识

### 5. 异常处理
- [ ] 每种异常情况都有处理方案
- [ ] 重试机制（Retry Policy）
- [ ] 熔断机制（Circuit Breaker）

### 6. 日志与监控
- [ ] 关键节点有日志记录
- [ ] 有进度追踪机制
- [ ] 有告警触发条件

## 输出验证（Output Validation）

### 7. 结果可验证
- [ ] 成功标准明确定义
- [ ] 失败标准明确定义
- [ ] 有自动化的验证脚本
```

### Handleless 能力分级

| 等级 | 名称 | 特征 | 示例 |
|------|------|------|------|
| **L0** | 手动执行 | 每一步都需要人工操作 | 人工部署、人工测试 |
| **L1** | 半自动 | 部分步骤自动化，需要人工确认 | CI/CD 但需要人工审批 |
| **L2** | 全自动 | 标准流程全自动，异常需人工 | 自动化部署、自动化测试 |
| **L3** | 自适应 | 全自动 + 自动处理已知异常 | 自动扩缩容、自动回滚 |
| **L4** | 自愈合 | 全自动 + 自动处理未知异常 | AI 驱动的故障自愈 |

### 实现 Handleless 的关键要素

```yaml
# .trellis/tasks/TASK-XXX/config.yaml

handleless:
  # 1. 输入验证
  input_validation:
    enabled: true
    schema: "input-schema.json"
    strict: true  # 严格模式，不符合则拒绝

  # 2. 自动决策
  decision_engine:
    enabled: true
    rules: "decision-rules.yaml"
    default_action: "abort"  # 默认安全策略

  # 3. 异常处理
  error_handling:
    retry_policy:
      max_retries: 3
      backoff_strategy: "exponential"
    fallback:
      enabled: true
      strategy: "rollback"

  # 4. 监控告警
  monitoring:
    metrics: ["execution_time", "success_rate", "error_rate"]
    alerts:
      - condition: "error_rate > 0.05"
        action: "notify_and_pause"

  # 5. 结果验证
  validation:
    automated: true
    checks: "validation-suite.yaml"
```

---

## 十一、最佳实践

### Spec 的维护

- **启动时加载**：每次会话开始时，让 AI 先读取相关 Spec
- **编码前更新**：如果发现 Spec 过时，立即更新
- **定期审查**：每月审查一次 Spec 的有效性

### Task 的管理

- **粒度适中**：一个 Task 对应一个可交付的功能点
- **文档先行**：先写 PRD 和 Tech Spec，再开始编码
- **及时归档**：完成的 Task 及时归档，保持目录整洁

### Workspace 的记录

- **每日记录**：每天结束时记录当天的进展
- **决策留痕**：重要的技术决策记录到 `decisions/`
- **问题追踪**：遇到的问题和解决方案记录到 `notes.md`

---

## 十二、实用命令

```bash
# 初始化 Trellis 项目
trellis init

# 创建新任务
trellis task create "任务名称"

# 检查 PRD 完整性
trellis check prd --task TASK-XXX

# 检查 Task 是否支持 Handleless
trellis check handleless --task TASK-XXX

# 生成检查报告
trellis report --task TASK-XXX --output report.md

# 完成任务
trellis task complete TASK-XXX

# 归档任务
trellis task archive TASK-XXX
```

---

## 十三、总结

### Trellis 的核心价值

| 价值点 | 说明 |
|--------|------|
| **结构化** | 将混沌的开发过程结构化、标准化 |
| **可复现** | 相同的输入产生相同的输出 |
| **可传承** | 知识不随人员流动而丢失 |
| **可扩展** | 支持自定义规范和技能 |
| **人机协作** | 明确人类和 AI 的分工 |

### 一句话总结

> **Trellis 是 AI 时代的软件工程方法论**，它通过结构化的工作流程、领域规范和任务管理，让人类和 AI 能够高效、可靠地协作开发软件。

---

## 十四、进阶主题

### 1. Skill 系统

Trellis 支持自定义 Skill（技能），扩展 AI 的能力：

```markdown
# .trellis/skills/my-skill/SKILL.md

## 触发条件
When: 用户需要处理 XXX 时
Trigger: "关键词"

## 执行步骤
1. 步骤一
2. 步骤二

## 输出格式
...
```

### 2. 钩子（Hooks）

在 Workflow 的关键节点插入自定义逻辑：
- Pre-phase：阶段开始前的准备
- Post-phase：阶段完成后的清理
- Checkpoint：关键决策点的验证

### 3. 跨会话记忆

通过 `trellis mem` CLI 实现：
- 查询历史会话
- 恢复任务上下文
- 追踪决策变更

---

> **文档版本**：v1.0
> **最后更新**：2026-08-11
> **适用项目**：Trellis 管理的所有项目
