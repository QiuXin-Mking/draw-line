using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Shell;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

public sealed class ShellTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T00")]
    public void Shell_registers_exactly_twelve_modules()
    {
        var viewModel = new AppShellViewModel();
        Assert.Equal(12, viewModel.Modules.Count);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T00")]
    public void Module_ids_are_unique_and_ordered()
    {
        var viewModel = new AppShellViewModel();
        var ids = viewModel.Modules.Select(m => m.Id).ToList();
        Assert.Equal(12, ids.Distinct().Count());
        Assert.Equal("M01", ids[0]);
        Assert.Equal("M12", ids[^1]);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T00")]
    public void Import_module_has_real_logic()
    {
        var viewModel = new AppShellViewModel();
        var import = viewModel.Modules.Single(m => m.Id == "M02");
        Assert.True(import.HasRealLogic);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T00")]
    public void Todo_badge_has_readable_text()
    {
        Assert.Contains("TODO", TodoBadge.StandardText);
        Assert.True(TodoBadge.StandardText.Length > 10);
    }
}
