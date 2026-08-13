namespace LeatherNesting.Desktop.Modules.Administration;

public enum PresetCategory
{
    Material,
    Strategy,
    Export,
    Layer,
    Process,
}

public enum AuditCategory
{
    All,
    Rule,
    Permission,
    Setting,
    Export,
}

public enum RoleKind
{
    Operator,
    ProcessEngineer,
    Reviewer,
    Administrator,
}

public sealed record PresetVersion(string Version, string PublishedAt, string Author, string Summary);

public sealed record PresetLibraryItem(
    string Id,
    string Name,
    PresetCategory Category,
    PresetVersion ProjectSnapshot,
    PresetVersion Latest,
    IReadOnlyList<PresetVersion> Versions)
{
    public bool HasNewerVersion => !string.Equals(ProjectSnapshot.Version, Latest.Version, StringComparison.Ordinal);
}

public sealed record AuditEvent(
    string Time,
    string Actor,
    AuditCategory Category,
    string Action,
    string Target,
    string Result);

public sealed record RolePermissionRow(
    string Capability,
    bool Operator,
    bool ProcessEngineer,
    bool Reviewer,
    bool Administrator)
{
    public bool IsAllowed(RoleKind role) => role switch
    {
        RoleKind.Operator => Operator,
        RoleKind.ProcessEngineer => ProcessEngineer,
        RoleKind.Reviewer => Reviewer,
        RoleKind.Administrator => Administrator,
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };
}

public sealed record AdministrationSettings(
    string Unit,
    string Tolerance,
    bool AutoSave,
    string Theme,
    string LogLevel);

/// <summary>M12 deterministic, in-memory presentation state. It does not authenticate, write rules, logs, or configuration.</summary>
public sealed class AdministrationViewModel
{
    public const string TodoInventory =
        "TODO · 规则写入、权限认证、日志落盘、配置持久化、外部适配器注册均未接入实际逻辑";

    private static readonly PresetLibraryItem[] PresetItems =
    [
        CreatePreset("RULE-MAT-01", "牛皮通用材料", PresetCategory.Material, "v2.3", "v2.5", "边距 12 mm · 间距 8 mm"),
        CreatePreset("RULE-NEST-02", "均衡排样策略", PresetCategory.Strategy, "v4.0", "v4.2", "90° 步进 · 大件优先"),
        CreatePreset("RULE-EXP-03", "生产 DXF 导出", PresetCategory.Export, "v1.8", "v1.8", "毫米 · 左下原点"),
        CreatePreset("RULE-LAYER-04", "供应商 A 图层映射", PresetCategory.Layer, "v3.1", "v3.4", "CUT / NOTCH / MARK"),
        CreatePreset("RULE-PROC-05", "标准剪口工艺", PresetCategory.Process, "v5.6", "v6.0", "V 型 · 3×2 mm"),
    ];

    private static readonly AuditEvent[] AuditEntries =
    [
        new("2026-08-13 09:42", "陈工", AuditCategory.Rule, "发布版本", "标准剪口工艺 v6.0", "DEMO · 待真实日志"),
        new("2026-08-13 09:16", "管理员", AuditCategory.Permission, "查看角色矩阵", "工艺工程师", "DEMO · 未认证"),
        new("2026-08-12 17:35", "王师傅", AuditCategory.Export, "查看导出预设", "生产 DXF 导出 v1.8", "DEMO · 未落盘"),
        new("2026-08-12 15:08", "管理员", AuditCategory.Setting, "调整日志级别", "常规 → 诊断", "DEMO · 未持久化"),
        new("2026-08-11 10:20", "李工", AuditCategory.Rule, "比较项目快照", "牛皮通用材料 v2.3 / v2.5", "DEMO · 只读"),
    ];

    private static readonly RolePermissionRow[] PermissionRows =
    [
        new("查看规则与版本", true, true, true, true),
        new("编辑 / 发布规则", false, true, false, true),
        new("查看审计", false, true, true, true),
        new("审批规则版本", false, false, true, true),
        new("管理角色权限", false, false, false, true),
        new("注册外部适配器", false, false, false, true),
        new("修改系统设置", false, false, false, true),
    ];

    public AdministrationViewModel()
    {
        SelectedPreset = PresetItems[0];
        Settings = new AdministrationSettings("mm", "0.10", true, "跟随系统", "常规");
    }

    public IReadOnlyList<PresetLibraryItem> Presets => PresetItems;

    public PresetLibraryItem SelectedPreset { get; private set; }

    public string PresetComparison => SelectedPreset.HasNewerVersion
        ? $"项目快照 {SelectedPreset.ProjectSnapshot.Version} · {SelectedPreset.ProjectSnapshot.Summary}\n最新预设 {SelectedPreset.Latest.Version} · {SelectedPreset.Latest.Summary}\n旧项目继续引用快照，不会自动升级。"
        : $"项目快照 {SelectedPreset.ProjectSnapshot.Version} 与最新预设一致。";

