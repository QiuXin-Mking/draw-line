# Geometry & Nesting Guidelines

> How to write geometry and nesting code safely in `LeatherNesting.Geometry`, using Clipper2 for precise boolean geometry.

---

## Overview

The Geometry layer computes polygon booleans, offsets, and nesting. Two rule sources exist and must not be confused:

- `Validation/PlacementValidator` — **validation only**; its overlap check uses a bounding box and is intentionally coarse.
- `Nesting/` — the placement engine; uses **Clipper2 booleans** for precise edge-level checks.

Precision rule: **placement / collision decisions use Clipper2 booleans, never bounding boxes.**

---

## Signatures

### Nesting engine (`Nesting/`)

```csharp
public sealed record NestRequest(
    IReadOnlyList<Loop2D> Pieces,
    Loop2D Material,
    double GapMm,
    IReadOnlyList<double> AllowedRotationsDegrees);

public sealed record NestPlacement(string PieceId, Transform2D Transform, Loop2D PlacedLoop);

public sealed record NestResult(
    IReadOnlyList<NestPlacement> Placements,
    IReadOnlyList<string> Unplaced,
    double Utilization);   // placed area sum / material area, 0..1
```

### Shared conversion bridge

```csharp
public static class ClipperPathAdapter
{
    public static Path64 ToPath64(Loop2D loop, long scale, ToleranceProfile tolerance);
    public static Point2D ToPoint2D(Point64 p, long scale);
    public static Point64 ToPoint64(Point2D p, long scale);
}
```

### Collision detector

```csharp
public sealed class ClipperCollisionDetector
{
    public bool IsPlacementValid(Loop2D candidate, IReadOnlyList<Loop2D> placed, Loop2D material, double gapMm);
    public bool Overlaps(Loop2D a, Loop2D b);
}
```

---

## Conventions

### 1. Precise collision = Clipper2 booleans, not bounding boxes

**Why**: `PlacementValidator`'s bounding-box overlap returns false positives for concave or angled shapes; nesting must be exact to maximize utilization.

**How**:
- **Overlap** — `Clipper.Intersect(subject, clip, FillRule.NonZero)` returns a non-empty `Paths64`. Edge contact (zero-area intersection) is **not** an overlap.
- **Contains / in-bounds** — `Clipper.Difference(candidate, material, FillRule.NonZero)` returns empty ⇒ candidate fully inside material.

```csharp
private static bool PathsOverlap(Path64 a, Path64 b)
{
    if (a.Count < 3 || b.Count < 3) return false;
    return Clipper.Intersect(new Paths64 { a }, new Paths64 { b }, FillRule.NonZero).Count > 0;
}
```

### 2. All `Loop2D` ↔ Clipper2 conversion goes through `ClipperPathAdapter`

**Why**: The mm↔integer scaling (`GeometryConstants.IntegerScale = 1_000_000`) and arc flattening must stay consistent; hand-rolling it per call site drifts rounding and arc chord tolerance.

**How**: Use `ClipperPathAdapter.ToPath64(loop, GeometryConstants.IntegerScale, tolerance)`. It deduplicates consecutive points and flattens `CircularArc2D` by `ToleranceProfile.FlattenChordToleranceMm`. `OffsetAdapter` already embeds the same logic; new geometry code reuses the adapter rather than copying it.

### 3. Gap enforced with a single inflate

**Why**: The gap constraint applies to **both** the material edge **and** every placed piece. Inflating once and reusing the result avoids two separate offsets (and two chances to drift).

**How**: Inflate the candidate by `gapMm` once, then require the inflated shape to be inside the material **and** clear of every placed piece:

```csharp
var inflated = gapMm > 0 ? Inflate(candidatePath, gapMm) : candidatePath;
if (!InsideMaterial(inflated, materialPath)) return false;
foreach (var placedPath in placedPaths)
    if (PathsOverlap(inflated, placedPath)) return false;
```

---

## Gotcha: `ClipperOffset` inflation direction depends on winding

> **Warning**: `ClipperOffset.Execute(delta, ...)` grows **outward only for positively-wound (CCW) paths**. A clockwise path with a positive delta shrinks inward instead.

`Loop2D` winding is normalized (outer = CCW) but not guaranteed on every input. Always respect the sign:

```csharp
var sign = Clipper.Area(path) >= 0 ? 1 : -1;
var delta = (long)Math.Round(sign * deltaMm * scale);
```

Symptom of getting this wrong: pieces "shrink" instead of keeping gap, silently producing overlapping placements.

---

## Validation & Error Matrix

| Condition | Behavior |
|---|---|
| `GapMm < 0` | throws `ArgumentOutOfRangeException` |
| `AllowedRotationsDegrees` empty | throws `ArgumentException` |
| `Pieces` empty | returns empty `NestResult` (utilization 0), no throw |
| piece cannot fit any rotation/position | added to `Unplaced`, does not block others |
| `Material.Area == 0` | utilization returns 0 (no division by zero) |

---

## Good / Bad

### Good — precise, shared, deterministic

```csharp
var engine = new NestEngine();
var result = engine.Nest(new NestRequest(pieces, material, gapMm: 5, allowedRotationsDegrees: [0, 90]));
// result.Placements[i].PlacedLoop is the transformed contour; result.Utilization in 0..1
```

### Bad — coarse overlap or hand-rolled scaling

```csharp
// Don't: bounding-box overlap for placement (false positives on concave shapes)
// Don't: p.X * 1_000_000 inlined at each call site (drifts rounding, skips arc flattening)
// Don't: ClipperOffset with a positive delta on a clockwise path (shrinks, not inflates)
```

---

## Tests Required

Nesting tests live in `tests/LeatherNesting.Geometry.Tests/NestingTests.cs` (`P3-NEST-001`..`P3-NEST-010`). A placement change must be covered by assertions on:

- overlap detected vs edge contact allowed (`P3-NEST-001/002`)
- gap enforced against both material edge and placed pieces (`P3-NEST-003/004`)
- rotation actually applied when 0° does not fit (`P3-NEST-005`)
- oversized piece → `Unplaced` (`P3-NEST-006`)
- determinism: same input → same transforms (`P3-NEST-007`)
- full-result legality re-checked with `ClipperCollisionDetector` (`P3-NEST-010`)
