using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Pieces;

public enum PieceCardField
{
    Thumbnail, Size, BoundingDimensions, Rotation, Completion,
    SingleSetQuantity, SetCount, RemainderQuantity, TotalQuantity,
}

public sealed class OrderGroupPanelView : UserControl
{
    public OrderGroupPanelView(OrderPiecePanelState state)
    {
        var count = new TextBlock { FontSize = 10.5, HorizontalAlignment = HorizontalAlignment.Right };
        void Refresh() => count.Text = $"片数：{state.GroupPieceCount}";
        state.Changed += (_, _) => Refresh();
        Refresh();

        var properties = new Button { Content = "属性…", FontSize = 10, Padding = new Thickness(5, 1) };
        properties.Click += async (_, _) =>
        {
            state.LoadImage13PropertyDemo();
            if (TopLevel.GetTopLevel(this) is Window owner)
                await new PiecePropertiesWindow(state).ShowDialog(owner);
        };

        var tools = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 3,
            Children =
        {
            CompactButton("📁"), CompactButton("添加组"), CompactButton("删除"), CompactButton("添加"), properties,
        }
        };
        var group = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"),
            Children =
        {
            Text(state.GroupName, FontWeight.SemiBold), count,
        }
        };
        Grid.SetColumn(count, 1);
        Content = new StackPanel
        {
            Margin = new Thickness(3, 2),
            Spacing = 2,
            Children =
        {
            Text(state.ChannelSummary), Text($"▾ {state.OrderName}\n   └ {state.GroupName}"), tools, group,
            new TextBlock { Text = state.PersistenceNotice, FontSize = 8.5, Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap },
        }
        };
    }

    private static Button CompactButton(string label) => new() { Content = label, FontSize = 10, Padding = new Thickness(4, 1) };
    private static TextBlock Text(string value, FontWeight? weight = null) => new() { Text = value, FontSize = 10.5, FontWeight = weight ?? FontWeight.Normal };
}

public sealed class PieceCardListView : UserControl
{
    public const int EvidencedVisibleCardCount = 6;
    private readonly StackPanel _cards = new() { Spacing = 1 };
    private readonly OrderPiecePanelState _state;

    public PieceCardListView(OrderPiecePanelState state)
    {
        _state = state;
        state.Changed += (_, _) => Refresh();
        Refresh();
        Content = new ScrollViewer { Content = _cards, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
    }

    public IReadOnlyList<PieceCardView> Cards => _cards.Children.OfType<PieceCardView>().ToArray();

    private void Refresh()
    {
        _cards.Children.Clear();
        foreach (var piece in _state.Pieces)
            _cards.Children.Add(new PieceCardView(_state, piece));
    }
}

public sealed class PieceCardView : Border
{
    public static IReadOnlyList<PieceCardField> EvidencedFieldOrder { get; } =
    [
        PieceCardField.Thumbnail, PieceCardField.Size, PieceCardField.BoundingDimensions,
        PieceCardField.Rotation, PieceCardField.Completion, PieceCardField.SingleSetQuantity,
        PieceCardField.SetCount, PieceCardField.RemainderQuantity, PieceCardField.TotalQuantity,
    ];

