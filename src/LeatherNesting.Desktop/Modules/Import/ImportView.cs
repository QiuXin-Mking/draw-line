using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using LeatherNesting.Application;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Import;

/// <summary>M02: real DXF import inspector, reusing the existing project/import workflow.</summary>
public sealed class ImportView : UserControl
{
    private readonly IImportCoordinator coordinator;
    private readonly TextBox projectName = new() { Text = "新项目" };
    private readonly TextBox sourcePath = new() { PlaceholderText = "选择或粘贴 .dxf 文件路径" };
    private readonly TextBlock status = new() { Margin = new Thickness(16) };
    private readonly TextBox diagnostics = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly Button confirm = new() { Content = "确认毫米并导入", IsEnabled = false };
    private readonly Button cancel = new() { Content = "取消导入", IsEnabled = false };
    private readonly Button enterWorkbench = new() { Content = "进入工艺工作台", IsEnabled = false };
    private readonly TabControl tabs = new();
    private readonly TabItem workbenchTab = new() { Header = "工艺工作台" };

    public ImportView(IImportCoordinator coordinator)
    {
        this.coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        var newProject = new Button { Content = "新建项目" };
        newProject.Click += (_, _) =>
        {
            this.coordinator.CreateProject(projectName.Text ?? "新项目");
            diagnostics.Text = string.Empty;
            Refresh();
        };
        var browse = new Button { Content = "选择 DXF…" };
        browse.Click += async (_, _) => await BrowseAsync();
        var inspect = new Button { Content = "导入 DXF" };
        inspect.Click += async (_, _) => await InspectAsync();
        confirm.Click += (_, _) =>
        {
            this.coordinator.ConfirmMillimetres();
            diagnostics.Text = string.Empty;
            Refresh();
        };
        cancel.Click += (_, _) =>
        {
            this.coordinator.CancelImport();
            diagnostics.Text = string.Empty;
            Refresh();
        };
        var save = new Button { Content = "保存项目…" };
        save.Click += async (_, _) => await SaveAsync();
        enterWorkbench.Click += async (_, _) => await EnterWorkbenchAsync();

        var locate = new Button { Content = "定位诊断对象（TODO）" };
        locate.Click += (_, _) => status.Text = $"诊断对象定位：{TodoBadge.StandardText}";

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(16),
            Children = { new TextBlock { Text = "项目名称", VerticalAlignment = VerticalAlignment.Center }, projectName, newProject, save, enterWorkbench },
        };
        var body = new StackPanel
        {
            Spacing = 12,
            Margin = new Thickness(16),
            Children =
            {
                new TextBlock { Text = "导入裁片 DXF 文件", FontSize = 16 },
                new TextBlock { Text = "步骤 1 选择文件  →  步骤 2 单位/比例确认  →  步骤 3 识别结果  →  步骤 4 问题处理与提交" },
                new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { sourcePath, browse, inspect, confirm, cancel } },
                new TextBlock { Text = "导入信息" },
                diagnostics,
                locate,
                new TodoBadge("自动修复：TODO · 演示占位，未接入实际逻辑"),
                new TodoBadge("批量图层映射：TODO · 演示占位，未接入实际逻辑"),
                new TodoBadge("拖放与多文件导入：TODO · 演示占位，未接入实际逻辑"),
                new TodoBadge("非 DXF 格式入口：TODO · 演示占位，未接入实际逻辑"),
            },
        };
        tabs.Items.Add(new TabItem { Header = "导入", Content = body });
        tabs.Items.Add(workbenchTab);
        Grid.SetRow(tabs, 1);
        Grid.SetRow(status, 2);
        Content = new Grid
        {
            RowDefinitions = RowDefinitions.Parse("Auto,*,Auto"),
            Children = { header, tabs, status },
        };
        sourcePath.TextChanged += (_, _) => RefreshWorkbenchAvailability();
        if (this.coordinator.State.Project is null) this.coordinator.CreateProject(projectName.Text!);
        Refresh();
    }

    private IStorageProvider? Storage => TopLevel.GetTopLevel(this)?.StorageProvider;

    private async Task BrowseAsync()
    {
        if (Storage is null) return;
        var files = await Storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择 DXF 文件",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("DXF 图纸") { Patterns = ["*.dxf", "*.DXF"] }],
        });
        var selected = files.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(selected)) sourcePath.Text = selected;
    }

    private async Task InspectAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourcePath.Text) || !File.Exists(sourcePath.Text))
                throw new InvalidOperationException("请选择存在的 DXF 文件。");
            await coordinator.InspectAsync(sourcePath.Text, CancellationToken.None);
        }
        catch (Exception exception) { diagnostics.Text = $"Blocking · UI-IMPORT · {exception.Message}"; }
        Refresh();
    }

    private async Task SaveAsync()
    {
        try
        {
            if (coordinator.State.Project is null) throw new InvalidOperationException("没有可保存的项目。");
            if (Storage is null) throw new InvalidOperationException("当前环境不可用文件对话框。");
            var destination = await Storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "保存皮革排料项目",
                SuggestedFileName = $"{coordinator.State.Project.Name}.lnproj",
                DefaultExtension = "lnproj",
                FileTypeChoices = [new FilePickerFileType("Leather Nesting 项目") { Patterns = ["*.lnproj"] }],
            });
            var path = destination?.TryGetLocalPath();
            if (string.IsNullOrWhiteSpace(path)) return;
            await coordinator.SaveAsync(path, CancellationToken.None);
        }
        catch (Exception exception) { diagnostics.Text = $"Blocking · UI-SAVE · {exception.Message}"; }
        Refresh();
    }

    private async Task EnterWorkbenchAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourcePath.Text) || !File.Exists(sourcePath.Text))
                throw new InvalidOperationException("请先选择并检查一个 DXF 文件。");
            workbenchTab.Content = await coordinator.CreateWorkbenchAsync(sourcePath.Text, CancellationToken.None);
            tabs.SelectedItem = workbenchTab;
        }
        catch (Exception exception)
        {
            diagnostics.Text = $"Blocking · UI-WORKBENCH · {exception.Message}";
        }
    }

    private void Refresh()
    {
        var project = coordinator.State.Project;
        status.Text = project is null ? "未创建项目" : $"{project.Name} · 修订 {project.Revision} · {(project.IsDirty ? "未保存" : "已保存")}";
        confirm.IsEnabled = coordinator.State.RequiresUnitConfirmation;
        cancel.IsEnabled = coordinator.State.RequiresUnitConfirmation;
        RefreshWorkbenchAvailability();
        if (coordinator.State.RequiresUnitConfirmation)
        {
            var inspection = coordinator.State.Inspection!;
            var layers = inspection.Entities.Select(item => item.Layer).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
            var entityStats = inspection.Entities
                .GroupBy(item => item.Kind)
                .OrderBy(group => group.Key)
                .Select(group => $"{group.Key} {group.Count()}");
            var severityStats = coordinator.State.Diagnostics
                .GroupBy(item => FormatSeverity(item.Severity))
                .Select(group => $"{group.Key} {group.Count()}");
            diagnostics.Text = $"DXF 声明单位：{FormatUnit(inspection.DeclaredUnit)}（必须人工确认）\n" +
                $"实体：{inspection.Entities.Count}（{string.Join("、", entityStats)}）；候选裁片：{inspection.ClosedPieceCandidates.Count}\n" +
                $"图层：{(layers.Length == 0 ? "无可识别实体" : string.Join("、", layers))}\n" +
                $"诊断等级：{(coordinator.State.Diagnostics.Count == 0 ? "无" : string.Join("、", severityStats))}\n" +
                string.Join(Environment.NewLine, coordinator.State.Diagnostics.Select(item =>
                    $"{FormatSeverity(item.Severity)} · {item.Code} · {item.Message}" +
                    (string.IsNullOrWhiteSpace(item.EntityId) ? string.Empty : $" · 对象 {item.EntityId}")));
        }
        else if (string.IsNullOrWhiteSpace(diagnostics.Text))
            diagnostics.Text = coordinator.State.HasConfirmedImport
                ? "单位已确认：毫米。当前 DXF 已提交到项目，可保存或进入工艺工作台。"
                : "尚未导入 DXF。";
    }

    private void RefreshWorkbenchAvailability() =>
        enterWorkbench.IsEnabled = coordinator.CanEnterWorkbench(sourcePath.Text ?? string.Empty);

    private static string FormatUnit(DxfDeclaredUnit unit) => unit switch
    {
        DxfDeclaredUnit.Millimetres => "毫米",
        DxfDeclaredUnit.Centimetres => "厘米",
        DxfDeclaredUnit.Metres => "米",
        DxfDeclaredUnit.Inches => "英寸",
        DxfDeclaredUnit.Feet => "英尺",
        DxfDeclaredUnit.Miles => "英里",
        DxfDeclaredUnit.Unitless => "无单位",
        _ => "未声明/未知",
    };

    private static string FormatSeverity(string severity) => severity.ToUpperInvariant() switch
    {
        "BLOCKING" => "阻断",
        "WARNING" => "警告",
        "INFO" => "提示",
        _ => severity,
    };
}
