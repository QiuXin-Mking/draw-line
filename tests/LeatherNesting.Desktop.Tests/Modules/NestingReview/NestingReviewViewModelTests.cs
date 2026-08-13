using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Modules.NestingReview;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.NestingReview;

public sealed class NestingReviewViewModelTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T09")]
    public void Demo_metrics_materials_and_unplaced_list_are_internally_consistent()
    {
        var viewModel = new NestingReviewViewModel();

        Assert.Equal(3, viewModel.Materials.Count);
        Assert.Equal("MAT-01", viewModel.SelectedMaterial.Id);
        Assert.Equal(86.4, viewModel.SelectedVersion.UtilizationPercent);
        Assert.Equal(92.3, viewModel.SelectedVersion.CompletionPercent);
        Assert.Equal(6.8, viewModel.SelectedVersion.UsedLengthMetres);
        Assert.NotEmpty(viewModel.UnplacedPieces);
        Assert.Equal(viewModel.UnplacedPieces.Sum(piece => piece.Quantity), viewModel.TotalUnplacedQuantity);
        Assert.Contains("小件", viewModel.LowUtilizationReasons);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T09")]
    public void Material_instance_and_version_selection_change_only_review_state()
    {
        var viewModel = new NestingReviewViewModel();

        viewModel.SelectMaterial("MAT-02");
        viewModel.SelectInstance("P-205-L-02");
        viewModel.SelectVersion("V2");

        Assert.Equal("MAT-02", viewModel.SelectedMaterial.Id);
        Assert.Equal("P-205-L-02", viewModel.SelectedInstance?.Id);
        Assert.Equal("V2", viewModel.SelectedVersion.Id);
        Assert.Equal(84.1, viewModel.SelectedVersion.UtilizationPercent);
        Assert.Equal("V1 → V2：利用率 +2.3%，完成率 +3.8%，用长 -0.4 m", viewModel.VersionComparison);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T09")]
    public void Collision_overlay_is_explicitly_a_demo_example()
    {
        var viewModel = new NestingReviewViewModel();

        Assert.False(viewModel.ShowCollisionOverlay);
        viewModel.ToggleCollisionOverlay();

        Assert.True(viewModel.ShowCollisionOverlay);
        Assert.Contains("碰撞示例", viewModel.CollisionOverlayMessage);
        Assert.Contains("不代表真实验证", viewModel.CollisionOverlayMessage);
    }

    [Theory]
    [InlineData(ReviewTodoAction.Drag)]
    [InlineData(ReviewTodoAction.Rotate)]
    [InlineData(ReviewTodoAction.Mirror)]
    [InlineData(ReviewTodoAction.Lock)]
    [InlineData(ReviewTodoAction.LocalRepack)]
    [InlineData(ReviewTodoAction.ValidateCollisions)]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T09")]
    public void Manual_adjustment_actions_are_todo_and_do_not_change_placement(ReviewTodoAction action)
    {
        var viewModel = new NestingReviewViewModel();
        var before = viewModel.SelectedMaterial.Instances.ToArray();

        viewModel.InvokeTodo(action);

        Assert.Equal(before, viewModel.SelectedMaterial.Instances);
        Assert.Contains(TodoBadge.StandardText, viewModel.TodoMessage);
        Assert.False(viewModel.HasRealCollisionValidation);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T09")]
    public void Module_definition_is_discoverable_as_M09()
    {
        IDesktopModule module = new NestingReviewModule();

        Assert.Equal("M09", module.Metadata.Id);
        Assert.Equal(9, module.Metadata.Order);
        Assert.IsType<NestingReviewView>(module.CreateView());
    }
}
