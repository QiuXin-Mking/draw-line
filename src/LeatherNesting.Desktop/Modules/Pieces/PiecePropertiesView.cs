using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Pieces;

public sealed class PiecePropertiesWindow : Window
{
    public PiecePropertiesWindow(OrderPiecePanelState state)
    {
        Title = "属性 · DEMO";
        Width = 1180;
        Height = 720;
        Background = AppTheme.PanelSurface;
        Content = new PiecePropertiesView(state);
    }
}

public sealed class PiecePropertiesView : UserControl
{
    private readonly OrderPiecePanelState _state;
    public static IReadOnlyList<string> ColumnOrder { get; } =
    ["图形", "名称", "尺寸", "角度", "微动", "优先级", "小片插刀", "单套", "套数", "总量", "余量", "附加间距", "面积", "片料耗", "片超料%"];

    public PiecePropertiesView(OrderPiecePanelState state)
    {
        _state = state;
        Background = AppTheme.PanelSurface;
        SplitRatio = "46*,54*";
        InitialFocusField = "单套";
        FirstSingleSetEditor = new TextBox
        {
            Text = state.Pieces[0].SingleSetQuantity.ToString(),
            Width = 34,
            Height = 24,
            Padding = new Thickness(2, 0),
            FontSize = 9.5,
            BorderBrush = AppTheme.ClassicFocus,
            BorderThickness = new Thickness(2),
        };
        AdvancedPropertiesCheckBox = new CheckBox { Content = "高级属性", IsChecked = true };
        SelectAllCheckBox = new CheckBox { Content = "全选", IsChecked = false };

        var right = new Grid { RowDefinitions = RowDefinitions.Parse("Auto,Auto,*,Auto") };
        var heading = BuildHeading(state);
        var batch = BuildBatchStrip(state);
        var table = BuildTable(state);
        var notice = new TextBlock { Text = $"{state.EvidenceGapNotice}  {state.PersistenceNotice}", Foreground = AppTheme.TodoAmber, FontSize = 10, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(4) };
        right.Children.Add(heading); right.Children.Add(batch); right.Children.Add(table); right.Children.Add(notice);
        Grid.SetRow(batch, 1); Grid.SetRow(table, 2); Grid.SetRow(notice, 3);

        var preview = new Border
        {
            Background = AppTheme.CanvasBlack,
            BorderBrush = AppTheme.ClassicBorderNeutral,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = new Border
            {
                Width = 240,
                Height = 390,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = AppTheme.GeometrySelectionFill,
                BorderBrush = AppTheme.GeometryOuterContour,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(70, 20, 90, 35),
                Child = new TextBlock { Text = "●     ●\n\nDEMO 裁片预览\n\n绿色内线 / 蓝色孔点", Foreground = AppTheme.GeometryInternalLine, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
            },
        };
        var split = new Grid { ColumnDefinitions = ColumnDefinitions.Parse(SplitRatio), Children = { preview, right } };
        Grid.SetColumn(right, 1);
        Content = split;
    }

    public string SplitRatio { get; }
    public string InitialFocusField { get; }
    public CheckBox AdvancedPropertiesCheckBox { get; }
    public CheckBox SelectAllCheckBox { get; }
    public TextBox FirstSingleSetEditor { get; }

    public void ApplyBatchQuantities(int singleSetQuantity, int setCount, int remainderQuantity)
    {
        foreach (var piece in _state.Pieces.Where(piece => piece.IsIncluded).ToArray())
            _state.UpdateQuantities(piece.Index, singleSetQuantity, setCount, remainderQuantity);
    }

    private Control BuildHeading(OrderPiecePanelState state)
    {
        var summary = new TextBlock { Text = $"{state.ChannelSummary} | 100套  总数:{state.GroupTotalQuantity}/{state.GroupTotalQuantity}", FontSize = 11, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
        {
            new TextBlock { Text = "字体大小:12", VerticalAlignment = VerticalAlignment.Center }, AdvancedPropertiesCheckBox,
            new Button { Content = "确定" }, new Button { Content = "取消" },
        }
        };
        var row = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"), Margin = new Thickness(4), Children = { summary, buttons } };
        Grid.SetColumn(buttons, 1);
        return row;
    }

    private Control BuildBatchStrip(OrderPiecePanelState state)
    {
        var strip = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 3,
            Margin = new Thickness(3),
            Children =
        {
            SelectAllCheckBox, Input("统一名称", "40", 44), Input("角度", "任意角度", 70), Input("微动角度", "360.0", 48),
            Input("优先级", "0", 32), new CheckBox { Content = "插刀", IsChecked = false }, Labelled("单套", FirstSingleSetEditor),
            Input("套数", "100", 38), Input("余量", "0", 32), Input("附加间距", "0.00", 42), Input("片料耗", "0.0000", 52),
        }
        };
        return new ScrollViewer { Content = strip, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
    }

    private static Control BuildTable(OrderPiecePanelState state)
    {
        var rows = new StackPanel();
        rows.Children.Add(TableRow(ColumnOrder, true));
        foreach (var piece in state.Pieces)
        {
            string[] values =
            [
                "☑", piece.Name, piece.BoundingDimensions, piece.Rotation, piece.FineRotation.ToString("0.0"), piece.Priority.ToString(),
                "☐", piece.SingleSetQuantity.ToString(), piece.SetCount.ToString(), piece.TotalQuantity.ToString(), piece.RemainderQuantity.ToString(),
                piece.ExtraSpacing.ToString("0.00"), piece.Area.ToString("0.0000"), piece.PieceConsumption.ToString("0.0000"), piece.PieceOveragePercent.ToString("0.0000"),
            ];
            rows.Children.Add(TableRow(values, false));
        }
        return new ScrollViewer { Content = rows, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
    }

    private static Control TableRow(IReadOnlyList<string> values, bool header)
    {
        var grid = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("45,48,70,78,48,54,64,44,44,48,48,62,58,58,62"), Height = header ? 30 : 35 };
        for (var i = 0; i < values.Count; i++)
        {
            var cell = new Border
            {
                BorderBrush = AppTheme.ClassicBorderNeutral,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Background = header ? AppTheme.HeaderSurface : AppTheme.PanelSurface,
                Child = new TextBlock { Text = values[i], FontSize = 9.5, FontWeight = header ? FontWeight.SemiBold : FontWeight.Normal, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Center },
            };
            grid.Children.Add(cell); Grid.SetColumn(cell, i);
        }
        return grid;
    }

    private static Control Input(string label, string value, double width) => new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Children = { new TextBlock { Text = $"{label}:", FontSize = 9.5, VerticalAlignment = VerticalAlignment.Center }, new TextBox { Text = value, Width = width, Height = 24, Padding = new Thickness(2, 0), FontSize = 9.5 } },
    };

    private static Control Labelled(string label, Control editor) => new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Children = { new TextBlock { Text = $"{label}:", FontSize = 9.5, VerticalAlignment = VerticalAlignment.Center }, editor },
    };
}
