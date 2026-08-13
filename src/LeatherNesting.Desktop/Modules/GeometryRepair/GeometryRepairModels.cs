namespace LeatherNesting.Desktop.Modules.GeometryRepair;

public enum RepairIssueSeverity { Blocking, Warning, Information }

public enum RepairToolAction
{
    CloseContour,
    JoinEndpoints,
    GenerateBoundary,
    OffsetInside,
    OffsetOutside,
    InsertNode,
    MoveNode,
    DeleteNode,
    BreakAtPoint,
    RemoveSegment,
}

public enum RepairTodoAction { BatchRepair, PersistToProject }

public sealed record RepairIssue(
    string ObjectId,
    string ObjectName,
    RepairIssueSeverity Severity,
    string Kind,
    string Detail,
    string Suggestion);

public sealed record RepairTool(RepairToolAction Action, string Label, string Description, bool IsConnected);

public sealed record RepairToolGroup(string Name, IReadOnlyList<RepairTool> Tools);

public sealed record RepairDifference(
    int BeforeLoopCount,
    int AfterLoopCount,
    int BeforeCurveCount,
    int AfterCurveCount,
    double BeforeAreaSquareMillimetres,
    double AfterAreaSquareMillimetres,
    string TopologyChange)
{
    public int AddedCurveCount => Math.Max(0, AfterCurveCount - BeforeCurveCount);

    public int RemovedCurveCount => Math.Max(0, BeforeCurveCount - AfterCurveCount);

    public static RepairDifference Empty(int loopCount, int curveCount, double area) =>
        new(loopCount, loopCount, curveCount, curveCount, area, area, "尚未生成预览差异。");
}
