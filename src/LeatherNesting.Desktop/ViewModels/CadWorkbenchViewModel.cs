using LeatherNesting.Application.CadEditing;
using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Features;
using LeatherNesting.Geometry.NodeEditing;
using LeatherNesting.Geometry.Offset;
using LeatherNesting.Geometry.Repair;

namespace LeatherNesting.Desktop.ViewModels;

/// <summary>Tool modes for the CAD workbench. Only one tool is active at a time.</summary>
public enum CadToolMode { Select, BoundaryRepair, Offset, NodeEdit, Break, Notch }

/// <summary>Workbench state: ready, previewing, or committed.</summary>
public enum WorkbenchState { Ready, Previewing, Committed }

/// <summary>State machine for the U4 CAD repair and process workbench.
/// Session state only — never mutates ProjectDocument until a Commit use case approves it.</summary>
public sealed class CadWorkbenchViewModel
{
    private CadToolMode _toolMode = CadToolMode.Select;
    private WorkbenchState _state = WorkbenchState.Ready;
    private CadOperationSession? _session;
    private readonly List<string> _problemMessages = [];

    public CadToolMode ToolMode => _toolMode;
    public WorkbenchState State => _state;
    public bool CanPreview => _state != WorkbenchState.Previewing && _session is not null;
    public bool CanCommit => _state == WorkbenchState.Previewing;
    public bool CanCancel => _state == WorkbenchState.Previewing;
    public bool CanUndo => _session?.CanUndo ?? false;
    public bool CanRedo => _session?.CanRedo ?? false;
    public IReadOnlyList<string> ProblemMessages => _problemMessages;

    public event EventHandler? Changed;

    public IReadOnlyList<Loop2D>? CurrentLoops => _session?.PreviewLoops;

    /// <summary>Initializes the workbench with a set of loops from the current project.</summary>
    public void LoadLoops(IReadOnlyList<Loop2D> loops)
    {
        ArgumentNullException.ThrowIfNull(loops);
        _session = new CadOperationSession(loops);
        _state = WorkbenchState.Ready;
        SelectedLoopId = null;
        _problemMessages.Clear();
        NotifyChanged();
    }

    public void SelectTool(CadToolMode mode)
    {
        if (_toolMode == mode && _problemMessages.Count == 0)
            return;
        _toolMode = mode;
        _problemMessages.Clear();
        NotifyChanged();
    }

    // --- Boundary Repair ---

    public void PreviewClose()
    {
        var loop = _session?.PreviewLoops.FirstOrDefault();
        if (loop is null)
        {
            ReportProblem("没有可闭合的轮廓。");
            return;
        }
        RunPreview(new CloseContourCommand(loop.StableId));
    }

    public void PreviewGapRepair() => RunPreview(new GapRepairCommand());

    public void PreviewBoundaryGeneration() => RunPreview(new BoundaryGenerateCommand());

    // --- Offset ---

    public void PreviewOffset(double distanceMm, OffsetDirection direction, OffsetJoinStyle joinStyle = OffsetJoinStyle.Miter)
        => RunPreview(new OffsetCommand(distanceMm, direction, joinStyle));

    // --- Node Edit ---

    public void PreviewInsertNode(Point2D point)
    {
        var loop = _session?.PreviewLoops.FirstOrDefault();
        if (loop is null)
        {
            ReportProblem("没有可编辑的轮廓。");
            return;
        }
        RunPreview(new InsertNodeCommand(loop.StableId, point));
    }

    public void PreviewMoveNode(int nodeIndex, Point2D newPosition)
    {
        var loop = _session?.PreviewLoops.FirstOrDefault();
        if (loop is null)
        {
            ReportProblem("没有可编辑的轮廓。");
            return;
        }
        RunPreview(new MoveNodeCommand(loop.StableId, nodeIndex, newPosition));
    }

    public void PreviewDeleteNode(int nodeIndex)
    {
        var loop = _session?.PreviewLoops.FirstOrDefault();
        if (loop is null)
        {
            ReportProblem("没有可编辑的轮廓。");
            return;
        }
        RunPreview(new DeleteNodeCommand(loop.StableId, nodeIndex));
    }

    // --- Break ---

    public void PreviewBreakAtPoint(Point2D point)
    {
        if (_session is null || _session.PreviewLoops.Count == 0)
        {
            ReportProblem("没有可打断的轮廓。");
            return;
        }
        var ops = new BreakOperations();
        foreach (var loop in _session.PreviewLoops)
        {
            var result = ops.BreakAtPoint(loop, point);
            _problemMessages.AddRange(result.Issues);
            _state = WorkbenchState.Previewing;
        }
        NotifyChanged();
    }

