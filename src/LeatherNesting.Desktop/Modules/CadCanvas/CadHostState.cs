using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Geometry;

namespace LeatherNesting.Desktop.Modules.CadCanvas;

/// <summary>Shared projection consumed by the fixed CAD host and the confirmed M02 import flow.</summary>
public sealed class CadHostState
{
    private IReadOnlyList<Loop2D> _loops = [];

    public string FileName { get; private set; } = "未打开文件";

    public IReadOnlyList<Loop2D> Loops => _loops;

    public bool IsDemoGeometry { get; private set; }

    public string StatusMessage { get; private set; } = "请选择 DXF 文件并确认毫米单位。";

    public event EventHandler? Changed;

    public void LoadConfirmedImport(string path, IReadOnlyList<Loop2D> loops)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(loops);
        FileName = Path.GetFileName(path);
        _loops = loops.ToArray();
        IsDemoGeometry = false;
        StatusMessage = _loops.Count == 0
            ? "DXF 已确认，但没有可显示的闭合轮廓。"
            : $"已载入 {FileName} · {_loops.Count} 个闭合轮廓";
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
        FileName = "未打开文件";
        _loops = [];
        IsDemoGeometry = false;
        StatusMessage = "请选择 DXF 文件并确认毫米单位。";
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
