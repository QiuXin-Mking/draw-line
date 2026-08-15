# LeatherNesting.Application

**应用层（Application Layer）**：六边形架构中的“应用边界 / 用例层”。它不实现 UI，也不实现具体的文件读写或几何算法，而是把这些能力抽象成接口（Ports），并把业务动作编排成一个个 **用例（Use Case）**。

它的下游是 `LeatherNesting.Domain`（领域模型）和 `LeatherNesting.Geometry`（几何算法），上游由 `LeatherNesting.Desktop`（Avalonia 桌面端）和 `LeatherNesting.Infrastructure`（DXF 读写、快照持久化等）注入具体实现。

```
LeatherNesting.Desktop / Infrastructure   ← 调用方 + 端口实现
              │  (依赖注入)
              ▼
LeatherNesting.Application                ← 本层：用例编排 + 端口接口
              │
              ▼
LeatherNesting.Domain / Geometry          ← 领域模型 / 几何算法
```

---

## 顶层文件

这些文件直接放在 `LeatherNesting.Application` 根目录，属于“用例 + 端口”一类。

| 文件 | 职责 |
| --- | --- |
| `Ports.cs` | **端口接口定义**。声明应用层需要外部提供的能力：`IProjectStore`（项目文档读写）、`INestingProjectStore`（排样项目读写）、`ISnapshotStore`（崩溃恢复快照）、`IClock`（时钟）、`IFileDialogService`（选择 DXF 文件）。接口在这里定义，实现由 Infrastructure/Desktop 提供。 |
| `DxfImport.cs` | **导入 DXF 用例**。定义 `DxfEntity` 等实体模型和 `IDxfReader` 端口；`ImportDxfUseCase` 负责“先检查后提交”——读取 DXF 实体、计算源文件 SHA-256 指纹，只有确认毫米单位后才允许写入项目。 |
| `NestingExport.cs` | **导出排样结果为 DXF 用例**。定义 `NestingDxfDocument`/`NestingDxfPiece` 输出模型和 `INestingDxfWriter` 端口；`ExportNestingDxfUseCase` 把 `NestResult` 组装成 DXF 文档并写出（含外轮廓、内孔、内部线）。 |
| `NestingProjectFactory.cs` | **工厂**：从一组几何轮廓 `Loop2D` 构建 `NestingProject` 聚合根，用轮廓的 `StableId` 作为裁片标识。 |
| `OperationSnapshotCoordinator.cs` | **崩溃恢复快照协调器**：每执行 N 次编辑操作（默认 10 次）触发一次快照保存，采用 fire-and-forget，失败不阻塞调用方。 |
| `SnapshotThrottle.cs` | **快照节流器**：累加操作次数，达到阈值后返回“应保存”并清零。配合 `OperationSnapshotCoordinator` 使用。 |

---

## `Domain/` 子目录

应用层自己的**领域模型 / 聚合根**（区别于 `LeatherNesting.Domain` 里更底层的领域对象）。这些是应用层持久化与传递的核心数据结构。

| 文件 | 职责 |
| --- | --- |
| `NestingProject.cs` | **聚合根**：一个可持久化的排样项目，= 项目文档元数据 + 裁片列表 + 材料列表 + 排样结果列表。 |
| `Piece.cs` | **裁片**：标识（Id/Name/Size）+ 几何轮廓 `Loop2D`。 |
| `Material.cs` | **材料（皮料）**：标识 + 几何边界 `Loop2D`。 |

---

## `CadEditing/` 子目录

**CAD 编辑命令**——工作台里对轮廓做闭合、修复、偏移、节点编辑、变换等操作，全部封装成可撤销/可重做/可预览的命令对象。

| 文件 | 职责 |
| --- | --- |
| `CadCommand.cs` | **命令基类**与上下文/结果类型。`CadCommand` 是一个可撤销操作（`Execute`/`Undo`/`Redo`）；`CadCommandContext` 携带当前轮廓集合；`CadCommandResult` 返回结果轮廓 + 诊断信息。 |
| `CadCommands.cs` | **具体编辑命令**。以 `LoopTransformCommand` 为基类（带快照式 undo/redo），实现：`CloseContourCommand`（闭合轮廓）、`GapRepairCommand`（间隙修复）、`BoundaryGenerateCommand`（边界生成）、`OffsetCommand`（偏移）、`MoveNodeCommand`/`InsertNodeCommand`/`DeleteNodeCommand`（节点编辑）、`BreakAtPointCommand`（单点剪断）、`RemoveSegmentCommand`（去段）、`TransformCommand`（变换裁片）。 |
| `CadCommandTransaction.cs` | **撤销/重做栈**：维护 undo/redo 两个栈，带最大深度限制（默认 100），负责命令的提交、撤销、重做、清空。 |
| `CadOperationSession.cs` | **预览会话**：编辑操作先“预览”（不写回项目、不进撤销栈），`Commit` 才真正提交，`Cancel` 丢弃。管理 pending 命令与预览轮廓。 |
| `CrashRecoveryLog.cs` | **崩溃恢复日志**：把已提交的命令追加到磁盘日志文件（时间戳 + 命令 ID + 描述），供崩溃后回放。只记录 commit，不记录预览帧。 |

---

## 依赖与构建产物

- **项目引用**：`LeatherNesting.Domain`、`LeatherNesting.Geometry`（见 `.csproj`）。
- `bin/`、`obj/`：构建产物与中间文件，**无需关注**（已 gitignore）。
- `packages.lock.json`：NuGet 依赖锁定文件，由 SDK 自动生成。
