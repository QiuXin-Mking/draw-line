#!/usr/bin/env python3
"""DXF 快速预览脚本 —— 将 DXF 渲染为 PNG 图片，不依赖任何 GUI 软件。"""

import sys
from pathlib import Path

import ezdxf
import matplotlib

matplotlib.rcParams["font.sans-serif"] = [
    "Arial Unicode MS", "Heiti SC", "PingFang SC", "SimHei", "sans-serif"
]
matplotlib.rcParams["axes.unicode_minus"] = False
import matplotlib.pyplot as plt


def get_polyline_points(e):
    """从 LWPOLYLINE 或 POLYLINE 实体提取顶点坐标列表 [(x, y), ...]"""
    pts = []
    if e.dxftype() == "LWPOLYLINE":
        pts = [(p[0], p[1]) for p in e.get_points()]
    elif e.dxftype() == "POLYLINE":
        pts = [(v.dxf.location.x, v.dxf.location.y) for v in e.vertices]
    return pts


def view_dxf(dxf_path, output_png=None):
    """读取 DXF，绘制所有多段线、直线、圆、弧、文本，保存 PNG 并打开。"""
    doc = ezdxf.readfile(dxf_path)
    msp = doc.modelspace()

    fig, ax = plt.subplots(figsize=(12, 10))
    ax.set_aspect("equal")
    ax.set_title(f"{Path(dxf_path).name}", fontsize=14)

    colors = plt.cm.tab10.colors
    color_idx = 0

    for e in msp:
        t = e.dxftype()

        if t in ("LWPOLYLINE", "POLYLINE"):
            pts = get_polyline_points(e)
            if not pts or len(pts) < 2:
                continue
            xs, ys = zip(*pts)
            ax.fill(xs, ys, alpha=0.3, color=colors[color_idx % len(colors)])
            ax.plot(xs, ys, color=colors[color_idx % len(colors)], linewidth=1.5)
            color_idx += 1

        elif t == "LINE":
            s = e.dxf.start
            e_end = e.dxf.end
            ax.plot([s.x, e_end.x], [s.y, e_end.y], color="black", linewidth=1.0)

        elif t == "CIRCLE":
            c = e.dxf.center
            import matplotlib.patches as mpatches
            circle = mpatches.Circle(
                (c.x, c.y), e.dxf.radius, fill=False, color="black", linewidth=1.0
            )
            ax.add_patch(circle)

        elif t == "ARC":
            c = e.dxf.center
            r = e.dxf.radius
            import matplotlib.patches as mpatches
            arc = mpatches.Arc(
                (c.x, c.y), 2 * r, 2 * r,
                angle=0,
                theta1=e.dxf.start_angle,
                theta2=e.dxf.end_angle,
                color="black", linewidth=1.0
            )
            ax.add_patch(arc)

        elif t == "TEXT":
            insert = e.dxf.insert
            ax.text(insert.x, insert.y, e.dxf.text, fontsize=6, ha="center", va="center")

    ax.autoscale()
    fig.tight_layout()

    if output_png is None:
        output_png = Path(dxf_path).with_suffix(".png")

    fig.savefig(output_png, dpi=150)
    print(f"✅ 已保存: {output_png}")

    import subprocess
    subprocess.run(["open", str(output_png)])
    return output_png


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("用法: python3 view_dxf.py <DXF文件路径> [输出PNG路径]")
        sys.exit(1)

    dxf = sys.argv[1]
    png = sys.argv[2] if len(sys.argv) > 2 else None
    view_dxf(dxf, png)
