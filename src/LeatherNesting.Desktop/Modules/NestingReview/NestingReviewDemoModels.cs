namespace LeatherNesting.Desktop.Modules.NestingReview;

public sealed record NestingInstanceDemo(
    string Id,
    string PieceCode,
    string Size,
    double X,
    double Y,
    double Width,
    double Height,
    double RotationDegrees,
    bool Mirrored);

public sealed record NestingMaterialPageDemo(
    string Id,
    string Name,
    string MaterialType,
    double WidthMillimetres,
    double LengthMillimetres,
    IReadOnlyList<NestingInstanceDemo> Instances,
    IReadOnlyList<string> FreeZones);

public sealed record NestingVersionDemo(
    string Id,
    string Label,
    double UtilizationPercent,
    double CompletionPercent,
    double UsedLengthMetres,
    string GeneratedAt);

public sealed record UnplacedPieceDemo(string PieceCode, string Size, int Quantity, string Reason);

public enum ReviewTodoAction
{
    Drag,
    Rotate,
    Mirror,
    Lock,
    LocalRepack,
    ValidateCollisions,
}
