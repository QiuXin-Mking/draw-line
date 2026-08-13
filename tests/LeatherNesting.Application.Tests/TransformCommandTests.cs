using LeatherNesting.Application.CadEditing;
using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Application.Tests;

public sealed class TransformCommandTests
{
    private static Loop2D Rectangle() => new("rect", LoopRole.Outer, [
        new LineSegment2D(new(0, 0), new(100, 0)),
        new LineSegment2D(new(100, 0), new(100, 50)),
        new LineSegment2D(new(100, 50), new(0, 50)),
        new LineSegment2D(new(0, 50), new(0, 0)),
    ]);

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UND-001")]
    public void Transform_command_moves_loop_and_undo_restores()
    {
        var command = new TransformCommand("rect", new Transform2D(10, 20, 0, false));
        var context = new CadCommandContext { CurrentLoops = [Rectangle()] };

        var result = command.Execute(context);
        Assert.True(result.Success);
        Assert.Equal(new Point2D(10, 20), result.ResultLoops[0].Curves[0].StartPoint);

        var undo = command.Undo(context);
        Assert.True(undo.Success);
        Assert.Equal(new Point2D(0, 0), undo.ResultLoops[0].Curves[0].StartPoint);
    }
}
