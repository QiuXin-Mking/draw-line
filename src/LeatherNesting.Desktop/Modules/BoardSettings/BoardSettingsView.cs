using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.BoardSettings;

/// <summary>「新建排版」入口弹出的版型设置模态对话框。</summary>
public sealed class BoardSettingsWindow : Window
{
    public BoardSettingsWindow() : this(new BoardSettingsViewModel())
    {
    }

    public BoardSettingsWindow(BoardSettingsViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        View = new BoardSettingsView(viewModel);

        Title = "版型设置";
        Width = 540;
        Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CanResize = false;
        // 独立弹窗不继承 MainWindow 的浅色主题，必须显式固定，否则 macOS 深色模式下控件按深色渲染。
        RequestedThemeVariant = ThemeVariant.Light;
        Background = AppTheme.PanelSurface;
        Content = View;

        View.ConfirmButton.Click += (_, _) =>
        {
            View.SyncToViewModel();
            if (viewModel.TryConfirm())
            {
                Config = viewModel.ConfirmedConfig;
                Close(true);
            }
            else
            {
                View.ShowErrors();
            }
        };
        View.CancelButton.Click += (_, _) =>
        {
            viewModel.Cancel();
            Close(false);
        };
    }

    public BoardSettingsViewModel ViewModel { get; }

    public BoardSettingsView View { get; }

    public BoardSettingsConfig? Config { get; private set; }
}

/// <summary>
/// 版型设置表单（2026-08-16 用户确认布局）：第 1 行版型名称；第 2 行版型方向（横向/纵向单选）；
/// 第 3 行材料宽度+长度；第 4 行材料层数+多层余片；第 5 行材料边缘+裁片间距；右下角确定/取消。
/// </summary>
public sealed class BoardSettingsView : UserControl
{
    private readonly BoardSettingsViewModel _viewModel;

    public BoardSettingsView() : this(new BoardSettingsViewModel())
    {
    }

    public BoardSettingsView(BoardSettingsViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Background = AppTheme.PanelSurface;

        NameEditor = TextEditor(_viewModel.Name, 130);
        HorizontalRadio = new RadioButton { Content = "横向", GroupName = "BoardDirection", Foreground = AppTheme.PrimaryText };
        VerticalRadio = new RadioButton { Content = "纵向", GroupName = "BoardDirection", IsChecked = true, Foreground = AppTheme.PrimaryText };
        MaterialWidthEditor = TextEditor(_viewModel.WidthText, 130);
        MaterialLengthEditor = TextEditor(_viewModel.LengthText, 130);
        LayerCountEditor = TextEditor(_viewModel.LayersText, 130);
        // Tunnel 优先于 TextBox 自身处理，拦截非阿拉伯数字。
        LayerCountEditor.AddHandler(InputElement.TextInputEvent, RejectNonArabicDigit, RoutingStrategies.Tunnel);
        MultiLayerRemainderCombo = new ComboBox
        {
            ItemsSource = BoardSettingsViewModel.RemnantPolicyOptions,
            SelectedItem = _viewModel.RemnantPolicy,
            Height = 28,
            Width = 130,
            Foreground = AppTheme.PrimaryText,
            Background = AppTheme.PanelSurface,
            BorderBrush = AppTheme.ClassicBorderNeutral,
            VerticalAlignment = VerticalAlignment.Center,
        };
        MaterialEdgeEditor = TextEditor(_viewModel.EdgeText, 130);
        PieceSpacingEditor = TextEditor(_viewModel.SpacingText, 130);

        WidthErrorText = ErrorLabel();
        LengthErrorText = ErrorLabel();
        LayersErrorText = ErrorLabel();
        EdgeErrorText = ErrorLabel();
        SpacingErrorText = ErrorLabel();

        var direction = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children = { HorizontalRadio, VerticalRadio },
        };

        ConfirmButton = new Button
        {
            Content = "确定",
            IsDefault = true,
            MinWidth = 88,
            Padding = new Thickness(20, 4),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = AppTheme.PrimaryText,
            Background = AppTheme.PanelSurface,
            BorderBrush = AppTheme.ClassicFocus,
            BorderThickness = new Thickness(2),
        };
        CancelButton = new Button
        {
            Content = "取消",
            IsCancel = true,
            MinWidth = 88,
            Padding = new Thickness(20, 4),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = AppTheme.PrimaryText,
            Background = AppTheme.PanelSurface,
            BorderBrush = AppTheme.ClassicBorderNeutral,
            BorderThickness = new Thickness(1),
        };

