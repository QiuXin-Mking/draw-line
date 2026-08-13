using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Modules.Export;
using LeatherNesting.Desktop.Shell;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.Export;

public sealed class ExportViewModelTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T11")]
    public void Exportable_demo_exposes_required_outputs_and_complete_demo_manifest()
    {
        var viewModel = new ExportViewModel();
        viewModel.SelectScenario("exportable");

        Assert.True(viewModel.CanRequestProductionExport);
        Assert.Equal(
            ["DXF", "JSON", "CSV", "PNG", "PDF"],
            viewModel.OutputOptions.Select(option => option.Label));
        Assert.Equal("毫米 (mm)", viewModel.Settings.Unit);
        Assert.Equal("材料左下角", viewModel.Settings.Origin);
        Assert.Equal("保持排样角度", viewModel.Settings.Rotation);
        Assert.False(string.IsNullOrWhiteSpace(viewModel.Settings.Directory));
        Assert.Contains("{项目号}", viewModel.Settings.NamingTemplate);
        Assert.Contains(viewModel.LayerMappings, mapping => mapping.Semantic == "外轮廓");
        Assert.Contains(viewModel.LayerMappings, mapping => mapping.Semantic == "冲孔/标记");

        var manifest = viewModel.BuildManifestPreview();

        Assert.StartsWith("DEMO", manifest);
        Assert.Contains("schemaVersion", manifest);
        Assert.Contains("projectNumber", manifest);
        Assert.Contains("projectVersion", manifest);
        Assert.Contains("inputFingerprint", manifest);
        Assert.Contains("outputHash", manifest);
        Assert.Contains("DXF", manifest);
        Assert.Contains("JSON", manifest);
        Assert.Contains("CSV", manifest);
        Assert.Contains("PNG", manifest);
        Assert.Contains("PDF", manifest);
        Assert.Contains("layerMappings", manifest);
        Assert.Contains("TODO", manifest);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T11")]
    public void Blocking_demo_disables_production_export_and_explains_why()
    {
        var viewModel = new ExportViewModel();

        Assert.False(viewModel.CanRequestProductionExport);
        Assert.Contains("2 个阻断", viewModel.ProductionExportStatus);
        Assert.False(viewModel.RequestProductionExport());
        Assert.Contains("禁止", viewModel.ActionMessage);
        Assert.Contains("校验", viewModel.ActionMessage);
    }

    [Theory]
    [InlineData(ExportTodoAction.WriteFiles)]
    [InlineData(ExportTodoAction.OpenOutputDirectory)]
    [InlineData(ExportTodoAction.LaunchExternalProgram)]
    [InlineData(ExportTodoAction.ExportPlt)]
    [InlineData(ExportTodoAction.ExportDwg)]
    [InlineData(ExportTodoAction.SendToDevice)]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T11")]
    public void External_paths_are_never_executed(ExportTodoAction action)
    {
        var viewModel = new ExportViewModel();
        viewModel.SelectScenario("exportable");

        Assert.False(viewModel.InvokeTodo(action));
        Assert.Contains("TODO", viewModel.ActionMessage);
        Assert.Contains("未执行", viewModel.ActionMessage);
        Assert.False(viewModel.HasExecutedExternalAction);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T11")]
    public void Production_request_only_previews_package_and_never_writes_files()
    {
        var viewModel = new ExportViewModel();
        viewModel.SelectScenario("exportable");

        Assert.True(viewModel.RequestProductionExport());
        Assert.Contains("DEMO", viewModel.ActionMessage);
        Assert.Contains("未写入", viewModel.ActionMessage);
        Assert.False(viewModel.HasExecutedExternalAction);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T11")]
    public void Output_selection_updates_manifest_without_touching_external_paths()
    {
        var viewModel = new ExportViewModel();

        viewModel.SetOutputSelected("pdf", false);

        Assert.DoesNotContain(viewModel.OutputOptions, option => option.Id == "pdf" && option.IsSelected);
        Assert.DoesNotContain("PDF:交接说明预览", viewModel.BuildManifestPreview());
        Assert.Contains("仅更新内存", viewModel.ActionMessage);
        Assert.False(viewModel.HasExecutedExternalAction);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T11")]
    public void Module_declares_m11_metadata_and_is_discoverable()
    {
        var module = new ExportModule();

        Assert.Equal("M11", module.Metadata.Id);
        Assert.Equal("导出", module.Metadata.Title);
        Assert.Equal(11, module.Metadata.Order);
        Assert.IsType<ExportView>(module.CreateView());

        var discovered = DesktopModuleDiscovery.Discover(typeof(ExportModule).Assembly);
        Assert.Contains(discovered, item => item is ExportModule && item.Metadata.Id == "M11");
    }
}
