using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.ProcessFeatures;

/// <summary>M05 presentation for existing process features and the independent grading-rule library.</summary>
public sealed class ProcessFeaturesView : UserControl
{
    private readonly ProcessFeaturesViewModel _viewModel = new();
    private readonly TextBlock _todoMessage = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };

    public ProcessFeaturesView()
    {
        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    Section("工艺特征", FeatureList()),
                    Section("普通剪口", NotchDetail()),
                    Section("剪口验证", NotchValidation()),
                    Section("码齿规则库 · v1.0", GradingLibrary()),
                    Section("尺码预览", SizePreview()),
                    Section("操作", Actions()),
                },
            },
        };
    }

    private static Control Section(string title, Control content) => new StackPanel
    {
        Spacing = 8,
        Children = { new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeight.Bold }, content },
    };

    private Control FeatureList()
    {
        var list = new StackPanel { Spacing = 4 };
        foreach (var feature in _viewModel.Features)
            list.Children.Add(Row(feature.Kind, $"{feature.Name} · {feature.Detail} · {feature.Tool}"));
        return Card(list);
    }

    private Control NotchDetail() => Card(new StackPanel
    {
        Spacing = 4,
        Children =
        {
            Row("形状", "V 型"),
            Row("宽度", "2.0 mm"),
            Row("深度", "0.8 mm"),
            Row("材料侧", "外侧"),
            Row("输出", "CUT"),
        },
    });

    private Control NotchValidation()
    {
        var validation = _viewModel.NotchValidation;
        var details = validation.Errors.Concat(validation.Warnings).DefaultIfEmpty("无诊断信息。");
        return Card(new StackPanel
        {
            Spacing = 4,
            Children =
            {
                Row("结果", validation.IsValid ? "通过" : "未通过"),
                new TextBlock { Text = string.Join("\n", details), Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap },
            },
        });
    }

    private Control GradingLibrary()
    {
        var list = new StackPanel { Spacing = 4 };
        list.Children.Add(Row("码数", "方形 · 半圆 · 尖角 · 半码"));
        foreach (var rule in _viewModel.GradingRules)
            list.Children.Add(Row(rule.Size.ToString("F1"), $"{rule.SquareCount} · {rule.HalfCircleCount} · {rule.PointCount} · {rule.HalfSizeCount}"));
        return Card(list);
    }

    private static Control SizePreview() => Card(new TextBlock
    {
        Text = "当前尺码：31.0 · 预览规则：方形 0 / 半圆 0 / 尖角 1 / 半码 0",
        TextWrapping = TextWrapping.Wrap,
    });

    private Control Actions()
    {
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        buttons.Children.Add(ActionButton("创建特征", _viewModel.CreateFeature));
        buttons.Children.Add(ActionButton("保存特征", _viewModel.SaveFeature));
        buttons.Children.Add(ActionButton("生成码齿", _viewModel.GenerateGrading));
        buttons.Children.Add(ActionButton("刀具映射", _viewModel.MapTool));
        return new StackPanel { Spacing = 8, Children = { buttons, new TodoBadge(), _todoMessage } };
    }

    private Button ActionButton(string label, Action action)
    {
        var button = new Button { Content = label };
        button.Click += (_, _) => { action(); _todoMessage.Text = _viewModel.TodoMessage; };
        return button;
    }

    private static Control Card(Control child) => new Border
    {
        Background = AppTheme.Surface,
        BorderBrush = AppTheme.SurfaceBorder,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(4),
        Padding = new Thickness(12),
        Child = child,
    };

    private static Control Row(string label, string value) => new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Spacing = 8,
        Children =
        {
            new TextBlock { Text = label, MinWidth = 90, Foreground = AppTheme.TextMuted },
            new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap },
        },
    };
}
