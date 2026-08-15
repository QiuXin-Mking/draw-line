# 皮革划线排样 Demo（Python 展示版）

独立出来的 Python 排样 Demo，用于**对外展示**排样算法效果：读入凉鞋鞋面裁片的 DXF 轮廓 → 在矩形皮革上自动排样 → 输出 DXF 排样图、PNG 预览和利用率汇总。

> 定位：这是**演示用**的确定性货架填充 Demo，不追求全局最优。正式产品是 C# 排样引擎（`src/LeatherNesting.Geometry/Nesting/`），基于 NFP + 局部搜索、支持任意角度。本 Demo 不再作为算法参照演进。

## 一键运行

```bash
cd python-demo
./run.sh        # 或 bash run.sh
```

首次运行会自动创建 `.venv` 并安装依赖（`ezdxf`、`Pillow`、`matplotlib`），之后秒开。脚本会：

1. 货架填充排样：三种皮革尺寸（2000×1000 / 2000×4000 / 2000×9000 mm），0°/180° 交替；
2. 自由角度排样展示：0° 与 175° 贴靠，输出展示图；
3. 汇总利用率并自动打开预览图（macOS）。

## 产物（`demo_output/`）

| 文件 | 说明 |
|---|---|
| `2000x1000.dxf` / `2000x4000.dxf` / `2000x9000.dxf` | 可用 CAD 打开的排样图（三图层：皮革 / 裁片 / 标注） |
| 同名 `.png` | 现场演示用的排样预览 |
| `2000x1000_free_angle_fill.png` | 自由角度（0° + 175°）整张填充预览 |
| `free_angle_175deg_showcase.png` | 两件鞋面 0°/175° 贴靠的展示图 |
| `summary.json` | 每种皮革的放入件数、未放入项、利用率 |

## 单独运行某个脚本

```bash
.venv/bin/python leather_nesting_demo.py --input 凉鞋.dxf --gap-mm 5 --leather 2000x1000
.venv/bin/python render_free_angle_fill.py
.venv/bin/python render_free_angle_showcase.py
.venv/bin/python view_dxf.py <DXF文件路径>   # 把任意 DXF 快速渲染成 PNG 并打开
```

## 说明

- 输入 `凉鞋.dxf` 头部单位标记为英寸，但几何比例与鞋面毫米尺寸一致，Demo 统一按毫米解释（见 `summary.json` 的 `coordinate_unit_assumption`）。
- `render_free_angle_*.py` 用 macOS 自带字体 `/Library/Fonts/Verdana.ttf`，在其他系统运行需自行替换字体路径。
- 目录已通过 `.gitignore` 排除 `demo_output/` 与 `.venv/`，生成物不入库。
