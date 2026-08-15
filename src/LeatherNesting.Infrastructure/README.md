# LeatherNesting.Infrastructure

**基础设施层（Infrastructure Layer）**：实现应用层 `Ports.cs` 里声明的端口接口——真正去读写文件的地方。这里只有两类职责：**DXF 读写**和**项目/快照持久化**。它依赖 Domain、Application、Geometry，向上层（Desktop）提供可注入的具体实现。

```
LeatherNesting.Infrastructure
   ├─ 依赖：Domain、Application（端口接口）、Geometry（几何转换）
   └─ 被引用：Desktop（依赖注入时使用）
```

---

## `Dxf/` — DXF 读写

> 与项目根 `CLAUDE.md` 的 DXF 输出约定一致：排样导出用颜色码（ACI 62）区分线角色——`0` 外轮廓、`3` 切割线、`5` 标记线/刀口。

| 文件 | 职责 |
| --- | --- |
| `IDxfWriter.cs` | **DXF 写出接口**（Stage 2 回读用）。 |
| `AsciiDxfReader.cs` | **ASCII DXF 读取器**（Stage 1）。零依赖，只盘点实体清单；几何修复属 Stage 2。实现应用层的 `IDxfReader`。 |
| `AsciiDxfGeometryReader.cs` | 把闭合的 LWPOLYLINE 和 ARC 实体读回成 `Loop2D` 几何。 |
| `AsciiDxfWriter.cs` | **ASCII DXF 写出器**（闭合轮廓 LWPOLYLINE），Stage 2 回读。实现 `IDxfWriter`。 |
| `AsciiNestingDxfWriter.cs` | **排样结果 DXF 写出器**：材料 + 裁片轮廓，按线角色着色（62 = 0/3/5），附带 TEXT 注释。实现应用层的 `INestingDxfWriter`。 |

---

## `Projects/` — 项目与快照持久化

| 文件 | 职责 |
| --- | --- |
| `ZipProjectStore.cs` | **项目文档存储**：以 ZIP 打包持久化 `ProjectDocument`（元数据 + 导入报告）。实现 `IProjectStore`。 |
| `ZipNestingProjectStore.cs` | **排样项目存储**：以 ZIP 持久化 `NestingProject`（元数据 + 裁片 + 材料 + 排样结果）。实现 `INestingProjectStore`。 |
| `ProjectSnapshotStore.cs` | **崩溃恢复快照存储**：把 `NestingProject` 快照写到项目旁的伴生文件，崩溃后可恢复。实现 `ISnapshotStore`。 |

---

## 依赖与构建产物

- **项目引用**：`LeatherNesting.Domain`、`LeatherNesting.Application`、`LeatherNesting.Geometry`。
- `bin/`、`obj/`：构建产物与中间文件，**无需关注**。
- `packages.lock.json`：NuGet 依赖锁定文件，由 SDK 自动生成。
