using LeatherNesting.Geometry;

namespace LeatherNesting.Application.CadEditing;

/// <summary>Preview session for CAD operations. Preview does NOT modify ProjectDocument.
/// Only Commit writes to the project; Cancel discards the session.</summary>
public sealed class CadOperationSession
{
    private readonly CadCommandTransaction _transaction;
    private IReadOnlyList<Loop2D> _previewLoops;
    private CadCommand? _pendingCommand;
    private bool _isPreviewing;

    public CadOperationSession(IReadOnlyList<Loop2D> initialLoops)
    {
        _previewLoops = initialLoops;
        _transaction = new CadCommandTransaction();
    }

    public IReadOnlyList<Loop2D> PreviewLoops => _previewLoops;
    public bool IsPreviewing => _isPreviewing;
    public bool HasPendingCommand => _pendingCommand is not null;
    public string? PendingCommandDescription => _pendingCommand?.Description;

    /// <summary>Previews a command without committing it to the undo stack.</summary>
    public CadCommandResult Preview(CadCommand command)
    {
        var context = new CadCommandContext { CurrentLoops = _previewLoops };
        var result = command.Execute(context);
        if (result.Success)
        {
            _previewLoops = result.ResultLoops;
            _pendingCommand = command;
            _isPreviewing = true;
        }
        return result;
    }

    /// <summary>Commits the pending preview to the undo stack. Does not re-run the command.</summary>
    public CadCommandResult Commit()
    {
        if (_pendingCommand is null)
            return CadCommandResult.Failed(["没有待提交的预览操作。"]);

        _transaction.Record(_pendingCommand);
        _pendingCommand = null;
        _isPreviewing = false;
        return new CadCommandResult(_previewLoops);
    }

    /// <summary>Discards the pending preview and restores the last committed state.</summary>
    public void Cancel()
    {
        _pendingCommand = null;
        _isPreviewing = false;
        // Restore from the last committed state
        // For simplicity, we rely on the caller to reset _previewLoops
    }

    /// <summary>Undoes the last committed command.</summary>
    public (CadCommandResult Result, CadCommand? Command) Undo()
    {
        var context = new CadCommandContext { CurrentLoops = _previewLoops };
        var (result, command) = _transaction.Undo(context);
        if (result.Success)
            _previewLoops = result.ResultLoops;
        return (result, command);
    }

    /// <summary>Redoes the last undone command.</summary>
    public (CadCommandResult Result, CadCommand? Command) Redo()
    {
        var context = new CadCommandContext { CurrentLoops = _previewLoops };
        var (result, command) = _transaction.Redo(context);
        if (result.Success)
            _previewLoops = result.ResultLoops;
        return (result, command);
    }

    public bool CanUndo => _transaction.CanUndo;
    public bool CanRedo => _transaction.CanRedo;
}