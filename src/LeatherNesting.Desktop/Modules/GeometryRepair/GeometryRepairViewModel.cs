using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Geometry;
using LeatherNesting.Geometry.Offset;

namespace LeatherNesting.Desktop.Modules.GeometryRepair;

/// <summary>
/// M04 adapter over the existing CAD workbench session. It owns demo geometry only and never writes a project.
/// </summary>
public sealed class GeometryRepairViewModel
{
    private static readonly IReadOnlyList<Loop2D> DemoLoops = [CreateOpenContour()];
    private CadWorkbenchViewModel _workbench = CreateWorkbench();
    private IReadOnlyList<Loop2D> _previewBefore = DemoLoops;

    public GeometryRepairViewModel()
    {
        Issues =
        [
            new("OPEN-001", "鞋面外轮廓 / 38-R", RepairIssueSeverity.Blocking, "开放轮廓", "端点间隙 0.05 mm，当前不能作为可排样外轮廓。", "预览闭合或容差连接，并复核新增边。"),
            new("EDGE-017", "后跟片 / 39-L", RepairIssueSeverity.Warning, "自交风险", "局部边界在节点 N17 附近存在交叉风险。", "使用节点工具检查；画布手势尚未接入。"),
            new("HOLE-004", "鞋舌孔 / 40", RepairIssueSeverity.Information, "孔包含关系", "孔距外轮廓最窄处 1.2 mm。", "偏移预览后重新确认包含关系。"),
        ];
        SelectedIssue = Issues[0];
        ToolGroups =
        [
            new("轮廓修复",
            [
                new(RepairToolAction.CloseContour, "闭合", "桥接当前轮廓首尾端点", true),
                new(RepairToolAction.JoinEndpoints, "连接", "按既有容差连接端点", true),
                new(RepairToolAction.GenerateBoundary, "生成轮廓", "从边界集生成唯一候选", true),
            ]),
            new("偏移",
            [
                new(RepairToolAction.OffsetInside, "内缩 1 mm", "材料内侧 · 斜接", true),
                new(RepairToolAction.OffsetOutside, "外扩 1 mm", "材料外侧 · 斜接", true),
            ]),
            new("节点",
            [
                new(RepairToolAction.InsertNode, "插入节点", "需要画布坐标手势", false),
                new(RepairToolAction.MoveNode, "移动节点", "需要节点命中与拖动手势", false),
                new(RepairToolAction.DeleteNode, "删除节点", "需要节点命中手势", false),
            ]),
            new("剪断",
            [
                new(RepairToolAction.BreakAtPoint, "单点剪断", "需要画布坐标手势", false),
                new(RepairToolAction.RemoveSegment, "两点去段", "需要两点选择手势", false),
            ]),
        ];
        Difference = BuildDifference(DemoLoops, DemoLoops, "尚未生成预览差异。");
        Feedback = "请选择工具生成预览；当前页面仅操作模块内演示几何。";
    }

    public IReadOnlyList<RepairIssue> Issues { get; }

    public RepairIssue SelectedIssue { get; private set; }

    public IReadOnlyList<RepairToolGroup> ToolGroups { get; }

    public RepairToolAction? SelectedTool { get; private set; }

    public WorkbenchState State => _workbench.State;

    public string StateLabel => State switch
    {
        WorkbenchState.Ready => "就绪",
        WorkbenchState.Previewing => "预览中",
        WorkbenchState.Committed => "已提交到会话",
        _ => throw new ArgumentOutOfRangeException(),
    };

    public bool CanCommit => _workbench.CanCommit;

    public bool CanCancel => _workbench.CanCancel;

    public bool CanUndo => _workbench.CanUndo;

    public bool CanRedo => _workbench.CanRedo;

    public IReadOnlyList<Loop2D> BeforeLoops => _previewBefore;

    public IReadOnlyList<Loop2D> CurrentLoops => _workbench.CurrentLoops ?? [];

    public RepairDifference Difference { get; private set; }

    public string Feedback { get; private set; }

    public string GeometrySignature => string.Join('|', CurrentLoops.Select(loop =>
        $"{loop.StableId}:{loop.Curves.Count}:{loop.Area:F4}:{loop.Length:F4}"));

    public void SelectIssue(string objectId)
    {
        var issue = Issues.FirstOrDefault(candidate => candidate.ObjectId == objectId);
        if (issue is not null)
            SelectedIssue = issue;
    }

