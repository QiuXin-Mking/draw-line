using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.BoardSettings;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.BoardSettings;

[Collection("Avalonia UI")]
public sealed class BoardSettingsViewTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "BOARD-001")]
    public void Fields_expose_the_evidence_defaults()
    {
        var view = new BoardSettingsView();

        Assert.Equal("a", view.NameEditor.Text);
        Assert.Equal("1380.00", view.MaterialWidthEditor.Text);
        Assert.Equal(string.Empty, view.MaterialLengthEditor.Text);
        Assert.Equal("1", view.LayerCountEditor.Text);
        Assert.Equal(string.Empty, view.MultiLayerRemainderEditor.Text);
        Assert.Equal("0.00", view.MaterialEdgeEditor.Text);
        Assert.Equal(string.Empty, view.PieceSpacingEditor.Text);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "BOARD-002")]
    public void Direction_defaults_to_vertical_and_radios_share_a_group()
    {
        var view = new BoardSettingsView();

        Assert.True(view.VerticalRadio.IsChecked);
        Assert.False(view.HorizontalRadio.IsChecked);
        Assert.Equal(view.VerticalRadio.GroupName, view.HorizontalRadio.GroupName);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "BOARD-003")]
    public void Confirm_button_is_default_and_uses_the_classic_focus_border()
    {
        var view = new BoardSettingsView();

        Assert.Equal("确定", view.ConfirmButton.Content);
        Assert.True(view.ConfirmButton.IsDefault);
        Assert.Same(AppTheme.ClassicFocus, view.ConfirmButton.BorderBrush);
        Assert.Equal(new Thickness(2), view.ConfirmButton.BorderThickness);
    }
}
