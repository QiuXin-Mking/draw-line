using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Views;

namespace LeatherNesting.Desktop.Modules.CadCanvas;

/// <summary>M03 CAD browser with module-local display state over the shared read-only canvas.</summary>
public sealed class CadCanvasView : UserControl
{
    private static readonly IBrush DarkCanvasHost = new SolidColorBrush(Color.FromRgb(0x12, 0x18, 0x1E));
    private static readonly IBrush DarkPanel = new SolidColorBrush(Color.FromRgb(0x20, 0x29, 0x32));
    private static readonly IBrush DarkBorder = new SolidColorBrush(Color.FromRgb(0x3C, 0x49, 0x55));
    private static readonly IBrush LightText = new SolidColorBrush(Color.FromRgb(0xE6, 0xEA, 0xEF));
    private static readonly IBrush MutedText = new SolidColorBrush(Color.FromRgb(0xA7, 0xB2, 0xBD));

    private readonly CadCanvasViewModel _viewModel;
    private readonly CanvasView _canvas = new() { MinWidth = 520, MinHeight = 440 };
    private readonly TextBlock _status = new() { Foreground = LightText };
    private readonly TextBlock _visibleSummary = new() { Foreground = MutedText, TextWrapping = TextWrapping.Wrap };

    public CadCanvasView()
    {
        _viewModel = new CadCanvasViewModel();
        _viewModel.RenderRequested += Render;
        _canvas.PointerMoved += OnCanvasPointerMoved;
        Content = Build();
        Refresh(refit: true);
    }

