using Avalonia.Controls;
using LeatherNesting.Application;
using LeatherNesting.Desktop.Modules.Import;
using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Desktop.Views;
using LeatherNesting.Desktop.Workspace;
using LeatherNesting.Infrastructure.Dxf;
using LeatherNesting.Infrastructure.Projects;

namespace LeatherNesting.Desktop.Adapters.Import;

/// <summary>Temporary desktop wiring until composition owns the shared workspace lifetime.</summary>
public static class DefaultImportCoordinatorFactory
{
    public static IImportCoordinator Create()
    {
        var workspace = new InMemoryWorkspaceSession();
        return Create(workspace, workspace);
    }

    public static IImportCoordinator Create(IWorkspaceSession workspace, IWorkspaceCommands workspaceCommands) =>
        new ImportCoordinator(
            new ImportDxfUseCase(new AsciiDxfReader()),
            new ZipProjectStore(),
            new AsciiImportGeometryReader(),
            workspace,
            workspaceCommands,
            new CadImportWorkbenchFactory(new AsciiImportGeometryReader()));
}

public sealed class AsciiImportGeometryReader : IImportGeometryReader
{
    private readonly AsciiDxfGeometryReader _reader = new();

    public Task<IReadOnlyList<LeatherNesting.Geometry.Loop2D>> ReadAsync(string path, CancellationToken cancellationToken) =>
        _reader.ReadAsync(path, cancellationToken);
}

public sealed class CadImportWorkbenchFactory(IImportGeometryReader geometryReader) : IImportWorkbenchFactory
{
    public async Task<Control> CreateAsync(string path, CancellationToken cancellationToken)
    {
        var loops = await geometryReader.ReadAsync(path, cancellationToken);
        if (loops.Count == 0) throw new InvalidOperationException("DXF 中没有可编辑的闭合轮廓。");
        var viewModel = new CadWorkbenchViewModel();
        viewModel.LoadLoops(loops);
        return new CadWorkbenchView(viewModel);
    }
}
