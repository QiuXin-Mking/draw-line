namespace LeatherNesting.Application.CadEditing;

/// <summary>Manages the undo/redo stack for CAD editing commands.</summary>
public sealed class CadCommandTransaction
{
    private readonly List<CadCommand> _undoStack = [];
    private readonly List<CadCommand> _redoStack = [];
    private readonly int _maxUndoDepth;

    public CadCommandTransaction(int maxUndoDepth = 100)
    {
        _maxUndoDepth = maxUndoDepth;
    }

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;

    /// <summary>Commits a command to the undo stack, clearing the redo stack.</summary>
    public CadCommandResult Commit(CadCommand command, CadCommandContext context)
    {
        var result = command.Execute(context);
        if (!result.Success) return result;

        _undoStack.Add(command);
        _redoStack.Clear();

        if (_undoStack.Count > _maxUndoDepth)
            _undoStack.RemoveAt(0);

        return result;
    }

    /// <summary>Undoes the last command and returns the restored state.</summary>
    public (CadCommandResult Result, CadCommand? Command) Undo(CadCommandContext context)
    {
        if (_undoStack.Count == 0)
            return (CadCommandResult.Failed(["没有可撤销的操作。"]), null);

        var command = _undoStack[^1];
        _undoStack.RemoveAt(_undoStack.Count - 1);

        var result = command.Undo(context);
        if (result.Success)
            _redoStack.Add(command);

        return (result, command);
    }

    /// <summary>Redoes the last undone command.</summary>
    public (CadCommandResult Result, CadCommand? Command) Redo(CadCommandContext context)
    {
        if (_redoStack.Count == 0)
            return (CadCommandResult.Failed(["没有可重做的操作。"]), null);

        var command = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);

        var result = command.Redo(context);
        if (result.Success)
            _undoStack.Add(command);

        return (result, command);
    }

    /// <summary>Clears both undo and redo stacks.</summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}