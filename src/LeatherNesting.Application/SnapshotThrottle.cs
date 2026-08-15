namespace LeatherNesting.Application;

/// <summary>Accumulates operations and flushes every N operations.</summary>
public sealed class SnapshotThrottle
{
    private readonly int _threshold;
    private int _count;

    public SnapshotThrottle(int threshold)
    {
        if (threshold <= 0)
            throw new ArgumentOutOfRangeException(nameof(threshold), "阈值必须为正整数。");
        _threshold = threshold;
    }

    /// <summary>Records one operation; returns true when the threshold is reached (and resets the count).</summary>
    public bool ShouldFlush()
    {
        _count++;
        if (_count < _threshold)
            return false;
        _count = 0;
        return true;
    }
}
