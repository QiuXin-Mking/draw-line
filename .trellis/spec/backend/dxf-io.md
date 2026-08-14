# DXF I/O Guidelines

> How DXF reading and writing are layered, and how nesting results are exported to DXF.

---

## Component Map

| Component | Layer | Role |
|---|---|---|
| `IDxfReader` | Application | Inspect DXF entities (inventory, no geometry) |
| `AsciiDxfReader` | Infrastructure | ASCII DXF entity inventory + diagnostics |
| `AsciiDxfGeometryReader` | Infrastructure | Read closed `LWPOLYLINE` back into `Loop2D` |
| `IDxfWriter` | Infrastructure | Write closed contours (Stage 2 round-trip) |
| `AsciiDxfWriter` | Infrastructure | ASCII `LWPOLYLINE` writer |
| `INestingDxfWriter` | Application | Write a nesting result as DXF |
| `AsciiNestingDxfWriter` | Infrastructure | 3-layer + TEXT annotation writer |
| `ExportNestingDxfUseCase` | Application | `NestResult` → DXF document → file |

---

## Signatures

```csharp
// Application port (in DxfImport.cs)
public interface IDxfReader { Task<DxfImportResult> ReadAsync(string path, CancellationToken ct); }

// Application port (in NestingExport.cs)
public interface INestingDxfWriter { Task WriteAsync(string path, NestingDxfDocument document, CancellationToken ct); }
public sealed record NestingDxfDocument(Loop2D Material, IReadOnlyList<NestingDxfPiece> Pieces, string Title);
public sealed record NestingDxfPiece(string PieceId, double RotationDegrees, Loop2D PlacedLoop);

// Infrastructure writer
public sealed class AsciiNestingDxfWriter : INestingDxfWriter { /* 3 layers + LWPOLYLINE + TEXT */ }

// Application use case
public sealed class ExportNestingDxfUseCase(INestingDxfWriter writer)
{
    public Task ExportAsync(string path, NestResult result, Loop2D material, double gapMm, CancellationToken ct);
}
```

---

## Conventions

### 1. Ports live in Application, ASCII implementations in Infrastructure

`IDxfReader` / `INestingDxfWriter` are Application-level ports; `Ascii*` implementations live under `Infrastructure/Dxf/`. Use cases depend on the port, never on a concrete writer.

### 2. Nesting DXF uses three layers

`AsciiNestingDxfWriter` emits exactly `LEATHER` (material boundary), `PIECES` (placed piece contours), `ANNOTATION` (piece id + rotation labels + utilization title). This mirrors the reference demo `leather_nesting_demo.py::write_dxf`.

### 3. `unplaced` pieces are NOT written to DXF

The DXF carries only placed geometry + annotations. `unplaced` belongs to the JSON output (`docs/todo/01-json输出契约待办.md`), not DXF.

### 4. Closed contours: don't rely on the source to close the polyline

`AsciiDxfGeometryReader` appends the closing vertex when reading closed `LWPOLYLINE`, and `Loop2D` appends the ring closing edge in area/winding. See the gotcha in [`geometry-nesting.md`](./geometry-nesting.md). New DXF producers should not assume the reader closes geometry.

### 5. Round-trip verification pattern

Export → `AsciiDxfGeometryReader.ReadAsync` → assert contour bbox/area match the source `PlacedLoop`. This catches layer, coordinate, and closing-vertex regressions.

---

## Gotcha: `AsciiDxfWriter` writes a bare `layer = "0"`

`AsciiDxfWriter` (Stage 2 round-trip) hardcodes `8`/`0`. It is **not** the nesting exporter — use `AsciiNestingDxfWriter` when layer separation and annotations are required.
