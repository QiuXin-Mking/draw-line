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
    public bool CanPreview => _state == WorkbenchState.Ready && _session is not null;
    public bool CanCommit => _state == WorkbenchState.Previewing;
    public bool CanCancel => _state == WorkbenchState.Previewing;
    public bool CanUndo => _session?.CanUndo ?? false;
    public bool CanRedo => _session?.CanRedo ?? false;
    public IReadOnlyList<string> ProblemMessages => _problemMessages;

    public IReadOnlyList<Loop2D>? CurrentLoops => _session?.PreviewLoops;

    /// <summary>Initializes the workbench with a set of loops from the current project.</summary>
    public void LoadLoops(IReadOnlyList<Loop2D> loops)
    {
        _session = new CadOperationSession(loops);
        _state = WorkbenchState.Ready;
        _problemMessages.Clear();
    }

    public void SelectTool(CadToolMode mode)
    {
        _toolMode = mode;
        _problemMessages.Clear();
    }

    // --- Boundary Repair ---

    public void PreviewClose()
    {
        if (_session is null) return;
        var loops = _session.PreviewLoops;
        var closer = new ContourCloser();
        var allRepaired = new List<Loop2D>();
        foreach (var loop in loops)
        {
            var result = closer.Close(loop);
            allRepaired.AddRange(result.RepairedLoops);
        }
        // For preview, just update the display
        _state = WorkbenchState.Previewing;
    }

    public void PreviewGapRepair()
    {
        if (_session is null) return;
        var loops = _session.PreviewLoops;
        // Flatten all curves to repair
        var allCurves = loops.SelectMany(l => l.Curves).ToList();
        var repair = new GapRepair();
        var result = repair.Repair(allCurves, "workbench");
        _state = WorkbenchState.Previewing;
        _problemMessages.Clear();
        _problemMessages.AddRange(result.Diagnostics);
        _problemMessages.AddRange(result.Warnings);
    }

    public void PreviewBoundaryGeneration()
    {
        if (_session is null) return;
        var loops = _session.PreviewLoops;
        var allCurves = loops.SelectMany(l => l.Curves).ToList();
        var generator = new BoundaryGenerator();
        var result = generator.Generate(allCurves, "workbench");
        _state = WorkbenchState.Previewing;
        _problemMessages.Clear();
        _problemMessages.AddRange(result.Diagnostics);
        if (result.ValidCandidates.Count > 1)
            _problemMessages.Add($"发现 {result.ValidCandidates.Count} 个候选环，请选择。");
    }

    // --- Offset ---

    public void PreviewOffset(double distanceMm, OffsetDirection direction, OffsetJoinStyle joinStyle = OffsetJoinStyle.Miter)
    {
        if (_session is null) return;
        var adapter = new OffsetAdapter();
        var result = adapter.Offset(_session.PreviewLoops, distanceMm, direction, joinStyle);
        _state = WorkbenchState.Previewing;
        _problemMessages.Clear();
        _problemMessages.AddRange(result.Diagnostics);
        _problemMessages.AddRange(result.TopologyWarnings);
        if (result.RequiresConfirmation)
            _problemMessages.Add("offset 导致拓扑变化，需要确认。");
    }

    // --- Node Edit ---

    public void PreviewInsertNode(Point2D point)
    {
        if (_session is null || _session.PreviewLoops.Count == 0) return;
        var ops = new NodeOperations();
        foreach (var loop in _session.PreviewLoops)
        {
            var result = ops.InsertNode(loop, point);
            if (result.Success)
            {
                _state = WorkbenchState.Previewing;
                return;
            }
            _problemMessages.AddRange(result.Issues);
        }
    }

    public void PreviewMoveNode(int nodeIndex, Point2D newPosition)
    {
        if (_session is null || _session.PreviewLoops.Count == 0) return;
        var ops = new NodeOperations();
        foreach (var loop in _session.PreviewLoops)
        {
            var result = ops.MoveNode(loop, nodeIndex, newPosition);
            if (result.Success) { _state = WorkbenchState.Previewing; return; }
            _problemMessages.AddRange(result.Issues);
        }
    }

    public void PreviewDeleteNode(int nodeIndex)
    {
        if (_session is null || _session.PreviewLoops.Count == 0) return;
        var ops = new NodeOperations();
        foreach (var loop in _session.PreviewLoops)
        {
            var result = ops.DeleteNode(loop, nodeIndex);
            if (result.Success) { _state = WorkbenchState.Previewing; return; }
            _problemMessages.AddRange(result.Issues);
        }
    }

    // --- Break ---

    public void PreviewBreakAtPoint(Point2D point)
    {
        if (_session is null || _session.PreviewLoops.Count == 0) return;
        var ops = new BreakOperations();
        foreach (var loop in _session.PreviewLoops)
        {
            var result = ops.BreakAtPoint(loop, point);
            _problemMessages.AddRange(result.Issues);
            _state = WorkbenchState.Previewing;
        }
    }

    public void PreviewRemoveSegment(Point2D pointA, Point2D pointB)
    {
        if (_session is null || _session.PreviewLoops.Count == 0) return;
        var ops = new BreakOperations();
        foreach (var loop in _session.PreviewLoops)
        {
            var result = ops.RemoveSegment(loop, pointA, pointB);
            _problemMessages.AddRange(result.Issues);
            _state = WorkbenchState.Previewing;
        }
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
        if (_session is null) return;
        var notch = new NotchFeature(contourId, anchorArcLength, shape, width, depth, materialSide, outputMode, layerOrTool);
        var validator = new NotchValidator();
        var contour = _session.PreviewLoops.FirstOrDefault(l => l.StableId == contourId);
        if (contour is null)
        {
            _problemMessages.Add($"未找到轮廓 {contourId}。");
            return;
        }
        var validation = validator.Validate(notch, contour, []);
        _problemMessages.Clear();
        _problemMessages.AddRange(validation.Errors);
        _problemMessages.AddRange(validation.Warnings);
        if (validation.IsValid) _state = WorkbenchState.Previewing;
    }

    // --- Commit / Cancel ---

    public void Commit()
    {
        _session?.Commit();
        _state = WorkbenchState.Committed;
    }

    public void Cancel()
    {
        _session?.Cancel();
        _state = WorkbenchState.Ready;
        _problemMessages.Clear();
    }

    public void Undo()
    {
        var (result, _) = _session?.Undo() ?? (CadCommandResult.Failed(["没有 session。"]), null);
        _problemMessages.Clear();
        _problemMessages.AddRange(result.Diagnostics);
        _state = WorkbenchState.Ready;
    }

    public void Redo()
    {
        var (result, _) = _session?.Redo() ?? (CadCommandResult.Failed(["没有 session。"]), null);
        _problemMessages.Clear();
        _problemMessages.AddRange(result.Diagnostics);
        _state = WorkbenchState.Ready;
    }
}