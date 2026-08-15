#!/usr/bin/env python3
"""固定方向的皮革鞋面自动排样 Demo。"""

import argparse
import json
import math
from collections import Counter
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional

import ezdxf


Point = tuple[float, float]


@dataclass(frozen=True)
class Piece:
    """One cuttable shoe-upper outer contour in source coordinates."""

    index: int
    points: tuple[Point, ...]
    area: float


@dataclass(frozen=True)
class Bounds:
    min_x: float
    min_y: float
    max_x: float
    max_y: float


@dataclass(frozen=True)
class Placement:
    piece: Piece
    x: float
    y: float
    points: tuple[Point, ...]
    rotation_degrees: int = 0


@dataclass(frozen=True)
class PackingResult:
    placements: list[Placement]
    unplaced: list[Piece]
    placed_counts: dict[int, int] = field(default_factory=dict)


def parse_leather_size(value: str) -> tuple[float, float]:
    """Parse a ``WIDTHxHEIGHT`` leather size expressed in millimetres."""
    parts = value.lower().split("x")
    if len(parts) != 2:
        raise ValueError(f"皮革尺寸必须写成 WIDTHxHEIGHT：{value!r}")
    try:
        width, height = (float(part) for part in parts)
    except ValueError as error:
        raise ValueError(f"皮革尺寸必须是数字：{value!r}") from error
    if not all(math.isfinite(number) and number > 0 for number in (width, height)):
        raise ValueError(f"皮革尺寸必须为正数：{value!r}")
    return width, height


def polygon_area(points: tuple[Point, ...]) -> float:
    """Return the absolute shoelace area of a simple polygon."""
    return abs(
        sum(
            x1 * y2 - x2 * y1
            for (x1, y1), (x2, y2) in zip(points, points[1:] + points[:1])
        )
        / 2.0
    )


def load_closed_lwpolylines(path: Path) -> tuple[list[Piece], Counter]:
    """Load valid closed LWPOLYLINE entities and count ignored entity types."""
    doc = ezdxf.readfile(path)
    pieces: list[Piece] = []
    ignored: Counter = Counter()
    for entity in doc.modelspace():
        if entity.dxftype() != "LWPOLYLINE" or not entity.closed:
            ignored[entity.dxftype()] += 1
            continue
        points = tuple((float(x), float(y)) for x, y in entity.get_points("xy"))
        if len(points) < 3 or len(set(points)) < 3:
            ignored["INVALID_LWPOLYLINE"] += 1
            continue
        pieces.append(Piece(len(pieces) + 1, points, polygon_area(points)))
    return pieces, ignored


def bounds_of(points: tuple[Point, ...]) -> Bounds:
    xs, ys = zip(*points)
    return Bounds(min(xs), min(ys), max(xs), max(ys))


def translate_points(points: tuple[Point, ...], dx: float, dy: float) -> tuple[Point, ...]:
    return tuple((x + dx, y + dy) for x, y in points)


def rotate_points(points: tuple[Point, ...], degrees: int) -> tuple[Point, ...]:
    """Rotate source coordinates around the origin using permitted footwear angles."""
    if degrees == 0:
        return points
    if degrees == 180:
        return tuple((-x, -y) for x, y in points)
    raise ValueError("首版只允许 0° 或 180° 旋转")


def bounds_are_separated_by_gap(first: Bounds, second: Bounds, gap_mm: float, epsilon: float = 1e-7) -> bool:
    """Return true when axis-aligned bounds guarantee the requested clearance."""
    return (
        first.max_x <= second.min_x - gap_mm + epsilon
        or second.max_x <= first.min_x - gap_mm + epsilon
        or first.max_y <= second.min_y - gap_mm + epsilon
        or second.max_y <= first.min_y - gap_mm + epsilon
    )


def cross(origin: Point, first: Point, second: Point) -> float:
    return (first[0] - origin[0]) * (second[1] - origin[1]) - (
        first[1] - origin[1]
    ) * (second[0] - origin[0])


