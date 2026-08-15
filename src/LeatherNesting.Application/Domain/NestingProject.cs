using LeatherNesting.Domain;
using LeatherNesting.Geometry.Nesting;

namespace LeatherNesting.Application.Domain;

/// <summary>Aggregate root persisted as a project: metadata + pieces + materials + nesting results.</summary>
public sealed record NestingProject(
    ProjectDocument Document,
    IReadOnlyList<Piece> Pieces,
    IReadOnlyList<Material> Materials,
    IReadOnlyList<NestResult> NestingResults);
