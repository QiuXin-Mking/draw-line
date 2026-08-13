using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Modules.Contracts;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.CadCanvas;

public sealed class CadCanvasViewModelTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T03")]
    public void Demo_geometry_is_non_empty_and_covers_canvas_categories()
    {
        var geometry = DemoGeometryFactory.Create();

        Assert.NotEmpty(geometry);
        Assert.Equal(Enum.GetValues<CadObjectCategory>(), geometry.Select(item => item.Category).Distinct().Order().ToArray());
        Assert.All(geometry, item => Assert.NotEmpty(item.Loop.Curves));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T03")]
    public void Category_visibility_filters_only_the_requested_canvas_objects()
    {
        var viewModel = new CadCanvasViewModel();
        var originalCount = viewModel.VisibleLoops.Count;
        var internalLineCount = viewModel.Objects.Count(item => item.Category == CadObjectCategory.InternalLine);
        CadCanvasRenderRequest? request = null;
        viewModel.RenderRequested += value => request = value;

        viewModel.SetCategoryVisibility(CadObjectCategory.InternalLine, false);

        Assert.Equal(originalCount - internalLineCount, viewModel.VisibleLoops.Count);
        Assert.DoesNotContain(viewModel.VisibleObjects, item => item.Category == CadObjectCategory.InternalLine);
        Assert.Contains(viewModel.VisibleObjects, item => item.Category == CadObjectCategory.OuterContour);
        Assert.Contains(viewModel.VisibleObjects, item => item.Category == CadObjectCategory.Hole);
        Assert.NotNull(request);
        Assert.False(request.Refit);

        viewModel.SetCategoryVisibility(CadObjectCategory.InternalLine, true);

        Assert.Equal(originalCount, viewModel.VisibleLoops.Count);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T03")]
    public void Fit_all_publishes_a_refit_render_request()
    {
        var viewModel = new CadCanvasViewModel();
        CadCanvasRenderRequest? request = null;
        viewModel.RenderRequested += value => request = value;

        viewModel.FitAll();

        Assert.NotNull(request);
        Assert.True(request.Refit);
        Assert.Equal(viewModel.VisibleLoops, request.Loops);
        Assert.Contains("全图", viewModel.StatusMessage);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T03")]
    public void Todo_tools_report_standard_text_without_changing_geometry_or_visibility()
    {
        var viewModel = new CadCanvasViewModel();
        var geometryIds = viewModel.Objects.Select(item => item.Id).ToArray();
        var visibleIds = viewModel.VisibleObjects.Select(item => item.Id).ToArray();

        foreach (var tool in Enum.GetValues<CadCanvasTodoTool>())
            viewModel.InvokeTodo(tool);

        Assert.Contains(TodoBadge.StandardText, viewModel.StatusMessage);
        Assert.Equal(geometryIds, viewModel.Objects.Select(item => item.Id));
        Assert.Equal(visibleIds, viewModel.VisibleObjects.Select(item => item.Id));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T03")]
    public void Module_definition_is_discoverable_as_m03()
    {
        IDesktopModule module = new CadCanvasModule();

        Assert.Equal("M03", module.Metadata.Id);
        Assert.Equal(3, module.Metadata.Order);
        Assert.IsType<CadCanvasView>(module.CreateView());
    }
}
