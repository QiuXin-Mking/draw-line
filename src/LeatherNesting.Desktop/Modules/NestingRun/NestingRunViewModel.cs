namespace LeatherNesting.Desktop.Modules.NestingRun;

public enum NestingRunState
{
    Ready,
    Preparing,
    Running,
    BestImproved,
    Completed,
    StoppedWithBest,
    Cancelled,
}

public sealed record NestingStrategySettings(
    string Preset,
    int TimeBudgetMinutes,
    string AllowedAngles,
    string PlacementOrder,
    int Seed,
    bool FillSmallPieces,
    string Disclaimer);

public sealed record NestingDemoMetrics(
    double UtilizationPercent,
    int PlacedPieces,
    int UnplacedPieces,
    int MaterialSheets,
    int CandidateCount,
    string Elapsed);

public sealed record NestingTimelineEntry(string State, string Detail);

/// <summary>
/// M08's deterministic presentation state. No timer, optimizer, cancellation token, project write,
/// or production placement is used; commands only advance an explicitly labelled demo timeline.
/// </summary>
public sealed class NestingRunViewModel
{
    public const string SimulatedStatus = "TODO · 模拟状态";
    public const string ProductionWarning = "DEMO · 未运行自动排样算法，不可用于生产";

    private static readonly NestingStrategySettings[] StrategyPresets =
    [
        new("快速", 2, "0° / 180°", "优先级 → 面积", 42, false, "TODO · 策略参数仅用于模拟状态，未连接自动排样"),
        new("均衡", 5, "0° / 90° / 180° / 270°", "面积 → 优先级", 2026, true, "TODO · 策略参数仅用于模拟状态，未连接自动排样"),
        new("精细", 15, "0°--359°", "大件优先 → 小件填空", 731, true, "TODO · 策略参数仅用于模拟状态，未连接自动排样"),
    ];

    private readonly List<NestingTimelineEntry> _timeline = [];

    public NestingRunViewModel()
    {
        Settings = StrategyPresets[1];
        RunStartMetrics = EmptyMetrics;
        BestMetrics = EmptyMetrics;
        Feedback = $"已就绪；{SimulatedStatus}。";
        OutcomeSummary = ProductionWarning;
        AddTimeline("准备", "等待用户启动演示");
    }

    public IReadOnlyList<string> PresetNames => StrategyPresets.Select(preset => preset.Preset).ToArray();

    public NestingStrategySettings Settings { get; private set; }

    public NestingRunState State { get; private set; } = NestingRunState.Ready;

    public NestingDemoMetrics RunStartMetrics { get; private set; }

    public NestingDemoMetrics BestMetrics { get; private set; }

    public IReadOnlyList<NestingTimelineEntry> Timeline => _timeline;

    public string Feedback { get; private set; }

    public string OutcomeSummary { get; private set; }

    public string StateLabel => State switch
    {
        NestingRunState.Ready => "准备",
        NestingRunState.Preparing => "准备中",
        NestingRunState.Running => "运行",
        NestingRunState.BestImproved => "发现更优",
        NestingRunState.Completed => "完成",
        NestingRunState.StoppedWithBest => "已停止 · 保留最佳",
        NestingRunState.Cancelled => "已取消 · 已回滚",
        _ => throw new InvalidOperationException($"Unsupported nesting demo state: {State}."),
    };

    public bool SelectPreset(string presetName)
    {
        if (State is not NestingRunState.Ready)
            return Reject("仅准备状态可以更改策略预设");

        var preset = StrategyPresets.SingleOrDefault(candidate => candidate.Preset == presetName);
        if (preset is null)
            return Reject($"未知策略预设“{presetName}”");

        Settings = preset;
        Feedback = $"已选择“{preset.Preset}”；{preset.Disclaimer}。";
        return true;
    }

    public bool UpdateSettings(
        string? timeBudgetMinutes,
        string allowedAngles,
        string placementOrder,
        string? seed,
        bool fillSmallPieces)
    {
        if (State is not NestingRunState.Ready)
            return Reject("仅准备状态可以编辑策略设置");

        if (!int.TryParse(timeBudgetMinutes, out var parsedBudget) || parsedBudget <= 0)
            return Reject("时间预算必须是大于 0 的整数分钟");
        if (!int.TryParse(seed, out var parsedSeed) || parsedSeed < 0)
            return Reject("种子必须是大于或等于 0 的整数");
        if (string.IsNullOrWhiteSpace(allowedAngles) || string.IsNullOrWhiteSpace(placementOrder))
            return Reject("允许角度和排放顺序必须选择");

        Settings = Settings with
        {
            TimeBudgetMinutes = parsedBudget,
            AllowedAngles = allowedAngles,
            PlacementOrder = placementOrder,
            Seed = parsedSeed,
            FillSmallPieces = fillSmallPieces,
        };
        Feedback = $"策略仅更新在内存 DEMO；{SimulatedStatus}，未写入项目或算法。";
        return true;
    }

