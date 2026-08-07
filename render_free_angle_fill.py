#!/usr/bin/env python3
"""Render a complete 2 m x 1 m free-angle sandal-upper fill preview."""

import math
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

from leather_nesting_demo import bounds_of, load_closed_lwpolylines, polygon_area, translate_points


LEATHER = (2000.0, 1000.0)
GAP_MM = 5.0
ROTATIONS = (0, 175)


def rotate(points, degrees):
    radians = math.radians(degrees)
    cosine, sine = math.cos(radians), math.sin(radians)
    return tuple((x * cosine - y * sine, x * sine + y * cosine) for x, y in points)


def make_shelf_layout(pieces):
    """Keep a guaranteed clearance by separating every outline's bounding box."""
    width, height = LEATHER
    placements = []
    cursor = 0
    row_y = GAP_MM
    while row_y < height - GAP_MM:
        row_x, row_height, placed_in_row = GAP_MM, 0.0, False
        while row_x < width - GAP_MM:
            selected = None
            for offset in range(len(pieces)):
                piece_index = (cursor + offset) % len(pieces)
                piece = pieces[piece_index]
                degrees = ROTATIONS[(len(placements) + piece_index) % len(ROTATIONS)]
                shape = rotate(piece.points, degrees)
                shape_bounds = bounds_of(shape)
                shape_width = shape_bounds.max_x - shape_bounds.min_x
                shape_height = shape_bounds.max_y - shape_bounds.min_y
                if row_x + shape_width <= width - GAP_MM and row_y + shape_height <= height - GAP_MM:
                    selected = piece_index, piece, degrees, shape, shape_bounds
                    break
            if selected is None:
                break
            piece_index, piece, degrees, shape, shape_bounds = selected
            placed = translate_points(shape, row_x - shape_bounds.min_x, row_y - shape_bounds.min_y)
            placements.append((piece, degrees, placed))
            row_x += shape_bounds.max_x - shape_bounds.min_x + GAP_MM
            row_height = max(row_height, shape_bounds.max_y - shape_bounds.min_y)
            cursor = (piece_index + 1) % len(pieces)
            placed_in_row = True
        if not placed_in_row:
            break
        row_y += row_height + GAP_MM
    return placements


def main():
    pieces, _ = load_closed_lwpolylines(Path(__file__).with_name("凉鞋.dxf"))
    placements = make_shelf_layout(pieces)
    utilization = sum(piece.area for piece, _, _ in placements) / (LEATHER[0] * LEATHER[1]) * 100
    output = Path(__file__).with_name("demo_output") / "2000x1000_free_angle_fill.png"

    margin, scale = 44, 0.8
    canvas_width = round(LEATHER[0] * scale)
    canvas_height = round(LEATHER[1] * scale)
    image = Image.new("RGB", (canvas_width + margin * 2, canvas_height + margin * 2), "white")
    drawing = ImageDraw.Draw(image, "RGBA")
    small = ImageFont.truetype("/Library/Fonts/Verdana.ttf", 16)
    label = ImageFont.truetype("/Library/Fonts/Verdana.ttf", 13)

    def convert(point):
        return (margin + point[0] * scale, margin + (LEATHER[1] - point[1]) * scale)

    boundary = ((0, 0), (LEATHER[0], 0), (LEATHER[0], LEATHER[1]), (0, LEATHER[1]), (0, 0))
    drawing.line([convert(point) for point in boundary], fill="black", width=3)
    colors = [
        (31, 119, 180, 145), (255, 127, 14, 145), (44, 160, 44, 145),
        (214, 39, 40, 145), (148, 103, 189, 145), (140, 86, 75, 145),
        (227, 119, 194, 145), (127, 127, 127, 145), (188, 189, 34, 145),
    ]
    for piece, degrees, points in placements:
        screen_points = [convert(point) for point in points]
        color = colors[(piece.index - 1) % len(colors)]
        drawing.polygon(screen_points, fill=color)
        drawing.line(screen_points + [screen_points[0]], fill=color[:3] + (255,), width=2)
        center_x = sum(point[0] for point in points) / len(points)
        center_y = sum(point[1] for point in points) / len(points)
        drawing.text(convert((center_x, center_y)), f"P{piece.index:02d}\n{degrees}deg", fill="black", font=label, anchor="mm")

    drawing.text(
        (margin, 14),
        f"2000x1000 mm | gap {GAP_MM:g} mm | free-angle test: 0deg + 175deg | utilization {utilization:.2f}%",
        fill="black", font=small,
    )
    output.parent.mkdir(parents=True, exist_ok=True)
    image.save(output, format="PNG")
    print(f"{output}\nplacements={len(placements)} utilization={utilization:.4f}%")


if __name__ == "__main__":
    main()
