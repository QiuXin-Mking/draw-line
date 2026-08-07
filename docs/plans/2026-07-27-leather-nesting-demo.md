# 皮革鞋面自动排样 Demo Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a dependency-light command-line Demo that packs the nine fixed shoe-upper outlines from `凉鞋.dxf` into 2000×1000, 2000×4000, and 2000×9000 mm leather rectangles and exports DXF/PNG/JSON results.

**Architecture:** A single `leather_nesting_demo.py` module owns DXF parsing, pure-Python polygon geometry, deterministic bottom-left placement, and artifact export. `unittest` suites exercise its public functions and a subprocess integration run; production output is isolated under `demo_output/`.

**Tech Stack:** Python 3.9 standard library, `ezdxf 1.4.2`, Pillow, `unittest`.

---

### Task 1: Establish the command-line and size parsing contract

**Files:**
- Create: `tests/test_cli_and_geometry.py`
- Create: `leather_nesting_demo.py`

**Step 1: Write the failing test**

```python
import unittest
from leather_nesting_demo import parse_leather_size


class LeatherSizeTests(unittest.TestCase):
    def test_parses_width_and_height_in_millimetres(self):
        self.assertEqual(parse_leather_size("2000x1000"), (2000.0, 1000.0))

    def test_rejects_malformed_or_non_positive_size(self):
        for value in ("2000", "x1000", "2000x0", "-1x1000"):
            with self.assertRaises(ValueError):
                parse_leather_size(value)
```

**Step 2: Run the test to verify it fails**

Run: `python3 -m unittest tests.test_cli_and_geometry.LeatherSizeTests -v`

Expected: `ModuleNotFoundError` because the Demo module does not exist.

**Step 3: Write minimal implementation**

Create `leather_nesting_demo.py` with `parse_leather_size(value: str) -> tuple[float, float]`. Split only on one lowercase or uppercase `x`, convert both values to floats, and reject values that are not finite positive numbers. Add `argparse` options `--input`, `--output-dir`, `--gap-mm`, and repeated `--leather` values, with the three agreed defaults.

**Step 4: Run the test to verify it passes**

Run: `python3 -m unittest tests.test_cli_and_geometry.LeatherSizeTests -v`

Expected: two passing tests.

**Step 5: Commit**

Git metadata is absent in this workspace; record this task as locally complete without a commit.

### Task 2: Extract the nine usable shoe-upper polygons from DXF

**Files:**
- Modify: `tests/test_cli_and_geometry.py`
- Modify: `leather_nesting_demo.py`

**Step 1: Write the failing test**

```python
from pathlib import Path
from leather_nesting_demo import load_closed_lwpolylines


class DxfLoadingTests(unittest.TestCase):
    def test_loads_the_nine_closed_shoe_upper_outlines(self):
        pieces, ignored = load_closed_lwpolylines(Path("凉鞋.dxf"))
        self.assertEqual(len(pieces), 9)
        self.assertTrue(all(len(piece.points) >= 3 for piece in pieces))
        self.assertTrue(all(piece.area > 0 for piece in pieces))
        self.assertGreaterEqual(ignored["TEXT"], 18)
```

**Step 2: Run the test to verify it fails**

Run: `python3 -m unittest tests.test_cli_and_geometry.DxfLoadingTests -v`

Expected: FAIL because `load_closed_lwpolylines` is missing.

**Step 3: Write minimal implementation**

Add immutable `Piece` and `Bounds` dataclasses, a shoelace `polygon_area(points)` helper, and `load_closed_lwpolylines(path)`. It must only select `LWPOLYLINE` entities where `entity.closed` is true and contain at least three distinct points. Return input-order indices, points, nonnegative area, and an entity-type ignore counter. Treat source coordinates as millimetres deliberately; print a warning if `$INSUNITS` is not millimetres.

**Step 4: Run the test to verify it passes**

Run: `python3 -m unittest tests.test_cli_and_geometry.DxfLoadingTests -v`

Expected: one passing test reporting exactly nine valid pieces.

**Step 5: Commit**

No commit is possible because this supplied directory is not a Git repository.

### Task 3: Implement collision-safe fixed-direction bottom-left placement

**Files:**
- Modify: `tests/test_cli_and_geometry.py`
- Modify: `leather_nesting_demo.py`

**Step 1: Write the failing tests**

```python
from leather_nesting_demo import Piece, pack_pieces


class PackingTests(unittest.TestCase):
    def test_places_non_overlapping_squares_with_requested_gap(self):
        pieces = [
            Piece(1, ((0, 0), (10, 0), (10, 10), (0, 10)), 100),
            Piece(2, ((0, 0), (10, 0), (10, 10), (0, 10)), 100),
        ]
        result = pack_pieces(pieces, leather=(40, 20), gap_mm=5)
        self.assertEqual(len(result.placements), 2)
        self.assertEqual(len(result.unplaced), 0)
        self.assertGreaterEqual(result.placements[1].x - result.placements[0].x, 15)

    def test_leaves_a_piece_unplaced_when_it_cannot_fit(self):
        piece = Piece(1, ((0, 0), (30, 0), (30, 10), (0, 10)), 300)
        result = pack_pieces([piece], leather=(20, 20), gap_mm=0)
        self.assertEqual(result.placements, [])
        self.assertEqual([p.index for p in result.unplaced], [1])
```

