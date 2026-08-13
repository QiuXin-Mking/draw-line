using LeatherNesting.Desktop.Demo;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Projects;

/// <summary>M01 demo state. Read-only scenario; every write-like action is a TODO that does not mutate data.</summary>
public sealed class ProjectsViewModel
{
    public DemoScenario Scenario => DemoScenarioFactory.Default;
    public string? TodoMessage { get; private set; }

    public void NewProject() => TodoMessage = $"新建项目：{TodoBadge.StandardText}";
    public void Duplicate() => TodoMessage = $"复制版本：{TodoBadge.StandardText}";
    public void Approve() => TodoMessage = $"审批：{TodoBadge.StandardText}";
    public void Restore() => TodoMessage = $"恢复：{TodoBadge.StandardText}";
    public void EditOrder() => TodoMessage = $"编辑订单信息：{TodoBadge.StandardText}";
}
