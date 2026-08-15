# 领域模型补全技术设计

## 分层约束（关键前提）

依赖方向：`Domain ← Geometry ← Application ← Infrastructure`。

`Domain` 是叶子（不引用任何项目），`Geometry` 引用 `Domain`。因此 **`Domain` 不能引用 `Geometry` 的 `Loop2D`**（否则循环依赖）。

领域实体（裁片/材料）含 `Loop2D` 几何，**不能放 `Domain` 层**，应放 **`Application` 层**（能引用 Domain + Geometry）。

## 实体设计（Application 层）

```csharp
namespace LeatherNesting.Application.Domain;

/// <summary>A cut piece: identity + size + geometry outline.</summary>
public sealed record Piece(string Id, string Name, string Size, Loop2D Outline);

/// <summary>A material sheet: identity + geometry boundary.</summary>
public sealed record Material(string Id, string Name, Loop2D Boundary);

// NestResult（排样结果）已在 Geometry 层（LeatherNesting.Geometry.Nesting），直接复用。
```

## 聚合根

```csharp
public sealed record NestingProject(
    ProjectDocument Document,                       // Domain 元数据（Id/Name/Revision/Imports）
    IReadOnlyList<Piece> Pieces,
    IReadOnlyList<Material> Materials,
    IReadOnlyList<NestResult> NestingResults);
```

`NestingProject` 是持久化的单位；`ProjectDocument` 继续承担元数据与导入追溯。

## 持久化

- 新增 `INestingProjectStore`（Application 层）：`SaveAsync(NestingProject)` / `LoadAsync`。
- `ZipProjectStore` 实现 `INestingProjectStore`，序列化 `NestingProject` 到 zip（保持 `.tmp` + `.bak` 崩溃安全写入）。
- 现有 `IProjectStore`（`ProjectDocument`）**不动**，避免波及 Desktop 的 `ImportCoordinator`。

## 几何多态序列化（关键技术点）

`Loop2D.Curves` 是 `IReadOnlyList<Curve2D>`，`Curve2D` 是 abstract record，子类 `LineSegment2D` / `CircularArc2D` / `Polyline2D`。`System.Text.Json` 默认不支持多态。

方案：自定义 `JsonConverter<Loop2D>`，逐曲线写类型 tag + 各子类字段（`Start/End`、`Centre/Radius/Angles`、`Points`）。序列化时只存构造所需字段（`StableId`/`Role`/`Curves`），计算属性 `IsClockwise`/`Area` 反序列化时重算。

## SchemaVersion 迁移

- `NestingProject` 带 `Version` 字段（起始 2）。
- 旧项目（`manifest.json` 仅含 `ProjectDocument`）加载时：`Document` 正常反序列化，`Pieces/Materials/NestingResults` 置空，不崩溃。

## 兼容与回滚

- 纯新增类型 + `IProjectStore` 接口签名变更；`Domain` 层不动。
- 回滚：还原 `IProjectStore`/`ZipProjectStore` 与新增类型即可。