def point_on_segment(point: Point, start: Point, end: Point, epsilon: float = 1e-9) -> bool:
    if abs(cross(start, end, point)) > epsilon:
        return False
    return (
        min(start[0], end[0]) - epsilon <= point[0] <= max(start[0], end[0]) + epsilon
        and min(start[1], end[1]) - epsilon <= point[1] <= max(start[1], end[1]) + epsilon
    )


def segments_intersect_strict(first_start: Point, first_end: Point, second_start: Point, second_end: Point) -> bool:
    """Return true only when two segments cross through their interiors."""
    first_a = cross(first_start, first_end, second_start)
    first_b = cross(first_start, first_end, second_end)
    second_a = cross(second_start, second_end, first_start)
    second_b = cross(second_start, second_end, first_end)
    epsilon = 1e-9
    return (
        (first_a > epsilon and first_b < -epsilon or first_a < -epsilon and first_b > epsilon)
        and (second_a > epsilon and second_b < -epsilon or second_a < -epsilon and second_b > epsilon)
    )


def point_in_polygon_strict(point: Point, polygon: tuple[Point, ...]) -> bool:
    """Return true for points strictly inside a polygon, not on its boundary."""
    inside = False
    for start, end in zip(polygon, polygon[1:] + polygon[:1]):
        if point_on_segment(point, start, end):
            return False
        if (start[1] > point[1]) != (end[1] > point[1]):
            crossing_x = (end[0] - start[0]) * (point[1] - start[1]) / (end[1] - start[1]) + start[0]
            if point[0] < crossing_x:
                inside = not inside
    return inside


def polygons_overlap(first: tuple[Point, ...], second: tuple[Point, ...]) -> bool:
    """Detect an area overlap; touching edges are allowed only when gap is zero."""
    if len(first) == len(second) and set(first) == set(second):
        return True
    for first_start, first_end in zip(first, first[1:] + first[:1]):
        for second_start, second_end in zip(second, second[1:] + second[:1]):
            if segments_intersect_strict(first_start, first_end, second_start, second_end):
                return True
    return point_in_polygon_strict(first[0], second) or point_in_polygon_strict(second[0], first)


def point_to_segment_distance(point: Point, start: Point, end: Point) -> float:
    delta_x, delta_y = end[0] - start[0], end[1] - start[1]
    length_squared = delta_x * delta_x + delta_y * delta_y
    if length_squared == 0:
        return math.dist(point, start)
    ratio = ((point[0] - start[0]) * delta_x + (point[1] - start[1]) * delta_y) / length_squared
    ratio = min(1.0, max(0.0, ratio))
    closest = (start[0] + ratio * delta_x, start[1] + ratio * delta_y)
    return math.dist(point, closest)


def segments_touch_or_intersect(first_start: Point, first_end: Point, second_start: Point, second_end: Point) -> bool:
    if segments_intersect_strict(first_start, first_end, second_start, second_end):
        return True
    return any(
        point_on_segment(point, start, end)
        for point, start, end in (
            (first_start, second_start, second_end),
            (first_end, second_start, second_end),
            (second_start, first_start, first_end),
            (second_end, first_start, first_end),
        )
    )


def segment_distance(first_start: Point, first_end: Point, second_start: Point, second_end: Point) -> float:
    if segments_touch_or_intersect(first_start, first_end, second_start, second_end):
        return 0.0
    return min(
        point_to_segment_distance(first_start, second_start, second_end),
        point_to_segment_distance(first_end, second_start, second_end),
        point_to_segment_distance(second_start, first_start, first_end),
        point_to_segment_distance(second_end, first_start, first_end),
    )


def polygon_distance(first: tuple[Point, ...], second: tuple[Point, ...]) -> float:
    return min(
        segment_distance(first_start, first_end, second_start, second_end)
        for first_start, first_end in zip(first, first[1:] + first[:1])
        for second_start, second_end in zip(second, second[1:] + second[:1])
    )


