using LeatherNesting.Desktop.Modules.BoardSettings;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.BoardSettings;

public sealed class BoardSettingsViewModelTests
{
    [Fact]
    public void Defaults_match_the_user_confirmed_material_spec()
    {
        var vm = new BoardSettingsViewModel();

        Assert.Equal(string.Empty, vm.Name);
        Assert.Equal("纵向", vm.Direction);
        Assert.Equal("1360.00", vm.WidthText);
        Assert.Equal("0.00", vm.LengthText);
        Assert.Equal("6", vm.LayersText);
        Assert.Equal("补齐", vm.RemnantPolicy);
        Assert.Equal("0.00", vm.EdgeText);
        Assert.Equal("2.00", vm.SpacingText);
    }

    [Fact]
    public void Direction_and_remnant_policy_options_are_confirmed_only()
    {
        Assert.Equal(new[] { "横向", "纵向" }, BoardSettingsViewModel.DirectionOptions);
        Assert.Equal(new[] { "补齐", "丢弃" }, BoardSettingsViewModel.RemnantPolicyOptions);
    }

    [Fact]
    public void Valid_input_confirms_a_config_with_all_values()
    {
        var vm = new BoardSettingsViewModel
        {
            Name = "夏季男鞋",
            Direction = "横向",
            WidthText = "1370",
            LengthText = "5000",
            LayersText = "3",
            RemnantPolicy = "丢弃",
            EdgeText = "5.5",
            SpacingText = "1.5",
        };

        Assert.True(vm.TryConfirm());

        var config = vm.ConfirmedConfig!;
        Assert.Equal("夏季男鞋", config.Name);
        Assert.Equal(BoardDirection.Horizontal, config.Direction);
        Assert.Equal(1370, config.WidthMm);
        Assert.Equal(5000, config.LengthMm);
        Assert.Equal(3, config.Layers);
        Assert.Equal("丢弃", config.RemnantPolicy);
        Assert.Equal(5.5, config.EdgeMm);
        Assert.Equal(1.5, config.SpacingMm);
    }

    [Fact]
    public void Zero_length_is_valid_infinite_roll_semantics()
    {
        var vm = new BoardSettingsViewModel { LengthText = "0" };

        Assert.True(vm.TryConfirm());
        Assert.Equal(0, vm.ConfirmedConfig!.LengthMm);
        Assert.Null(vm.LengthError);
    }

    [Fact]
    public void Invalid_numeric_field_prevents_confirm_and_sets_inline_error()
    {
        var vm = new BoardSettingsViewModel { WidthText = "abc" };

        Assert.False(vm.TryConfirm());
        Assert.NotNull(vm.WidthError);
        Assert.Null(vm.ConfirmedConfig);
    }

    [Fact]
    public void Negative_width_is_rejected()
    {
        var vm = new BoardSettingsViewModel { WidthText = "-5" };

        Assert.False(vm.TryConfirm());
        Assert.NotNull(vm.WidthError);
    }

    [Fact]
    public void Layers_must_be_a_positive_arabic_integer()
    {
        var vm = new BoardSettingsViewModel { LayersText = "abc" };
        Assert.False(vm.TryConfirm());
        Assert.NotNull(vm.LayersError);

        vm = new BoardSettingsViewModel { LayersText = "0" };
        Assert.False(vm.TryConfirm());
        Assert.NotNull(vm.LayersError);
    }

    [Fact]
    public void Edge_and_spacing_reject_non_numeric_and_negative()
    {
        var vm = new BoardSettingsViewModel { EdgeText = "-1", SpacingText = "x" };

        Assert.False(vm.TryConfirm());
        Assert.NotNull(vm.EdgeError);
        Assert.NotNull(vm.SpacingError);
    }

    [Fact]
    public void Cancel_does_not_touch_confirmed_state()
    {
        var vm = new BoardSettingsViewModel();

        vm.Cancel();
        Assert.Null(vm.ConfirmedConfig);
    }

    [Fact]
    public void Store_confirm_replaces_current_and_raises_event()
    {
        var store = new BoardSettingsStore();
        var config = new BoardSettingsConfig("A", BoardDirection.Vertical, 1, 0, 6, "补齐", 0, 2);
        BoardSettingsConfig? raised = null;
        store.Confirmed += (_, c) => raised = c;

        store.Confirm(config);

        Assert.True(store.IsConfirmed);
        Assert.Same(config, store.Current);
        Assert.Same(config, raised);
    }
}
