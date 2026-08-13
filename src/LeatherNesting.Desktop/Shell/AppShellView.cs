using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Workspace;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Persistent five-region classic workstation frame with module navigation kept behind the shell.</summary>
public sealed class AppShellView : UserControl
{
    private readonly AppShellViewModel _viewModel;
    private readonly ContentControl _content = new();
    private readonly TextBlock _statusText = new();
    private readonly TextBlock _statusProjectText = new();
    private readonly TextBlock _statusVersionText = new();

    public AppShellView() : this(DesktopComposition.CreateShellViewModel())
    {
    }

    public AppShellView(AppShellViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        OrderGroupHost = new ClassicPaneHost("订单 / 排版组", BuildOrderGroupDemo());
        PieceListHost = new ClassicPaneHost("裁片列表 · DEMO", BuildPieceListDemo());
        ProgressSummaryHost = new ClassicPaneHost("进度汇总 · DEMO", BuildProgressDemo());
        LayoutCandidateHost = new ClassicPaneHost("版型数量：6 · DEMO", BuildCandidateDemo());
        OutputInformationHost = new ClassicPaneHost("排版输出信息 · DEMO", BuildOutputDemo());
        PersistentPaneHosts =
        [
            OrderGroupHost, PieceListHost, ProgressSummaryHost,
            LayoutCandidateHost, OutputInformationHost,
        ];

        VerticalRuler = BuildVerticalRuler();
        HorizontalRuler = BuildHorizontalRuler();
        CanvasSurface = new Border
        {
            Background = AppTheme.CadCanvasBackground,
            BorderBrush = AppTheme.ClassicBorder,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = BuildCanvasDemo(),
        };
        LeftRail = BuildLeftRail();
        RightRail = BuildRightRail();
        BodyGrid = BuildBody();
        TopCommands = BuildTopBar();
        StatusDemoText = new TextBlock
        {
            Text = "DEMO · 骨架数据仅用于界面对照",
            Foreground = AppTheme.TodoAmber,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
        };
        StatusBar = BuildStatusBar();
        Content = BuildLayout();

        _viewModel.SnapshotChanged += (_, snapshot) => RefreshSnapshot(snapshot);
        ShowModule(_viewModel.Modules.Single(module => module.Id == "M03"));
        RefreshSnapshot(_viewModel.Snapshot);
    }

    public ContentControl WorkspaceContent => _content;
    public TopCommandArea TopCommands { get; }
    public Grid BodyGrid { get; }
    public Grid LeftRail { get; }
    public Grid RightRail { get; }
    public ClassicPaneHost OrderGroupHost { get; }
    public ClassicPaneHost PieceListHost { get; }
    public ClassicPaneHost ProgressSummaryHost { get; }
    public ClassicPaneHost LayoutCandidateHost { get; }
    public ClassicPaneHost OutputInformationHost { get; }
    public IReadOnlyList<ClassicPaneHost> PersistentPaneHosts { get; }
    public Border CanvasSurface { get; }
    public Border VerticalRuler { get; }
    public Border HorizontalRuler { get; }
    public Border StatusBar { get; }
    public TextBlock StatusDemoText { get; }

