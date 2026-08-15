using LeatherNesting.Application.CadEditing;
using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Application.Tests;

public sealed class CadOperationSessionTests
{
    private static Loop2D Rect(string id) => new(id, LoopRole.Outer, [
        new LineSegment2D(new(0, 0), new(10, 0)),
        new LineSegment2D(new(10, 0), new(10, 10)),
        new LineSegment2D(new(10, 10), new(0, 10)),
        new LineSegment2D(new(0, 10), new(0, 0)),
    ]);

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-SESS-001")]
    public void Cancel_restores_preview_state()
    {
        var session = new CadOperationSession([Rect("l1")]);

        var preview = session.Preview(new AddLoopCommand("l2", Rect("l2")));
        Assert.True(preview.Success);
        Assert.Equal(2, session.PreviewLoops.Count);

        session.Cancel();

        Assert.Single(session.PreviewLoops); // restored to pre-preview state
        Assert.False(session.IsPreviewing);
        Assert.False(session.HasPendingCommand);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-SESS-002")]
    public void Commit_keeps_preview_and_clears_pending()
    {
        var session = new CadOperationSession([Rect("l1")]);

        session.Preview(new AddLoopCommand("l2", Rect("l2")));
        var commit = session.Commit();

        Assert.True(commit.Success);
        Assert.Equal(2, session.PreviewLoops.Count); // committed state is kept
        Assert.False(session.HasPendingCommand);
        Assert.False(session.IsPreviewing);
    }
}
