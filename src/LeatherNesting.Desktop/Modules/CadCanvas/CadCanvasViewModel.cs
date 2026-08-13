using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Geometry;

namespace LeatherNesting.Desktop.Modules.CadCanvas;

public enum CadCanvasTodoTool
{
    HitTesting,
    BoxSelection,
    MultiSelection,
    LayerPersistence,
    ComplexCurveEditing,
}

public sealed record CadCanvasRenderRequest(IReadOnlyList<Loop2D> Loops, bool Refit);

public sealed class CadCanvasViewModel
{
    private readonly Dictionary<CadObjectCategory, bool> _categoryVisibility =
        Enum.GetValues<CadObjectCategory>().ToDictionary(category => category, _ => true);

    public CadCanvasViewModel()
    {
        Objects = DemoGeometryFactory.Create();
    }

    public event Action<CadCanvasRenderRequest>? RenderRequested;

    public IReadOnlyList<DemoObject> Objects { get; }

    public IReadOnlyList<DemoObject> VisibleObjects =>
        Objects.Where(item => _categoryVisibility[item.Category]).ToArray();

    public IReadOnlyList<Loop2D> VisibleLoops => VisibleObjects.Select(item => item.Loop).ToArray();

    public string StatusMessage { get; private set; } = "DEMO 几何已载入 · 滚轮缩放 · 空白处拖拽平移";

    public bool IsCategoryVisible(CadObjectCategory category) => _categoryVisibility[category];

    public void SetCategoryVisibility(CadObjectCategory category, bool isVisible)
    {
        _categoryVisibility[category] = isVisible;
        StatusMessage = $"{CategoryLabel(category)}已{(isVisible ? "显示" : "隐藏")} · 当前 {VisibleLoops.Count} 个对象";
        RequestRender(refit: false);
    }

    public void FitAll()
    {
        StatusMessage = "已请求全图显示 · 视图适配当前可见对象";
        RequestRender(refit: true);
    }

    public void ReportZoomGuidance(string direction)
    {
        StatusMessage = $"{direction}：请将指针放在画布上滚动滚轮；缩放中心跟随指针。";
    }

    public void ReportCoordinates(Point2D point)
    {
        StatusMessage = $"X {point.X:F2} mm · Y {point.Y:F2} mm · 滚轮缩放";
    }

    public void InvokeTodo(CadCanvasTodoTool tool)
    {
        StatusMessage = $"{TodoLabel(tool)}：{TodoBadge.StandardText}；不会修改当前项目或演示几何。";
    }

    private void RequestRender(bool refit) => RenderRequested?.Invoke(new CadCanvasRenderRequest(VisibleLoops, refit));

    public static string CategoryLabel(CadObjectCategory category) => category switch
    {
        CadObjectCategory.OuterContour => "外轮廓",
        CadObjectCategory.Hole => "孔",
        CadObjectCategory.InternalLine => "内部线 / 标记",
        _ => throw new ArgumentOutOfRangeException(nameof(category)),
    };

    public static string TodoLabel(CadCanvasTodoTool tool) => tool switch
    {
        CadCanvasTodoTool.HitTesting => "真实命中测试",
        CadCanvasTodoTool.BoxSelection => "框选",
        CadCanvasTodoTool.MultiSelection => "多选",
        CadCanvasTodoTool.LayerPersistence => "图层持久化",
        CadCanvasTodoTool.ComplexCurveEditing => "复杂曲线编辑",
        _ => throw new ArgumentOutOfRangeException(nameof(tool)),
    };
}
