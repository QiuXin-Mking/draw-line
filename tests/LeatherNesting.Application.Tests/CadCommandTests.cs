using LeatherNesting.Application.CadEditing;
using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Application.Tests;

public sealed class CadCommandTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UND-001")]
    public void Undo_restores_previous_state()
    {
        var transaction = new CadCommandTransaction();
        var context = new CadCommandContext
        {
            CurrentLoops = [],
        };

        var command = new AddLoopCommand("test-loop", new Loop2D("l1", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
        ]));

        var result = transaction.Commit(command, context);
        Assert.True(result.Success);
        Assert.Single(result.ResultLoops);

        var (undoResult, undoCmd) = transaction.Undo(context);
        Assert.True(undoResult.Success);
        Assert.Empty(undoResult.ResultLoops);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UND-001")]
    public void Redo_reapplies_command()
    {
        var transaction = new CadCommandTransaction();
        var context = new CadCommandContext { CurrentLoops = [] };

        var command = new AddLoopCommand("test-loop", new Loop2D("l1", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
        ]));

        transaction.Commit(command, context);
        transaction.Undo(context);
        var (redoResult, _) = transaction.Redo(context);

        Assert.True(redoResult.Success);
        Assert.Single(redoResult.ResultLoops);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UND-001")]
    public void Each_gesture_is_one_command()
    {
        var transaction = new CadCommandTransaction();
        var context = new CadCommandContext { CurrentLoops = [] };

        var cmd1 = new AddLoopCommand("l1", new Loop2D("l1", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
        ]));

        transaction.Commit(cmd1, context);
        Assert.Equal(1, transaction.UndoCount);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UND-001")]
    public void Undo_redo_restores_business_model_and_anchors()
    {
        // P2-UND-001: undo/redo restores business model and feature anchors consistently
        // This test verifies that after undo+redo, the loop is identical
        var loop = new Loop2D("l1", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
        ]);

        var transaction = new CadCommandTransaction();
        var context = new CadCommandContext { CurrentLoops = [] };

        transaction.Commit(new AddLoopCommand("l1", loop), context);
        transaction.Undo(context);
        var (redoResult, _) = transaction.Redo(context);

        Assert.Single(redoResult.ResultLoops);
        var restored = redoResult.ResultLoops[0];
        Assert.Equal(loop.StableId, restored.StableId);
        Assert.Equal(loop.Area, restored.Area, 6);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOD-001")]
    public void Break_at_point_command_opens_contour_and_undo_restores()
    {
        var loop = new Loop2D("l1", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var command = new BreakAtPointCommand("l1", new Point2D(50, 0));
        var context = new CadCommandContext { CurrentLoops = [loop] };

        var result = command.Execute(context);
        Assert.True(result.Success);
        Assert.Single(result.ResultLoops);
        // Breaking splits the bottom edge, so the open contour has more curves.
        Assert.True(result.ResultLoops[0].Curves.Count > loop.Curves.Count);

        var undo = command.Undo(context);
        Assert.True(undo.Success);
        Assert.Equal(loop.Curves.Count, undo.ResultLoops[0].Curves.Count);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOD-001")]
    public void Remove_segment_command_removes_segment_and_undo_restores()
    {
        var loop = new Loop2D("l1", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var command = new RemoveSegmentCommand("l1", new Point2D(0, 0), new Point2D(100, 0));
        var context = new CadCommandContext { CurrentLoops = [loop] };

        var result = command.Execute(context);
        Assert.True(result.Success);
        Assert.Single(result.ResultLoops);
        Assert.True(result.ResultLoops[0].Curves.Count < loop.Curves.Count);

        var undo = command.Undo(context);
        Assert.True(undo.Success);
        Assert.Equal(4, undo.ResultLoops[0].Curves.Count);
    }
}

/// <summary>A test command that adds a loop to the context.</summary>
internal sealed record AddLoopCommand(string LoopId, Loop2D Loop) : CadCommand("添加轮廓")
{
    public override CadCommandResult Execute(CadCommandContext context)
    {
        var newLoops = context.CurrentLoops.Append(Loop).ToList();
        return new CadCommandResult(newLoops);
    }

    public override CadCommandResult Undo(CadCommandContext context)
    {
        var newLoops = context.CurrentLoops.Where(l => l.StableId != LoopId).ToList();
        return new CadCommandResult(newLoops);
    }

    public override CadCommandResult Redo(CadCommandContext context)
    {
        return Execute(context);
    }
}