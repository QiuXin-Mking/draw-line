using LeatherNesting.Geometry;

namespace LeatherNesting.Application.Domain;

/// <summary>A cut piece: identity + size + geometry outline.</summary>
public sealed record Piece(string Id, string Name, string Size, Loop2D Outline);