    private Control BuildLayout()
    {
        var grid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*"),
            RowDefinitions = RowDefinitions.Parse("Auto,*,Auto"),
            Background = AppTheme.ClassicPanelBackground,
        };
        grid.Children.Add(TopCommands);
        grid.Children.Add(BodyGrid);
        grid.Children.Add(StatusBar);
        Grid.SetRow(BodyGrid, 1);
        Grid.SetRow(StatusBar, 2);
        return grid;
    }

    private TopCommandArea BuildTopBar() => new(command =>
    {
        _viewModel.ActivateToolbarCommand(command);
        _content.Content = _viewModel.CurrentView;
    });

    private Grid BuildBody()
    {
        var center = new Grid
        {
            RowDefinitions = RowDefinitions.Parse("*,20"),
            ColumnDefinitions = ColumnDefinitions.Parse("22,*"),
            Background = AppTheme.CadCanvasBackground,
            Children = { VerticalRuler, CanvasSurface, HorizontalRuler },
        };
        Grid.SetColumn(CanvasSurface, 1);
        Grid.SetColumn(HorizontalRuler, 1);
        Grid.SetRow(HorizontalRuler, 1);

        var body = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("13*,74*,13*"),
            Children = { LeftRail, center, RightRail },
        };
        Grid.SetColumn(center, 1);
        Grid.SetColumn(RightRail, 2);
        return body;
    }

    private Grid BuildLeftRail()
    {
        var rail = new Grid
        {
            RowDefinitions = RowDefinitions.Parse("20*,60*,20*"),
            Children = { OrderGroupHost, PieceListHost, ProgressSummaryHost },
        };
        Grid.SetRow(PieceListHost, 1);
        Grid.SetRow(ProgressSummaryHost, 2);
        return rail;
    }

    private Grid BuildRightRail()
    {
        var rail = new Grid
        {
            RowDefinitions = RowDefinitions.Parse("62*,38*"),
            Children = { LayoutCandidateHost, OutputInformationHost },
        };
        Grid.SetRow(OutputInformationHost, 1);
        return rail;
    }

    private Border BuildStatusBar()
    {
        var row = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto,Auto"),
            Children = { _statusText, _statusProjectText, StatusDemoText, _statusVersionText },
        };
        Grid.SetColumn(_statusProjectText, 1);
        Grid.SetColumn(StatusDemoText, 2);
        Grid.SetColumn(_statusVersionText, 3);
        foreach (var child in row.Children)
            child.Margin = new Thickness(6, 0);

        return new Border
        {
            Height = AppTheme.StatusBarHeight,
            Background = AppTheme.ClassicHeaderBackground,
            BorderBrush = AppTheme.ClassicBorder,
            BorderThickness = new Thickness(1, 1, 1, 0),
            Child = row,
        };
    }

    private void ShowModule(ModuleDescriptor module)
    {
        _viewModel.Select(module);
        _content.Content = _viewModel.CurrentView;
    }

    private void RefreshSnapshot(WorkspaceSnapshot snapshot)
    {
        var project = snapshot.CurrentProject;
        _statusText.Text = "就绪";
        _statusProjectText.Text = $"项目：{project?.Name ?? "未打开"}  状态：{project?.Status ?? "—"}";
        _statusVersionText.Text = "LeatherNesting 0.1 · 2026-08-13";
    }

    private static Border BuildVerticalRuler() => new()
    {
        Width = 22,
        Background = AppTheme.RulerBackground,
        Child = new TextBlock
        {
            Text = "0\n\n100\n\n200\n\n300\n\n400\n\n500",
            Foreground = AppTheme.RulerForeground,
            FontSize = 9,
            TextAlignment = TextAlignment.Center,
        },
    };

    private static Border BuildHorizontalRuler() => new()
    {
        Height = 20,
        Background = AppTheme.RulerBackground,
        Child = new TextBlock
        {
            Text = "0        100        200        300        400        500        600        700        800",
            Foreground = AppTheme.RulerForeground,
            FontSize = 9,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        },
    };

    private static Control BuildCanvasDemo()
    {
        var material = new Border
        {
            Width = 54,
            Height = 420,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 28, 0, 0),
            Background = new SolidColorBrush(Color.FromRgb(0x32, 0x43, 0x6D)),
            BorderBrush = Brushes.Red,
            BorderThickness = new Thickness(1),
            Child = new TextBlock
            {
                Text = "DEMO\n\n裁\n片\n排\n样",
                Foreground = AppTheme.RulerForeground,
                FontSize = 9,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        var evidence = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(8),
            Padding = new Thickness(5, 3),
            Background = new SolidColorBrush(Color.FromArgb(0xC0, 0x65, 0x4A, 0x31)),
            Child = new TextBlock
            {
                Text = "DEMO  序号  名称    刀  层  片\n      2    40    1000  1  1000",
                Foreground = Brushes.White,
                FontSize = 10,
            },
        };
        return new Grid { Children = { material, evidence } };
    }

    private static Control BuildOrderGroupDemo() => CompactText(
        "P_00030; ch 0\n▾ 贴皮测试（皮）\n   └ 40\n添加组  删除  添加\n40                         片数：10");

    private static Control BuildPieceListDemo()
    {
        var list = new StackPanel { Spacing = 1 };
        var sizes = new[] { "205*110", "173*129", "77*169", "172*75", "104*70", "104*96" };
        for (var index = 0; index < sizes.Length; index++)
        {
            list.Children.Add(new Border
            {
                Height = 57,
                Background = AppTheme.DemoPanelBackground,
                BorderBrush = AppTheme.ClassicBorder,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(3, 2),
                Child = CompactText($"{index + 1}  ☑  40             {sizes[index]}\n任意角度  0 | 排完\n单套 1  套数 1  余量 100  总量 100"),
            });
        }
        return new ScrollViewer { Content = list };
    }

    private static Control BuildProgressDemo() => CompactText(
        "总数：900/1000   面积：5.56/6.39(m²)\n组进度：████░░░░ 13.07%\n总订单：900/12100  5.56/77.23(m²)\n█████████░ 92.81%");

    private static Control BuildCandidateDemo() => CompactText(
        "2  ■ 61.60%  1000片\n   宽1.380 × 长10.085 × 层1\n3  ■ 58.42%  无限长\n4  ■ 54.18%  无限长\n5  ■ 49.76%  无限长\n6  ■ 47.33%  无限长");

    private static Control BuildOutputDemo() => CompactText(
        "          ◔  61.60%\n■ 已利用    ■ 未利用\n材料：宽1.380 × 长10.085 × 层数1\n面积：8.35 / 13.92(m²)\n片数：1000 × 1 = 1000片\n单耗：0.0139  总耗：13.9169\n当前耗时：0分12秒");

    private static TextBlock CompactText(string text) => new()
    {
        Text = text,
        FontSize = 10.5,
        Foreground = AppTheme.TextPrimary,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(3, 2),
        LineHeight = 14,
    };
}
