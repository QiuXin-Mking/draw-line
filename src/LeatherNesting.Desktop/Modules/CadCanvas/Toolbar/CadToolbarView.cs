using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.DesignSystem.CadTools;

namespace LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

/// <summary>
/// Compact, presentation-only projection of <see cref="CadToolCatalog"/>.
/// Business state is supplied through the public presentation methods.
/// </summary>
public sealed class CadToolbarView : StackPanel
{
    private const double ToolSize = 24;
    private const double DisabledOpacity = 0.55;

    private readonly Action<CadToolDefinition>? _onToolInvoked;
    private readonly IReadOnlyDictionary<CadToolCommandKey, Button> _buttonsByKey;
    private readonly IReadOnlyDictionary<CadToolGroup, Border> _separatorBeforeGroup;
    private IReadOnlyList<CadToolDefinition> _visibleDefinitions = CadToolCatalog.All;
    private CadToolCommandKey? _activeKey;

    public CadToolbarView(Action<CadToolDefinition>? onToolInvoked = null)
    {
        _onToolInvoked = onToolInvoked;
        Orientation = Orientation.Horizontal;
        HorizontalAlignment = HorizontalAlignment.Right;
        VerticalAlignment = VerticalAlignment.Top;
        Spacing = 1;

        FillCheckBox = new CheckBox
        {
            IsChecked = true,
            Width = 18,
            Height = ToolSize,
            VerticalAlignment = VerticalAlignment.Center,
        };
        FillLabel = new TextBlock
        {
            Text = "填充",
            Foreground = AppTheme.DangerText,
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center,
        };
        FillValueInput = new TextBox
        {
            Text = "255",
            Width = 34,
            Height = ToolSize,
            MinWidth = 0,
            MaxLength = 3,
            Padding = new Thickness(2, 1),
            CornerRadius = new CornerRadius(0),
            TextAlignment = TextAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        Children.Add(FillCheckBox);
        Children.Add(FillLabel);
        Children.Add(FillValueInput);

        var buttons = new List<Button>(CadToolCatalog.All.Count);
        var separators = new List<Border>(Enum.GetValues<CadToolGroup>().Length - 1);
        var separatorBeforeGroup = new Dictionary<CadToolGroup, Border>();

        CadToolGroup? previousGroup = null;
        foreach (var definition in CadToolCatalog.All)
        {
            if (previousGroup is not null && previousGroup != definition.Group)
            {
                var separator = CreateSeparator();
                separators.Add(separator);
                separatorBeforeGroup.Add(definition.Group, separator);
                Children.Add(separator);
            }

            var button = CreateButton(definition);
            buttons.Add(button);
            Children.Add(button);
            previousGroup = definition.Group;
        }

        Buttons = Array.AsReadOnly(buttons.ToArray());
        Separators = Array.AsReadOnly(separators.ToArray());
        _buttonsByKey = buttons
            .Select((button, index) => (CadToolCatalog.All[index].CommandKey, Button: button))
            .ToDictionary(pair => pair.CommandKey, pair => pair.Button);
        _separatorBeforeGroup = separatorBeforeGroup;
    }

    public IReadOnlyList<CadToolDefinition> Definitions => CadToolCatalog.All;

    public IReadOnlyList<CadToolDefinition> VisibleDefinitions => _visibleDefinitions;

    public IReadOnlyList<Button> Buttons { get; }

    public IReadOnlyList<Border> Separators { get; }

    public CheckBox FillCheckBox { get; }

    public TextBlock FillLabel { get; }

    public TextBox FillValueInput { get; }

    public CadToolCommandKey? ActiveKey => _activeKey;

    public void SetVisibleKeys(IEnumerable<CadToolCommandKey> visibleKeys)
    {
        ArgumentNullException.ThrowIfNull(visibleKeys);
        var requested = visibleKeys.ToHashSet();
        EnsureKnownKeys(requested);

        _visibleDefinitions = Array.AsReadOnly(
            CadToolCatalog.All.Where(definition => requested.Contains(definition.CommandKey)).ToArray());
        foreach (var definition in CadToolCatalog.All)
            _buttonsByKey[definition.CommandKey].IsVisible = requested.Contains(definition.CommandKey);

        foreach (var separator in Separators)
            separator.IsVisible = false;

        CadToolGroup? previousVisibleGroup = null;
        foreach (var definition in _visibleDefinitions)
        {
            if (previousVisibleGroup is not null && previousVisibleGroup != definition.Group)
                _separatorBeforeGroup[definition.Group].IsVisible = true;
            previousVisibleGroup = definition.Group;
        }
    }

    public void SetActiveKey(CadToolCommandKey? activeKey)
    {
        if (activeKey is not null && !_buttonsByKey.ContainsKey(activeKey.Value))
            throw new ArgumentOutOfRangeException(nameof(activeKey), activeKey, "Unknown CAD tool command key.");

        _activeKey = activeKey;
        foreach (var definition in CadToolCatalog.All)
            RefreshAppearance(definition.CommandKey);
    }

    public void SetEnabledKeys(IEnumerable<CadToolCommandKey> enabledKeys)
    {
        ArgumentNullException.ThrowIfNull(enabledKeys);
        var requested = enabledKeys.ToHashSet();
        EnsureKnownKeys(requested);

        foreach (var definition in CadToolCatalog.All)
        {
            var button = _buttonsByKey[definition.CommandKey];
            button.IsEnabled = requested.Contains(definition.CommandKey);
            RefreshAppearance(definition.CommandKey);
        }
    }

    public void SetToolEnabled(CadToolCommandKey commandKey, bool isEnabled)
    {
        var button = ButtonFor(commandKey);
        button.IsEnabled = isEnabled;
        RefreshAppearance(commandKey);
    }

    private Button CreateButton(CadToolDefinition definition)
    {
        var button = new Button
        {
            Name = definition.ControlId,
            Content = CadToolIconFactory.Create(definition.IconKey),
            Width = ToolSize,
            Height = ToolSize,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Thickness(1),
            CornerRadius = new CornerRadius(0),
            BorderThickness = new Thickness(1),
            Background = AppTheme.ToolbarSurface,
            BorderBrush = AppTheme.ClassicBorderNeutral,
            Focusable = true,
            TabIndex = definition.Order,
        };
        ToolTip.SetTip(button, definition.Tooltip);
        AutomationProperties.SetName(button, definition.Label);
        AutomationProperties.SetAutomationId(button, definition.ControlId);

        button.Click += (_, _) => _onToolInvoked?.Invoke(definition);
        button.PointerEntered += (_, _) => RefreshAppearance(definition.CommandKey);
        button.PointerExited += (_, _) => RefreshAppearance(definition.CommandKey);
        button.GotFocus += (_, _) => RefreshAppearance(definition.CommandKey);
        button.LostFocus += (_, _) => RefreshAppearance(definition.CommandKey);
        return button;
    }

    private static Border CreateSeparator() => new()
    {
        Width = 1,
        Height = ToolSize,
        Margin = new Thickness(2, 0),
        Background = AppTheme.ClassicBorderNeutral,
        IsHitTestVisible = false,
    };

    private Button ButtonFor(CadToolCommandKey commandKey)
    {
        if (!_buttonsByKey.TryGetValue(commandKey, out var button))
            throw new ArgumentOutOfRangeException(nameof(commandKey), commandKey, "Unknown CAD tool command key.");
        return button;
    }

    private void RefreshAppearance(CadToolCommandKey commandKey)
    {
        var button = ButtonFor(commandKey);
        AutomationProperties.SetItemStatus(button, _activeKey == commandKey ? "当前工具" : null);
        if (!button.IsEnabled)
        {
            button.Background = AppTheme.DisabledSurface;
            button.BorderBrush = AppTheme.ClassicBorderNeutral;
            button.Opacity = DisabledOpacity;
            return;
        }

        button.Opacity = 1;
        button.Background = _activeKey == commandKey
            ? AppTheme.SelectionSurface
            : button.IsPointerOver ? AppTheme.ToolbarHoverSurface : AppTheme.ToolbarSurface;
        button.BorderBrush = _activeKey == commandKey || button.IsKeyboardFocusWithin
            ? AppTheme.ClassicFocus
            : AppTheme.ClassicBorderNeutral;
    }

    private static void EnsureKnownKeys(IEnumerable<CadToolCommandKey> keys)
    {
        var known = CadToolCatalog.All.Select(definition => definition.CommandKey).ToHashSet();
        foreach (var key in keys)
        {
            if (!known.Contains(key))
                throw new ArgumentOutOfRangeException(nameof(keys), key, "Unknown CAD tool command key.");
        }
    }
}
