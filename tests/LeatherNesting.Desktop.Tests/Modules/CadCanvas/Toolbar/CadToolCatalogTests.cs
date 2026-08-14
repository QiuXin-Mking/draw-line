using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.CadCanvas.Toolbar;

public sealed class CadToolCatalogTests
{
    private const string PendingEvidence = "TODO待实机确认";

    [Fact]
    [Trait("TestId", "AC-CAD-T01")]
    public void All_freezes_the_27_item_order_and_unique_identifiers()
    {
        var tools = CadToolCatalog.All;

        Assert.Equal(27, tools.Count);
        Assert.Equal(Enumerable.Range(1, 27), tools.Select(tool => tool.Order));
        Assert.Equal(
            Enumerable.Range(4, 27).Select(number => $"CAD-{number:00}"),
            tools.Select(tool => tool.ControlId));
        Assert.Equal(tools.Count, tools.Select(tool => tool.ControlId).Distinct().Count());
        Assert.Equal(tools.Count, tools.Select(tool => tool.CommandKey).Distinct().Count());
        Assert.Equal(tools.Count, tools.Select(tool => tool.IconKey).Distinct().Count());
        Assert.Equal(
            [
                CadToolCommandKey.ExportToOrder, CadToolCommandKey.Select, CadToolCommandKey.Refit,
                CadToolCommandKey.DrawPolyline, CadToolCommandKey.DrawRectangle, CadToolCommandKey.DrawCircle,
                CadToolCommandKey.DrawLine, CadToolCommandKey.TextAnnotation, CadToolCommandKey.Dimension,
                CadToolCommandKey.EditNodeOrFillet, CadToolCommandKey.HolePattern, CadToolCommandKey.DrawSpline,
                CadToolCommandKey.Notch, CadToolCommandKey.SharpCornerContour, CadToolCommandKey.CloseContour,
                CadToolCommandKey.RoundContour, CadToolCommandKey.SmoothCurve, CadToolCommandKey.UvCurveDirection,
                CadToolCommandKey.SharpenCorner, CadToolCommandKey.EraseSegment, CadToolCommandKey.RegionOrdering,
                CadToolCommandKey.Transform, CadToolCommandKey.Undo, CadToolCommandKey.Redo,
                CadToolCommandKey.Cancel, CadToolCommandKey.Delete, CadToolCommandKey.Settings,
            ],
            tools.Select(tool => tool.CommandKey));
    }

    [Fact]
    [Trait("TestId", "AC-CAD-T02")]
    public void All_keeps_the_first_five_screenshot_confirmed_commands_and_tooltips()
    {
        var expected = new[]
        {
            ("CAD-04", CadToolCommandKey.ExportToOrder, "导到订单", "导到订单 Ctrl+T", "Ctrl+T"),
            ("CAD-05", CadToolCommandKey.Select, "鼠标选择模式", "切换鼠标选择模式 (ESC)", "Esc"),
            ("CAD-06", CadToolCommandKey.Refit, "范围缩放", "范围缩放", (string?)null),
            ("CAD-07", CadToolCommandKey.DrawPolyline, "绘制多段线", "绘制多段线", (string?)null),
            ("CAD-08", CadToolCommandKey.DrawRectangle, "绘制矩形", "绘制矩形", (string?)null),
        };

        Assert.Equal(
            expected,
            CadToolCatalog.All.Take(5).Select(tool =>
                (tool.ControlId, tool.CommandKey, tool.Label, tool.Tooltip, tool.Shortcut)));
        Assert.All(CadToolCatalog.All.Take(5), tool =>
        {
            Assert.Equal(CadToolConfidence.Confirmed, tool.Confidence);
            Assert.DoesNotContain(PendingEvidence, tool.Tooltip);
        });
    }

