using LeatherNesting.Desktop.Modules.Materials;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.Materials;

public sealed class MaterialsViewModelTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T07")]
    public void Sheet_and_roll_materials_have_clear_different_fields()
    {
        var viewModel = new MaterialsViewModel();
        var sheet = viewModel.Materials.Single(material => material.Kind == MaterialKind.Sheet);
        var rolls = viewModel.Materials.Where(material => material.Kind == MaterialKind.Roll).ToArray();

        Assert.NotNull(sheet.LengthMm);
        Assert.NotEmpty(rolls);
        Assert.All(rolls, roll => Assert.Null(roll.LengthMm));
        Assert.Contains("片料", sheet.EstimateLabel);
        Assert.All(rolls, roll => Assert.Contains("卷料", roll.EstimateLabel));
    }

    [Theory]
    [InlineData("", "1000", "1")]
    [InlineData("-1", "1000", "1")]
    [InlineData("2000", "0", "1")]
    [InlineData("2000", "1000", "-2")]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T07")]
    public void Empty_or_non_positive_input_shows_field_error(string width, string length, string layers)
    {
        var viewModel = new MaterialsViewModel();

        Assert.False(viewModel.UpdateSelected(width, length, layers));
        Assert.True(viewModel.WidthError is not null || viewModel.LengthError is not null || viewModel.LayerError is not null);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T07")]
    public void Editing_only_changes_the_in_memory_demo_material()
    {
        var viewModel = new MaterialsViewModel();

        Assert.True(viewModel.UpdateSelected("2100", "1100", "2"));
        Assert.Equal(2100, viewModel.Selected.WidthMm);
        Assert.Contains("TODO", viewModel.TodoMessage);
        Assert.Contains("DEMO", viewModel.Summary);
    }
}
