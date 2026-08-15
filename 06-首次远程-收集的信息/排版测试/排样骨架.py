#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""排样结果 JSON 骨架抽取器。

目标：数据文件里坐标明细太多（1.5MB / 2600 条），AI 无法整读。
本脚本只抽取「结构骨架」（schema + 分布统计），不展开具体坐标，
并渲染成 Markdown 骨架图。

用法：
    python3 排样骨架.py [输入.json] [输出.md]
"""
import json
import sys
from collections import Counter
from pathlib import Path

# 线型编码（pen）含义
PEN_MEANING = {
    0: "外轮廓（Bound，闭合折线）",
    3: "切割线（Cutoff，切穿）",
    5: "标记线（Cutoff，半切 / 记号）",
}

# 顶层字段语义（含仅几何载体才出现的字段）
FIELD_MEANING = {
    "Name": "部件名（款内子片编号，如 40 / 40_m0…40_m8）",
    "Size": "尺码（本例全为 40 码）",
    "StyleID": "款号 / 样式 ID（本例 = 40）",
    "Fabric": "面料号（本例 = 40）",
    "X": "放置原点 X（mm，沿幅宽方向）",
    "Y": "放置原点 Y（mm，沿卷料长度方向）",
    "Rotate": "旋转角（度）",
    "Rxx": "旋转矩阵分量（≈cosθ）",
    "Rxy": "旋转矩阵分量（≈sinθ）",
    "Ryx": "旋转矩阵分量（≈-sinθ）",
    "Ryy": "旋转矩阵分量（≈cosθ）",
    "Flip": "是否镜像（本例全为 False）",
    "Color": "部件染色 RGBA（[R,G,B,A]）",
    "Copys": "复制份数（仅几何载体）",
    "Material": "物料编码（仅几何载体，'P_00030;ch 0'）",
    "MaterialID": "物料 ID（仅几何载体）",
    "OrderID": "订单 ID（仅几何载体）",
    "Blocks": "部件几何轮廓列表（仅几何载体）",
}


def _curve_points(flat):
    """Curve 为扁平化 [x0,y0,x1,y1,...]，返回点数。"""
    return len(flat) // 2


def _type_of(v):
    """把 JSON 值映射成简洁类型字符串。"""
    if isinstance(v, bool):
        return "bool"
    if isinstance(v, int):
        return "int"
    if isinstance(v, float):
        return "float"
    if isinstance(v, str):
        return "string"
    if isinstance(v, list):
        if v and all(isinstance(x, (int, float)) for x in v):
            return f"number[{len(v)}]"
        if v and isinstance(v[0], dict):
            return f"object[{len(v)}]"
        return "array"
    if isinstance(v, dict):
        return "object"
    return type(v).__name__


def _fmt_rgba(c):
    return f"({c[0]},{c[1]},{c[2]},{c[3]})"


def extract_skeleton(path):
    """解析 JSON 并返回结构骨架 dict（不含坐标明细）。"""
    with open(path, encoding="utf-8") as f:
        data = json.load(f)

    n = len(data)
    geo = [e for e in data if e.get("Blocks")]       # 几何载体
    plain = [e for e in data if not e.get("Blocks")]  # 纯放置

    # 顶层字段集合（含缺省情况）
    all_keys = sorted(set().union(*(e.keys() for e in data)))
    geo_only = sorted(set().union(*(e.keys() for e in geo)) - set().union(*(e.keys() for e in plain)))
    common = sorted(set(all_keys) - set(geo_only))

    # 坐标范围
    xs = [e["X"] for e in data]
    ys = [e["Y"] for e in data]
    rs = [e["Rotate"] for e in data]

    # 部件分布：Name → 数量 / 颜色
    name_count = Counter(e["Name"] for e in data)
    name_color = {}
    for e in data:
        name_color.setdefault(e["Name"], e["Color"])

    # 几何块（部件轮廓）摘要
    blocks = []
    for e in geo:
        for b in e.get("Blocks", []):
            bd = b["Bound"]
            blocks.append({
                "Name": b["Name"],
                "BoundPts": _curve_points(bd["Curve"]),
                "BoundPen": bd["pen"],
                "Cutoffs": [(len(c["Curve"]) // 2, c["pen"]) for c in b["Cutoff"]],
            })

    # pen 取值全集
    pens = {0}
    for e in geo:
        for b in e.get("Blocks", []):
            pens.add(b["Bound"]["pen"])
            for c in b["Cutoff"]:
                pens.add(c["pen"])

    # 各字段的实际 JSON 类型（取首个非空样本）
    field_types = {}
    for k in all_keys:
        for e in data:
            if k in e and e[k] is not None:
                field_types[k] = _type_of(e[k])
                break

    return {
        "src": Path(path).name,
        "bytes": Path(path).stat().st_size,
        "count": n,
        "geo_count": len(geo),
        "plain_count": len(plain),
        "common": common,
        "geo_only": geo_only,
        "x_range": (min(xs), max(xs)),
        "y_range": (min(ys), max(ys)),
        "rot_range": (min(rs), max(rs)),
        "name_count": name_count,
        "name_color": name_color,
        "blocks": blocks,
        "pens": sorted(pens),
        "field_types": field_types,
    }


def render_markdown(sk):
    """把骨架 dict 渲染成 Markdown。"""
    L = []
    A = L.append
    A("# 排样结果数据结构骨架")
    A("")
    A(f"> 数据文件：`{sk['src']}`（{sk['bytes']/1024/1024:.2f} MB，{sk['count']} 条记录）")
    A("> 坐标明细过多，本文只抽取**结构骨架 + 分布统计**，不展开具体坐标。")
    A("")

    # 1 顶层
    A("## 1. 顶层结构")
    A("")
    A("- JSON 顶层是一个**数组**（`list`），长度 **{}**。".format(sk["count"]))
    A("- 每条记录是一个「放置（placement）」对象，但字段集合按角色分两类。")
    A("")

    # 2 角色
    A("## 2. 记录角色（两种）")
    A("")
    A("| 角色 | 数量 | 特征 |")
    A("|------|------|------|")
    A("| 几何载体（带 `Blocks`） | {} | 额外携带几何轮廓 + 物料/订单字段 |".format(sk["geo_count"]))
    A("| 纯放置实例（无 `Blocks`） | {} | 只有位置/姿态/身份字段 |".format(sk["plain_count"]))
    A("")
    A("- 唯一的几何载体 `data[0]` 既是「40」款的一个放置，又携带整款几何（10 个部件轮廓）。")
    A("- 其余 {} 条只引用部件名（`Name`），不重复携带几何。".format(sk["plain_count"]))
    A("")

    # 3 字段总表
    A("## 3. 字段总表")
    A("")
    A("### 3.1 公共放置字段（{} 条都有）".format(sk["count"]))
    A("")
    A("| 字段 | 类型 | 含义 |")
    A("|------|------|------|")
    for k in sk["common"]:
        A("| `{}` | {} | {} |".format(k, sk["field_types"].get(k, "—"), FIELD_MEANING.get(k, "—")))
    A("")
    A("### 3.2 仅几何载体字段（{} 条）".format(sk["geo_count"]))
    A("")
    A("| 字段 | 类型 | 含义 |")
    A("|------|------|------|")
    for k in sk["geo_only"]:
        A("| `{}` | {} | {} |".format(k, sk["field_types"].get(k, "—"), FIELD_MEANING.get(k, "—")))
    A("")

    # 4 几何结构
    A("## 4. 几何结构（Blocks）")
    A("")
    A("```")
    A("Blocks[]                    部件几何列表（款「40」共 10 个 block）")
    A("  └─ Bound {                外轮廓")
    A("        Curve: [x0,y0,x1,y1,…]  扁平化坐标对")
    A("        Power: 1              曲线阶（1 = 折线/直线段）")
    A("        pen: 0                线型 = 外轮廓")
    A("     }")
    A("  └─ Cutoff[]               内部切割 / 标记线列表")
    A("        └─ { Curve, Power, pen }  结构同 Bound；pen = 3 切割 / 5 标记")
    A("  └─ Fabric / Name / Size    归属面料 / 部件名 / 尺码")
    A("```")
    A("")
    A("**pen（线型）取值：**")
    A("")
    A("| pen | 含义 |")
    A("|-----|------|")
    for p in sk["pens"]:
        A("| {} | {} |".format(p, PEN_MEANING.get(p, "—")))
    A("")

    # 5 部件清单
    A("## 5. 部件清单（款「40」的 10 个部件）")
    A("")
    A("| 部件 Name | 放置数量 | 颜色 RGBA | 外轮廓点数 | 切割/标记线数 |")
    A("|-----------|---------|-----------|-----------|--------------|")
    for b in sk["blocks"]:
        nm = b["Name"]
        cnt = sk["name_count"].get(nm, 0)
        col = _fmt_rgba(sk["name_color"].get(nm, [0, 0, 0, 0]))
        nc = len(b["Cutoffs"])
        A("| `{}` | {} | `{}` | {} | {} |".format(nm, cnt, col, b["BoundPts"], nc))
    A("")

    # 6 坐标范围
    A("## 6. 坐标与规模")
    A("")
    A("| 维度 | 范围 | 说明 |")
    A("|------|------|------|")
    A("| X | {:.2f} ~ {:.2f} | ≈ 幅宽 1380 mm |".format(*sk["x_range"]))
    A("| Y | {:.2f} ~ {:.2f} | 沿卷料长度方向 |".format(*sk["y_range"]))
    A("| Rotate | {:.2f} ~ {:.2f} | 度 |".format(*sk["rot_range"]))
    A("")
    A("---")
    A("")
    A("> 生成脚本：`排样骨架.py`（`extract_skeleton()` → `render_markdown()`）")
    A("")
    return "\n".join(L)


def main():
    src = sys.argv[1] if len(sys.argv) > 1 else "260812_110920.json"
    out = sys.argv[2] if len(sys.argv) > 2 else "排样骨架图.md"
    sk = extract_skeleton(src)
    md = render_markdown(sk)
    Path(out).write_text(md, encoding="utf-8")
    print(f"已生成 {out}（{len(md)} 字符）")


if __name__ == "__main__":
    main()