    [Fact]
    public void All_freezes_the_evidence_labels_groups_and_confidence()
    {
        Assert.Equal(
            [
                "导到订单", "鼠标选择模式", "范围缩放", "绘制多段线", "绘制矩形", "绘制圆", "绘制直线",
                "文字标注", "尺寸/距离标注", "添加节点/圆角编辑", "点阵/孔位工具", "绘制自由曲线/样条",
                "马牙齿/三角剪口", "尖角轮廓/折角处理", "闭合轮廓", "圆角轮廓/倒圆角", "曲线平滑",
                "UV 曲线/曲线方向", "尖角化/V 形处理", "擦除线段", "面域/前后关系操作", "对象变换/缩放",
                "撤销", "重做", "取消当前命令", "删除选中对象", "工具设置/当前模式设置",
            ],
            CadToolCatalog.All.Select(tool => tool.Label));
        Assert.Equal(
            [
                CadToolGroup.A, CadToolGroup.A, CadToolGroup.A,
                CadToolGroup.B, CadToolGroup.B, CadToolGroup.B, CadToolGroup.B,
                CadToolGroup.B, CadToolGroup.B, CadToolGroup.B,
                CadToolGroup.C, CadToolGroup.C, CadToolGroup.C,
                CadToolGroup.D, CadToolGroup.D, CadToolGroup.D, CadToolGroup.D,
                CadToolGroup.D, CadToolGroup.D, CadToolGroup.D,
                CadToolGroup.E, CadToolGroup.E, CadToolGroup.E, CadToolGroup.E,
                CadToolGroup.E, CadToolGroup.E, CadToolGroup.E,
            ],
            CadToolCatalog.All.Select(tool => tool.Group));
        Assert.Equal(
            [
                CadToolConfidence.Confirmed, CadToolConfidence.Confirmed, CadToolConfidence.Confirmed,
                CadToolConfidence.Confirmed, CadToolConfidence.Confirmed, CadToolConfidence.High,
                CadToolConfidence.Medium, CadToolConfidence.High, CadToolConfidence.Medium,
                CadToolConfidence.Low, CadToolConfidence.Medium, CadToolConfidence.High,
                CadToolConfidence.High, CadToolConfidence.Low, CadToolConfidence.Medium,
                CadToolConfidence.Medium, CadToolConfidence.Medium, CadToolConfidence.Medium,
                CadToolConfidence.Medium, CadToolConfidence.High, CadToolConfidence.Low,
                CadToolConfidence.High, CadToolConfidence.High, CadToolConfidence.High,
                CadToolConfidence.High, CadToolConfidence.High, CadToolConfidence.High,
            ],
            CadToolCatalog.All.Select(tool => tool.Confidence));
    }

    [Fact]
    public void Medium_and_low_confidence_tooltips_disclose_pending_real_machine_confirmation()
    {
        var inferred = CadToolCatalog.All.Where(tool =>
            tool.Confidence is CadToolConfidence.Medium or CadToolConfidence.Low);

        Assert.NotEmpty(inferred);
        Assert.All(inferred, tool => Assert.Contains(PendingEvidence, tool.Tooltip));
        Assert.All(
            CadToolCatalog.All.Where(tool =>
                tool.Confidence is CadToolConfidence.Confirmed or CadToolConfidence.High),
            tool => Assert.DoesNotContain(PendingEvidence, tool.Tooltip));
    }

    [Fact]
    public void Supported_modes_keep_all_edit_tools_and_only_six_review_commands()
    {
        Assert.All(
            CadToolCatalog.All,
            tool => Assert.True(tool.SupportedModes.HasFlag(CadToolbarMode.CadEdit)));

        Assert.Equal(
            [
                CadToolCommandKey.Select,
                CadToolCommandKey.Undo,
                CadToolCommandKey.Redo,
                CadToolCommandKey.Cancel,
                CadToolCommandKey.Delete,
                CadToolCommandKey.Settings,
            ],
            CadToolCatalog.All
                .Where(tool => tool.SupportedModes.HasFlag(CadToolbarMode.NestingReview))
                .Select(tool => tool.CommandKey));
    }

    [Fact]
    public void Implementation_states_and_shortcuts_match_the_current_product_boundary()
    {
        Assert.Equal(CadToolImplementationState.Implemented, Tool(CadToolCommandKey.Refit).ImplementationState);
        Assert.Equal(CadToolImplementationState.Partial, Tool(CadToolCommandKey.Select).ImplementationState);
        Assert.Equal(CadToolImplementationState.Partial, Tool(CadToolCommandKey.Cancel).ImplementationState);
        Assert.All(
            CadToolCatalog.All.Where(tool => tool.CommandKey is not (
                CadToolCommandKey.Refit or CadToolCommandKey.Select or CadToolCommandKey.Cancel)),
            tool => Assert.Equal(CadToolImplementationState.Todo, tool.ImplementationState));

        Assert.Equal("Ctrl+Z", Tool(CadToolCommandKey.Undo).Shortcut);
        Assert.Equal("Ctrl+Y", Tool(CadToolCommandKey.Redo).Shortcut);
        Assert.Equal(
            ["Ctrl+T", "Esc", "Ctrl+Z", "Ctrl+Y"],
            CadToolCatalog.All.Where(tool => tool.Shortcut is not null).Select(tool => tool.Shortcut));
    }

    [Fact]
    public void Cancel_delete_and_erase_are_distinct_contracts()
    {
        Assert.Equal("CAD-23", Tool(CadToolCommandKey.EraseSegment).ControlId);
        Assert.Equal("CAD-28", Tool(CadToolCommandKey.Cancel).ControlId);
        Assert.Equal("CAD-29", Tool(CadToolCommandKey.Delete).ControlId);
    }

    private static CadToolDefinition Tool(CadToolCommandKey commandKey) =>
        Assert.Single(CadToolCatalog.All, tool => tool.CommandKey == commandKey);
}
