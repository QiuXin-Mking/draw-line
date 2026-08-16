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
    public void Fields_expose_the_user_confirmed_defaults()
    {
        var view = new BoardSettingsView();

        Assert.Equal(string.Empty, view.NameEditor.Text);
        Assert.Equal("1360.00", view.MaterialWidthEditor.Text);
        Assert.Equal("0.00", view.MaterialLengthEditor.Text);
        Assert.Equal("6", view.LayerCountEditor.Text);
        Assert.Equal("0.00", view.MaterialEdgeEditor.Text);
        Assert.Equal("2.00", view.PieceSpacingEditor.Text);
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

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "BOARD-004")]
    public void Multi_layer_remainder_is_a_dropdown_with_the_confirmed_options()
    {
        var view = new BoardSettingsView();

        Assert.Equal(BoardSettingsViewModel.RemnantPolicyOptions, view.MultiLayerRemainderCombo.ItemsSource);
        Assert.Equal("补齐", view.MultiLayerRemainderCombo.SelectedItem);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "BOARD-005")]
    public void Cancel_button_is_present_beside_confirm()
    {
        var view = new BoardSettingsView();

        Assert.Equal("取消", view.CancelButton.Content);
        Assert.True(view.CancelButton.IsCancel);
        Assert.NotNull(view.ConfirmButton);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "BOARD-006")]
    public void Layer_count_filter_accepts_only_arabic_digits()
    {
        Assert.True(BoardSettingsView.IsArabicDigitText("7"));
        Assert.True(BoardSettingsView.IsArabicDigitText("0126"));
        Assert.False(BoardSettingsView.IsArabicDigitText("a"));
        Assert.False(BoardSettingsView.IsArabicDigitText("7a"));
    }
}