    public PieceCardView(OrderPiecePanelState state, OrderPieceRecord piece)
    {
        Height = 76;
        Background = AppTheme.DemoPanelBackground;
        BorderBrush = AppTheme.ClassicBorder;
        BorderThickness = new Thickness(0, 0, 0, 1);
        Padding = new Thickness(2, 1);

        var enabled = new CheckBox { IsChecked = piece.IsIncluded, VerticalAlignment = VerticalAlignment.Center };
        enabled.IsCheckedChanged += (_, _) => state.SetIncluded(piece.Index, enabled.IsChecked == true);
        var visibility = new TextBlock
        {
            Text = "◉",
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTip.SetTip(visibility, "可见");
        var thumbnail = new Border
        {
            Width = 36,
            Height = 45,
            CornerRadius = new CornerRadius(13, 5, 14, 7),
            Background = ThumbnailBrush(piece.Index),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
        };
        var identityControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 1,
            Children = { enabled, visibility },
        };
        var identity = new StackPanel
        {
            Width = 54,
            Spacing = 1,
            Children =
        {
            new TextBlock { Text = piece.Index.ToString(), FontSize = 10 }, identityControls, thumbnail,
        }
        };
        var size = new TextBlock { Text = piece.Size, Foreground = Brushes.Red, FontSize = 11, FontWeight = FontWeight.Bold };
        var dimensions = new TextBlock { Text = piece.BoundingDimensions, FontSize = 9.5, HorizontalAlignment = HorizontalAlignment.Right };
        var header = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"), Children = { size, dimensions } };
        Grid.SetColumn(dimensions, 1);
        var rotation = new TextBlock { Text = piece.Rotation, FontSize = 9.5 };
        var completion = new TextBlock { Text = piece.Completion, FontSize = 9.5, HorizontalAlignment = HorizontalAlignment.Right };
        var policy = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"), Children = { rotation, completion } };
        Grid.SetColumn(completion, 1);
        var quantities = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto,*"), RowDefinitions = RowDefinitions.Parse("Auto,Auto") };
        AddQuantity(quantities, state, piece, "单套", piece.SingleSetQuantity, 0, 0, QuantityField.SingleSet);
        AddQuantity(quantities, state, piece, "套数", piece.SetCount, 2, 0, QuantityField.SetCount);
        AddQuantity(quantities, state, piece, "余量", piece.RemainderQuantity, 0, 1, QuantityField.Remainder);
        AddQuantity(quantities, state, piece, "总量", piece.TotalQuantity, 2, 1, QuantityField.Total);
        var details = new StackPanel { Spacing = 1, Children = { header, policy, quantities } };
        var layout = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("56,*"), Children = { identity, details } };
        Grid.SetColumn(details, 1);
        Child = layout;
    }

    private enum QuantityField { SingleSet, SetCount, Remainder, Total }

    private static void AddQuantity(Grid grid, OrderPiecePanelState state, OrderPieceRecord piece, string label, int value, int column, int row, QuantityField field)
    {
        var caption = new TextBlock { Text = label, FontSize = 8.5, VerticalAlignment = VerticalAlignment.Center };
        var input = new TextBox { Text = value.ToString(), FontSize = 8.5, Height = 20, MinWidth = 27, Padding = new Thickness(2, 0), IsReadOnly = field == QuantityField.Total };
        input.LostFocus += (_, _) =>
        {
            if (!int.TryParse(input.Text, out var parsed)) return;
            state.UpdateQuantities(piece.Index,
                field == QuantityField.SingleSet ? parsed : piece.SingleSetQuantity,
                field == QuantityField.SetCount ? parsed : piece.SetCount,
                field == QuantityField.Remainder ? parsed : piece.RemainderQuantity);
        };
        grid.Children.Add(caption);
        grid.Children.Add(input);
        Grid.SetColumn(caption, column); Grid.SetRow(caption, row);
        Grid.SetColumn(input, column + 1); Grid.SetRow(input, row);
    }

    private static IBrush ThumbnailBrush(int index)
    {
        Color[] colors = [Colors.SeaGreen, Colors.Wheat, Colors.IndianRed, Colors.SteelBlue, Colors.Sienna, Colors.LightPink];
        return new ImmutableSolidColorBrush(colors[(index - 1) % colors.Length]);
    }
}

public sealed class ProgressSummaryView : UserControl
{
    private readonly TextBlock _count = new() { FontSize = 9.5 };
    private readonly OrderPiecePanelState _state;

    public ProgressSummaryView(OrderPiecePanelState state)
    {
        _state = state;
        state.Changed += (_, _) => Refresh();
        var groupProgress = Bar(13.07, state.GroupProgress);
        var orderProgress = Bar(92.81, state.OrderProgress);
        Content = new StackPanel
        {
            Margin = new Thickness(3, 2),
            Spacing = 2,
            Children =
        {
            _count, new TextBlock { Text = "组进度：", FontSize = 9.5 }, groupProgress,
            new TextBlock { Text = $"总订单：{state.OrderCountSummary}  {state.OrderAreaSummary}", FontSize = 9.5 }, orderProgress,
            new TextBlock { Text = state.EvidenceGapNotice, FontSize = 8, Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap },
        }
        };
        Refresh();
    }

    private void Refresh() => _count.Text = $"总数：{_state.GroupCountSummary}   面积：{_state.GroupAreaSummary}";

    private static ProgressBar Bar(double value, string text) => new()
    {
        Minimum = 0,
        Maximum = 100,
        Value = value,
        Height = 15,
        ShowProgressText = true,
        ProgressTextFormat = text,
        Foreground = AppTheme.ToolbarAccent,
    };
}
