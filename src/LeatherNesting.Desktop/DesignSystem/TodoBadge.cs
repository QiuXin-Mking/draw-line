using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace LeatherNesting.Desktop.DesignSystem;

/// <summary>Textual TODO marker. Standard copy: "TODO · 演示占位，未接入实际逻辑".</summary>
public sealed class TodoBadge : Border
{
    public const string StandardText = "TODO · 演示占位，未接入实际逻辑";

    public TodoBadge(string? text = null)
    {
        Background = new SolidColorBrush(Color.FromRgb(0xFD, 0xF3, 0xE2));
        BorderBrush = AppTheme.TodoAmber;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(3);
        Padding = new Thickness(8, 3);
        HorizontalAlignment = HorizontalAlignment.Left;
        Child = new TextBlock
        {
            Text = text ?? StandardText,
            Foreground = AppTheme.TodoAmber,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
        };
    }
}
