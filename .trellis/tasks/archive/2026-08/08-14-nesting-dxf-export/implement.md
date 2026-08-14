# 排样结果 DXF 输出 实现计划

1. 定义排样输出模型 `NestingDxfDocument` / `NestingDxfPiece`（Application 或 Infrastructure 端口层）。
2. 新增 `AsciiNestingDxfWriter`：写 `LEATHER` / `PIECES` / `ANNOTATION` 三图层 + LWPOLYLINE + TEXT 标注 + 标题。
3. 新增 `ExportNestingDxfUseCase`（Application 层）：`NestResult` + `Material` + `gapMm` → 组装 `NestingDxfDocument` → 调 writer 写文件。
4. 单元测试：
   - 空排样 → 仅皮革 + 标题，不崩溃。
   - 含 N 片 → 读回实体数正确、图层正确。
   - round-trip：裁片轮廓 == `PlacedLoop`。
5. 全量测试：`dotnet test`，确认无回归。

**验证**：`dotnet test tests/LeatherNesting.Infrastructure.Tests`（或新建 DXF 导出测试）。

## 回滚点

- 纯新增，删除 use case + writer 即可回滚，不改动现有 `IDxfWriter` / `AsciiDxfWriter`。
