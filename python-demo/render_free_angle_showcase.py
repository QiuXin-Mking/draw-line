#!/usr/bin/env python3
"""Render a truthful free-angle nesting showcase from the supplied sandal DXF."""

import math
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

from leather_nesting_demo import bounds_of, load_closed_lwpolylines, translate_points


def rotate(points, degrees):
    radians = math.radians(degrees)
    cosine, sine = math.cos(radians), math.sin(radians)
    return tuple((x * cosine - y * sine, x * sine + y * cosine) for x, y in points)


def main():
    source = Path(__file__).with_name("凉鞋.dxf")
    output = Path(__file__).with_name("demo_output") / "free_angle_175deg_showcase.png"
    pieces, _ = load_closed_lwpolylines(source)
    piece = pieces[0]

    first = rotate(piece.points, 0)
    second = rotate(piece.points, 175)
    first_bounds = bounds_of(first)
    second_bounds = bounds_of(second)
    first = translate_points(first, -first_bounds.min_x + 10, -first_bounds.min_y + 10)
    # The 165 mm offset was found by coarse angle search; the real contours keep ~2 mm clearance.
    second = translate_points(second, -second_bounds.min_x + 175, -second_bounds.min_y + 10)

    image = Image.new("RGB", (1600, 900), "#f8fafc")
    drawing = ImageDraw.Draw(image, "RGBA")
    regular = ImageFont.truetype("/Library/Fonts/Verdana.ttf", 28)
    title = ImageFont.truetype("/Library/Fonts/Verdana Bold.ttf", 42)
    small = ImageFont.truetype("/Library/Fonts/Verdana.ttf", 20)

    drawing.rounded_rectangle((80, 120, 1520, 770), radius=24, fill="#f4d6a6", outline="#633f23", width=5)
    drawing.text((80, 38), "SANDAL UPPER  |  FREE-ANGLE NESTING", font=title, fill="#192a3a")
    drawing.text((80, 88), "Real contour from SANDAL.DXF  •  2 mm clearance", font=small, fill="#516474")

    scale = 2.5
    origin_x, origin_y = 550, 315
    def to_canvas(points):
        return [(origin_x + x * scale, origin_y + y * scale) for x, y in points]

    first_canvas = to_canvas(first)
    second_canvas = to_canvas(second)
    drawing.polygon(first_canvas, fill=(29, 118, 181, 185), outline=(18, 76, 117, 255), width=5)
    drawing.polygon(second_canvas, fill=(235, 115, 50, 185), outline=(164, 74, 24, 255), width=5)
    drawing.text((590, 785), "0 DEG", font=regular, fill="#174a72")
    drawing.text((1110, 785), "175 DEG", font=regular, fill="#a44915")

    drawing.rounded_rectangle((105, 210, 435, 620), radius=20, fill=(255, 255, 255, 220), outline="#d2a469", width=2)
    drawing.text((135, 250), "COARSE SEARCH", font=regular, fill="#182b3a")
    drawing.text((135, 315), "BEST PAIR ANGLE", font=small, fill="#5f7180")
    drawing.text((135, 345), "~175 DEG", font=title, fill="#a44915")
    drawing.text((135, 425), "LOCAL DENSITY", font=small, fill="#5f7180")
    drawing.text((135, 455), "~61.2%", font=title, fill="#174a72")
    drawing.text((135, 545), "Not a full-hide", font=small, fill="#5f7180")
    drawing.text((135, 575), "global optimum.", font=small, fill="#5f7180")

    drawing.text((80, 820), "Finding: arbitrary rotation helps only slightly; the best angle is close to 180 DEG.", font=small, fill="#3e5262")
    output.parent.mkdir(parents=True, exist_ok=True)
    image.save(output, format="PNG")
    print(output)


if __name__ == "__main__":
    main()