        var footer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { ConfirmButton, CancelButton },
        };

        Content = new StackPanel
        {
            Margin = new Thickness(20, 16),
            Spacing = 14,
            Children =
            {
                Field("版型名称", NameEditor),
                Field("版型方向", direction),
                Row(Field("材料宽度(mm)", MaterialWidthEditor, WidthErrorText),
                    Field("材料长度(mm)", MaterialLengthEditor, LengthErrorText)),
                Row(Field("材料层数", LayerCountEditor, LayersErrorText),
                    Field("多层余片", MultiLayerRemainderCombo)),
                Row(Field("材料边缘(mm)", MaterialEdgeEditor, EdgeErrorText),
                    Field("裁片间距(mm)", PieceSpacingEditor, SpacingErrorText)),
                footer,
            },
        };
    }

    public TextBox NameEditor { get; }
    public RadioButton HorizontalRadio { get; }
    public RadioButton VerticalRadio { get; }
    public TextBox MaterialWidthEditor { get; }
    public TextBox MaterialLengthEditor { get; }
    public TextBox LayerCountEditor { get; }
    public ComboBox MultiLayerRemainderCombo { get; }
    public TextBox MaterialEdgeEditor { get; }
    public TextBox PieceSpacingEditor { get; }
    public Button ConfirmButton { get; }
    public Button CancelButton { get; }

    public TextBlock WidthErrorText { get; }
    public TextBlock LengthErrorText { get; }
    public TextBlock LayersErrorText { get; }
    public TextBlock EdgeErrorText { get; }
    public TextBlock SpacingErrorText { get; }

    /// <summary>把控件值同步回表单模型（确定时由窗口调用）。</summary>
    public void SyncToViewModel()
    {
        _viewModel.Name = NameEditor.Text ?? string.Empty;
        _viewModel.Direction = VerticalRadio.IsChecked == true ? "纵向" : "横向";
        _viewModel.WidthText = MaterialWidthEditor.Text ?? string.Empty;
        _viewModel.LengthText = MaterialLengthEditor.Text ?? string.Empty;
        _viewModel.LayersText = LayerCountEditor.Text ?? string.Empty;
        _viewModel.RemnantPolicy = MultiLayerRemainderCombo.SelectedItem as string ?? BoardSettingsConfig.Default.RemnantPolicy;
        _viewModel.EdgeText = MaterialEdgeEditor.Text ?? string.Empty;
        _viewModel.SpacingText = PieceSpacingEditor.Text ?? string.Empty;
    }

    /// <summary>把校验错误显示到对应字段旁（非法输入时由窗口调用）。</summary>
    public void ShowErrors()
    {
        WidthErrorText.Text = _viewModel.WidthError ?? string.Empty;
        WidthErrorText.IsVisible = _viewModel.WidthError is not null;
        LengthErrorText.Text = _viewModel.LengthError ?? string.Empty;
        LengthErrorText.IsVisible = _viewModel.LengthError is not null;
        LayersErrorText.Text = _viewModel.LayersError ?? string.Empty;
        LayersErrorText.IsVisible = _viewModel.LayersError is not null;
        EdgeErrorText.Text = _viewModel.EdgeError ?? string.Empty;
        EdgeErrorText.IsVisible = _viewModel.EdgeError is not null;
        SpacingErrorText.Text = _viewModel.SpacingError ?? string.Empty;
        SpacingErrorText.IsVisible = _viewModel.SpacingError is not null;
    }

    /// <summary>材料层数只允许阿拉伯数字：拒绝其它字符输入。</summary>
    private static void RejectNonArabicDigit(object? sender, TextInputEventArgs e)
    {
        if (e.Text is { } text && !IsArabicDigitText(text))
            e.Handled = true;
    }

    /// <summary>层数输入过滤谓词（公开便于测试）。</summary>
    public static bool IsArabicDigitText(string text) => text.All(char.IsAsciiDigit);

    private static TextBox TextEditor(string value, double width = 0) => new()
    {
        Text = value,
        Height = 28,
        Width = width > 0 ? width : double.NaN,
        Padding = new Thickness(6, 0),
        VerticalAlignment = VerticalAlignment.Center,
        VerticalContentAlignment = VerticalAlignment.Center,
        Foreground = AppTheme.PrimaryText,
        Background = AppTheme.PanelSurface,
        BorderBrush = AppTheme.ClassicBorderNeutral,
        SelectionBrush = AppTheme.SelectionSurface,
        SelectionForegroundBrush = AppTheme.PrimaryText,
    };

    private static TextBlock ErrorLabel() => new()
    {
        FontSize = 10,
        Foreground = AppTheme.DangerText,
        IsVisible = false,
    };

    private const double LabelWidth = 80;

    private static Control Field(string label, Control editor, TextBlock? error = null)
    {
        var caption = new TextBlock
        {
            Text = label,
            FontSize = 12,
            Foreground = AppTheme.PrimaryText,
            Width = LabelWidth,
            VerticalAlignment = VerticalAlignment.Center,
        };
        editor.VerticalAlignment = VerticalAlignment.Center;
        var line = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { caption, editor },
        };
        if (error is null)
            return line;
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children = { line, error },
        };
        return panel;
    }

    private static Control Row(params Control[] children)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 24,
        };
        foreach (var child in children)
            row.Children.Add(child);
        return row;
    }
}
