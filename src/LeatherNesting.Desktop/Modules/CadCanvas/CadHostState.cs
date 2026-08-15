using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Geometry;

namespace LeatherNesting.Desktop.Modules.CadCanvas;

/// <summary>Shared projection consumed by the fixed CAD host and the confirmed M02 import flow.</summary>
public sealed class CadHostState
{
    private bool _suppressWorkbenchRefresh;

    public CadHostState()
    {
        Workbench.Changed += (_, _) => RefreshFromWorkbench();
    }

    public CadWorkbenchViewModel Workbench { get; } = new();

    public string FileName { get; private set; } = "未打开文件";

    public IReadOnlyList<Loop2D> Loops => Workbench.CurrentLoops ?? [];

    public bool IsDemoGeometry { get; private set; }

    public string StatusMessage { get; private set; } = "请选择 DXF 文件并确认毫米单位。";

    public event EventHandler? Changed;

    public void LoadConfirmedImport(string path, IReadOnlyList<Loop2D> loops)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(loops);
        var snapshot = loops.ToArray();
        _suppressWorkbenchRefresh = true;
        try
        {
            Workbench.LoadLoops(snapshot);
        }
        finally
        {
            _suppressWorkbenchRefresh = false;
        }
        FileName = Path.GetFileName(path);
        IsDemoGeometry = false;
        StatusMessage = snapshot.Length == 0
            ? "DXF 已确认，但没有可显示的闭合轮廓。"
            : $"已载入 {FileName} · {snapshot.Length} 个闭合轮廓";
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void ReportError(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        StatusMessage = message;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void ReportUnsupported(string action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        StatusMessage = $"{action}：{TodoBadge.StandardText}";
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _suppressWorkbenchRefresh = true;
        try
        {
            Workbench.LoadLoops([]);
        }
        finally
        {
            _suppressWorkbenchRefresh = false;
        }
        FileName = "未打开文件";
        IsDemoGeometry = false;
        StatusMessage = "请选择 DXF 文件并确认毫米单位。";
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshFromWorkbench()
    {
        if (_suppressWorkbenchRefresh)
            return;

        var problems = Workbench.ProblemMessages;
        StatusMessage = problems.Count > 0
            ? string.Join("；", problems)
            : Workbench.State switch
            {
                WorkbenchState.Previewing => "CAD 预览待提交；可提交到会话或取消。",
                WorkbenchState.Committed => "CAD 编辑已提交到可撤销会话（未写入项目文件）。",
                _ when Workbench.SelectedLoopId is not null => $"已选中轮廓 {Workbench.SelectedLoopId}。",
                _ when Loops.Count > 0 => $"已载入 {FileName} · {Loops.Count} 个闭合轮廓",
                _ => "请选择 DXF 文件并确认毫米单位。",
            };
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