    public bool Prepare()
    {
        if (State is not NestingRunState.Ready)
            return Reject("当前状态不能再次准备");

        RunStartMetrics = EmptyMetrics;
        BestMetrics = RunStartMetrics;
        State = NestingRunState.Preparing;
        OutcomeSummary = ProductionWarning;
        Feedback = $"正在准备演示输入；{SimulatedStatus}。";
        AddTimeline("准备中", "输入检查、算法准备和真实计时均未接入");
        return true;
    }

    public bool Start()
    {
        if (State is not NestingRunState.Preparing)
            return Reject("只有准备中状态可以开始运行");

        State = NestingRunState.Running;
        Feedback = $"模拟运行已开始；{SimulatedStatus}。";
        AddTimeline("运行", "进度、已用时间、候选搜索和结果均为固定演示值");
        return true;
    }

    public bool ReportBetterDemoResult()
    {
        if (State is not NestingRunState.Running)
            return Reject("只有运行状态可以演示发现更优");

        BestMetrics = new NestingDemoMetrics(84.6, 118, 6, 2, 384, "00:01:42 · 模拟");
        State = NestingRunState.BestImproved;
        Feedback = $"发现更优演示指标；{SimulatedStatus}。";
        AddTimeline("发现更优", "固定 DEMO 指标，不代表有效、最优或已验证的排样方案");
        return true;
    }

    public bool ResumeDemoRun()
    {
        if (State is not NestingRunState.BestImproved)
            return Reject("只有发现更优状态可以继续模拟运行");

        State = NestingRunState.Running;
        Feedback = $"继续模拟改善；{SimulatedStatus}。";
        AddTimeline("运行", "从固定演示最佳值继续；没有后台任务或真实计时");
        return true;
    }

    public bool Complete()
    {
        if (State is not (NestingRunState.Running or NestingRunState.BestImproved))
            return Reject("当前状态不能完成");

        EnsureDemoBestExists();
        State = NestingRunState.Completed;
        OutcomeSummary = $"模拟流程完成；未校验、未写入方案。{ProductionWarning}。";
        Feedback = $"已完成演示；{SimulatedStatus}。";
        AddTimeline("完成", "方案写入、完整校验和生产结果均未接入");
        return true;
    }

    public bool Stop()
    {
        if (State is not (NestingRunState.Running or NestingRunState.BestImproved))
            return Reject("当前状态不能停止");

        EnsureDemoBestExists();
        State = NestingRunState.StoppedWithBest;
        OutcomeSummary = $"停止语义：保留当前最佳的完整 DEMO 指标；未写入方案。{ProductionWarning}。";
        Feedback = $"模拟运行已停止；{SimulatedStatus}。";
        AddTimeline("停止", "仅保留当前最佳 DEMO 指标；真实安全停止和保存未接入");
        return true;
    }

    public bool Cancel()
    {
        if (State is not (NestingRunState.Preparing or NestingRunState.Running or NestingRunState.BestImproved))
            return Reject("当前状态不能取消");

        BestMetrics = RunStartMetrics;
        State = NestingRunState.Cancelled;
        OutcomeSummary = $"取消语义：回滚本次临时 DEMO 指标，未写入方案。{ProductionWarning}。";
        Feedback = $"模拟运行已取消；{SimulatedStatus}。";
        AddTimeline("取消", "恢复运行前 DEMO 快照；真实取消响应和项目回滚未接入");
        return true;
    }

    public bool Reset()
    {
        if (State is not (NestingRunState.Completed or NestingRunState.StoppedWithBest or NestingRunState.Cancelled))
            return Reject("只有结束状态可以重置");

        State = NestingRunState.Ready;
        RunStartMetrics = EmptyMetrics;
        BestMetrics = EmptyMetrics;
        OutcomeSummary = ProductionWarning;
        Feedback = $"已重置演示；{SimulatedStatus}。";
        _timeline.Clear();
        AddTimeline("准备", "等待用户启动演示");
        return true;
    }

    private static NestingDemoMetrics EmptyMetrics => new(0, 0, 124, 0, 0, "00:00:00 · 模拟");

    private void EnsureDemoBestExists()
    {
        if (BestMetrics.UtilizationPercent == 0)
            BestMetrics = new NestingDemoMetrics(78.2, 110, 14, 2, 216, "00:00:58 · 模拟");
    }

    private bool Reject(string reason)
    {
        Feedback = $"{reason}；不能执行。{SimulatedStatus}。";
        return false;
    }

    private void AddTimeline(string state, string detail) =>
        _timeline.Add(new NestingTimelineEntry(state, $"{detail} · {SimulatedStatus}"));
}
