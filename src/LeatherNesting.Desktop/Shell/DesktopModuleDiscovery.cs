using System.Reflection;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Discovers concrete module definitions declared in the desktop assembly.</summary>
public static class DesktopModuleDiscovery
{
    public static IReadOnlyList<IDesktopModule> Discover(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var discovered = assembly.DefinedTypes
            .Where(type => !type.IsAbstract && typeof(IDesktopModule).IsAssignableFrom(type))
            .Where(type => type.GetConstructor(Type.EmptyTypes) is not null)
            .Select(type => (IDesktopModule)Activator.CreateInstance(type.AsType())!)
            .ToArray();

        return DesktopModuleCatalog.CreateValidated(discovered);
    }

    public static IReadOnlyList<IDesktopModule> CreateCatalog(Assembly assembly, IEnumerable<IDesktopModule> compatibilityModules)
    {
        ArgumentNullException.ThrowIfNull(compatibilityModules);

        var discovered = Discover(assembly);
        var compatibility = compatibilityModules
            .Where(module => discovered.All(found => !StringComparer.Ordinal.Equals(found.Metadata.Id, module.Metadata.Id)));
        return DesktopModuleCatalog.CreateValidated(discovered.Concat(compatibility));
    }
}