    public IReadOnlyList<AuditEvent> AuditEvents => AuditEntries;

    public AuditCategory AuditFilter { get; private set; } = AuditCategory.All;

    public IReadOnlyList<AuditEvent> FilteredAuditEvents => AuditFilter == AuditCategory.All
        ? AuditEntries
        : AuditEntries.Where(entry => entry.Category == AuditFilter).ToArray();

    public IReadOnlyList<RolePermissionRow> Permissions => PermissionRows;

    public RoleKind SelectedRole { get; private set; } = RoleKind.Administrator;

    public bool CanEditRules => SelectedRole is RoleKind.ProcessEngineer or RoleKind.Administrator;

    public bool CanManageAdapters => SelectedRole == RoleKind.Administrator;

    public string RuleActionExplanation => CanEditRules
        ? $"当前演示角色“{RoleLabel(SelectedRole)}”具有规则编辑矩阵权限；但{TodoInventory}。"
        : $"权限不足：当前演示角色“{RoleLabel(SelectedRole)}”不能编辑规则，需工艺工程师或管理员。权限认证未接入。";

    public string AdapterActionExplanation => CanManageAdapters
        ? $"当前演示角色“管理员”具有矩阵权限；TODO · 外部适配器注册未接入实际逻辑。"
        : $"权限不足：当前演示角色“{RoleLabel(SelectedRole)}”不能注册适配器，需管理员。权限认证未接入。";

    public AdministrationSettings Settings { get; private set; }

    public string SettingsFeedback { get; private set; } = "TODO · 设置仅供展示，尚未编辑或持久化。";

    public string ActionFeedback { get; private set; } = TodoInventory;

    public string UnimplementedCapabilities => TodoInventory;

    public void SelectPreset(string presetId)
    {
        SelectedPreset = PresetItems.Single(item => item.Id == presetId);
        ActionFeedback = $"已切换只读演示预设“{SelectedPreset.Name}”；TODO · 规则写入未接入。";
    }

    public void SetAuditFilter(AuditCategory category) => AuditFilter = category;

    public void SelectRole(RoleKind role)
    {
        SelectedRole = role;
        ActionFeedback = $"演示角色已在内存中切换为“{RoleLabel(role)}”；TODO · 权限认证与角色持久化未接入。";
    }

    public void UpdateSettings(string unit, string tolerance, bool autoSave, string theme, string logLevel)
    {
        Settings = new AdministrationSettings(unit, tolerance, autoSave, theme, logLevel);
        SettingsFeedback = "设置已仅在本模块内存 DEMO 中更新；TODO · 配置持久化、主题应用、自动保存和日志落盘均未接入。";
    }

    public void RequestRuleWrite() => ActionFeedback = CanEditRules
        ? "TODO · 规则写入未接入；未发布、未修改任何项目或预设。"
        : RuleActionExplanation;

    public void RequestAdapterRegistration() => ActionFeedback = CanManageAdapters
        ? "TODO · 外部适配器注册未接入；未加载或写入任何适配器。"
        : AdapterActionExplanation;

    public static string RoleLabel(RoleKind role) => role switch
    {
        RoleKind.Operator => "操作员",
        RoleKind.ProcessEngineer => "工艺工程师",
        RoleKind.Reviewer => "审核员",
        RoleKind.Administrator => "管理员",
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };

    public static string CategoryLabel(PresetCategory category) => category switch
    {
        PresetCategory.Material => "材料",
        PresetCategory.Strategy => "策略",
        PresetCategory.Export => "导出",
        PresetCategory.Layer => "图层",
        PresetCategory.Process => "工艺",
        _ => throw new ArgumentOutOfRangeException(nameof(category)),
    };

    public static string AuditCategoryLabel(AuditCategory category) => category switch
    {
        AuditCategory.All => "全部",
        AuditCategory.Rule => "规则",
        AuditCategory.Permission => "权限",
        AuditCategory.Setting => "设置",
        AuditCategory.Export => "导出",
        _ => throw new ArgumentOutOfRangeException(nameof(category)),
    };

    private static PresetLibraryItem CreatePreset(
        string id,
        string name,
        PresetCategory category,
        string snapshotVersion,
        string latestVersion,
        string summary)
    {
        var snapshot = new PresetVersion(snapshotVersion, "2026-07-18", "李工", $"{summary} · 项目冻结值");
        var latest = new PresetVersion(latestVersion, "2026-08-12", "陈工", $"{summary} · 当前库版本");
        return new PresetLibraryItem(
            id,
            name,
            category,
            snapshot,
            latest,
            snapshotVersion == latestVersion ? [latest] : [latest, snapshot]);
    }
}
