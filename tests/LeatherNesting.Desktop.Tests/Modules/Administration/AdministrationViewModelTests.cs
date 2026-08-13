using LeatherNesting.Desktop.Modules.Administration;
using LeatherNesting.Desktop.Modules.Contracts;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.Administration;

public sealed class AdministrationViewModelTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T12")]
    public void Project_snapshot_is_distinct_from_latest_preset()
    {
        var viewModel = new AdministrationViewModel();

        Assert.True(viewModel.SelectedPreset.HasNewerVersion);
        Assert.NotEqual(viewModel.SelectedPreset.ProjectSnapshot.Version, viewModel.SelectedPreset.Latest.Version);
        Assert.Contains("项目快照", viewModel.PresetComparison);
        Assert.Contains("最新预设", viewModel.PresetComparison);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T12")]
    public void Audit_filter_limits_timeline_without_changing_source_events()
    {
        var viewModel = new AdministrationViewModel();
        var total = viewModel.AuditEvents.Count;

        viewModel.SetAuditFilter(AuditCategory.Permission);

        Assert.NotEmpty(viewModel.FilteredAuditEvents);
        Assert.All(viewModel.FilteredAuditEvents, entry => Assert.Equal(AuditCategory.Permission, entry.Category));
        Assert.Equal(total, viewModel.AuditEvents.Count);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T12")]
    public void Insufficient_permission_disables_action_and_explains_why()
    {
        var viewModel = new AdministrationViewModel();

        viewModel.SelectRole(RoleKind.Operator);

        Assert.False(viewModel.CanEditRules);
        Assert.False(viewModel.CanManageAdapters);
        Assert.Contains("权限不足", viewModel.RuleActionExplanation);
        Assert.Contains("管理员", viewModel.RuleActionExplanation);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T12")]
    public void Editing_settings_only_changes_memory_and_reports_every_persistence_todo()
    {
        var viewModel = new AdministrationViewModel();

        viewModel.UpdateSettings("inch", "0.08", false, "深色", "诊断");

        Assert.Equal("inch", viewModel.Settings.Unit);
        Assert.Contains("内存", viewModel.SettingsFeedback);
        Assert.Contains("TODO", viewModel.SettingsFeedback);
        Assert.Contains("规则写入", viewModel.UnimplementedCapabilities);
        Assert.Contains("权限认证", viewModel.UnimplementedCapabilities);
        Assert.Contains("日志落盘", viewModel.UnimplementedCapabilities);
        Assert.Contains("配置持久化", viewModel.UnimplementedCapabilities);
        Assert.Contains("适配器注册", viewModel.UnimplementedCapabilities);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T12")]
    public void Module_definition_is_discoverable_as_m12()
    {
        IDesktopModule module = new AdministrationModule();

        Assert.Equal("M12", module.Metadata.Id);
        Assert.Equal(12, module.Metadata.Order);
        Assert.IsType<AdministrationView>(module.CreateView());
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T12")]
    public void Preset_library_covers_every_required_rule_category()
    {
        var viewModel = new AdministrationViewModel();

        Assert.Equal(Enum.GetValues<PresetCategory>().Length, viewModel.Presets.Select(item => item.Category).Distinct().Count());
        Assert.All(viewModel.Presets, item => Assert.NotEmpty(item.Versions));
    }

    [Theory]
    [InlineData(RoleKind.Operator, false, false)]
    [InlineData(RoleKind.ProcessEngineer, true, false)]
    [InlineData(RoleKind.Reviewer, false, false)]
    [InlineData(RoleKind.Administrator, true, true)]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T12")]
    public void Role_switch_drives_rule_and_adapter_actions(RoleKind role, bool canEditRules, bool canManageAdapters)
    {
        var viewModel = new AdministrationViewModel();

        viewModel.SelectRole(role);

        Assert.Equal(canEditRules, viewModel.CanEditRules);
        Assert.Equal(canManageAdapters, viewModel.CanManageAdapters);
        Assert.Contains("内存", viewModel.ActionFeedback);
        Assert.Contains("TODO", viewModel.ActionFeedback);
    }
}