    public void PreviewRemoveSegment(Point2D pointA, Point2D pointB)
    {
        if (_session is null || _session.PreviewLoops.Count == 0)
        {
            ReportProblem("没有可删除的线段。");
            return;
        }
        var ops = new BreakOperations();
        foreach (var loop in _session.PreviewLoops)
        {
            var result = ops.RemoveSegment(loop, pointA, pointB);
            _problemMessages.AddRange(result.Issues);
            _state = WorkbenchState.Previewing;
        }
        NotifyChanged();
    }

    // --- Notch ---

    public void PreviewNotch(
        string contourId,
        double anchorArcLength,
        NotchShape shape,
        double width,
        double depth,
        MaterialSide materialSide,
        NotchOutputMode outputMode = NotchOutputMode.Cut,
        string layerOrTool = "CUT")
    {
        if (_session is null)
        {
            ReportProblem("没有 session。");
            return;
        }
        var notch = new NotchFeature(contourId, anchorArcLength, shape, width, depth, materialSide, outputMode, layerOrTool);
        var validator = new NotchValidator();
        var contour = _session.PreviewLoops.FirstOrDefault(l => l.StableId == contourId);
        if (contour is null)
        {
            ReportProblem($"未找到轮廓 {contourId}。");
            return;
        }
        var validation = validator.Validate(notch, contour, []);
        _problemMessages.Clear();
        _problemMessages.AddRange(validation.Errors);
        _problemMessages.AddRange(validation.Warnings);
        if (validation.IsValid) _state = WorkbenchState.Previewing;
        NotifyChanged();
    }

    // --- Selection & Transform ---

    public string? SelectedLoopId { get; private set; }

    public void SelectPiece(Point2D point)
    {
        if (_session is null)
        {
            ReportProblem("没有 session。");
            return;
        }
        var selected = _session.PreviewLoops.LastOrDefault(l => l.ContainsPoint(point))?.StableId;
        if (selected == SelectedLoopId)
            return;
        SelectedLoopId = selected;
        _problemMessages.Clear();
        NotifyChanged();
    }

    public void ClearSelection()
    {
        if (SelectedLoopId is null)
            return;
        SelectedLoopId = null;
        NotifyChanged();
    }

    public void MoveSelected(Point2D delta)
    {
        if (_session is null || SelectedLoopId is null)
        {
            ReportProblem("请先选中要移动的轮廓。");
            return;
        }
        RunPreview(new TransformCommand(SelectedLoopId, new Transform2D(delta.X, delta.Y, 0, false)));
    }

    public void RotateSelected(double degrees)
    {
        if (_session is null || SelectedLoopId is null)
        {
            ReportProblem("请先选中要旋转的轮廓。");
            return;
        }
        var loop = _session.PreviewLoops.FirstOrDefault(l => l.StableId == SelectedLoopId);
        if (loop is null)
        {
            ReportProblem("选中的轮廓已不存在。");
            return;
        }
        RunPreview(new TransformCommand(SelectedLoopId, Transform2D.RotateAbout(loop.Centroid, degrees)));
    }

    // --- Commit / Cancel ---

    public void Commit()
    {
        var result = _session?.Commit() ?? CadCommandResult.Failed(["没有 session。"]);
        Complete(result, WorkbenchState.Committed);
    }

    public void Cancel()
    {
        if (_session is null)
        {
            ReportProblem("没有 session。");
            return;
        }
        if (_state != WorkbenchState.Previewing)
        {
            ReportProblem("没有可取消的预览操作。");
            return;
        }
        _session.Cancel();
        _state = WorkbenchState.Ready;
        _problemMessages.Clear();
        NotifyChanged();
    }

    public void Undo()
    {
        var (result, _) = _session?.Undo() ?? (CadCommandResult.Failed(["没有 session。"]), null);
        Complete(result, WorkbenchState.Ready);
    }

    public void Redo()
    {
        var (result, _) = _session?.Redo() ?? (CadCommandResult.Failed(["没有 session。"]), null);
        Complete(result, WorkbenchState.Ready);
    }

    private void RunPreview(LoopTransformCommand command)
    {
        if (_session is null)
        {
            ReportProblem("没有 session。");
            return;
        }
        if (_state == WorkbenchState.Previewing)
        {
            ReportProblem("请先提交或取消当前预览。");
            return;
        }
        var result = _session.Preview(command);
        Complete(result, WorkbenchState.Previewing);
    }

    private void Complete(CadCommandResult result, WorkbenchState successState)
    {
        _problemMessages.Clear();
        _problemMessages.AddRange(result.Diagnostics);
        if (result.Success)
            _state = successState;
        NotifyChanged();
    }

    private void ReportProblem(string message)
    {
        _problemMessages.Clear();
        _problemMessages.Add(message);
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
