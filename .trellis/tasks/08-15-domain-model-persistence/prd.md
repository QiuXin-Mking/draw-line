# 领域模型补全与项目持久化

## Goal

让项目文档（`ProjectDocument`）能承载并持久化实际的业务成果——裁片、材料、排样结果——而不是当前只有「导入报告」。让「读 DXF → 修复 → 排样 → 输出」的成果能保存、加载、恢复。

## 现状（confirmed facts）

- `Domain` 层只有 `ProjectDocument`（`Id/Name/SchemaVersion/Revision/IsDirty/Imports`），`Imports` 只有 `ImportReport`。
- **没有** `Piece`（裁片）、`Material`（材料）、`NestingResult`（排样结果）等实体。
- 几何 `Loop2D` 在 `Geometry` 层，`Domain` 层不引用它。
- `ZipProjectStore` 用 `System.Text.Json` 序列化 `ProjectDocument`（无多态），只存 `manifest.json`。
- 排样结果 `NestResult`（`Geometry` 层）已实现；裁片/材料在 UI 层是 Demo 数据，无领域实体。

## Requirements

1. `Domain` 层新增核心实体（最小集，覆盖已实现链路）：
   - **裁片**：标识、名称、尺码、几何轮廓。
   - **材料**：标识、名称、几何轮廓。
   - **排样结果**：位姿列表、未放置列表、利用率。
2. `ProjectDocument` 能聚合上述实体（或通过聚合根持有）。
3. `ZipProjectStore` 能序列化/反序列化含几何多态的实体（几何曲线类型 `LineSegment2D/CircularArc2D/Polyline2D` 的多态序列化）。
4. `SchemaVersion` 从 1 升到 2，支持旧项目（只有导入报告）加载不崩溃。

## Acceptance Criteria

- [ ] 新建项目 → 导入裁片 → 保存 → 重新加载，裁片几何与位姿无损恢复。
- [ ] 保存的项目包含排样结果，重新加载后 `NestResult` 一致。
- [ ] 旧版项目（仅 `manifest.json` 导入报告）仍能加载，不崩溃。
- [ ] round-trip 测试：实体 → 序列化 → 反序列化 → 实体相等。

## Out of Scope

- 工艺特征（剪口、节点编辑）持久化（后续）。
- 订单信息、版本历史、多材料批次调度。
- 瑕疵区、纹路方向（排样扩展）。
- JSON 输出（另立任务）。

## 决策

- **实体范围**：已确认**最小集**——裁片 + 材料 + 排样结果（覆盖已实现的「读 DXF → 排样 → 输出」链路）。
