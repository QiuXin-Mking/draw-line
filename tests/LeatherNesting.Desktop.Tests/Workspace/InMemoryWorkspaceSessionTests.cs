using LeatherNesting.Desktop.Workspace;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Workspace;

public sealed class InMemoryWorkspaceSessionTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "F01-001")]
    public void Snapshot_is_immutable_when_a_command_replaces_it()
    {
        var session = new InMemoryWorkspaceSession();
        var before = session.Snapshot;

        session.SetCurrentProject(new WorkspaceProjectSummary("project-42", "凉鞋样板", "LN-42", "草稿"));

        Assert.NotSame(before, session.Snapshot);
        Assert.Null(before.CurrentProject);
        Assert.Equal("project-42", session.Snapshot.CurrentProject?.Id);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "F01-002")]
    public void Commands_publish_replacement_snapshots_in_command_order()
    {
        var session = new InMemoryWorkspaceSession();
        var notifications = new List<WorkspaceSnapshot>();
        session.SnapshotChanged += (_, snapshot) => notifications.Add(snapshot);

        session.NavigateTo("M03");
        session.OpenObject("piece-vamp", "M04");
        session.ShowTodo("修复仍待接线");

        Assert.Collection(
            notifications,
            snapshot => Assert.Equal("M03", snapshot.ActiveModuleId),
            snapshot =>
            {
                Assert.Equal("M04", snapshot.ActiveModuleId);
                Assert.Equal("piece-vamp", snapshot.SelectedObjectId);
            },
            snapshot => Assert.Equal("修复仍待接线", snapshot.TodoHint));
        Assert.Same(notifications[^1], session.Snapshot);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "F01-003")]
    public void Identical_command_does_not_publish_a_state_change()
    {
        var session = new InMemoryWorkspaceSession();
        var notifications = 0;
        session.SnapshotChanged += (_, _) => notifications++;

        session.ShowDemoHint("演示数据");
        session.ShowDemoHint("演示数据");

        Assert.Equal(1, notifications);
    }
}
