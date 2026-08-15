using LeatherNesting.Geometry;

namespace LeatherNesting.Application.Domain;

/// <summary>A material sheet: identity + geometry boundary.</summary>
public sealed record Material(string Id, string Name, Loop2D Boundary);
