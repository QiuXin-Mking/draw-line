namespace LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

/// <summary>The single, evidence-ordered registry for the CAD toolbar.</summary>
public static class CadToolCatalog
{
    private const CadToolbarMode Edit = CadToolbarMode.CadEdit;
    private const CadToolbarMode EditAndReview = CadToolbarMode.CadEdit | CadToolbarMode.NestingReview;
    private const string PendingEvidence = "TODO待实机确认";

    public static IReadOnlyList<CadToolDefinition> All { get; } = Array.AsReadOnly<CadToolDefinition>(
    [
        Tool(1, "CAD-04", CadToolCommandKey.ExportToOrder, "导到订单", "导到订单 Ctrl+T", CadToolGroup.A,
            CadToolIconKey.ExportToOrder, CadToolConfidence.Confirmed, Edit, CadToolImplementationState.Todo, "Ctrl+T"),
        Tool(2, "CAD-05", CadToolCommandKey.Select, "鼠标选择模式", "切换鼠标选择模式 (ESC)", CadToolGroup.A,
            CadToolIconKey.Select, CadToolConfidence.Confirmed, EditAndReview, CadToolImplementationState.Partial, "Esc"),
        Tool(3, "CAD-06", CadToolCommandKey.Refit, "范围缩放", "范围缩放", CadToolGroup.A,
            CadToolIconKey.Refit, CadToolConfidence.Confirmed, Edit, CadToolImplementationState.Implemented),

        Tool(4, "CAD-07", CadToolCommandKey.DrawPolyline, "绘制多段线", "绘制多段线", CadToolGroup.B,
            CadToolIconKey.DrawPolyline, CadToolConfidence.Confirmed, Edit, CadToolImplementationState.Todo),
        Tool(5, "CAD-08", CadToolCommandKey.DrawRectangle, "绘制矩形", "绘制矩形", CadToolGroup.B,
            CadToolIconKey.DrawRectangle, CadToolConfidence.Confirmed, Edit, CadToolImplementationState.Todo),
        Tool(6, "CAD-09", CadToolCommandKey.DrawCircle, "绘制圆", "绘制圆", CadToolGroup.B,
            CadToolIconKey.DrawCircle, CadToolConfidence.High, Edit, CadToolImplementationState.Todo),
        InferredTool(7, "CAD-10", CadToolCommandKey.DrawLine, "绘制直线", CadToolGroup.B,
            CadToolIconKey.DrawLine, CadToolConfidence.Medium),
        Tool(8, "CAD-11", CadToolCommandKey.TextAnnotation, "文字标注", "文字标注", CadToolGroup.B,
            CadToolIconKey.TextAnnotation, CadToolConfidence.High, Edit, CadToolImplementationState.Todo),
        InferredTool(9, "CAD-12", CadToolCommandKey.Dimension, "尺寸/距离标注", CadToolGroup.B,
            CadToolIconKey.Dimension, CadToolConfidence.Medium),
        InferredTool(10, "CAD-13", CadToolCommandKey.EditNodeOrFillet, "添加节点/圆角编辑", CadToolGroup.B,
            CadToolIconKey.EditNodeOrFillet, CadToolConfidence.Low),

        InferredTool(11, "CAD-14", CadToolCommandKey.HolePattern, "点阵/孔位工具", CadToolGroup.C,
            CadToolIconKey.HolePattern, CadToolConfidence.Medium),
        Tool(12, "CAD-15", CadToolCommandKey.DrawSpline, "绘制自由曲线/样条", "绘制自由曲线/样条", CadToolGroup.C,
            CadToolIconKey.DrawSpline, CadToolConfidence.High, Edit, CadToolImplementationState.Todo),
        Tool(13, "CAD-16", CadToolCommandKey.Notch, "马牙齿/三角剪口", "马牙齿/三角剪口", CadToolGroup.C,
            CadToolIconKey.Notch, CadToolConfidence.High, Edit, CadToolImplementationState.Todo),

        InferredTool(14, "CAD-17", CadToolCommandKey.SharpCornerContour, "尖角轮廓/折角处理", CadToolGroup.D,
            CadToolIconKey.SharpCornerContour, CadToolConfidence.Low),
        InferredTool(15, "CAD-18", CadToolCommandKey.CloseContour, "闭合轮廓", CadToolGroup.D,
            CadToolIconKey.CloseContour, CadToolConfidence.Medium),
        InferredTool(16, "CAD-19", CadToolCommandKey.RoundContour, "圆角轮廓/倒圆角", CadToolGroup.D,
            CadToolIconKey.RoundContour, CadToolConfidence.Medium),
        InferredTool(17, "CAD-20", CadToolCommandKey.SmoothCurve, "曲线平滑", CadToolGroup.D,
            CadToolIconKey.SmoothCurve, CadToolConfidence.Medium),
        InferredTool(18, "CAD-21", CadToolCommandKey.UvCurveDirection, "UV 曲线/曲线方向", CadToolGroup.D,
            CadToolIconKey.UvCurveDirection, CadToolConfidence.Medium),
        InferredTool(19, "CAD-22", CadToolCommandKey.SharpenCorner, "尖角化/V 形处理", CadToolGroup.D,
            CadToolIconKey.SharpenCorner, CadToolConfidence.Medium),
        Tool(20, "CAD-23", CadToolCommandKey.EraseSegment, "擦除线段", "擦除线段", CadToolGroup.D,
            CadToolIconKey.EraseSegment, CadToolConfidence.High, Edit, CadToolImplementationState.Todo),

        InferredTool(21, "CAD-24", CadToolCommandKey.RegionOrdering, "面域/前后关系操作", CadToolGroup.E,
            CadToolIconKey.RegionOrdering, CadToolConfidence.Low),
        Tool(22, "CAD-25", CadToolCommandKey.Transform, "对象变换/缩放", "对象变换/缩放", CadToolGroup.E,
            CadToolIconKey.Transform, CadToolConfidence.High, Edit, CadToolImplementationState.Todo),
        Tool(23, "CAD-26", CadToolCommandKey.Undo, "撤销", "撤销 Ctrl+Z", CadToolGroup.E,
            CadToolIconKey.Undo, CadToolConfidence.High, EditAndReview, CadToolImplementationState.Todo, "Ctrl+Z"),
        Tool(24, "CAD-27", CadToolCommandKey.Redo, "重做", "重做 Ctrl+Y", CadToolGroup.E,
            CadToolIconKey.Redo, CadToolConfidence.High, EditAndReview, CadToolImplementationState.Todo, "Ctrl+Y"),
        Tool(25, "CAD-28", CadToolCommandKey.Cancel, "取消当前命令", "取消当前命令", CadToolGroup.E,
            CadToolIconKey.Cancel, CadToolConfidence.High, EditAndReview, CadToolImplementationState.Partial),
        Tool(26, "CAD-29", CadToolCommandKey.Delete, "删除选中对象", "删除选中对象", CadToolGroup.E,
            CadToolIconKey.Delete, CadToolConfidence.High, EditAndReview, CadToolImplementationState.Todo),
        Tool(27, "CAD-30", CadToolCommandKey.Settings, "工具设置/当前模式设置", "工具设置/当前模式设置", CadToolGroup.E,
            CadToolIconKey.Settings, CadToolConfidence.High, EditAndReview, CadToolImplementationState.Todo),
    ]);

    private static CadToolDefinition InferredTool(
        int order,
        string controlId,
        CadToolCommandKey commandKey,
        string label,
        CadToolGroup group,
        CadToolIconKey iconKey,
        CadToolConfidence confidence) =>
        Tool(order, controlId, commandKey, label, $"{label} · {PendingEvidence}", group, iconKey,
            confidence, Edit, CadToolImplementationState.Todo);

    private static CadToolDefinition Tool(
        int order,
        string controlId,
        CadToolCommandKey commandKey,
        string label,
        string tooltip,
        CadToolGroup group,
        CadToolIconKey iconKey,
        CadToolConfidence confidence,
        CadToolbarMode supportedModes,
        CadToolImplementationState implementationState,
        string? shortcut = null) =>
        new(order, controlId, commandKey, label, tooltip, group, iconKey, confidence,
            supportedModes, implementationState, shortcut);
}
