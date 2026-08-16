# ADR：排样算法实现语言 —— C# 托管 vs C++ 下沉（P/Invoke）

Status: accepted（2026-08-15）

## 背景

排样（nesting）是本软件的计算密集核心，涉及 NFP（No-Fit Polygon）、布尔运算、碰撞检测与局部搜索等浮点几何重计算，也是最有价值的商业资产（IP）。它存在两条实现路线：

1. **纯 C# 托管**（当前路线）—— 算法与 UI 同栈，`LeatherNesting.Geometry` 内实现。
2. **C++ native 下沉** —— 把排样内核写成 C++，C# 通过 P/Invoke 调用，仅保留 UI 与业务在托管侧。

本 ADR 聚焦**排样算法这一处**的语言选型，是 [`03-技术栈选择.md`](./03-技术栈选择.md)（整体技术栈 C#/.NET + Avalonia，弃 C++/Qt）在算法层的细化，不改变 `03` 的整体结论。

## Decision

**当前阶段：排样算法用纯 C# 实现，暂不下沉 C++。**

下沉 native 作为「性能不足」或「逆向防护升级」时的**后续选项**保留，但必须以 **profile 实测数据** 为触发条件，不预先优化（avoid premature optimization）。当且仅当实测证明 C# 内核在目标数据规模下无法满足耗时要求，才将最热的算法内核（约 20% 代码）下沉 C++，经 BenchmarkDotNet A/B 对比量化收益后再决定长期保留双语言。

## 理由与权衡

### 性能对比（C++ 领先，但差距被高估）

| 场景 | C++ | C# 托管 | 说明 |
|------|-----|---------|------|
| 纯浮点几何（NFP/布尔/碰撞） | 最快 | 慢 1.5~5× | C++ 自动 SIMD 化 |
| C# 用 Span + SIMD + 避免分配后 | — | 差距缩到 1.2~2× | 热路径可控 |
| 跨边界调用开销 | — | 单次 ~几十 ns | blittable 数据下开销可忽略 |

关键结论：**很多"慢"不是语言问题，而是算法/数据结构问题**（朴素 O(n²) 碰撞、未建空间索引、装箱）。换 C++ 不改算法，该慢仍慢。应先优化算法与数据结构。

### 逆向防护（C++ 领先，但 Native AOT 可拉近）

- C# 编译为 IL，`ILSpy`/`dnSpy` 可还原接近源码；C++ 为原生机器码，需 Ghidra/IDA 反汇编，难度更高。
- 但 **Native AOT**（`PublishAot`）可把 C# 编译为机器码、不带 IL，逆向难度对齐 C++（Avalonia 12 + .NET 10 已支持，见 `03` 的防护小节）。二者差距只对专业逆向者有意义，而那种人两种都能破。
- 结论：为这点防护差距多花 2~3 倍开发成本**不划算**；若防护是首要目标，优先走 Native AOT 而非整算法重写 C++。

### 开发与维护成本（C# 明显更省）

- **开发效率**：GC、无手动内存、语法现代、标准库全；排样算法在 C# 中迭代更快。
- **维护**：双语言意味着同一算法两处实现，改 bug 改两遍。
- **构建**：C++ 需为 win-x64（可能 arm64/linux）单独编译、上 CMake，跨平台 ABI 管理。
- **人才**：国内 C# 比 C++ 好招、成本低。
- **沉没成本**：`LeatherNesting.Geometry` 已有 C# 几何/排样代码，整算法重写即浪费。

### 互操作方式（若未来下沉，选 P/Invoke）

| 方式 | 机制 | 评价 |
|------|------|------|
| **P/Invoke（`DllImport`）** ⭐ | C# 调 C 导出函数（C ABI） | 最干净、跨平台、无中间层 |
| C++/CLI | 混合编译托管+原生 | 仅 Windows 且 .NET 支持有限，不推荐 |
| 独立进程 / IPC | 进程间通信 | 过重，除非内核需进程隔离 |

### 下沉时的接口设计原则（存档备用）

核心不是"能调"，而是**怎么调才快**：跨边界单次调用开销很小，真正的坑是「频繁小调用 + 数据拷贝」。因此接口须满足：

1. **批量、扁平、不透明句柄**：一个大 `solve` 调用把重活整体留在 C++ 侧，而非每个小几何运算都跨边界。
2. **扁平数组传数据**：顶点用 `double[]` + blittable 类型 / `unsafe` 固定指针，避免逐元素 marshal。
3. **上下文驻留 native**：`void*` 句柄存 C++ 侧状态，C# 只持 `IntPtr`。
4. **可借力现成库**：NFP/布尔运算不重复造轮子，可用 CGAL（GPL/商业，重）或 boost.geometry；本项目已用 Clipper2。

示意（C 导出接口）：

```c
extern "C" {
    void* nest_create(double sheet_w, double sheet_h);
    void  nest_destroy(void* ctx);
    int   nest_add_parts(void* ctx, const double* coords,
                         const int* offsets, int part_count);
    int   nest_solve(void* ctx, double angle_step, double* utilization_out);
    int   nest_get_placement(void* ctx, int idx, double* x, double* y, double* a);
}
```

## 触发条件（何时重新评估本决策）

满足任一条件时，重新打开本 ADR 评估下沉 native：

1. **性能**：BenchmarkDotNet / `dotnet-trace` 实测，排样在目标数据规模（裁片数、任意角度、目标机型）下超过可接受耗时，且 C# 层已完成 Span/SIMD/空间索引/去分配优化仍不达标。
2. **防护**：商业化时若 Native AOT 与混淆仍不足以保护核心算法，且预算允许，考虑算法内核 native + 授权。
3. **团队**：未来团队具备 C++ 能力且愿意承担双语言维护成本。

## Evidence and limits

- 性能数字（C++ 快 1.5~5×，C# 优化后 1.2~2×）为经验量级估算，非本项目实测；**是否下沉必须以本项目真实数据的 profile 结果为准**。
- Avalonia 12 + .NET 10 的 Native AOT 已可用，但存在反射标注、第三方控件兼容、原生库（`libSkiaSharp.dll` 等）仍需随包分发等限制，见 [Avalonia Native AOT 文档](https://docs.avaloniaui.net/docs/deployment/native-aot)。
- 本项目处于原型阶段，`LeatherNesting.Geometry` 尚未在真实规模下跑通并测量，故现阶段无实测数据支撑下沉决策。

## Consequences

- 排样算法继续在 C# 托管侧实现，与整体技术栈（`03`）一致，Mac 上可端到端验证，开发效率与跨平台优势保留。
- 本 ADR 存档了 native 下沉的接口设计原则与触发条件，未来若下沉无需重新调研。
- 商业化防护路径优先顺序为：C# 优化 → Native AOT → （可选）混淆器 → （可选）算法内核 native + 授权（加密狗/在线激活），与 `03` 的防护小节一致。
