# 修复阶段 2 Geometry 测试失败用例

## Goal

使 `tests/LeatherNesting.Geometry.Tests` 中当前 10 个失败的阶段 2 用例全部通过，同时不回归已有的 28 个通过用例。失败全部是几何实现缺陷（非测试错误），覆盖轮廓修复、offset、节点编辑、拓扑、公差等阶段 2 核心能力。

## Background

主任务 `08-07-leather-nesting-windows-clone` 的阶段 2（CAD 诊断、修复与普通工艺）几何实现已就位、能编译，但 38 个 Geometry 测试中 10 个失败，集中暴露实现缺口：公差校验缺失、gap 修复/边界生成、containment tree、端点索引、节点操作校验、offset 的 winding 处理。

## Confirmed Facts

复现命令与结果：

```bash
dotnet test tests/LeatherNesting.Geometry.Tests -c Release
# 结果：通过 28 / 失败 10
```

失败明细（9 个测试方法 = 10 个失败用例，其中 1 个 Theory 含 2 数据行）：

| 测试 | 断言失败 | 根因分类 |
|---|---|---|
| ToleranceProfileTests.Zero_or_negative_tolerance_throws(0 / -0.01) | `Assert.Throws<ArgumentOutOfRangeException>` 未抛异常 | ToleranceProfile 构造缺参数校验 |
| GeometryPropertyTests.Endpoint_index_finds_gaps | 未找到 gap | EndpointIndex 未检测间隙 |
| RepairTests.Gap_repair_connects_disconnected_curves | `Assert.True` 连接失败 | GapRepair 未连接断开曲线 |
| TopologyTests.Gap_005_tol_01_previews_bridge | 报「50.000mm 超过公差」，但 gap=0.05/tol=0.1 应在容差内 | gap 距离单位/尺度错误（50 vs 0.05，差 1000 倍） |
| NodeOperationTests.Move_creating_self_intersection_is_blocked | 移动产生自交未被阻断 | 节点移动缺自交校验 |
| NodeOperationTests.Delete_below_three_points_is_blocked | 删除至 <3 点未被阻断 | 节点删除缺点数下限校验 |
| NodeOperationTests.Single_point_break_conserves_total_length | 单点剪断长度不守恒 | 剪断实现未守恒总长 |
| GeometryPropertyTests.Containment_tree_detects_outer_and_hole | 空集合，未识别外环+孔 | ContainmentTree 未建立/识别孔 |
| OffsetTests.Reversed_winding_offset_equivalent_within_tolerance | Area 1242 vs 572 差异巨大 | offset 未正确处理 winding 方向 |

## Requirements

测试是契约，本任务修复实现而非修改测试预期（若确需改测试，须单独说明理由并获批准）：

- **R1 公差校验**：`ToleranceProfile` 构造时对非正（≤0）或 NaN/∞ 的公差抛 `ArgumentOutOfRangeException`。
- **R2 端点索引**：`EndpointIndex` 能检测端点间的间隙。
- **R3 Gap 修复**：`GapRepair` 在 gap 处于容差内时连接断开曲线；距离计算单位正确。
- **R4 边界生成/桥接**：`ContourCloser`/`BoundaryGenerator` 在 gap≤容差时预览桥接、gap>容差时拒绝，距离单位正确。
- **R5 节点操作校验**：移动节点产生自交时阻断；删除后点数 <3 时阻断；单点剪断保持总长度守恒。
- **R6 包含树**：`ContainmentTree` 正确识别外环与内孔（单外环 + 单孔 → 1 个孔）。
- **R7 Offset winding**：反转 winding 或曲线顺序后，材料空间 offset 结果在公差内等价。

## Acceptance Criteria

- [ ] 10 个失败用例全部通过。
- [ ] 原有 28 个通过用例无回归（38 个全部通过）。
- [ ] `dotnet build LeatherNesting.sln -c Release` 0 warning / 0 error。
- [ ] `dotnet test tests/LeatherNesting.Geometry.Tests -c Release` 全部通过。
- [ ] 不修改测试断言语义；确需改测试须记录理由并获批准。

## Out of Scope

- 阶段 2 其余交付：UI 工艺工作台（CadWorkbench）、Command transaction 的 UI 集成、golden DXF round-trip 完整实现等。
- 阶段 3+（订单/码齿/材料/排样）。
- 性能优化与真机平台验收。

## Open Questions

无阻塞问题；根因已定位到具体子系统，具体修法在实现阶段按子系统逐一诊断（必要时拆分子任务）。
