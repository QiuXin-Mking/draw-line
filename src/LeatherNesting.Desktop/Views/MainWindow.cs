using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using LeatherNesting.Application;
using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Infrastructure.Dxf;
using LeatherNesting.Infrastructure.Projects;

namespace LeatherNesting.Desktop.Views;

public sealed class MainWindow : Window
{
    private readonly ProjectWorkflowViewModel workflow = new(new ImportDxfUseCase(new AsciiDxfReader()));
    private readonly ZipProjectStore projectStore = new();
    private readonly TextBox projectName = new() { Text = "新项目" };
    private readonly TextBox sourcePath = new() { PlaceholderText = "选择或粘贴 .dxf 文件路径" };
    private readonly TextBlock status = new() { Margin = new Avalonia.Thickness(16) };
    private readonly TextBox diagnostics = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly Button confirm = new() { Content = "确认毫米并导入", IsEnabled = false };
    private readonly Button cancel = new() { Content = "取消导入", IsEnabled = false };
    private readonly TabControl tabs = new();
    private readonly TabItem workbenchTab = new() { Header = "工艺工作台" };

    public MainWindow()
    {
        Title = "Leather Nesting";
        MinWidth = 1024;
        MinHeight = 640;
        Width = 1366;
        Height = 768;
        var newProject = new Button { Content = "新建项目" };
        newProject.Click += (_, _) => { workflow.CreateProject(projectName.Text ?? "新项目"); Refresh(); };
        var browse = new Button { Content = "选择 DXF…" };
        browse.Click += async (_, _) => await BrowseAsync();
        var inspect = new Button { Content = "导入 DXF" };
        inspect.Click += async (_, _) => await InspectAsync();
        confirm.Click += (_, _) => { workflow.ConfirmMillimetres(); Refresh(); };
        cancel.Click += (_, _) => { workflow.CancelImport(); Refresh(); };
        var save = new Button { Content = "保存项目…" };
        save.Click += async (_, _) => await SaveAsync();
        var enterWorkbench = new Button { Content = "进入工艺工作台" };
        enterWorkbench.Click += async (_, _) => await EnterWorkbenchAsync();

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Avalonia.Thickness(16),
            Children = { new TextBlock { Text = "项目名称", VerticalAlignment = VerticalAlignment.Center }, projectName, newProject, save, enterWorkbench },
        };
        var body = new StackPanel
        {
            Spacing = 12,
            Margin = new Avalonia.Thickness(16),
            Children =
            {
                new TextBlock { Text = "导入裁片 DXF 文件", FontSize = 16 },
                new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { sourcePath, browse, inspect, confirm, cancel } },
                new TextBlock { Text = "导入信息" },
                diagnostics,
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
        workflow.CreateProject(projectName.Text!);
        Refresh();
    }

    private async Task BrowseAsync()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
            await workflow.InspectAsync(sourcePath.Text, CancellationToken.None);
        }
        catch (Exception exception) { diagnostics.Text = $"Blocking · UI-IMPORT · {exception.Message}"; }
        Refresh();
    }

    private async Task SaveAsync()
    {
        try
        {
            if (workflow.Project is null) throw new InvalidOperationException("没有可保存的项目。");
            var destination = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "保存皮革排料项目",
                SuggestedFileName = $"{workflow.Project.Name}.lnproj",
                DefaultExtension = "lnproj",
                FileTypeChoices = [new FilePickerFileType("Leather Nesting 项目") { Patterns = ["*.lnproj"] }],
            });
            var path = destination?.TryGetLocalPath();
            if (string.IsNullOrWhiteSpace(path)) return;
            await projectStore.SaveAsync(path, workflow.Project, CancellationToken.None);
            workflow.MarkSaved();
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
            var loops = await new AsciiDxfGeometryReader().ReadAsync(sourcePath.Text, CancellationToken.None);
            if (loops.Count == 0)
                throw new InvalidOperationException("DXF 中没有可编辑的闭合轮廓。");
            var viewModel = new CadWorkbenchViewModel();
            viewModel.LoadLoops(loops);
            workbenchTab.Content = new CadWorkbenchView(viewModel);
            tabs.SelectedItem = workbenchTab;
        }
        catch (Exception exception)
        {
            diagnostics.Text = $"Blocking · UI-WORKBENCH · {exception.Message}";
        }
    }

    private void Refresh()
    {
        var project = workflow.Project;
        status.Text = project is null ? "未创建项目" : $"{project.Name} · 修订 {project.Revision} · {(project.IsDirty ? "未保存" : "已保存")}";
        confirm.IsEnabled = workflow.RequiresUnitConfirmation;
        cancel.IsEnabled = workflow.RequiresUnitConfirmation;
        if (workflow.RequiresUnitConfirmation)
        {
            var inspection = workflow.Inspection!;
            var layers = inspection.Entities.Select(item => item.Layer).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
            diagnostics.Text = $"DXF 声明单位：{FormatUnit(inspection.DeclaredUnit)}（必须人工确认）\n图层：{(layers.Length == 0 ? "无可识别实体" : string.Join("、", layers))}\n" +
                string.Join(Environment.NewLine, workflow.Diagnostics.Select(item => $"{item.Severity} · {item.Code} · {item.Message}"));
        }
        else if (string.IsNullOrWhiteSpace(diagnostics.Text)) diagnostics.Text = "尚未导入 DXF。";
    }

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
}
