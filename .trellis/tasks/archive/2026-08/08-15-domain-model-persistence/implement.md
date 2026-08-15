# 领域模型补全实现计划

1. 新增 `Application/Domain/Piece.cs`、`Material.cs`（含 `Loop2D` 几何）。
2. 新增 `Application/Domain/NestingProject.cs`（聚合根：Document + Pieces + Materials + NestingResults + Version）。
3. 新增 `Infrastructure/Dxf/Loop2DJsonConverter.cs`（几何多态序列化）。
4. 修改 `IProjectStore` 签名：`SaveAsync(NestingProject)` / `LoadAsync` 返回 `NestingProject`。
5. 修改 `ZipProjectStore`：序列化/反序列化 `NestingProject`，旧项目兼容（几何字段缺省置空）。
6. 单元测试：
   - 实体 round-trip：Piece/Material/NestingProject 序列化 → 反序列化 → 相等。
   - 几何多态：含 Line/Polyline/Arc 的 Loop2D round-trip 无损。
   - 旧项目加载：仅 Document 的 manifest 不崩溃。
7. 全量测试：`dotnet test`，确认无回归。

**验证**：`dotnet test tests/LeatherNesting.Infrastructure.Tests`

## 回滚点

- 步骤 4/5（接口签名变更）前：纯新增，无风险。
- 步骤 4/5 后：还原 `IProjectStore`/`ZipProjectStore` 到步骤 3 状态即可。
