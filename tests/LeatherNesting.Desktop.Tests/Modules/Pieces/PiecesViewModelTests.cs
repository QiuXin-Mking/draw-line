using LeatherNesting.Desktop.Modules.Pieces;
using LeatherNesting.Desktop.Shell;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.Pieces;

public sealed class PiecesViewModelTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T06")]
    public void Unplaced_quantity_is_never_negative()
    {
        var viewModel = new PiecesViewModel();

        var completed = viewModel.Pieces.Single(piece => piece.Code == "HEEL-38-L");
        var outstanding = viewModel.Pieces.Single(piece => piece.Code == "VAMP-39-R");

        Assert.Equal(0, completed.UnplacedQuantity);
        Assert.Equal(outstanding.RequiredQuantity - outstanding.PlacedQuantity, outstanding.UnplacedQuantity);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T06")]
    public void Search_sort_and_unfinished_filter_apply_to_the_demo_projection()
    {
        var viewModel = new PiecesViewModel();

        viewModel.SearchText = "vamp";
        viewModel.SortBy(PieceSortField.UnplacedQuantity);
        viewModel.ShowUnfinishedOnly = true;

        var visible = viewModel.VisiblePieces;

        Assert.NotEmpty(visible);
        Assert.All(visible, piece => Assert.Contains("VAMP", piece.Code, StringComparison.OrdinalIgnoreCase));
        Assert.All(visible, piece => Assert.True(piece.UnplacedQuantity > 0));
        Assert.True(visible.Zip(visible.Skip(1)).All(pair => pair.First.UnplacedQuantity >= pair.Second.UnplacedQuantity));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T06")]
    public void Bulk_selection_changes_only_demo_selection_and_marks_the_action_todo()
    {
        var viewModel = new PiecesViewModel();
        var requiredBefore = viewModel.Pieces.Select(piece => piece.RequiredQuantity).ToArray();

        viewModel.SetSelected("VAMP-39-R", true);
        viewModel.SetSelected("VAMP-39-L", true);
        viewModel.ApplyBulkPriority("高");

        Assert.Equal(new[] { "VAMP-39-L", "VAMP-39-R" }, viewModel.SelectedCodes.Order());
        Assert.Equal(requiredBefore, viewModel.Pieces.Select(piece => piece.RequiredQuantity));
        Assert.All(viewModel.Pieces.Where(piece => !viewModel.SelectedCodes.Contains(piece.Code)), piece => Assert.False(piece.IsSelected));
        Assert.Contains("TODO", viewModel.TodoMessage);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T06")]
    public void Editing_an_order_quantity_is_in_memory_and_explicitly_todo()
    {
        var viewModel = new PiecesViewModel();

        viewModel.SetRequiredQuantity("VAMP-39-L", 15);

        var piece = viewModel.Pieces.Single(record => record.Code == "VAMP-39-L");
        Assert.Equal(15, piece.RequiredQuantity);
        Assert.Equal(7, piece.UnplacedQuantity);
        Assert.Contains("TODO", viewModel.TodoMessage);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T06")]
    public void Module_declares_m06_metadata_and_creates_pieces_view()
    {
        var module = new PiecesModule();

        Assert.Equal("M06", module.Metadata.Id);
        Assert.Equal("裁片", module.Metadata.Title);
        Assert.IsType<PiecesView>(module.CreateView());
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T06")]
    public void Desktop_discovery_finds_the_local_m06_definition()
    {
        var modules = DesktopModuleDiscovery.Discover(typeof(PiecesModule).Assembly);

        Assert.Contains(modules, module => module is PiecesModule && module.Metadata.Id == "M06");
    }
}
