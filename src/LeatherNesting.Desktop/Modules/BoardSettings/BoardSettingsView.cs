using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.BoardSettings;

/// <summary>「新建排版」入口弹出的版型设置模态对话框。</summary>
public sealed class BoardSettingsWindow : Window
{
    public BoardSettingsWindow()
    {
        Title = "版型设置";
        Width = 380;
        Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CanResize = false;
        Background = AppTheme.PanelSurface;
        Content = new BoardSettingsView();
    }
}

/// <summary>版型设置表单：版型名称 / 方向 / 材料尺寸与排样参数。</summary>
public sealed class BoardSettingsView : UserControl
{
    public BoardSettingsView()
    {
        Background = AppTheme.PanelSurface;

        NameEditor = TextEditor("a");
        HorizontalRadio = new RadioButton { Content = "横向", GroupName = "BoardDirection", Foreground = AppTheme.PrimaryText };
        VerticalRadio = new RadioButton { Content = "纵向", GroupName = "BoardDirection", IsChecked = true, Foreground = AppTheme.PrimaryText };
        MaterialWidthEditor = TextEditor("1380.00");
        MaterialLengthEditor = TextEditor(string.Empty);
        LayerCountEditor = TextEditor("1");
        MultiLayerRemainderEditor = TextEditor(string.Empty);
        MaterialEdgeEditor = TextEditor("0.00");
        PieceSpacingEditor = TextEditor(string.Empty);

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
        ConfirmButton.Click += (_, _) => CloseWindow();

        var form = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("Auto,*"),
            RowDefinitions = RowDefinitions.Parse("38,38,38,38,38,38,38,38"),
            Margin = new Thickness(20, 20, 20, 0),
        };
        AddField(form, 0, "版型名称", NameEditor);
        AddField(form, 1, "版型方向", direction);
        AddField(form, 2, "材料宽度(mm)", MaterialWidthEditor);
        AddField(form, 3, "材料长度(mm)", MaterialLengthEditor);
        AddField(form, 4, "材料层数", LayerCountEditor);
        AddField(form, 5, "多层余片", MultiLayerRemainderEditor);
        AddField(form, 6, "材料边缘(mm)", MaterialEdgeEditor);
        AddField(form, 7, "裁片间距(mm)", PieceSpacingEditor);

        var footer = new Border
        {
            Padding = new Thickness(20, 12),
            BorderBrush = AppTheme.ClassicBorderNeutral,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Child = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Children = { ConfirmButton },
            },
        };

        Content = new Grid
        {
            RowDefinitions = RowDefinitions.Parse("*,Auto"),
            Children = { form, footer },
        };
        Grid.SetRow(footer, 1);
    }

    public TextBox NameEditor { get; }
    public RadioButton HorizontalRadio { get; }
    public RadioButton VerticalRadio { get; }
    public TextBox MaterialWidthEditor { get; }
    public TextBox MaterialLengthEditor { get; }
    public TextBox LayerCountEditor { get; }
    public TextBox MultiLayerRemainderEditor { get; }
    public TextBox MaterialEdgeEditor { get; }
    public TextBox PieceSpacingEditor { get; }
    public Button ConfirmButton { get; }

    private void CloseWindow() => (TopLevel.GetTopLevel(this) as Window)?.Close();

    private static void AddField(Grid grid, int row, string label, Control editor)
    {
        var caption = new TextBlock
        {
            Text = label,
            Foreground = AppTheme.PrimaryText,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
        };
        editor.VerticalAlignment = VerticalAlignment.Center;
        grid.Children.Add(caption);
        grid.Children.Add(editor);
        Grid.SetColumn(caption, 0);
        Grid.SetRow(caption, row);
        Grid.SetColumn(editor, 1);
        Grid.SetRow(editor, row);
    }

    private static TextBox TextEditor(string value) => new()
    {
        Text = value,
        Height = 28,
        Padding = new Thickness(6, 0),
        VerticalAlignment = VerticalAlignment.Center,
        Foreground = AppTheme.PrimaryText,
    };
}
