# Project Persistence Guidelines

> How business content (pieces, materials, nesting results) is persisted, and how geometry survives JSON round-trips.

---

## Layering Constraint (read first)

Dependency direction is `Domain ← Geometry ← Application ← Infrastructure`. `Domain` cannot reference `Geometry` (would be circular). Therefore **entities that carry `Loop2D` geometry live in `Application`, not `Domain`**:

- `Domain` keeps metadata only (`ProjectDocument`, `ImportReport`).
- `Application.Domain` holds `Piece` / `Material` / `NestingProject` (geometry-bearing).

## Component Map

| Component | Layer | Role |
|---|---|---|
| `NestingProject` | Application | Aggregate root: `Document` + `Pieces` + `Materials` + `NestingResults` |
| `Piece` / `Material` | Application | Entities with `Loop2D` geometry |
| `INestingProjectStore` | Application | Persistence port |
| `ZipNestingProjectStore` | Infrastructure | Zip + `manifest.json` persistence |

## Signatures

```csharp
public sealed record Piece(string Id, string Name, string Size, Loop2D Outline);
public sealed record Material(string Id, string Name, Loop2D Boundary);
public sealed record NestingProject(
    ProjectDocument Document,
    IReadOnlyList<Piece> Pieces,
    IReadOnlyList<Material> Materials,
    IReadOnlyList<NestResult> NestingResults);

public interface INestingProjectStore
{
    Task SaveAsync(string path, NestingProject project, CancellationToken ct);
    Task<NestingProject> LoadAsync(string path, CancellationToken ct);
}
```

## Conventions

### 1. Geometric polymorphic serialization

`Curve2D` subclasses (`LineSegment2D` / `CircularArc2D` / `Polyline2D`) serialize polymorphically via attributes, not hand-rolled converters:

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LineSegment2D), "line")]
[JsonDerivedType(typeof(CircularArc2D), "arc")]
[JsonDerivedType(typeof(Polyline2D), "polyline")]
public abstract record Curve2D { /* ... */ }
```

Types with explicit constructors (`Point2D`, `Loop2D`, `Transform2D`) mark them `[JsonConstructor]`. Any new `Curve2D` subclass must add a `[JsonDerivedType]` entry or round-trip silently drops the curve.

### 2. JsonOptions must be case-insensitive

Constructors use lowercase parameters (`x`, `y`, `translateX`) but properties are PascalCase. Always set `PropertyNameCaseInsensitive = true`, or deserialization falls back to a parameterless constructor and fails.

### 3. Legacy tolerance

`LoadAsync` coalesces missing collections to empty (`project.Pieces ?? []`), so older manifests that predate `Pieces`/`Materials`/`NestingResults` still load without crashing.

### 4. Crash-safe write

Persist via `.tmp` write → `File.Move` overwrite, keeping a `.bak` copy of the previous file, and delete `.tmp` in `finally`. See `ZipProjectStore` / `ZipNestingProjectStore`.

## Snapshot recovery

`ProjectSnapshotStore` persists a crash-recovery snapshot of the whole `NestingProject` to a sibling `<project>.autosave` file, reusing `ZipNestingProjectStore` serialization.

- `SaveSnapshotAsync` writes the snapshot; call after each committed CAD operation (fire-and-forget).
- `LoadSnapshotAsync` returns `null` when no snapshot exists **or** when the snapshot is corrupt (treat corrupt as absent, not as a crash).
- `ClearSnapshot` deletes the snapshot on normal save/exit, so a surviving `.autosave` on startup signals an unclean exit.

## Gotchas

- **Domain can't hold geometry** — putting `Loop2D` in `Domain` creates a project-reference cycle. Geometry-bearing entities go in `Application.Domain`.
- **Missing `[JsonDerivedType]`** — a new curve subclass without a discriminator fails to deserialize and is silently lost.
