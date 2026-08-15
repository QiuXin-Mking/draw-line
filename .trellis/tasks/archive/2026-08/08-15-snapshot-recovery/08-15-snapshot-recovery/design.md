# 崩溃恢复（快照节流触发）技术设计

## 组件

| 组件 | 层 | 职责 |
|---|---|---|
| `SnapshotThrottle` | Application | 节流计数器（每 N 次操作触发一次） |
| `NestingProjectFactory` | Application | Loop2D → `NestingProject`（`Piece.Id = StableId`，`Name/Size` 空） |
| `ISnapshotStore` | Application | 快照端口 |
| `ProjectSnapshotStore` | Infrastructure | 实现 `ISnapshotStore` |
| `OperationSnapshotCoordinator` | Application | 编排：操作计数 → 阈值 → 构建 + 写快照 |

## 节流协议

- `RecordOperation()` 每次 `Commit`/`Undo`/`Redo` 后调用。
- 累计到阈值（10）→ 触发快照 → 计数归零。
- 快照写入**异步 fire-and-forget**，不阻塞 UI 线程。

## Loop2D → NestingProject

`NestingProjectFactory.FromLoops(loops, document)`：

- `Piece.Id = Loop2D.StableId`，`Name`/`Size` 空字符串。
- `Document` 用当前项目元数据；`Materials`/`NestingResults` 空。

## 签名

```csharp
public sealed class SnapshotThrottle(int threshold)
{
    public bool ShouldFlush();   // 计数 +1，达到 threshold 返回 true 并归零
}

public interface ISnapshotStore
{
    Task SaveSnapshotAsync(string projectPath, NestingProject project, CancellationToken ct);
    Task<NestingProject?> LoadSnapshotAsync(string projectPath, CancellationToken ct);
    void ClearSnapshot(string projectPath);
}

public sealed class OperationSnapshotCoordinator(
    ISnapshotStore store, string projectPath, Func<IReadOnlyList<Loop2D>> loops)
{
    public void RecordOperation();  // 节流触发快照
}
```

## 兼容与回滚

- 纯新增，`ProjectSnapshotStore` 加 `ISnapshotStore` 接口，不改现有方法。
- 回滚：删除新增类型即可。
