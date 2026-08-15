# LeatherNesting.Domain

**领域层（Domain Layer）**：最底层的核心领域对象，**不依赖任何其他项目**（`.csproj` 无任何 ProjectReference）。这里只放跨层共享的、最稳定的“文档级”模型——项目文档元数据与导入报告。几何算法放在 `LeatherNesting.Geometry`，应用层聚合模型放在 `LeatherNesting.Application`，本层只保留它们共同依赖的那一小撮根对象。

```
LeatherNesting.Domain  ←  被 Geometry / Application / Infrastructure / Desktop 共同引用
```

---

## 文件说明

只有一个源文件 `ProjectDocument.cs`，定义了四类类型：

| 类型 | 职责 |
| --- | --- |
| `ProjectDocument` | **项目文档元数据**（聚合根）。记录项目 Id、名称、Schema 版本、修订号（Revision）、是否脏（IsDirty）、导入报告列表。提供 `CreateNew`、`CommitImport`（提交一次导入并递增修订）、`MarkSaved` 等纯函数式方法。 |
| `ImportReport` | **一次导入的报告**：源文件路径、源文件 SHA-256 指纹、单位决策、诊断信息列表。用于追溯“这个项目是从哪个 DXF 导入的”。 |
| `ImportDiagnostic` | **单条导入诊断**：代码、严重度、消息、可选的实体 Id。 |
| `UnitDecision` | **单位决策枚举**：`Unresolved`（未确定）或 `ConfirmedMillimetres`（已确认毫米）。导入必须显式确认毫米后才能提交。 |

---

## 依赖与构建产物

- **项目引用**：无（最底层）。
- `bin/`、`obj/`：构建产物与中间文件，**无需关注**。
- `packages.lock.json`：NuGet 依赖锁定文件，由 SDK 自动生成。
