# ADR-01: Stage 1 DXF adapter

Status: accepted for Stage 1 inventory; replacement boundary retained.

## Decision

Use the dependency-free `AsciiDxfReader` behind `IDxfReader` for the Stage 1 DXF inspector. It inventories ASCII entities, identifies closed `LWPOLYLINE` and well-terminated old-style `POLYLINE` candidates, emits blocking diagnostics for open or unterminated legacy polylines, and requires an explicit unit decision. `IDxfWriter` remains an interface because export is Stage 5.

## Evidence and limits

The repository samples include a 2000 DXF with 9 `LWPOLYLINE` and 18 `TEXT` entities, plus R10 reference DXFs with 81 or 84 legacy `POLYLINE` entities. The adapter deliberately does not repair, flatten, or close geometry. ARC/SPLINE/bulge topology and round-trip writing remain Stage 2/5 acceptance work.

## Alternatives

`netDxf` is rejected as a permanent dependency because its upstream repository is archived. A third-party library may be introduced later only after the real-sample, license, maintenance, binary-size and macOS/Windows test gate in `design.md` §5.1. That change affects Infrastructure only; Domain and UI contracts remain stable.

## Consequences

This delivers a safe diagnostic path now, but does not claim CAD completeness. The reader never infers a closed contour from a missing `SEQEND`, and a closed legacy polyline is only a candidate when it has at least three inventoried vertices. All inputs with absent or conflicting units stop at Unit Review and are stored with their decision once confirmed. If the source can no longer be read to calculate its SHA-256 fingerprint after inspection, the workflow returns a blocking diagnostic and cannot commit the import.