def placement_is_valid(
    points: tuple[Point, ...],
    placements: list[Placement],
    leather: tuple[float, float],
    gap_mm: float,
) -> bool:
    epsilon = 1e-7
    width, height = leather
    bounds = bounds_of(points)
    if (
        bounds.min_x < gap_mm - epsilon
        or bounds.min_y < gap_mm - epsilon
        or bounds.max_x > width - gap_mm + epsilon
        or bounds.max_y > height - gap_mm + epsilon
    ):
        return False
    for placement in placements:
        if bounds_are_separated_by_gap(bounds, bounds_of(placement.points), gap_mm):
            continue
        if polygons_overlap(points, placement.points):
            return False
        if gap_mm > 0 and polygon_distance(points, placement.points) < gap_mm - epsilon:
            return False
    return True


def candidate_origins(placements: list[Placement], gap_mm: float) -> list[tuple[float, float]]:
    x_candidates = {gap_mm}
    y_candidates = {gap_mm}
    for placement in placements:
        placed_bounds = bounds_of(placement.points)
        x_candidates.add(placed_bounds.max_x + gap_mm)
        y_candidates.add(placed_bounds.max_y + gap_mm)
    return sorted(
        ((x, y) for x in x_candidates for y in y_candidates),
        key=lambda candidate: (candidate[1], candidate[0]),
    )


def first_valid_placement(
    piece: Piece,
    rotation_degrees: int,
    placements: list[Placement],
    leather: tuple[float, float],
    gap_mm: float,
) -> Optional[Placement]:
    rotated_points = rotate_points(piece.points, rotation_degrees)
    piece_bounds = bounds_of(rotated_points)
    for x, y in candidate_origins(placements, gap_mm):
        points = translate_points(rotated_points, x - piece_bounds.min_x, y - piece_bounds.min_y)
        if placement_is_valid(points, placements, leather, gap_mm):
            return Placement(piece, x, y, points, rotation_degrees)
    return None


def pack_pieces(pieces: list[Piece], leather: tuple[float, float], gap_mm: float) -> PackingResult:
    """Place 0° pieces by descending area with a deterministic bottom-left rule."""
    if gap_mm < 0:
        raise ValueError("间隙不能小于 0")
    placements: list[Placement] = []
    unplaced: list[Piece] = []
    for piece in sorted(pieces, key=lambda item: (-item.area, item.index)):
        placement = first_valid_placement(piece, 0, placements, leather, gap_mm)
        if placement is None:
            unplaced.append(piece)
        else:
            placements.append(placement)
    return PackingResult(placements, unplaced, dict(Counter(p.piece.index for p in placements)))


def fill_leather(
    sizes: list[Piece],
    leather: tuple[float, float],
    gap_mm: float,
    rotations: tuple[int, ...] = (0, 180),
) -> PackingResult:
    """Repeat shoe sizes in a safe, linear-time shelf layout until the hide is full."""
    if gap_mm < 0:
        raise ValueError("间隙不能小于 0")
    if not sizes:
        return PackingResult([], [], {})
    rotations = tuple(dict.fromkeys(rotations))
    if not rotations or any(rotation not in (0, 180) for rotation in rotations):
        raise ValueError("旋转角度只能是 0° 或 180°")
    placements: list[Placement] = []
    ordered_sizes = sorted(sizes, key=lambda item: item.index)
    width, height = leather
    cursor = 0
    row_y = gap_mm
    while row_y < height - gap_mm:
        row_x = gap_mm
        row_height = 0.0
        placed_in_row = False
        while row_x < width - gap_mm:
            selected: Optional[tuple[Piece, int, tuple[Point, ...], Bounds, int]] = None
            for offset in range(len(ordered_sizes)):
                size_index = (cursor + offset) % len(ordered_sizes)
                size = ordered_sizes[size_index]
                rotation = rotations[(len(placements) + size_index) % len(rotations)]
                points = rotate_points(size.points, rotation)
                shape_bounds = bounds_of(points)
                shape_width = shape_bounds.max_x - shape_bounds.min_x
                shape_height = shape_bounds.max_y - shape_bounds.min_y
                if (
                    row_x + shape_width <= width - gap_mm
                    and row_y + shape_height <= height - gap_mm
                ):
                    selected = (size, rotation, points, shape_bounds, size_index)
                    break
            if selected is None:
                break
            size, rotation, points, shape_bounds, size_index = selected
            translated = translate_points(
                points, row_x - shape_bounds.min_x, row_y - shape_bounds.min_y
            )
            placements.append(Placement(size, row_x, row_y, translated, rotation))
            row_x += shape_bounds.max_x - shape_bounds.min_x + gap_mm
            row_height = max(row_height, shape_bounds.max_y - shape_bounds.min_y)
            cursor = (size_index + 1) % len(ordered_sizes)
            placed_in_row = True
        if not placed_in_row:
            break
        row_y += row_height + gap_mm
    return PackingResult(placements, [], dict(Counter(p.piece.index for p in placements)))


