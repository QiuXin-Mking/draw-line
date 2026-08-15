# 皮革划线排样软件

## 项目目标

本项目的目标是复刻一款皮革裁切划线软件的核心功能 —— **不规则形状排样优化算法**，最终输出可供通用软件（如 AutoCAD、Illustrator 等）使用的排样图表（DXF 格式）。

## 背景

在皮革制品（如鞋面、皮包、皮衣等）的生产过程中，需要从大张皮革上裁切出各种不规则形状的裁片。由于皮革形状不规则、且每个裁片形状各异，如何在皮革上尽可能紧密地排列裁片、找到最优的剪切路径，是降低材料浪费、控制成本的关键。

传统划线软件能够：

1. **导入裁片形状** —— 读取 DXF 等格式的裁片轮廓数据
2. **排样优化** —— 在给定的皮革区域内，自动计算裁片的最优摆放位置和角度
3. **输出划线图** —— 生成最终的排样图，标明每个裁片的位置和剪切路径

## 核心功能（计划实现）

- **DXF 解析**：读取裁片的 DXF 轮廓数据，提取多边形/曲线边界
- **排样算法**：实现不规则形状的二维排样（Nesting）算法，支持：
  - 旋转、翻转裁片以寻找最优姿态
  - 碰撞检测（No-Fit Polygon / 分离轴定理）
  - 启发式搜索（遗传算法 / 模拟退火 / 贪心 + 局部优化）
- **利用率计算**：统计皮革面积利用率，评估排样效果
- **DXF 输出**：将排样结果导出为 DXF 图表，可直接用于生产

## 仓库结构

| 路径 | 说明 |
|------|------|
| `src/` | C# 正式产品（分层：Domain / Geometry / Application / Infrastructure / Desktop） |
| `tests/` | 与 `src/` 1:1 的测试镜像 |
| `python-demo/` | Python 排样展示 Demo（对外演示，见「可运行 Demo」） |
| `docs/` | ADR（`docs/adr/`）、待办（`docs/todo/`）、架构治理文档 |
| `凉鞋.dxf` | 凉鞋裁片输入样例（C# 测试 fixture 与 Python demo 共用） |

## 技术路线（待定）

排样问题是经典的 NP-hard 组合优化问题，对于不规则形状尤为复杂。可能涉及的技术：

- **几何计算**：多边形布尔运算、Minkowski Sum、No-Fit Polygon (NFP)
- **优化算法**：遗传算法 (GA)、模拟退火 (SA)、粒子群优化 (PSO)、强化学习
- **碰撞检测**：分离轴定理 (SAT)、GJK 算法
- **文件格式**：DXF 读写（ezdxf / libdxfrw）

## 开发状态

🚧 项目初期，正在规划和原型阶段。

## 可运行 Demo（Python 展示版）

Python 排样 Demo 已独立到 [`python-demo/`](./python-demo/)，用于**对外展示**算法效果（读 DXF → 排样 → 输出 DXF/PNG/利用率）。

```bash
cd python-demo && ./run.sh
```

一键生成三种皮革尺寸的排样 DXF/PNG 与利用率汇总，并展示自由角度（0° + 175°）排样效果。详见 [`python-demo/README.md`](./python-demo/README.md)。

> 注意：这是演示用的确定性货架填充 Demo，不保证全局最优。正式产品为 C# 排样引擎（`src/LeatherNesting.Geometry/Nesting/`，NFP + 局部搜索，支持任意角度）。Python Demo 仅作展示，不再作为算法参照演进。

## 参考资料

- DXF 格式规范：[Autodesk DXF Reference](https://www.autodesk.com/developer-network/platform-technologies/autocad-dxf)
- 排样问题综述：*Irregular Packing Problems: A Review of Mathematical Models*
- 相关开源项目：Deepnest、SVGNest
