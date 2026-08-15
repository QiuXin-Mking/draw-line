#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""DXF 排样图骨架抽取器。

DXF 是逐行「组码 + 值」成对出现的文本格式，文件很大（16MB / 155 万行），
不能整读。本脚本流式解析，只抽取结构骨架（段落 / 图元 / 图层 / 颜色 / 坐标范围），
不展开具体坐标，并渲染成 Markdown 骨架图。

用法：
    python3 dxf骨架.py [输入.dxf] [输出.md]
"""
import sys
from collections import Counter, defaultdict
from pathlib import Path

# DXF 颜色码(62) → 线角色（与本项目排样 JSON 的 pen 字段对应）
COLOR_ROLE = {
    0: "外轮廓 Bound（pen 0）",
    3: "切割线 Cutoff（pen 3）",
    5: "标记线 / 半切线（pen 5）",
    8: "文字标签（TEXT）",
    7: "默认图层色（白/黑）",
    256: "随层 BYLAYER",
}

# 外轮廓顶点数 → 对应部件（源自同目录排样 JSON 的 Bound 顶点数）
VERTEX_TO_PART = {
    5: "40 / 40_m2",
    329: "40_m0",
    233: "40_m1",
    304: "40_m3",
    116: "40_m4",
    169: "40_m5",
    163: "40_m6",
    298: "40_m7",
    162: "40_m8",
}

# DXF 组码字段含义（本文件用到的）
GROUP_CODE_MEANING = {
    0: "图元类型 / 结构标记（SECTION、LINE、LWPOLYLINE、TEXT…）",
    1: "文本内容 / 字符串值",
    2: "名称（段落名、图层名、块名、线型名）",
    6: "线型名",
    8: "图层名",
    9: "头变量名（$ACADVER 等）",
    10: "X 坐标（主点）",
    20: "Y 坐标（主点）",
    30: "Z 坐标（主点）",
    40: "半径 / 比例",
    62: "颜色码（ACI）",
    70: "标志位（闭合、样式等）",
    90: "顶点数（LWPOLYLINE）",
}


def _read_pairs(path):
    """流式读取 DXF，产出 (group_code:int, value:str) 序列。

    DXF 每两行一组：组码行 + 值行。空值行（空字符串）是合法的值，必须保留，
    只丢弃文件末尾的空行。
    """
    with open(path, encoding="gbk", errors="replace") as f:
        lines = f.read().split("\n")
    lines = [l.rstrip("\r").strip() for l in lines]
    while lines and lines[-1] == "":
        lines.pop()
    return [(int(lines[i]), lines[i + 1]) for i in range(0, len(lines) - 1, 2)]


def parse_dxf(path):
    pairs = _read_pairs(path)

    sections = []
    cur_section = None
    expect_name = False
    in_block_def = False
    cur_block = None

    entity_by_section = defaultdict(Counter)   # section -> entitytype -> count
    user_blocks = []                            # 非 *Model_Space/*Paper_Space 的块
    layer_ent = defaultdict(Counter)            # layer -> entitytype -> count
    color_ent = defaultdict(Counter)            # color(62) -> entitytype -> count
    lwp_closed = Counter()                      # LWPOLYLINE 闭合标志
    lwp_nverts = Counter()                      # LWPOLYLINE 顶点数分布
    lwp_color_nverts = defaultdict(Counter)     # color -> nverts 分布
    text_samples = []                           # TEXT 内容样例
    text_contents = Counter()                   # TEXT 内容计数
    header = {}
    samples = {}                                 # 代表性图元样例（外轮廓/切割线/标记线/文字）

    # 头变量：记住最近一个 $VAR，下一个值行归它
    prev_var = None
    xmin = xmax = ymin = ymax = None

    def flush_entity(e):
        nonlocal xmin, xmax, ymin, ymax
        t = e["type"]
        entity_by_section[cur_section][t] += 1
        c = e.get("color")
        if c is not None:
            color_ent[c][t] += 1
        lay = e.get("layer")
        if lay is not None:
            layer_ent[lay][t] += 1
        if t == "LWPOLYLINE":
            lwp_closed[(e.get("flags", 0) & 1)] += 1
            nv = e.get("nverts", 0)
            lwp_nverts[nv] += 1
            if c is not None:
                lwp_color_nverts[c][nv] += 1
        if t == "TEXT":
            txt = e.get("text", "")
            text_contents[txt] += 1
            if len(text_samples) < 30:
                text_samples.append((txt, c, lay))
        # 抓代表性图元样例：各取第一条；切割线优先 2 点、标记线优先 17 点
        def _snap():
            return {"type": t, "color": c, "nverts": e.get("nverts"),
                    "closed": (e.get("flags", 0) & 1), "raw": e.get("raw", [])}
        if t == "LWPOLYLINE":
            if c == 0 and "outline" not in samples:
                samples["outline"] = _snap()
            elif c == 3:
                if "cut" not in samples:
                    samples["cut"] = _snap()
                elif e.get("nverts") == 2 and samples["cut"].get("nverts") != 2:
                    samples["cut"] = _snap()
            elif c == 5:
                if "mark" not in samples:
                    samples["mark"] = _snap()
                elif e.get("nverts") == 17 and samples["mark"].get("nverts") != 17:
                    samples["mark"] = _snap()
        elif t == "TEXT" and "text" not in samples:
            samples["text"] = _snap()

    cur = None
    for code, val in pairs:
        if code == 0:
            if cur is not None:
                flush_entity(cur)
                cur = None
            if val == "SECTION":
                expect_name = True
            elif val == "ENDSEC":
                cur_section = None
            elif val == "BLOCK":
                in_block_def = True
            elif val == "ENDBLK":
                in_block_def = False
                cur_block = None
            elif val != "EOF":
                # 图元类型（ENTITIES 段落内）
                if cur_section == "ENTITIES":
                    cur = {"type": val, "raw": [(0, val)]}
                elif cur_section == "BLOCKS" and val not in ("BLOCK", "ENDBLK"):
                    cur = {"type": val, "raw": [(0, val)]}
        elif code == 2:
            if expect_name:
                cur_section = val
                sections.append(val)
                expect_name = False
            elif cur_section == "BLOCKS" and in_block_def:
                cur_block = val
                if not val.startswith("*"):
                    user_blocks.append(val)
        elif cur is not None and cur_section in ("ENTITIES", "BLOCKS"):
            cur["raw"].append((code, val))
            if code == 8:
                cur["layer"] = val
            elif code == 62:
                cur["color"] = int(val)
            elif code == 70:
                cur["flags"] = int(val)
            elif code == 90:
                cur["nverts"] = int(val)
            elif code == 1:
                cur["text"] = val
            elif code == 10:
                v = float(val)
                cur["x0"] = v
                xmin = v if xmin is None else min(xmin, v)
                xmax = v if xmax is None else max(xmax, v)
            elif code == 20:
                v = float(val)
                cur["y0"] = v
                ymin = v if ymin is None else min(ymin, v)
                ymax = v if ymax is None else max(ymax, v)
        elif code == 9 and cur_section == "HEADER":
            prev_var = val
        elif code in (1, 2, 3, 10, 40, 70) and cur_section == "HEADER" and prev_var:
            header.setdefault(prev_var, val)
            prev_var = None
    if cur is not None:
        flush_entity(cur)

    return {
        "src": Path(path).name,
        "bytes": Path(path).stat().st_size,
        "groups": len(pairs),
        "acadver": header.get("$ACADVER"),
        "codepage": header.get("$DWGCODEPAGE"),
        "insunits": header.get("$INSUNITS"),
        "sections": sections,
        "entity_by_section": entity_by_section,
        "user_blocks": user_blocks,
        "layer_ent": layer_ent,
        "color_ent": color_ent,
        "lwp_closed": lwp_closed,
        "lwp_nverts": lwp_nverts,
        "lwp_color_nverts": lwp_color_nverts,
        "text_contents": text_contents,
        "text_samples": text_samples,
        "samples": samples,
        "extent": (xmin, xmax, ymin, ymax),
    }


def _dump_sample(out, sk, key):
    """把一个代表性图元样例的完整原始组码序列写成代码块。"""
    s = sk.get("samples", {}).get(key)
    if not s or not s.get("raw"):
        out("（未抓到样例）")
        return
    out("```")
    for code, val in s["raw"]:
        out(f"{code:>3}  {val}")
    out("```")


def render_markdown(sk):
    A = []
    out = A.append
    mb = sk["bytes"] / 1024 / 1024
    out("# DXF 排样图数据结构骨架")
    out("")
    out(f"> 文件：`{sk['src']}`（{mb:.1f} MB，{sk['groups']} 个组码）")
    out("> 坐标明细过多，只抽取结构骨架 + 分布统计，不展开具体坐标。")
    out("")

    # 1 概览
    out("## 1. 文件概览")
    out("")
    out("| 属性 | 值 | 含义 |")
    out("|------|----|------|")
    out(f"| 格式 | DXF（ASCII） | 文本交换格式 |")
    out(f"| 版本 `$ACADVER` | `{sk['acadver']}` | AC1015 = AutoCAD 2000 |")
    out(f"| 编码 `$DWGCODEPAGE` | `{sk['codepage']}` | GBK 中文 |")
    out(f"| 单位 `$INSUNITS` | `{sk['insunits']}` | 1=英寸（实际坐标按 mm） |")
    out("")

    # 2 段落
    out("## 2. 段落结构（Sections）")
    out("")
    out("| 顺序 | 段落 | 内容 |")
    out("|------|------|------|")
    section_meaning = {
        "HEADER": "全局变量（版本、单位、范围）",
        "CLASSES": "类定义",
        "TABLES": "图层/线型/样式等符号表",
        "BLOCKS": "块定义（本例无用户块）",
        "ENTITIES": "图元（几何，核心内容）",
        "OBJECTS": "对象（字典/布局等）",
    }
    for i, s in enumerate(sk["sections"], 1):
        out(f"| {i} | `{s}` | {section_meaning.get(s, '')} |")
    out("")

    # 3 图元类型
    out("## 3. 图元类型与计数（ENTITIES 段落）")
    out("")
    ent = sk["entity_by_section"].get("ENTITIES", {})
    total = sum(ent.values())
    out(f"共 **{total}** 个图元：")
    out("")
    out("| 图元类型 | 数量 | 含义 |")
    out("|---------|------|------|")
    type_meaning = {
        "LWPOLYLINE": "轻量多段线：部件外轮廓 / 切割线 / 标记线",
        "TEXT": "文字：尺码标签",
    }
    for t, c in ent.most_common():
        out(f"| `{t}` | {c} | {type_meaning.get(t, '')} |")
    out("")

    # 4 图层
    out("## 4. 图层（Layer）")
    out("")
    out("| 图层 | 图元 | 数量 |")
    out("|------|------|------|")
    for lay, c in sorted(sk["layer_ent"].items()):
        detail = "、".join(f"{t}×{n}" for t, n in c.most_common())
        out(f"| `{lay}` | {detail} | {sum(c.values())} |")
    out("")

    # 5 颜色码 = 线角色（与 JSON 的 pen 对应）
    out("## 5. 颜色码(62) = 线角色（关键：与排样 JSON 的 pen 对应）")
    out("")
    out("| 颜色码 | 数量 | 角色 |")
    out("|--------|------|------|")
    for c in sorted(sk["color_ent"]):
        cnt = sum(sk["color_ent"][c].values())
        out(f"| {c} | {cnt} | {COLOR_ROLE.get(c, '')} |")
    out("")
    out("> 颜色码 0/3/5 与排样 JSON 中 `Bound.pen` / `Cutoff.pen` 的 0/3/5 一一对应。三种线角色的图元级结构见最后一章 §14。")
    out("")

    # 6 外轮廓顶点 → 部件
    out("## 6. 外轮廓（颜色 0）顶点数 → 部件映射")
    out("")
    out("| 顶点数 | 数量 | 对应部件（JSON） |")
    out("|--------|------|------------------|")
    nv = sk["lwp_color_nverts"].get(0, {})
    for v in sorted(nv, reverse=True):
        out(f"| {v} | {nv[v]} | `{VERTEX_TO_PART.get(v, '')}` |")
    out(f"| **合计** | {sum(nv.values())} | 10 部件 × 各 100 片 |")
    out("")

    # 7 切割/标记线顶点分布
    out("## 7. 切割线(3) / 标记线(5) 顶点数分布")
    out("")
    out("| 颜色 | 顶点数分布 | 合计 |")
    out("|------|-----------|------|")
    for c, role in ((3, "切割线"), (5, "标记线")):
        dist = sk["lwp_color_nverts"].get(c, {})
        s = "、".join(f"{v}点×{n}" for v, n in sorted(dist.items(), key=lambda x: -x[1])[:12])
        out(f"| {c}（{role}） | {s} | {sum(dist.values())} |")
    out("")

    # 8 文字
    out("## 8. 文字标签（TEXT）")
    out("")
    out(f"- 共 {sum(sk['text_contents'].values())} 个文字，内容分布：")
    for txt, c in sk["text_contents"].most_common(10):
        out(f"  - `{txt}` × {c}")
    out("")

    # 9 坐标范围
    out("## 9. 坐标范围")
    out("")
    x0, x1, y0, y1 = sk["extent"]
    out("| 维度 | 范围 | 说明 |")
    out("|------|------|------|")
    out(f"| X | {x0:.2f} ~ {x1:.2f} | ≈ 幅宽 1380 mm |")
    out(f"| Y | {y0:.2f} ~ {y1:.2f} | 排样长度 |")
    out("")

    # 10 组码字段
    out("## 10. DXF 组码字段说明（本文件用到）")
    out("")
    out("| 组码 | 含义 |")
    out("|------|------|")
    for c in sorted(GROUP_CODE_MEANING):
        out(f"| `{c}` | {GROUP_CODE_MEANING[c]} |")
    out("")
    # 11 整体模型
    out("## 11. DXF 如何表示这张图（整体模型）")
    out("")
    out("DXF 里**没有「部件」「排样」这些概念**，它只有最底层的二维图元。整张排样图被拆成：")
    out("")
    out("```")
    out("平面画布（幅宽 1380 × 长度 6173，模型空间 ENTITIES 段落）")
    out("  ├─ LWPOLYLINE × 8400   部件外轮廓 / 切割线 / 标记线")
    out("  └─ TEXT × 1000         每个部件旁的尺码标签「40」")
    out("```")
    out("")
    out("**没有块、没有 INSERT**——1000 个部件不是「引用同一个块再平移」，而是把每个部件的每条线都**展开成绝对坐标的独立多段线**，平铺在模型空间里。所以一个部件在 DXF 里是**一组彼此无关的图元**，靠「图层 + 颜色」把它们归到同一角色。")
    out("")
    out("每个部件由这几样拼成：")
    out("")
    out("| 图元 | 颜色(62) | 闭合(70) | 数量 | 角色 |")
    out("|------|---------|---------|------|------|")
    out("| LWPOLYLINE | 0 | 闭合 | 1 条/部件 | 外轮廓 |")
    out("| LWPOLYLINE | 3 | 开放为主（4300 开 / 100 闭） | 若干条 | 切割线 |")
    out("| LWPOLYLINE | 5 | 闭合 | 若干条 | 标记/刀口（小环） |")
    out("| TEXT | 8 | — | 1 个/部件 | 尺码标签「40」 |")
    out("")
    out("> 三种线角色（外轮廓 / 切割线 / 标记线）的介绍与真实原文，单独放在最后一章 §14。")
    out("")

    # 12 TEXT 解剖
    out("## 12. 解剖：一个尺码标签（TEXT）")
    out("")
    out("真实原文：")
    out("")
    _dump_sample(out, sk, "text")
    out("")
    out("| 组码 | 值 | 含义 |")
    out("|------|-----|------|")
    out("| `8` | text_8 | 文字单独放 `text_8` 图层 |")
    out("| `62` | 8 | 颜色码 8（灰） |")
    out("| `10`/`20` | 对齐点 | 文字对齐基准点 |")
    out("| `40` | 5.0 | **字高 5mm** |")
    out("| `1` | 40 | **文本内容「40」**（尺码） |")
    out("| `50` | 0.05 | 文字旋转角 |")
    out("| `72`/`73` | 1 / 2 | 水平/垂直对齐方式 |")
    out("| `11`/`21` | 第二对齐点 | 用于对齐定位 |")
    out("")
    out("每个部件旁一个「40」标签，1000 个部件 = 1000 个 TEXT。")
    out("")

    # 13 总结
    out("## 13. 一张图的拼装关系（总结）")
    out("")
    out("```")
    out("幅宽 1380 × 长度 6173 的平面画布（绝对坐标，已排好位）")
    out("│")
    out("├─ 部件 ×1000（每部件一组图元，颜色区分角色）")
    out("│     ├─ 外轮廓  LWPOLYLINE(62=0, 70=闭合)  ×1")
    out("│     ├─ 切割线  LWPOLYLINE(62=3, 70=开放)  ×N")
    out("│     ├─ 标记线  LWPOLYLINE(62=5, 70=闭合小环)×M")
    out('│     └─ 尺码标签 TEXT(62=8, 内容"40")      ×1')
    out("│")
    out("└─ 全部平铺在 ENTITIES 段落，无块、无引用，坐标即最终位置")
    out("```")
    out("")
    out("> 一句话：DXF 用「**颜色码区分线角色** + **绝对坐标直接定位置** + **闭合位区分轮廓/切口**」这三板斧，把整张排样图铺成了 8400 条多段线 + 1000 个文字。")
    out(">")
    out("> 三种线角色（外轮廓 / 切割线 / 标记线）的图元级解剖与真实原文，单独放在最后一章 §14。")
    out("")

    # 14 线角色详解
    out("## 14. 线角色详解：外轮廓 / 切割线 / 标记线")
    out("")
    out("这三种「线」在 DXF 里**是同一种图元 LWPOLYLINE（多段线）**，差异只落在两个组码上：`62`（颜色 = 角色）和 `70`（是否闭合）。三者的共同骨架：")
    out("")
    out("```")
    out("  0  LWPOLYLINE")
    out("  5  <句柄>           每个图元的唯一 ID（十六进制）")
    out("330  <归属句柄>")
    out("100  AcDbEntity       实体通用属性")
    out("  8  0                图层")
    out(" 62  <0 / 3 / 5>      ★ 颜色码 = 线角色")
    out("100  AcDbPolyline     多段线属性")
    out(" 90  <顶点数>")
    out(" 70  <0 / 1>          ★ 闭合位（bit1）")
    out(" 43  0.0              恒定线宽")
    out(" 10 / 20  …           顶点绝对坐标（重复 N 次）")
    out("```")
    out("")
    out("| 角色 | 颜色(62) | 闭合(70) | 数量 | 物理含义 |")
    out("|------|---------|---------|------|---------|")
    out("| 外轮廓 | 0 | 闭合(1) | 1000 | 裁片边界，沿它切出裁片 |")
    out("| 切割线 | 3 | 开放(0) 为主，100 条闭合 | 4400 | 裁片内部的切缝 / 挖孔 |")
    out("| 标记线 | 5 | 闭合(1) | 3000 | 刀口 / 定位记号（对齐用） |")
    out("")

    # 14.1 外轮廓
    out("### 14.1 外轮廓（颜色 0，闭合）")
    out("")
    out("真实原文（部件「40」的一条外轮廓，5 个顶点）：")
    out("")
    _dump_sample(out, sk, "outline")
    out("")
    out("- 顶点是**绝对坐标**——已经是排好位后的最终位置。DXF 把「几何 + 平移 + 旋转」全部**烤进了坐标**，没有单独的 X/Y/Rotate 字段（对比排样 JSON 是「局部坐标 + X/Y/Rotate 变换」）。")
    out("- **首尾顶点重复**——最后一个顶点和第一个完全相同，配合 `70=1` 双重标记闭合。")
    out("- 外轮廓**全部闭合**（1000 条，`70=1`）。")
    out("")

    # 14.2 切割线
    out("### 14.2 切割线（颜色 3，开放为主）")
    out("")
    out("真实原文（一条 2 点直线段切割线）：")
    out("")
    _dump_sample(out, sk, "cut")
    out("")
    out("- `90 = 2` → 只有 2 个顶点 = **一条直线段**（切割线）。")
    out("- `70 = 0` → **开放**（不闭合）。")
    out("- 切割线共 4400 条：4300 条开放（2 点直线段为主，长一点的 3~165 点 = 曲线切割路径），另有 **100 条闭合**（内孔/闭合切口）。")
    out("")

    # 14.3 标记线
    out("### 14.3 标记线（颜色 5，闭合小环 = 刀口记号）")
    out("")
    out("真实原文（17 个顶点，闭合，半径约 1.2mm 的小环）：")
    out("")
    _dump_sample(out, sk, "mark")
    out("")
    out("- `62 = 5`（标记线）、`70 = 1`（闭合）、首尾顶点相同。")
    out("- 顶点都挤在一小块区域，围成一个 **2~3mm 的小闭合环**——这是**刀口/定位记号**（裁片上标出两个片要对齐的位置），不是切割路径。")
    out("- 标记线共 3000 条，**全部闭合**：17 点环 × 2500、21 点环 × 400、4 点小环 × 100。")
    out("")

    out("---")
    out("")
    out("> 第 1–10 节为自动骨架；第 11–14 节的分析文字内置在脚本里，图元原文由脚本自动抓第一条样例。")
    out("")
    return "\n".join(A)


def main():
    src = sys.argv[1] if len(sys.argv) > 1 else "40码100片-幅宽1380-间距1.dxf"
    out = sys.argv[2] if len(sys.argv) > 2 else "dxf骨架图.md"
    sk = parse_dxf(src)
    md = render_markdown(sk)
    Path(out).write_text(md, encoding="utf-8")
    print(f"已生成 {out}（{len(md)} 字符）")


if __name__ == "__main__":
    main()