    private Control Build()
    {
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                ToolbarButton("全图", () => _viewModel.FitAll()),
                ToolbarButton("放大 +", () => ShowZoomGuidance("放大")),
                ToolbarButton("缩小 −", () => ShowZoomGuidance("缩小")),
                TodoButton("缩放到选择 · TODO", CadCanvasTodoTool.HitTesting),
                new TextBlock
                {
                    Text = "滚轮缩放 · 空白处拖拽平移",
                    Foreground = MutedText,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0),
                },
            },
        };

        var workspace = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("250,*,280"),
            ColumnSpacing = 10,
            Children =
            {
                BuildObjectTree(),
                BuildCanvasHost(),
                BuildDisplayPanel(),
            },
        };
        Grid.SetColumn(workspace.Children[1], 1);
        Grid.SetColumn(workspace.Children[2], 2);

        return new Border
        {
            Background = DarkCanvasHost,
            Padding = new Thickness(14),
            Child = new Grid
            {
                RowDefinitions = RowDefinitions.Parse("Auto,*,Auto"),
                RowSpacing = 10,
                Children =
                {
                    toolbar,
                    workspace,
                    BuildStatusBar(),
                },
            },
        };
    }

    private Control BuildObjectTree()
    {
        var objects = new StackPanel { Spacing = 5 };
        foreach (var group in _viewModel.Objects.GroupBy(item => item.Id.Split('-')[1]))
        {
            objects.Children.Add(new TextBlock
            {
                Text = group.Key == "A" ? "▾ 鞋面 A" : "▾ 后跟片 B",
                Foreground = LightText,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(0, 6, 0, 2),
            });
            foreach (var item in group)
                objects.Children.Add(new TextBlock
                {
                    Text = $"  └ {CadCanvasViewModel.CategoryLabel(item.Category)}  [{item.Id}]",
                    Foreground = MutedText,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                });
        }

        return PanelCard("对象树 · DEMO", new ScrollViewer { Content = objects });
    }

    private Control BuildCanvasHost()
    {
        var rulers = new Grid
        {
            RowDefinitions = RowDefinitions.Parse("24,*"),
            ColumnDefinitions = ColumnDefinitions.Parse("32,*"),
            Children =
            {
                new TextBlock { Text = "mm", Foreground = MutedText, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center },
                Ruler("0       25       50       75       100       125       150       175       200"),
                VerticalRuler(),
                new Border
                {
                    Background = DarkCanvasHost,
                    BorderBrush = DarkBorder,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(6),
                    Child = _canvas,
                },
            },
        };
        Grid.SetColumn(rulers.Children[1], 1);
        Grid.SetRow(rulers.Children[2], 1);
        Grid.SetRow(rulers.Children[3], 1);
        Grid.SetColumn(rulers.Children[3], 1);
        return rulers;
    }

    private Control BuildDisplayPanel()
    {
        var categories = new StackPanel { Spacing = 7 };
        foreach (var category in Enum.GetValues<CadObjectCategory>())
        {
            var checkBox = new CheckBox
            {
                Content = CadCanvasViewModel.CategoryLabel(category),
                IsChecked = _viewModel.IsCategoryVisible(category),
                Foreground = LightText,
            };
            checkBox.IsCheckedChanged += (_, _) =>
                _viewModel.SetCategoryVisibility(category, checkBox.IsChecked == true);
            categories.Children.Add(checkBox);
        }

        var todoTools = new WrapPanel();
        foreach (var tool in Enum.GetValues<CadCanvasTodoTool>())
        {
            if (tool == CadCanvasTodoTool.HitTesting)
                continue;
            todoTools.Children.Add(TodoButton($"{CadCanvasViewModel.TodoLabel(tool)} · TODO", tool));
        }

        return PanelCard("显示与图层", new StackPanel
        {
            Spacing = 12,
            Children =
            {
                categories,
                _visibleSummary,
                Divider(),
                new TextBlock { Text = "图例", Foreground = LightText, FontWeight = FontWeight.Bold },
                Legend("外轮廓", Brushes.Navy),
                Legend("孔 / 内部线", Brushes.OrangeRed),
                Legend("选中", Brushes.DodgerBlue),
                Divider(),
                new TodoBadge(),
                todoTools,
            },
        });
    }

    private Control BuildStatusBar()
    {
        var panel = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"),
            Children =
            {
                _status,
                new TextBlock
                {
                    Text = "坐标单位 mm  ·  比例随滚轮调整  ·  DEMO 数据",
                    Foreground = MutedText,
                    HorizontalAlignment = HorizontalAlignment.Right,
                },
            },
        };
        Grid.SetColumn(panel.Children[1], 1);
        return new Border
        {
            Background = DarkPanel,
            BorderBrush = DarkBorder,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 6),
            Child = panel,
        };
    }

    private void Render(CadCanvasRenderRequest request)
    {
        _canvas.SetData(request.Loops, request.Refit);
        RefreshSummary();
    }

    private void Refresh(bool refit)
    {
        _canvas.SetData(_viewModel.VisibleLoops, refit);
        RefreshSummary();
    }

    private void RefreshSummary()
    {
        _status.Text = _viewModel.StatusMessage;
        _visibleSummary.Text = $"可见 {_viewModel.VisibleObjects.Count} / {_viewModel.Objects.Count} 个对象\n" +
                               $"源图层：{string.Join(" · ", _viewModel.VisibleObjects.Select(item => item.Layer).Distinct())}";
    }

    private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        _viewModel.ReportCoordinates(_canvas.ToModel(e.GetPosition(_canvas)));
        _status.Text = _viewModel.StatusMessage;
    }

    private void ShowZoomGuidance(string direction)
    {
        _viewModel.ReportZoomGuidance(direction);
        _status.Text = _viewModel.StatusMessage;
    }

    private Button ToolbarButton(string label, Action action)
    {
        var button = new Button { Content = label };
        button.Click += (_, _) => action();
        return button;
    }

    private Button TodoButton(string label, CadCanvasTodoTool tool)
    {
        var button = new Button { Content = label, Margin = new Thickness(0, 0, 6, 6) };
        button.Click += (_, _) =>
        {
            _viewModel.InvokeTodo(tool);
            _status.Text = _viewModel.StatusMessage;
        };
        return button;
    }

    private static Border PanelCard(string title, Control content)
    {
        var grid = new Grid
        {
            RowDefinitions = RowDefinitions.Parse("Auto,*"),
            RowSpacing = 8,
            Children =
            {
                new TextBlock { Text = title, Foreground = LightText, FontWeight = FontWeight.Bold },
                content,
            },
        };
        Grid.SetRow(content, 1);
        return new Border
        {
            Background = DarkPanel,
            BorderBrush = DarkBorder,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            Child = grid,
        };
    }

    private static TextBlock Ruler(string text) => new()
    {
        Text = text,
        Foreground = MutedText,
        FontFamily = FontFamily.Default,
        FontSize = 10,
        VerticalAlignment = VerticalAlignment.Center,
        TextTrimming = TextTrimming.CharacterEllipsis,
    };

    private static TextBlock VerticalRuler() => new()
    {
        Text = "60\n\n40\n\n20\n\n0",
        Foreground = MutedText,
        FontSize = 10,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        TextAlignment = TextAlignment.Center,
    };

    private static Control Legend(string label, IBrush brush) => new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Spacing = 7,
        Children =
        {
            new Border { Width = 22, Height = 4, Background = brush, VerticalAlignment = VerticalAlignment.Center },
            new TextBlock { Text = label, Foreground = MutedText },
        },
    };

    private static Border Divider() => new() { Height = 1, Background = DarkBorder };
}
