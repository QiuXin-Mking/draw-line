namespace LeatherNesting.Application.CadEditing;

/// <summary>Records committed commands for crash recovery. Never records preview frames.</summary>
public sealed class CrashRecoveryLog
{
    private readonly string _logPath;
    private readonly object _lock = new();

    public CrashRecoveryLog(string logPath)
    {
        _logPath = logPath;
    }

    /// <summary>Appends a committed command to the recovery log.</summary>
    public void RecordCommit(CadCommand command)
    {
        lock (_lock)
        {
            File.AppendAllText(_logPath,
                $"{command.Timestamp:O}\t{command.CommandId}\t{command.Description}\n");
        }
    }

    /// <summary>Reads the recovery log entries.</summary>
    public IReadOnlyList<RecoveryEntry> ReadEntries()
    {
        lock (_lock)
        {
            if (!File.Exists(_logPath)) return [];
            return File.ReadAllLines(_logPath)
                .Select(line => line.Split('\t'))
                .Where(parts => parts.Length >= 3)
                .Select(parts => new RecoveryEntry(
                    DateTimeOffset.Parse(parts[0]),
                    parts[1],
                    parts[2]))
                .ToList();
        }
    }

    /// <summary>Clears the recovery log.</summary>
    public void Clear()
    {
        lock (_lock)
        {
            if (File.Exists(_logPath))
                File.Delete(_logPath);
        }
    }

    public sealed record RecoveryEntry(
        DateTimeOffset Timestamp,
        string CommandId,
        string Description);
}