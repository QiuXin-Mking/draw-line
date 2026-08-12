# AutoNester-L — 德国 Fraunhofer 排样引擎

> 来源: scapos.de | 国家: 德国（Fraunhofer SCAI 研究所）

## 产品形态

AutoNester-L 是**排样算法库（DLL）**，不是最终用户软件。由 Fraunhofer SCAI（德国顶尖计算机应用研究所）研发，scapos AG 分发。定位是让 CAD/ERP 厂商集成排样能力。

## 核心算法

- 模拟退火（Simulated Annealing）
- 多轮贪心策略（Multi-iterated Greedy）
- 高效启发式（Efficient Heuristics）
- 高质量模式识别（Pattern Recognition）
- 动态统计参数调整（局部搜索中自适应）

## 皮革专用能力

- 最多 **16 级质量分区**
- 自动缺陷检测与避让
- 自由角度旋转（不限于 90° 倍数）
- 多皮张同时排样
- 预放置 + 折叠/镜像件支持
- 连接标记
- 毛向和翻转限制

## 性能

- 利用率 ~83–84%（公开示例）
- ~30 秒/次排样（标准 PC）

## API 模式

- DLL 库形式
- Windows 全版本支持
- 可嵌入 CAD / ERP 系统

## 关键启示

1. **排样引擎可以独立售卖**：AutoNester-L 证明了"只卖排样算法"是一个可行的商业模式
2. **16 级质量的实现方式**：不是简单的 boolean（好/坏），而是多级渐变质量
3. **学术背景 = 算法壁垒**：Fraunhofer 的论文支撑使得竞品很难超越

## 对我们 Phase 1 的参考价值

- **中高**。AutoNester-L 的算法路线（贪心种子 + 模拟退火优化）是我们 Phase 2 算法升级的直接参考。DLL 分发模式也值得借鉴——未来可以把排样引擎独立出来作为 SDK 授权。
