using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.Pieces;
using LeatherNesting.Desktop.Workspace;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Persistent five-region classic workstation frame with module navigation kept behind the shell.</summary>
public sealed class AppShellView : UserControl
{
    private readonly AppShellViewModel _viewModel;
    private readonly ContentControl _content = new();
    private readonly TextBlock _statusText = new() { Foreground = AppTheme.PrimaryText };
    private readonly TextBlock _statusProjectText = new() { Foreground = AppTheme.PrimaryText };
    private readonly TextBlock _statusVersionText = new() { Foreground = AppTheme.PrimaryText };

    public AppShellView() : this(DesktopComposition.CreateShellViewModel())
    {
    }

    public AppShellView(OrderPiecePanelState orderPieceState)
        : this(DesktopComposition.CreateShellViewModel(), orderPieceState)
    {
    }

    public AppShellView(AppShellViewModel viewModel)
        : this(viewModel, OrderPiecePanelState.CreateImage27Demo())
    {
    }

    public AppShellView(AppShellViewModel viewModel, OrderPiecePanelState orderPieceState)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        OrderPieceState = orderPieceState ?? throw new ArgumentNullException(nameof(orderPieceState));

        OrderGroupHost = new ClassicPaneHost("订单 / 排版组", new OrderGroupPanelView(OrderPieceState));
        PieceListHost = new ClassicPaneHost("裁片列表 · DEMO", new PieceCardListView(OrderPieceState));
        ProgressSummaryHost = new ClassicPaneHost("进度汇总 · DEMO", new ProgressSummaryView(OrderPieceState));
        LayoutCandidateHost = new ClassicPaneHost("CAD 参数", null);
        OutputInformationHost = new ClassicPaneHost("排版输出信息 · DEMO", BuildOutputDemo());
        PersistentPaneHosts =
        [
            OrderGroupHost, PieceListHost, ProgressSummaryHost,
            LayoutCandidateHost, OutputInformationHost,
        ];

        VerticalRuler = BuildVerticalRuler();
        HorizontalRuler = BuildHorizontalRuler();
        CadWorkspace = new CadWorkspaceHost(_viewModel.CadHost, OpenImportModule, _viewModel.ActivateContextCommand);
        CadProperties = new CadPropertyPane(_viewModel.CadHost);
        _content.HorizontalAlignment = HorizontalAlignment.Stretch;
        _content.VerticalAlignment = VerticalAlignment.Stretch;
        _content.Margin = new Thickness(70, 38);
        _content.Background = AppTheme.PanelSurface;
        _content.IsVisible = false;
        CanvasSurface = new Border
        {
            Background = AppTheme.CanvasBlack,
            BorderBrush = AppTheme.ClassicBorderNeutral,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = CadWorkspace,
        };
        LayoutCandidateHost.HostedContent = CadProperties;
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
        _viewModel.CadHost.Changed += (_, _) =>
        {
            if (_viewModel.CurrentModule?.Id == "M02" && _viewModel.CadHost.Loops.Count > 0)
                ShowModule(_viewModel.Modules.Single(module => module.Id == "M03"));
        };
        ShowModule(_viewModel.Modules.Single(module => module.Id == "M03"));
        RefreshSnapshot(_viewModel.Snapshot);
    }

    public ContentControl WorkspaceContent => _content;
    public OrderPiecePanelState OrderPieceState { get; }
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
    public CadWorkspaceHost CadWorkspace { get; }
    public CadPropertyPane CadProperties { get; }
    public Border VerticalRuler { get; }
    public Border HorizontalRuler { get; }
    public Border StatusBar { get; }
    public TextBlock StatusDemoText { get; }

    private Control BuildLayout()
    {
        var bodyLayer = new Grid { Children = { BodyGrid, _content } };
        var grid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*"),
            RowDefinitions = RowDefinitions.Parse("Auto,*,Auto"),
            Background = AppTheme.PanelSurface,
        };
        grid.Children.Add(TopCommands);
        grid.Children.Add(bodyLayer);
        grid.Children.Add(StatusBar);
        Grid.SetRow(bodyLayer, 1);
        Grid.SetRow(StatusBar, 2);
        return grid;
    }

    private TopCommandArea BuildTopBar() => new(
        command =>
        {
            _viewModel.ActivateToolbarCommand(command);
            RefreshModuleOverlay();
        },
        command =>
        {
            _viewModel.ActivateMenuCommand(command);
            RefreshModuleOverlay();
        });

    private Grid BuildBody()
    {
        var center = new Grid
        {
            RowDefinitions = RowDefinitions.Parse("*,20"),
            ColumnDefinitions = ColumnDefinitions.Parse("22,*"),
            Background = AppTheme.CanvasBlack,
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
            Background = AppTheme.StatusSurface,
            BorderBrush = AppTheme.ClassicBorderNeutral,
            BorderThickness = new Thickness(1, 1, 1, 0),
            Child = row,
        };
    }

    private void ShowModule(ModuleDescriptor module)
    {
        _viewModel.Select(module);
        RefreshModuleOverlay();
    }

    private void RefreshModuleOverlay()
    {
        _content.Content = _viewModel.CurrentView;
        _content.IsVisible = _viewModel.CurrentModule?.Id == "M02";
    }

    private void OpenImportModule()
    {
        ShowModule(_viewModel.Modules.Single(module => module.Id == "M02"));
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
        Background = AppTheme.RulerChrome,
        Child = new TextBlock
        {
            Text = "0\n\n100\n\n200\n\n300\n\n400\n\n500",
            Foreground = AppTheme.RulerTick,
            FontSize = 9,
            TextAlignment = TextAlignment.Center,
        },
    };

    private static Border BuildHorizontalRuler() => new()
    {
        Height = 20,
        Background = AppTheme.RulerChrome,
        Child = new TextBlock
        {
            Text = "0        100        200        300        400        500        600        700        800",
            Foreground = AppTheme.RulerTick,
            FontSize = 9,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        },
    };

    private static Control BuildOutputDemo() => CompactText(
        "          ◔  61.60%\n■ 已利用    ■ 未利用\n材料：宽1.380 × 长10.085 × 层数1\n面积：8.35 / 13.92(m²)\n片数：1000 × 1 = 1000片\n单耗：0.0139  总耗：13.9169\n当前耗时：0分12秒");

    private static TextBlock CompactText(string text) => new()
    {
        Text = text,
        FontSize = 10.5,
        Foreground = AppTheme.PrimaryText,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(3, 2),
        LineHeight = 14,
    };
}
