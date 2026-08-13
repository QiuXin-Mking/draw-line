using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.NestingReview;

/// <summary>M09 result canvas and manual-review demonstration page.</summary>
public sealed class NestingReviewView : UserControl
{
    private readonly NestingReviewViewModel _viewModel = new();
    private readonly NestingCanvas _canvas = new() { MinHeight = 420 };
    private readonly StackPanel _metrics = new() { Spacing = 6 };
    private readonly StackPanel _inspector = new() { Spacing = 6 };
    private readonly TextBlock _todo = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _collisionNote = new() { Foreground = Brushes.OrangeRed, TextWrapping = TextWrapping.Wrap };

    public NestingReviewView()
    {
        _canvas.InstanceSelected += (_, id) => { _viewModel.SelectInstance(id); Refresh(); };
        Content = BuildLayout();
        Refresh();
    }

    private Control BuildLayout()
    {
        var materialTabs = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        foreach (var material in _viewModel.Materials)
        {
            var button = new Button { Content = $"{material.Id} · {material.MaterialType}" };
            button.Click += (_, _) => { _viewModel.SelectMaterial(material.Id); Refresh(); };
            materialTabs.Children.Add(button);
        }

        var versions = new ComboBox { Width = 190, ItemsSource = _viewModel.Versions, DisplayMemberBinding = new Avalonia.Data.Binding(nameof(NestingVersionDemo.Label)) };
        versions.SelectionChanged += (_, _) =>
        {
            if (versions.SelectedItem is NestingVersionDemo version)
            {
                _viewModel.SelectVersion(version.Id);
                Refresh();
            }
        };

        var collision = new CheckBox { Content = "显示碰撞示例覆盖层" };
        collision.IsCheckedChanged += (_, _) => { _viewModel.ToggleCollisionOverlay(); Refresh(); };
        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children =
        {
            TodoButton("拖动", ReviewTodoAction.Drag), TodoButton("旋转", ReviewTodoAction.Rotate),
            TodoButton("镜像", ReviewTodoAction.Mirror), TodoButton("锁定", ReviewTodoAction.Lock),
            TodoButton("局部重排", ReviewTodoAction.LocalRepack), TodoButton("真实碰撞验证", ReviewTodoAction.ValidateCollisions),
        }};

        var canvasCard = Card(new StackPanel { Spacing = 8, Children =
        {
            new TextBlock { Text = "材料画布 · 点击实例选择", FontWeight = FontWeight.Bold }, materialTabs, _canvas, collision, _collisionNote,
        }});
        var side = new StackPanel { Spacing = 12, Children =
        {
            Card(new StackPanel { Spacing = 8, Children = { new TextBlock { Text = "方案指标", FontWeight = FontWeight.Bold }, _metrics, versions, new TextBlock { Text = _viewModel.VersionComparison, TextWrapping = TextWrapping.Wrap } } }),
            Card(new StackPanel { Spacing = 8, Children = { new TextBlock { Text = "实例检查器", FontWeight = FontWeight.Bold }, _inspector } }),
            Card(BuildUnplaced()),
            Card(new StackPanel { Spacing = 5, Children = { new TextBlock { Text = "低利用率原因", FontWeight = FontWeight.Bold }, new TextBlock { Text = _viewModel.LowUtilizationReasons, TextWrapping = TextWrapping.Wrap, Foreground = AppTheme.TextMuted } } }),
        }};
        var body = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,310"), ColumnSpacing = 14, Children = { canvasCard, side } };
        Grid.SetColumn(side, 1);

        return new ScrollViewer { Content = new StackPanel { Margin = new Thickness(20), Spacing = 12, Children =
        {
            new TextBlock { Text = "排样结果复核与人工微调", FontSize = 21, FontWeight = FontWeight.Bold },
            new TodoBadge("TODO · 拖动、旋转、镜像、锁定、局部重排和真实碰撞验证均未接入"),
            toolbar, _todo, body,
        }}};
    }

    private Control BuildUnplaced()
    {
        var panel = new StackPanel { Spacing = 5, Children = { new TextBlock { Text = $"未放清单 · {_viewModel.TotalUnplacedQuantity} 件", FontWeight = FontWeight.Bold } } };
        foreach (var piece in _viewModel.UnplacedPieces)
            panel.Children.Add(new TextBlock { Text = $"{piece.PieceCode} / {piece.Size} × {piece.Quantity}\n{piece.Reason}", TextWrapping = TextWrapping.Wrap, Foreground = AppTheme.TextMuted });
        return panel;
    }

    private Button TodoButton(string text, ReviewTodoAction action)
    {
        var button = new Button { Content = $"{text} · TODO" };
        button.Click += (_, _) => { _viewModel.InvokeTodo(action); Refresh(); };
        return button;
    }

    private void Refresh()
    {
        _canvas.Material = _viewModel.SelectedMaterial;
        _canvas.SelectedInstanceId = _viewModel.SelectedInstance?.Id;
        _canvas.ShowCollisionOverlay = _viewModel.ShowCollisionOverlay;
        _canvas.InvalidateVisual();
        _todo.Text = _viewModel.TodoMessage;
        _collisionNote.Text = _viewModel.ShowCollisionOverlay ? _viewModel.CollisionOverlayMessage : "碰撞覆盖层已隐藏。真实碰撞验证仍为 TODO。";

        _metrics.Children.Clear();
        var version = _viewModel.SelectedVersion;
        _metrics.Children.Add(Metric("利用率", $"{version.UtilizationPercent:F1}%"));
        _metrics.Children.Add(Metric("完成率", $"{version.CompletionPercent:F1}%"));
        _metrics.Children.Add(Metric("用长", $"{version.UsedLengthMetres:F1} m"));
        _metrics.Children.Add(new TextBlock { Text = $"{_viewModel.SelectedMaterial.Name}\n{_viewModel.SelectedMaterial.WidthMillimetres:F0} × {_viewModel.SelectedMaterial.LengthMillimetres:F0} mm", Foreground = AppTheme.TextMuted });

        _inspector.Children.Clear();
        var selected = _viewModel.SelectedInstance;
        _inspector.Children.Add(new TextBlock { Text = selected is null ? "尚未选择实例" : $"{selected.Id}\n裁片：{selected.PieceCode} / {selected.Size}\n坐标：{selected.X:F0}, {selected.Y:F0} mm\n角度：{selected.RotationDegrees:F0}° · {(selected.Mirrored ? "已镜像" : "未镜像")}", TextWrapping = TextWrapping.Wrap });
        _inspector.Children.Add(new TextBlock { Text = "空余区", FontWeight = FontWeight.Bold });
        foreach (var zone in _viewModel.SelectedMaterial.FreeZones)
            _inspector.Children.Add(new TextBlock { Text = $"• {zone}", Foreground = AppTheme.TextMuted });
    }

    private static Control Metric(string label, string value) => new StackPanel { Orientation = Orientation.Horizontal, Children =
    {
        new TextBlock { Text = label, Width = 80, Foreground = AppTheme.TextMuted }, new TextBlock { Text = value, FontWeight = FontWeight.Bold },
    }};

    private static Border Card(Control child) => new()
    {
        Background = AppTheme.Surface,
        BorderBrush = AppTheme.SurfaceBorder,
        BorderThickness = new Thickness(1),
        Padding = new Thickness(10),
        Child = child,
    };
}
