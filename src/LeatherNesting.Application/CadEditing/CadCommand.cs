using LeatherNesting.Geometry;

namespace LeatherNesting.Application.CadEditing;

/// <summary>Base class for CAD editing commands. Each command represents one undoable operation.</summary>
public abstract record CadCommand
{
    public string CommandId { get; } = Guid.NewGuid().ToString("N")[..8];
    public string Description { get; }
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

    protected CadCommand(string description)
    {
        Description = description;
    }

    /// <summary>Executes the command and returns the new state.</summary>
    public abstract CadCommandResult Execute(CadCommandContext context);

    /// <summary>Reverses the command and returns the previous state.</summary>
    public abstract CadCommandResult Undo(CadCommandContext context);

    /// <summary>Re-applies the command after an undo.</summary>
    public virtual CadCommandResult Redo(CadCommandContext context) => Execute(context);
}

/// <summary>Context provided to commands for execution.</summary>
public sealed record CadCommandContext
{
    public IReadOnlyList<Loop2D> CurrentLoops { get; init; } = [];

    public CadCommandContext With(IReadOnlyList<Loop2D> loops) => this with { CurrentLoops = loops };
}

/// <summary>Result of executing or reversing a command.</summary>
public sealed record CadCommandResult
{
    public IReadOnlyList<Loop2D> ResultLoops { get; }
    public IReadOnlyList<string> Diagnostics { get; }
    public bool Success { get; }

    public CadCommandResult(IReadOnlyList<Loop2D> resultLoops, IReadOnlyList<string>? diagnostics = null)
    {
        ResultLoops = resultLoops;
        Diagnostics = diagnostics ?? [];
        Success = Diagnostics.Count == 0;
    }

    public static CadCommandResult Failed(IReadOnlyList<string> diagnostics) =>
        new([], diagnostics);
}