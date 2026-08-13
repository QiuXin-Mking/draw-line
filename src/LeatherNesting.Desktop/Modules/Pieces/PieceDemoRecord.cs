namespace LeatherNesting.Desktop.Modules.Pieces;

/// <summary>In-memory M06 record. It deliberately does not represent a persisted order or nesting result.</summary>
public sealed class PieceDemoRecord(
    string code,
    string name,
    string size,
    string side,
    int requiredQuantity,
    int placedQuantity,
    string priority,
    string allowedAngles,
    bool mirrorAllowed,
    int gapMillimetres)
{
    public string Code { get; } = code;
    public string Name { get; } = name;
    public string Size { get; } = size;
    public string Side { get; } = side;
    public int RequiredQuantity { get; set; } = requiredQuantity;
    public int PlacedQuantity { get; } = placedQuantity;
    public string Priority { get; set; } = priority;
    public string AllowedAngles { get; } = allowedAngles;
    public bool MirrorAllowed { get; } = mirrorAllowed;
    public int GapMillimetres { get; } = gapMillimetres;
    public bool IsSelected { get; set; }
    public int UnplacedQuantity => Math.Max(RequiredQuantity - PlacedQuantity, 0);
}
