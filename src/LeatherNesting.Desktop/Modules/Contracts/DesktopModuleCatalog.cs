namespace LeatherNesting.Desktop.Modules.Contracts;

/// <summary>Validates and presents module definitions in navigation order.</summary>
public static class DesktopModuleCatalog
{
    public static IReadOnlyList<IDesktopModule> CreateValidated(IEnumerable<IDesktopModule> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);

        var orderedModules = modules.OrderBy(module => module.Metadata.Order).ToArray();
        var duplicateId = orderedModules
            .GroupBy(module => module.Metadata.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateId is not null)
        {
            throw new InvalidOperationException($"Desktop module ID '{duplicateId.Key}' is registered more than once.");
        }

        return Array.AsReadOnly(orderedModules);
    }
}
