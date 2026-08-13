using LeatherNesting.Geometry;
using LeatherNesting.Geometry.NodeEditing;
using LeatherNesting.Geometry.Offset;
using LeatherNesting.Geometry.Repair;

namespace LeatherNesting.Application.CadEditing;

/// <summary>Base class for commands that replace the current loop set, with snapshot undo/redo.</summary>
public abstract record LoopTransformCommand : CadCommand
{
    private IReadOnlyList<Loop2D>? _before;
    private IReadOnlyList<Loop2D>? _after;

    protected LoopTransformCommand(string description) : base(description) { }

    protected abstract CadCommandResult Transform(CadCommandContext context);

    public override CadCommandResult Execute(CadCommandContext context)
    {
        _before = context.CurrentLoops;
        var result = Transform(context);
        if (result.Success)
            _after = result.ResultLoops;
        return result;
    }

    public override CadCommandResult Undo(CadCommandContext context)
        => _before is null ? CadCommandResult.Failed(["没有可撤销的状态。"]) : new CadCommandResult(_before);

    public override CadCommandResult Redo(CadCommandContext context)
        => _after is null ? CadCommandResult.Failed(["没有可重做的状态。"]) : new CadCommandResult(_after);

    protected static int FindLoopIndex(IReadOnlyList<Loop2D> loops, string loopId)
    {
        for (var i = 0; i < loops.Count; i++)
            if (loops[i].StableId == loopId)
                return i;
        return -1;
    }

    protected static CadCommandResult ReplaceLoop(IReadOnlyList<Loop2D> loops, int index, Loop2D replacement)
    {
        var updated = loops.ToList();
        updated[index] = replacement;
        return new CadCommandResult(updated);
    }
}

/// <summary>Closes a single open contour by bridging its start/end gap.</summary>
public sealed record CloseContourCommand(string loopId) : LoopTransformCommand("闭合轮廓")
{
    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var index = FindLoopIndex(context.CurrentLoops, loopId);
        if (index < 0)
            return CadCommandResult.Failed([$"未找到轮廓 {loopId}。"]);
        var result = new ContourCloser().Close(context.CurrentLoops[index]);
        if (result.Diagnostics.Count > 0)
            return CadCommandResult.Failed(result.Diagnostics);
        return ReplaceLoop(context.CurrentLoops, index, result.RepairedLoops[0]);
    }
}

/// <summary>Repairs gaps across all curves, replacing the loop set with the repaired closed loops.</summary>
public sealed record GapRepairCommand : LoopTransformCommand
{
    public GapRepairCommand() : base("间隙修复") { }

    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var allCurves = context.CurrentLoops.SelectMany(l => l.Curves).ToList();
        var result = new GapRepair().Repair(allCurves, "workbench");
        if (result.Diagnostics.Count > 0)
            return CadCommandResult.Failed(result.Diagnostics);
        if (result.RepairedLoops.Count == 0)
            return CadCommandResult.Failed(["修复后未产生闭合轮廓。"]);
        return new CadCommandResult(result.RepairedLoops);
    }
}

/// <summary>Generates a boundary from curves; fails when zero or multiple candidates exist (needs selection).</summary>
public sealed record BoundaryGenerateCommand : LoopTransformCommand
{
    public BoundaryGenerateCommand() : base("边界生成") { }

    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var allCurves = context.CurrentLoops.SelectMany(l => l.Curves).ToList();
        var generator = new BoundaryGenerator();
        var result = generator.Generate(allCurves, "workbench");
        if (result.Diagnostics.Count > 0)
            return CadCommandResult.Failed(result.Diagnostics);
        if (result.ValidCandidates.Count == 0)
            return CadCommandResult.Failed(["没有有效的边界候选。"]);
        if (result.ValidCandidates.Count > 1)
            return CadCommandResult.Failed([$"发现 {result.ValidCandidates.Count} 个候选边界，需要用户选择。"]);
        var loop = generator.GenerateFromCandidate(result.ValidCandidates[0], "boundary-1");
        return loop is null
            ? CadCommandResult.Failed(["候选边界无法生成轮廓。"])
            : new CadCommandResult([loop]);
    }
}

/// <summary>Offsets all loops inward or outward, replacing the loop set.</summary>
public sealed record OffsetCommand(double distanceMm, OffsetDirection direction, OffsetJoinStyle joinStyle = OffsetJoinStyle.Miter)
    : LoopTransformCommand($"offset {direction} {distanceMm}mm")
{
    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var result = new OffsetAdapter().Offset(context.CurrentLoops, distanceMm, direction, joinStyle);
        if (result.Diagnostics.Count > 0)
            return CadCommandResult.Failed(result.Diagnostics);
        if (result.OffsetLoops.Count == 0)
            return CadCommandResult.Failed(["offset 后没有结果轮廓。"]);
        return new CadCommandResult(result.OffsetLoops);
    }
}

