# 左侧订单组与裁片列表点击后缩回左侧

## Goal

让用户通过点击左边缘的常驻细条，把左侧栏（订单组、裁片列表、进度汇总）整体向窗口左侧缩回，只留下一条细条，中央画布随之变宽；再次点击细条恢复完整面板。

用户价值：CAD 画布区域可临时扩宽，长图排版时左侧信息一键收起/展开。复刻参照软件 LEFT-02「面板固定/停靠」的折叠能力。

## Background（已确认事实，仓库证据）

- 左侧栏 `BuildLeftRail()`（`src/LeatherNesting.Desktop/Shell/AppShellView.cs:167-177`）是一个三行 `Grid`：
  - 第 0 行：`OrderGroupHost`（订单组）、第 1 行：`PieceListHost`（裁片列表）、第 2 行：`ProgressSummaryHost`（进度汇总）。
- `BuildBody()`（`AppShellView.cs:144-165`）三列 `13*,74*,13*`：左栏、中央画布、右栏。
- `BuildLayout()`（`AppShellView.cs:115-130`）外层 Grid 单列 `"*"`，行为 `Auto,*,Auto`（顶栏、bodyLayer、状态栏）。
- `ClassicPaneHost`（`DesignSystem/ClassicPaneHost.cs`）：Border 包 Header（标题）+ ContentControl，无折叠能力。
- 参照软件：`05-视频上面梳理的信息.md/03-.md` LEFT-02「面板固定/停靠 IconButton」默认「固定」，折叠/展开为可选能力。
- 现有测试 `ShellFrameTests.cs` FRAME-001 固定 BodyGrid 三列 `13*,74*,13*` 与左栏三行 `20*,60*,20*`；FRAME-002 固定各 host 标题与边框样式。测试直接构造 `AppShellView`，断言结构状态（IsVisible、ColumnDefinition.Width、Grid 行列），不依赖布局测量。

## Requirements

- [ ] 左边缘存在一条始终可见的窄细条（折叠触发器），与左侧栏同高。
- [ ] 点击细条后，左侧栏三个面板（订单组、裁片列表、进度汇总）整体缩回左边缘，仅剩细条；中央画布列宽释放。
- [ ] 再次点击细条，左侧栏完整恢复（标题、内容、三行比例 `20*,60*,20*` 不变）。
- [ ] 折叠/展开双向可逆，折叠状态存于 shell 视图内（与 `OrderCardView._isExpanded` 同模式，纯视图状态，不做持久化）。
- [ ] 细条在 M02 模块覆盖层弹出时仍保持常驻（它是 shell 级 chrome，不属于 body 覆盖层）。

## Acceptance Criteria

- [ ] `AppShellView` 暴露折叠触发器与状态：初始为展开（`IsLeftRailCollapsed == false`，`LeftRail.IsVisible == true`，左栏列宽 `13*`，细条箭头朝左 `◀`）。
- [ ] 触发一次折叠后：`IsLeftRailCollapsed == true`，`LeftRail.IsVisible == false`，左栏列宽变为 `0`，细条箭头朝右 `▶`。
- [ ] 再次触发后完全恢复初始状态（13\*、可见、`◀`）。
- [ ] 新增 UI 测试覆盖「展开初始态 → 折叠 → 恢复」三步状态转换；既有 FRAME-001（BodyGrid 三列几何）保持通过。

## Key Decisions

- **折叠触发器**：左边缘常驻细条按钮（用户选定），宽约 14px、与左栏同高，内含方向箭头（展开 `◀` / 折叠 `▶`），点击切换。
- **折叠范围**：整条左栏（订单组 + 裁片列表 + 进度汇总）一并缩回，仅剩细条。注：原始描述只提「订单组和裁片列表」，但所选方案预览明确「只留细条」，故整栏折叠；如需保留进度汇总请在评审中指出。
- **布局实现**：细条作为外层 Grid 的新 `Auto` 列（列 0、行 1），不动 BodyGrid 的三列几何 —— FRAME-001 保持绿色；折叠通过「隐藏 `LeftRail` + 左栏列宽置 0」显式实现，不依赖 star 列自动收缩语义。

## Out of Scope

- 「订单组」内单个订单卡片的展开/折叠（`OrderCardView` 已有 ▸/▾，不涉及）。
- 右侧栏（CAD 参数 / 排版输出信息）的折叠。
- 折叠状态的持久化与跨会话记忆。
- 折叠动画（本次只做状态切换；如需动画另立任务）。

## Open Questions

无（用户已确认触发器方案；折叠范围以所选预览为准，见 Key Decisions 备注）。