    public bool Preview(RepairToolAction action)
    {
        SelectedTool = action;
        var tool = ToolGroups.SelectMany(group => group.Tools).Single(candidate => candidate.Action == action);
        if (!tool.IsConnected)
        {
            Feedback = $"{tool.Label}：{TodoBadge.StandardText}；需要画布手势，不会修改真实项目或演示几何。";
            return false;
        }

        if (_workbench.State == WorkbenchState.Previewing)
            CancelPreview();

        _previewBefore = CurrentLoops.ToArray();
        _workbench.SelectTool(ToCadTool(action));
        switch (action)
        {
            case RepairToolAction.CloseContour:
                _workbench.PreviewClose();
                break;
            case RepairToolAction.JoinEndpoints:
                _workbench.PreviewGapRepair();
                break;
            case RepairToolAction.GenerateBoundary:
                _workbench.PreviewBoundaryGeneration();
                break;
            case RepairToolAction.OffsetInside:
                _workbench.PreviewOffset(1, OffsetDirection.Inside);
                break;
            case RepairToolAction.OffsetOutside:
                _workbench.PreviewOffset(1, OffsetDirection.Outside);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }

        var success = _workbench.State == WorkbenchState.Previewing;
        Difference = BuildDifference(
            _previewBefore,
            CurrentLoops,
            success ? DescribeTopology(_previewBefore, CurrentLoops) : "预览失败，几何保持不变。");
        Feedback = success
            ? $"{tool.Label}预览已生成；蓝色为沿用，绿色为新增，红色用于冲突提示。尚未写入项目。"
            : string.Join("；", _workbench.ProblemMessages.DefaultIfEmpty("预览未生成。"));
        return success;
    }

    public void CancelPreview()
    {
        if (!_workbench.CanCancel)
            return;

        _workbench.Cancel();
        // CadOperationSession.Cancel does not restore its preview list. Reloading the adapter's captured
        // snapshot preserves the published cancel behavior without changing the shared implementation.
        var restored = _previewBefore.ToArray();
        _workbench = new CadWorkbenchViewModel();
        _workbench.LoadLoops(restored);
        Difference = BuildDifference(restored, restored, "预览已取消，已恢复预览前几何。");
        Feedback = "预览已取消；真实项目未修改。";
    }

    public bool CommitPreview()
    {
        if (!_workbench.CanCommit)
            return false;

        _workbench.Commit();
        Difference = BuildDifference(_previewBefore, CurrentLoops, DescribeTopology(_previewBefore, CurrentLoops));
        Feedback = "已提交到可撤销的 CAD 会话；写入真实项目仍为 TODO。";
        return true;
    }

    public bool Undo()
    {
        if (!_workbench.CanUndo)
            return false;

        _workbench.Undo();
        Difference = BuildDifference(_previewBefore, CurrentLoops, "已撤销最近一次会话操作。");
        Feedback = "已撤销会话操作；真实项目未修改。";
        return true;
    }

    public bool Redo()
    {
        if (!_workbench.CanRedo)
            return false;

        _workbench.Redo();
        Difference = BuildDifference(_previewBefore, CurrentLoops, "已重做最近一次会话操作。");
        Feedback = "已重做会话操作；真实项目未修改。";
        return true;
    }

    public void InvokeTodo(RepairTodoAction action)
    {
        var label = action switch
        {
            RepairTodoAction.BatchRepair => "批量修复",
            RepairTodoAction.PersistToProject => "写入项目版本",
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
        Feedback = $"{label}：{TodoBadge.StandardText}；操作未执行，真实项目不变。";
    }

    private static CadWorkbenchViewModel CreateWorkbench()
    {
        var workbench = new CadWorkbenchViewModel();
        workbench.LoadLoops(DemoLoops);
        return workbench;
    }

    private static CadToolMode ToCadTool(RepairToolAction action) => action switch
    {
        RepairToolAction.CloseContour or RepairToolAction.JoinEndpoints or RepairToolAction.GenerateBoundary => CadToolMode.BoundaryRepair,
        RepairToolAction.OffsetInside or RepairToolAction.OffsetOutside => CadToolMode.Offset,
        _ => CadToolMode.Select,
    };

    private static RepairDifference BuildDifference(IReadOnlyList<Loop2D> before, IReadOnlyList<Loop2D> after, string topology) =>
        new(
            before.Count,
            after.Count,
            before.Sum(loop => loop.Curves.Count),
            after.Sum(loop => loop.Curves.Count),
            before.Sum(loop => loop.Area),
            after.Sum(loop => loop.Area),
            topology);

    private static string DescribeTopology(IReadOnlyList<Loop2D> before, IReadOnlyList<Loop2D> after)
    {
        var curveDelta = after.Sum(loop => loop.Curves.Count) - before.Sum(loop => loop.Curves.Count);
        var loopDelta = after.Count - before.Count;
        return $"轮廓 {Signed(loopDelta)}；曲线 {Signed(curveDelta)}；原始对象保留在会话历史中。";
    }

    private static string Signed(int value) => value switch
    {
        > 0 => $"新增 {value}",
        < 0 => $"减少 {-value}",
        _ => "无增减",
    };

    private static Loop2D CreateOpenContour() => new("OPEN-001", LoopRole.Outer,
    [
        new LineSegment2D(new Point2D(0.05, 0), new Point2D(100, 0)),
        new LineSegment2D(new Point2D(100, 0), new Point2D(100, 55)),
        new LineSegment2D(new Point2D(100, 55), new Point2D(0, 55)),
        new LineSegment2D(new Point2D(0, 55), new Point2D(0, 0)),
    ]);
}
