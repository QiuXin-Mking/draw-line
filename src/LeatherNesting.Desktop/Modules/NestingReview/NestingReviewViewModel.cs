using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.NestingReview;

/// <summary>Immutable M09 demo placement data plus transient review selections.</summary>
public sealed class NestingReviewViewModel
{
    private readonly IReadOnlyList<NestingMaterialPageDemo> _materials =
    [
        Material("MAT-01", "头层牛皮 · 黑色", "片料", 2000, 1000,
            new("P-101-L-01", "P-101", "L", 70, 90, 250, 150, 0, false),
            new("P-101-R-01", "P-101", "R", 360, 100, 250, 150, 180, true),
            new("P-205-L-01", "P-205", "L", 680, 80, 180, 260, 90, false)),
        Material("MAT-02", "超纤革 · 米白", "卷料", 1370, 6800,
            new("P-205-L-02", "P-205", "L", 120, 120, 180, 260, 90, false),
            new("P-310-M-01", "P-310", "M", 390, 100, 220, 190, 0, false),
            new("P-418-S-01", "P-418", "S", 690, 150, 160, 130, 180, true)),
        Material("MAT-03", "里料 · 灰色", "卷料", 1200, 3100,
            new("P-512-M-01", "P-512", "M", 120, 80, 300, 170, 0, false),
            new("P-512-M-02", "P-512", "M", 470, 100, 300, 170, 180, false)),
    ];

    private readonly IReadOnlyList<NestingVersionDemo> _versions =
    [
        new("V1", "版本 1 · 初始方案", 81.8, 88.5, 7.2, "2026-08-13 09:42"),
        new("V2", "版本 2 · 当前最佳", 84.1, 92.3, 6.8, "2026-08-13 10:06"),
    ];

    public NestingReviewViewModel()
    {
        SelectedMaterial = _materials[0];
        SelectedVersion = new NestingVersionDemo("CURRENT", "当前演示方案", 86.4, 92.3, 6.8, "2026-08-13 10:18");
    }

    public IReadOnlyList<NestingMaterialPageDemo> Materials => _materials;
    public IReadOnlyList<NestingVersionDemo> Versions => _versions;
    public IReadOnlyList<UnplacedPieceDemo> UnplacedPieces { get; } =
    [
        new("P-418", "XS", 2, "剩余空余区宽度不足"),
        new("P-620", "L", 1, "方向约束与材料余量冲突"),
    ];

    public NestingMaterialPageDemo SelectedMaterial { get; private set; }
    public NestingInstanceDemo? SelectedInstance { get; private set; }
    public NestingVersionDemo SelectedVersion { get; private set; }
    public bool ShowCollisionOverlay { get; private set; }
    public bool HasRealCollisionValidation => false;
    public string TodoMessage { get; private set; } = TodoBadge.StandardText;
    public int TotalUnplacedQuantity => UnplacedPieces.Sum(piece => piece.Quantity);
    public string LowUtilizationReasons => "低利用率原因 · 小件无法填入狭长空余区；方向约束限制旋转；边缘安全距保留 12 mm。";
    public string VersionComparison => "V1 → V2：利用率 +2.3%，完成率 +3.8%，用长 -0.4 m";
    public string CollisionOverlayMessage => "DEMO · 碰撞示例覆盖层，仅用于说明问题呈现，不代表真实验证结果。";

    public void SelectMaterial(string materialId)
    {
        SelectedMaterial = _materials.Single(material => material.Id == materialId);
        SelectedInstance = null;
    }

    public void SelectInstance(string instanceId) =>
        SelectedInstance = SelectedMaterial.Instances.Single(instance => instance.Id == instanceId);

    public void SelectVersion(string versionId) =>
        SelectedVersion = _versions.Single(version => version.Id == versionId);

    public void ToggleCollisionOverlay() => ShowCollisionOverlay = !ShowCollisionOverlay;

    public void InvokeTodo(ReviewTodoAction action)
    {
        var label = action switch
        {
            ReviewTodoAction.Drag => "拖动实例",
            ReviewTodoAction.Rotate => "旋转实例",
            ReviewTodoAction.Mirror => "镜像实例",
            ReviewTodoAction.Lock => "锁定实例",
            ReviewTodoAction.LocalRepack => "局部重排",
            ReviewTodoAction.ValidateCollisions => "真实碰撞验证",
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
        TodoMessage = $"{label}：{TodoBadge.StandardText}";
    }

    private static NestingMaterialPageDemo Material(
        string id,
        string name,
        string type,
        double width,
        double length,
        params NestingInstanceDemo[] instances) =>
        new(id, name, type, width, length, Array.AsReadOnly(instances),
        ["右上角 · 420 × 180 mm", "尾部余料 · 1370 × 360 mm"]);
}
