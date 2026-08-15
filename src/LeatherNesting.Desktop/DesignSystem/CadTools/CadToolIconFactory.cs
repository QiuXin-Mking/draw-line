using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

namespace LeatherNesting.Desktop.DesignSystem.CadTools;

/// <summary>Creates fresh vector artwork for every registered CAD toolbar icon.</summary>
public static class CadToolIconFactory
{
    public static Control Create(CadToolIconKey key)
    {
        if (CadToolIconGroupA.TryCreate(key, out var icon) ||
            CadToolIconGroupB.TryCreate(key, out icon) ||
            CadToolIconGroupC.TryCreate(key, out icon) ||
            CadToolIconGroupD.TryCreate(key, out icon) ||
            CadToolIconGroupE.TryCreate(key, out icon))
        {
            return icon!;
        }

        throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown CAD tool icon key.");
    }
}
