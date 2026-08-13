using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Pieces;

/// <summary>High-density M06 demo page for pieces, sizes, order quantities, and placement constraints.</summary>
public sealed class PiecesView : UserControl
{
    private readonly PiecesViewModel _viewModel = new();
    private readonly StackPanel _records = new() { Spacing = 4 };
    private readonly TextBlock _todo = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _inspector = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _summary = new() { FontWeight = FontWeight.Bold };

    public PiecesView() => Content = BuildLayout();

    private Control BuildLayout()
    {
        Refresh();
        var search = new TextBox { Width = 220, PlaceholderText = "搜索裁片编号或名称" };
        search.TextChanged += (_, _) => { _viewModel.SearchText = search.Text ?? string.Empty; Refresh(); };
        var unfinished = new CheckBox { Content = "仅未完成" };
        unfinished.IsCheckedChanged += (_, _) => { _viewModel.ShowUnfinishedOnly = unfinished.IsChecked == true; Refresh(); };
        var sort = new ComboBox { Width = 130, ItemsSource = Enum.GetValues<PieceSortField>(), SelectedItem = PieceSortField.Code };
        sort.SelectionChanged += (_, _) => { if (sort.SelectedItem is PieceSortField field) { _viewModel.SortBy(field); Refresh(); } };
        var bulk = new Button { Content = "批量设为高优先级" };
        bulk.Click += (_, _) => { _viewModel.ApplyBulkPriority("高"); Refresh(); };

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { search, unfinished, sort, bulk } };
        var table = new Border { Background = AppTheme.Surface, BorderBrush = AppTheme.SurfaceBorder, BorderThickness = new Thickness(1), Padding = new Thickness(10), Child = _records };
        var inspector = new Border { Background = AppTheme.Surface, BorderBrush = AppTheme.SurfaceBorder, BorderThickness = new Thickness(1), Padding = new Thickness(12), Child = new StackPanel { Spacing = 8, Children =
        {
            new TextBlock { Text = "裁片检查器", FontWeight = FontWeight.Bold }, _inspector, new TodoBadge("TODO · 保存订单、批量写入及真实排样回写未接入"),
        } } };
        var grid = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,280"), ColumnSpacing = 16, Children = { table, inspector } };
        Grid.SetColumn(inspector, 1);
        return new ScrollViewer { Content = new StackPanel { Margin = new Thickness(24), Spacing = 12, Children =
        {
            new TextBlock { Text = "裁片、尺码与订单数量", FontSize = 20, FontWeight = FontWeight.Bold }, _summary, new TodoBadge(), toolbar,
            HeaderRow(), grid, _todo,
        } } };
    }

    private static Control HeaderRow() => new TextBlock
    {
        Text = "缩略图 / 编号               尺码 左右  需求  已放  未放  优先级  角度 / 镜像 / 间距",
        FontWeight = FontWeight.Bold, Foreground = AppTheme.TextMuted,
    };

    private void Refresh()
    {
        _summary.Text = $"计划 {_viewModel.PlannedQuantity} · 已放 {_viewModel.PlacedQuantity} · 未放 {_viewModel.UnplacedQuantity}";
        _todo.Text = _viewModel.TodoMessage ?? "演示数据仅在内存中变更；统计未连接真实排样。";
        _records.Children.Clear();
        foreach (var piece in _viewModel.VisiblePieces)
            _records.Children.Add(PieceRow(piece));
    }

    private Control PieceRow(PieceDemoRecord piece)
    {
        var selected = new CheckBox { IsChecked = piece.IsSelected, VerticalAlignment = VerticalAlignment.Center };
        selected.IsCheckedChanged += (_, _) => _viewModel.SetSelected(piece.Code, selected.IsChecked == true);
        var quantity = new TextBox { Text = piece.RequiredQuantity.ToString(), Width = 42 };
        quantity.LostFocus += (_, _) => { if (int.TryParse(quantity.Text, out var value)) { _viewModel.SetRequiredQuantity(piece.Code, value); Refresh(); } };
        var select = new Button { Content = $"▱  {piece.Code} · {piece.Name}", HorizontalContentAlignment = HorizontalAlignment.Left, MinWidth = 200 };
        select.Click += (_, _) => { _viewModel.SelectPiece(piece.Code); _inspector.Text = $"{piece.Code}\n尺码：{piece.Size} · {piece.Side}\n需求 {piece.RequiredQuantity} / 已放 {piece.PlacedQuantity} / 未放 {piece.UnplacedQuantity}\n优先级：{piece.Priority}\n允许角度：{piece.AllowedAngles}\n镜像：{(piece.MirrorAllowed ? "允许" : "禁止")} · 间距：{piece.GapMillimetres} mm"; Refresh(); };
        return new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children =
        {
            selected, select, new TextBlock { Text = piece.Size, Width = 28 }, new TextBlock { Text = piece.Side, Width = 34 }, quantity,
            new TextBlock { Text = piece.PlacedQuantity.ToString(), Width = 30 }, new TextBlock { Text = piece.UnplacedQuantity.ToString(), Width = 30, Foreground = piece.UnplacedQuantity > 0 ? AppTheme.TodoAmber : AppTheme.TextMuted },
            new TextBlock { Text = piece.Priority, Width = 35 }, new TextBlock { Text = $"{piece.AllowedAngles} / {(piece.MirrorAllowed ? "镜像" : "不镜像")} / {piece.GapMillimetres} mm" },
        } };
    }
}
