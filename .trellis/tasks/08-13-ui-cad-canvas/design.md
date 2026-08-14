# 技术设计：CAD 上下文工具栏

## 1. 边界

工具栏分为四层，避免视图、图标和业务状态互相耦合：

```text
CadToolCatalog（27 项不可变定义）
        ↓
CadToolbarState（模式、ActiveTool、CanExecute、TODO 反馈）
        ↓
CadToolbarView（顺序、分组、按钮状态、Tooltip）
        ↓
CadWorkspaceHost / CadEvidenceCanvas（真实画布能力与 Shell 集成）
```

- Catalog 只描述工具，不引用 Avalonia Control。
- Icon Factory 只把 `IconKey` 转换为原创矢量图形，不处理点击。
- State 不依赖按钮文字；只接收稳定 `CommandKey`。
- Host 负责把已支持命令路由到真实画布，其余路由到统一 TODO。

## 2. 建议文件边界

| 文件/目录 | 单一职责 | 并行所有权建议 |
| --- | --- | --- |
| `Modules/CadCanvas/Toolbar/CadToolDefinition.cs` | 枚举、记录类型、模式和实现状态 | 合同 Agent |
| `Modules/CadCanvas/Toolbar/CadToolCatalog.cs` | 27 项唯一顺序和元数据 | 合同 Agent |
| `DesignSystem/CadTools/CadToolIcon*.cs` | 分组矢量图标 | 图标 Agent，可按组拆文件 |
| `Modules/CadCanvas/Toolbar/CadToolbarState.cs` | Active、模式、可用性、命令结果 | 状态 Agent |
| `Modules/CadCanvas/Toolbar/CadToolbarView.cs` | 29 个控件、分隔线和绑定 | 视图 Agent |
| `Shell/CadWorkspaceHost.cs` | 接入现有宿主和真实范围缩放 | 集成 Agent，最后修改 |
| `tests/.../CadToolbar*Tests.cs` | 注册表、状态与视图合同 | 检查 Agent |

为了让 worktree 易于合并，图标组必须分文件，例如 `CadToolIconGroupA.cs` 至 `CadToolIconGroupE.cs`；不同 Agent 不同时修改同一个大型 switch 文件。

## 3. 数据合同

建议定义：

```text
CadToolDefinition
├── Order: int
├── ControlId: string
├── CommandKey: enum
├── Label: string
├── Tooltip: string
├── Group: enum A..E
├── IconKey: enum
├── Confidence: Confirmed | High | Medium | Low
├── SupportedModes: flags
├── ImplementationState: Implemented | Partial | Todo
└── Shortcut: string?
```

`CadToolCatalog.All` 是唯一真源。视图顺序、测试期望、TODO 台账和人工验收表均从该 Catalog 生成或核对，禁止维护四份字符串数组。

## 4. 状态与命令流

```text
用户点击按钮
  → 根据 CommandKey 查询 CanExecute
  → 若互斥工具：取消旧预览 → 设置 ActiveTool
  → 若已实现：调用已注册 handler
  → 若未实现：CadHostState.ReportUnsupported(Label)
  → 更新状态栏、选中态和可用性
```

模式切换：

```text
CadEdit      → Catalog 中 SupportedModes 包含 CadEdit 的 27 项
NestingReview → 过滤为 Select/Undo/Redo/Cancel/Delete/Settings 6 项
```

不要在切换模式时销毁 Catalog 或重新分配 ID；只生成可见投影。

## 5. 视觉合同

- 外框 24×24 px，CornerRadius=0，Padding 1–2 px。
- 图标建议占 16–18 px，线宽在 1.0–1.5 px 之间。
- 常态：浅灰底、深灰图标；悬停：略深底；选中：蓝色边框与浅蓝底；禁用：降低不透明度。
- 填充复选框、红色“填充”文字和 `255` 输入框作为工具栏前导控件，不属于 27 项 Catalog。
- 组间分隔由 View 根据 `Group` 变化自动插入，不把 separator 当成工具按钮计数。
- 窄窗口允许工具栏区域水平滚动作为保底，但 1366×768 基准必须单行完整可见。

## 6. 兼容与回滚

- 现有 `CadWorkspaceHost.DrawingToolButtons` 可在迁移期保留为从新 View 暴露的只读列表，避免现有测试一次性失效。
- 真实 `Refit()` handler 必须保留；所有其他当前临时按钮迁移到 Catalog。
- 每个阶段独立提交。若集成失败，可回退宿主接线提交，不需要回退 Catalog、图标或测试合同。
- 不修改几何模型，故回滚不会影响项目文件兼容性。
