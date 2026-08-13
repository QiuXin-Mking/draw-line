using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules;

/// <summary>Placeholder page for a module whose real implementation is a later task.</summary>
public sealed class ModulePlaceholderView : UserControl
{
    public ModulePlaceholderView(string moduleId, string title)
    {
        Content = new StackPanel
        {
            Spacing = 12,
            Margin = new Thickness(24),
            Children =
            {
                new TextBlock { Text = title, FontSize = 20, FontWeight = FontWeight.Bold },
                new TextBlock { Text = $"{moduleId} 模块页面待实现。", Foreground = AppTheme.TextMuted },
                new TodoBadge(),
            },
        };
    }
}
