using LeatherNesting.Application.Domain;
using LeatherNesting.Domain;
using LeatherNesting.Geometry;
using PieceEntity = LeatherNesting.Application.Domain.Piece;

namespace LeatherNesting.Application;

/// <summary>Builds a <see cref="NestingProject"/> from a working set of loops,
/// using each loop's StableId as the piece identity.</summary>
public static class NestingProjectFactory
{
    public static NestingProject FromLoops(IReadOnlyList<Loop2D> loops, ProjectDocument document) =>
        new(
            document,
            loops.Select(l => new PieceEntity(l.StableId, string.Empty, string.Empty, l)).ToList(),
            [],
            []);
}