def size_name(leather: tuple[float, float]) -> str:
    return f"{leather[0]:g}x{leather[1]:g}"


def utilization_percent(result: PackingResult, leather: tuple[float, float]) -> float:
    used_area = sum(placement.piece.area for placement in result.placements)
    return used_area / (leather[0] * leather[1]) * 100.0


def write_dxf(output_path: Path, result: PackingResult, leather: tuple[float, float], gap_mm: float) -> None:
    """Write the leather boundary and placed pieces to a millimetre DXF file."""
    document = ezdxf.new("R2010", setup=True)
    document.units = ezdxf.units.MM
    document.layers.add("LEATHER", color=1)
    document.layers.add("PIECES", color=5)
    document.layers.add("ANNOTATION", color=3)
    modelspace = document.modelspace()
    width, height = leather
    modelspace.add_lwpolyline(
        ((0, 0), (width, 0), (width, height), (0, height)),
        close=True,
        dxfattribs={"layer": "LEATHER"},
    )
    label_height = max(8.0, min(width, height) / 80.0)
    for placement in result.placements:
        modelspace.add_lwpolyline(
            placement.points, close=True, dxfattribs={"layer": "PIECES"}
        )
        center_x = sum(point[0] for point in placement.points) / len(placement.points)
        center_y = sum(point[1] for point in placement.points) / len(placement.points)
        text = modelspace.add_text(
            f"P{placement.piece.index:02d} {placement.rotation_degrees}deg",
            dxfattribs={"height": label_height, "layer": "ANNOTATION"},
        )
        text.dxf.insert = (center_x, center_y)
    status = (
        f"Leather {size_name(leather)} mm | gap {gap_mm:g} mm | "
        f"placed {len(result.placements)} | utilization {utilization_percent(result, leather):.2f}%"
    )
    heading = modelspace.add_text(
        status,
        dxfattribs={"height": label_height, "layer": "ANNOTATION"},
    )
    heading.dxf.insert = (0, height + label_height * 1.5)
    document.saveas(output_path)


def write_preview(output_path: Path, result: PackingResult, leather: tuple[float, float], gap_mm: float) -> None:
    """Render a fast, dependency-local PNG preview matching the DXF placement."""
    from PIL import Image, ImageDraw

    width, height = leather
    margin = 44
    scale = 1600.0 / max(width, height)
    canvas_width = max(1, round(width * scale))
    canvas_height = max(1, round(height * scale))
    image = Image.new("RGB", (canvas_width + margin * 2, canvas_height + margin * 2), "white")
    drawing = ImageDraw.Draw(image, "RGBA")

    def convert(point: Point) -> tuple[float, float]:
        return (margin + point[0] * scale, margin + (height - point[1]) * scale)

    drawing.line(
        [convert(point) for point in ((0, 0), (width, 0), (width, height), (0, height), (0, 0))],
        fill="black",
        width=2,
    )
    colours = [
        (31, 119, 180, 140),
        (255, 127, 14, 140),
        (44, 160, 44, 140),
        (214, 39, 40, 140),
        (148, 103, 189, 140),
        (140, 86, 75, 140),
        (227, 119, 194, 140),
        (127, 127, 127, 140),
        (188, 189, 34, 140),
    ]
    for placement in result.placements:
        points = [convert(point) for point in placement.points]
        colour = colours[(placement.piece.index - 1) % len(colours)]
        drawing.polygon(points, fill=colour)
        drawing.line(points + [points[0]], fill=colour[:3] + (255,), width=2)
        center_x = sum(point[0] for point in placement.points) / len(placement.points)
        center_y = sum(point[1] for point in placement.points) / len(placement.points)
        drawing.text(
            convert((center_x, center_y)),
            f"P{placement.piece.index:02d}\n{placement.rotation_degrees}deg",
            fill="black",
            anchor="mm",
        )
    drawing.text(
        (margin, 14),
        f"{size_name(leather)} mm | gap {gap_mm:g} mm | utilization {utilization_percent(result, leather):.2f}%",
        fill="black",
    )
    image.save(output_path, format="PNG")


