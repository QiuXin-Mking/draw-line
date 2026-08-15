#!/usr/bin/env bash
# 皮革划线排样 Demo · 一键展示脚本
#
# 用法（在 python-demo/ 目录下）：
#   bash run.sh        # 或 ./run.sh
#
# 首次运行会自动创建 .venv 并安装依赖；之后秒开。
set -euo pipefail
cd "$(dirname "$0")"

echo "════════════════════════════════════════════"
echo "  皮革划线排样 Demo · 一键展示"
echo "════════════════════════════════════════════"

# 1) Python 环境
PYTHON="${PYTHON:-python3}"
if ! command -v "$PYTHON" >/dev/null 2>&1; then
  echo "✗ 未找到 python3，请先安装 Python 3.9+"; exit 1
fi

# 2) 依赖（首次自动装到本地 .venv）
if [ ! -d ".venv" ]; then
  echo "→ 首次运行：创建虚拟环境并安装依赖…"
  "$PYTHON" -m venv .venv
  ./.venv/bin/pip install -q -r requirements.txt
fi
PY=".venv/bin/python"

# 3) 货架填充排样（三种皮革尺寸，0°/180° 交替）
echo ""
echo "→ [1/3] 货架填充排样（三种皮革）…"
"$PY" leather_nesting_demo.py \
  --input 凉鞋.dxf \
  --output-dir demo_output \
  --gap-mm 5 \
  --leather 2000x1000 2000x4000 2000x9000

# 4) 自由角度排样展示（0° + 175°）
echo ""
echo "→ [2/3] 自由角度排样展示…"
"$PY" render_free_angle_fill.py
"$PY" render_free_angle_showcase.py

# 5) 汇总
echo ""
echo "════════════════════════════════════════════"
echo "  生成产物"
echo "════════════════════════════════════════════"
for f in demo_output/*.dxf demo_output/*.png demo_output/summary.json; do
  [ -f "$f" ] && printf '  · %s\n' "$f"
done

echo ""
echo "→ 利用率汇总（summary.json）："
"$PY" - <<'PY'
import json
data = json.load(open("demo_output/summary.json", encoding="utf-8"))
for name, run in data["runs"].items():
    print(f"   {name} mm : 放入 {len(run['placed_indices'])} 件 · 利用率 {run['utilization_percent']:.2f}%")
PY

# 6) 自动打开预览（macOS）
if command -v open >/dev/null 2>&1; then
  echo ""
  echo "→ 打开预览图…"
  open demo_output/2000x1000.png demo_output/free_angle_175deg_showcase.png 2>/dev/null || true
fi

echo ""
echo "✅ 展示完成。产物在 demo_output/ 下（DXF / PNG / summary.json）。"
