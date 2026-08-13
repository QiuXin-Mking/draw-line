namespace LeatherNesting.Desktop.Modules.Contracts;

/// <summary>Immutable navigation metadata declared by a desktop module.</summary>
public sealed class DesktopModuleMetadata
{
    public DesktopModuleMetadata(string id, string title, string group, int order)
    {
        Id = id;
        Title = title;
        Group = group;
        Order = order;
    }

    public string Id { get; }

    public string Title { get; }

    public string Group { get; }

    public int Order { get; }
}