def run_demo(
    input_path: Path,
    output_dir: Path,
    gap_mm: float,
    leathers: list[tuple[float, float]],
    fill: bool = True,
) -> dict:
    """Pack source pieces into each requested leather size and write all artifacts."""
    if not math.isfinite(gap_mm) or gap_mm < 0:
        raise ValueError("间隙必须是大于等于 0 的数字")
    pieces, ignored = load_closed_lwpolylines(input_path)
    if not pieces:
        raise ValueError("输入 DXF 中没有可用的闭合 LWPOLYLINE 外轮廓")
    output_dir.mkdir(parents=True, exist_ok=True)
    runs = {}
    for leather in leathers:
        name = size_name(leather)
        result = fill_leather(pieces, leather, gap_mm) if fill else pack_pieces(pieces, leather, gap_mm)
        write_dxf(output_dir / f"{name}.dxf", result, leather, gap_mm)
        write_preview(output_dir / f"{name}.png", result, leather, gap_mm)
        runs[name] = {
            "leather_mm": {"width": leather[0], "height": leather[1]},
            "placed_indices": [placement.piece.index for placement in result.placements],
            "placed_counts": result.placed_counts,
            "rotations_degrees": [placement.rotation_degrees for placement in result.placements],
            "unplaced_indices": [piece.index for piece in result.unplaced],
            "used_area_mm2": round(sum(placement.piece.area for placement in result.placements), 3),
            "leather_area_mm2": leather[0] * leather[1],
            "utilization_percent": round(utilization_percent(result, leather), 4),
        }
    summary = {
        "input": str(input_path),
        "coordinate_unit_assumption": "mm",
        "gap_mm": gap_mm,
        "fill_until_full": fill,
        "allowed_rotations_degrees": [0, 180] if fill else [0],
        "input_piece_count": len(pieces),
        "ignored_entities": dict(ignored),
        "runs": runs,
    }
    (output_dir / "summary.json").write_text(
        json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    return summary


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="固定方向皮革鞋面自动排样 Demo")
    parser.add_argument(
        "--input",
        type=Path,
        default=Path(__file__).with_name("凉鞋.dxf"),
        help="输入 DXF；默认使用同目录下的 凉鞋.dxf",
    )
    parser.add_argument("--output-dir", type=Path, default=Path("demo_output"))
    parser.add_argument("--gap-mm", type=float, default=5.0)
    parser.add_argument(
        "--leather",
        nargs="+",
        default=["2000x1000", "2000x4000", "2000x9000"],
        help="一个或多个 WIDTHxHEIGHT 毫米尺寸",
    )
    parser.add_argument(
        "--single-set",
        action="store_true",
        help="只排输入文件中的一套 9 个尺码，不重复填满皮革",
    )
    return parser


def main() -> int:
    arguments = build_parser().parse_args()
    try:
        leathers = [parse_leather_size(value) for value in arguments.leather]
        summary = run_demo(
            arguments.input,
            arguments.output_dir,
            arguments.gap_mm,
            leathers,
            fill=not arguments.single_set,
        )
    except (OSError, ValueError, ezdxf.DXFError) as error:
        print(f"错误：{error}")
        return 2
    for name, run in summary["runs"].items():
        print(
            f"{name} mm: 放入 {len(run['placed_indices'])} 件，"
            f"利用率 {run['utilization_percent']:.2f}%"
        )
    print(f"已输出到：{arguments.output_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