**Step 2: Run the tests to verify they fail**

Run: `python3 -m unittest tests.test_cli_and_geometry.PackingTests -v`

Expected: FAIL because `pack_pieces` and placement types are missing.

**Step 3: Write minimal implementation**

Implement:

- Translation and bounds helpers.
- Orientation, point-on-segment, segment intersection, point-in-polygon, and polygon-overlap functions.
- Segment-to-segment distance, used to reject pieces closer than `gap_mm`.
- Candidate positions `(gap, gap)`, then positions formed by aligning an unplaced piece's minimum x/y to existing placed pieces' maximum x/y plus the gap.
- `pack_pieces` sorted by descending area then original index; evaluate candidates by `(y, x)` and return the first valid candidate for each piece.

The validator must ensure each translated vertex is within `[gap, width-gap] × [gap, height-gap]`, has no overlap with existing pieces, and respects edge-to-edge gap. No rotation or mirroring code is added.

**Step 4: Run the tests to verify they pass**

Run: `python3 -m unittest tests.test_cli_and_geometry.PackingTests -v`

Expected: two passing tests.

**Step 5: Run all unit tests**

Run: `python3 -m unittest discover -s tests -v`

Expected: all Task 1–3 tests pass.

**Step 6: Commit**

No commit is possible because this supplied directory is not a Git repository.

### Task 4: Export production artifacts and add end-to-end verification

**Files:**
- Create: `tests/test_integration.py`
- Modify: `leather_nesting_demo.py`

**Step 1: Write the failing integration test**

```python
import json
import subprocess
import tempfile
import unittest
from pathlib import Path


class DemoIntegrationTests(unittest.TestCase):
    def test_generates_dxf_png_and_summary_for_all_three_leathers(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            output_dir = Path(temp_dir) / "output"
            completed = subprocess.run(
                ["python3", "leather_nesting_demo.py", "--output-dir", str(output_dir)],
                check=True, text=True, capture_output=True,
            )
            summary = json.loads((output_dir / "summary.json").read_text())
            self.assertEqual(set(summary["runs"]), {"2000x1000", "2000x4000", "2000x9000"})
            for name in summary["runs"]:
                self.assertTrue((output_dir / f"{name}.dxf").is_file())
                self.assertTrue((output_dir / f"{name}.png").is_file())
                self.assertGreaterEqual(summary["runs"][name]["utilization_percent"], 0)
```

**Step 2: Run the test to verify it fails**

Run: `python3 -m unittest tests.test_integration.DemoIntegrationTests -v`

Expected: FAIL because the command has not yet emitted artifacts.

**Step 3: Write minimal implementation**

For each leather size, create an `ezdxf` R2010 document marked in millimetres. Add layers `LEATHER`, `PIECES`, and `ANNOTATION`; write the leather rectangle, each translated closed polyline, its `P01`–`P09` label, and a title with size and utilization. Add a Pillow-rendered preview with the same boundary, fill colours and labels; this avoids a Matplotlib font-cache delay in the sandbox. Write `summary.json` with input, gap, each size's placed/unplaced indices, used area, leather area, and utilization percentage. Use `Path.mkdir(parents=True, exist_ok=True)` for the requested output directory.

**Step 4: Run the integration test to verify it passes**

Run: `python3 -m unittest tests.test_integration.DemoIntegrationTests -v`

Expected: one passing test; all nine pieces should be placed for the three supplied leather sizes.

**Step 5: Run the complete verification suite**

Run: `python3 -m unittest discover -s tests -v`

Expected: all unit and integration tests pass.

**Step 6: Commit**

No commit is possible because this supplied directory is not a Git repository.

### Task 5: Produce the demonstrable result and operating notes

**Files:**
- Modify: `README.md`
- Create: `demo_output/2000x1000.dxf`
- Create: `demo_output/2000x1000.png`
- Create: `demo_output/2000x4000.dxf`
- Create: `demo_output/2000x4000.png`
- Create: `demo_output/2000x9000.dxf`
- Create: `demo_output/2000x9000.png`
- Create: `demo_output/summary.json`

**Step 1: Run the Demo against the supplied input**

Run: `python3 leather_nesting_demo.py --input 凉鞋.dxf --output-dir demo_output --gap-mm 5 --leather 2000x1000 2000x4000 2000x9000`

Expected: three named result groups and a non-error process exit.

**Step 2: Inspect generated files**

Run: `file demo_output/*.dxf demo_output/*.png demo_output/summary.json`

Expected: three AutoCAD DXF files, three PNG images, and UTF-8 JSON.

**Step 3: Document exact usage and limitations**

Add a short `README.md` section with the run command, artifact descriptions, fixed 0°/no-mirror constraint, 5 mm default gap, and the warning that this is deterministic heuristic packing rather than a global optimum.

**Step 4: Re-run the complete suite after documentation**

Run: `python3 -m unittest discover -s tests -v`

Expected: all tests remain green.

**Step 5: Commit**

No commit is possible because this supplied directory is not a Git repository.
