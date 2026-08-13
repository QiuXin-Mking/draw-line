using LeatherNesting.Desktop.Demo;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Projects;

/// <summary>M01 demo state. Read-only scenario; every write-like action is a TODO that does not mutate data.</summary>
public sealed class ProjectsViewModel
{
    private static readonly IReadOnlyList<string> ProjectStates = Array.AsReadOnly(new[]
    {
        "草稿",
        "导入待确认",
        "可配置",
        "排样中",
        "已完成 / 未完全满足",
        "校验失败 / 校验通过",
        "已批准",
        "已导出",
        "输入变更 → 已过期",
    });

    public ProjectsViewModel()
        : this(DemoScenarioFactory.Projects)
    {
    }

    public ProjectsViewModel(IProjectsDemoProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        Scenario = DemoScenario.From(provider);
    }

    public DemoScenario Scenario { get; }
    public IReadOnlyList<string> StatusTrace => ProjectStates;
    public string? SelectedVersionDetail { get; private set; }
    public string? TodoMessage { get; private set; }

    public void SelectVersion(VersionEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        SelectedVersionDetail = $"{entry.Version}（{entry.Date}）：{entry.Summary}";
    }

    public void NewProject() => TodoMessage = $"新建项目：{TodoBadge.StandardText}";
    public void Duplicate() => TodoMessage = $"复制版本：{TodoBadge.StandardText}";
    public void Approve() => TodoMessage = $"审批：{TodoBadge.StandardText}";
    public void Restore() => TodoMessage = $"恢复：{TodoBadge.StandardText}";
    public void EditOrder() => TodoMessage = $"编辑订单信息：{TodoBadge.StandardText}";
}
