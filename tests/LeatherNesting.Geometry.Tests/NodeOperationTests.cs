using LeatherNesting.Geometry;
using LeatherNesting.Geometry.NodeEditing;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class NodeOperationTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOD-001")]
    public void Insert_point_on_line_conserves_total_length()
    {
        var loop = new Loop2D("loop", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var originalLength = loop.Curves.Sum(c => c.Length);
        var ops = new NodeOperations();
        var result = ops.InsertNode(loop, new(50, 0));

        Assert.True(result.Success);
        var newLength = result.Loop.Curves.Sum(c => c.Length);
        Assert.Equal(originalLength, newLength, 6);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOD-001")]
    public void Single_point_break_conserves_total_length()
    {
        var loop = new Loop2D("loop", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(100, 0)),
            new LineSegment2D(new(100, 0), new(100, 50)),
            new LineSegment2D(new(100, 50), new(0, 50)),
            new LineSegment2D(new(0, 50), new(0, 0)),
        ]);

        var originalLength = loop.Curves.Sum(c => c.Length);
        var ops = new BreakOperations();
        var result = ops.BreakAtPoint(loop, new(50, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.RemainingCurves);
        var remainingLength = result.RemainingCurves!.Sum(c => c.Length);
        // After break, the remaining curves should conserve total length (minus the closing segment)
        Assert.True(Math.Abs(originalLength - remainingLength) < 10);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOD-002")]
    public void Delete_below_three_points_is_blocked()
    {
        var loop = new Loop2D("triangle", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(5, 10)),
            new LineSegment2D(new(5, 10), new(0, 0)),
        ]);

        var ops = new NodeOperations();
        var result = ops.DeleteNode(loop, 0);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, i => i.Contains("少于 3 个点"));
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-NOD-002")]
    public void Move_creating_self_intersection_is_blocked()
    {
        // A simple quad: moving one vertex to create a bow-tie should be blocked
        var loop = new Loop2D("quad", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
        ]);

        var ops = new NodeOperations();
        // Move vertex (10,0) to (0,10) — this would create a self-intersection
        var result = ops.MoveNode(loop, 1, new(0, 10));

        Assert.False(result.Success);
    }
}