/// <summary>Moves a node of a loop to a new position, blocking self-intersection.</summary>
public sealed record MoveNodeCommand(string loopId, int nodeIndex, Point2D newPosition)
    : LoopTransformCommand("移动节点")
{
    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var index = FindLoopIndex(context.CurrentLoops, loopId);
        if (index < 0)
            return CadCommandResult.Failed([$"未找到轮廓 {loopId}。"]);
        var result = new NodeOperations().MoveNode(context.CurrentLoops[index], nodeIndex, newPosition);
        return result.Success
            ? ReplaceLoop(context.CurrentLoops, index, result.Loop)
            : CadCommandResult.Failed(result.Issues);
    }
}

/// <summary>Inserts a node at the nearest point on a loop, splitting the containing curve.</summary>
public sealed record InsertNodeCommand(string loopId, Point2D point)
    : LoopTransformCommand("插入节点")
{
    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var index = FindLoopIndex(context.CurrentLoops, loopId);
        if (index < 0)
            return CadCommandResult.Failed([$"未找到轮廓 {loopId}。"]);
        var result = new NodeOperations().InsertNode(context.CurrentLoops[index], point);
        return result.Success
            ? ReplaceLoop(context.CurrentLoops, index, result.Loop)
            : CadCommandResult.Failed(result.Issues);
    }
}

/// <summary>Deletes a node from a loop, blocking when fewer than three points would remain.</summary>
public sealed record DeleteNodeCommand(string loopId, int nodeIndex)
    : LoopTransformCommand("删除节点")
{
    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var index = FindLoopIndex(context.CurrentLoops, loopId);
        if (index < 0)
            return CadCommandResult.Failed([$"未找到轮廓 {loopId}。"]);
        var result = new NodeOperations().DeleteNode(context.CurrentLoops[index], nodeIndex);
        return result.Success
            ? ReplaceLoop(context.CurrentLoops, index, result.Loop)
            : CadCommandResult.Failed(result.Issues);
    }
}

/// <summary>Breaks a closed loop at a single point, producing an open contour (still represented as a Loop2D).</summary>
public sealed record BreakAtPointCommand(string loopId, Point2D point)
    : LoopTransformCommand("单点剪断")
{
    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var index = FindLoopIndex(context.CurrentLoops, loopId);
        if (index < 0)
            return CadCommandResult.Failed([$"未找到轮廓 {loopId}。"]);
        var result = new BreakOperations().BreakAtPoint(context.CurrentLoops[index], point);
        if (!result.Success)
            return CadCommandResult.Failed(result.Issues);
        if (result.RemainingCurves is null || result.RemainingCurves.Count == 0)
            return CadCommandResult.Failed(["剪断后没有剩余曲线。"]);
        var openLoop = new Loop2D(loopId, context.CurrentLoops[index].Role, result.RemainingCurves);
        return ReplaceLoop(context.CurrentLoops, index, openLoop);
    }
}

/// <summary>Removes a segment between two points, producing open curves (still represented as a Loop2D).</summary>
public sealed record RemoveSegmentCommand(string loopId, Point2D pointA, Point2D pointB)
    : LoopTransformCommand("去段")
{
    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var index = FindLoopIndex(context.CurrentLoops, loopId);
        if (index < 0)
            return CadCommandResult.Failed([$"未找到轮廓 {loopId}。"]);
        var result = new BreakOperations().RemoveSegment(context.CurrentLoops[index], pointA, pointB);
        if (!result.Success)
            return CadCommandResult.Failed(result.Issues);
        if (result.RemainingCurves is null || result.RemainingCurves.Count == 0)
            return CadCommandResult.Failed(["去段后没有剩余曲线。"]);
        var openLoop = new Loop2D(loopId, context.CurrentLoops[index].Role, result.RemainingCurves);
        return ReplaceLoop(context.CurrentLoops, index, openLoop);
    }
}

/// <summary>Applies a translation/rotation/mirror transform to a single loop.</summary>
public sealed record TransformCommand(string loopId, Transform2D transform)
    : LoopTransformCommand("变换裁片")
{
    protected override CadCommandResult Transform(CadCommandContext context)
    {
        var index = FindLoopIndex(context.CurrentLoops, loopId);
        if (index < 0)
            return CadCommandResult.Failed([$"未找到轮廓 {loopId}。"]);
        return ReplaceLoop(context.CurrentLoops, index, transform.Apply(context.CurrentLoops[index]));
    }
}
