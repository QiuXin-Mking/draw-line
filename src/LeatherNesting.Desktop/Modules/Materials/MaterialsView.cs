using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Materials;

/// <summary>M07 material bill and nesting-constraint demonstration page.</summary>
public sealed class MaterialsView : UserControl
{
    private readonly MaterialsViewModel _viewModel = new();
    private readonly StackPanel _detail = new() { Spacing = 10 };
    private readonly TextBlock _todo = new() { Foreground = AppTheme.TodoAmber, TextWrapping = TextWrapping.Wrap };

    public MaterialsView()
    {
        Content = new ScrollViewer { Content = new StackPanel { Margin = new Thickness(24), Spacing = 16, Children =
        {
            new TextBlock { Text = "材料、料单与约束", FontSize = 22, FontWeight = FontWeight.Bold },
            new TextBlock { Text = _viewModel.Summary, Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap },
            new TodoBadge("TODO · 材料持久化、真实可用区、真皮边界/瑕疵与真实面积计算均未接入"),
            Section("多材料料单（DEMO）", BuildMaterialList()),
            Section("材料参数与约束", _detail),
            Section("演示统计", new TextBlock { Text = "DEMO · 面积、利用率和卷料用长为演示估算，不可用于生产。", Foreground = AppTheme.TextMuted, TextWrapping = TextWrapping.Wrap }),
        }}};
        RefreshDetail();
    }

    private Control BuildMaterialList()
    {
        var list = new StackPanel { Spacing = 6 };
        foreach (var material in _viewModel.Materials)
        {
            var button = new Button { HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Left };
            button.Content = $"{material.Id} · {material.Name} · {(material.Kind == MaterialKind.Sheet ? "片料" : "卷料")} · {material.EstimateLabel}: {material.DemoEstimate}";
            button.Click += (_, _) => { _viewModel.Select(material.Id); RefreshDetail(); };
            list.Children.Add(button);
        }
        return list;
    }

    private void RefreshDetail()
    {
        _detail.Children.Clear();
        var material = _viewModel.Selected;
        _detail.Children.Add(new TextBlock { Text = $"{material.Name} · {(material.Kind == MaterialKind.Sheet ? "片料：宽×长" : "卷料：固定可用宽，按用长计")}", FontWeight = FontWeight.Bold });

        var width = Field("宽度 (mm)", material.WidthMm.ToString(CultureInfo.InvariantCulture));
        TextBox? length = null;
        if (material.Kind == MaterialKind.Sheet)
            length = Field("长度 (mm)", material.LengthMm!.Value.ToString(CultureInfo.InvariantCulture));
        else
            _detail.Children.Add(ReadOnlyRow("长度", "卷料不设定总长；下方用长为 DEMO 估算"));
        var layers = Field("层数", material.Layers.ToString(CultureInfo.InvariantCulture));
        _detail.Children.Add(ReadOnlyRow("边缘 / 间距", $"{material.EdgeMm} mm / {material.SpacingMm} mm"));
        _detail.Children.Add(ReadOnlyRow("允许方向", material.Direction));
        _detail.Children.Add(ReadOnlyRow("可用区", material.UsableArea));
        _detail.Children.Add(ReadOnlyRow(material.EstimateLabel, $"DEMO · {material.DemoEstimate}"));

        var save = new Button { Content = "更新演示参数", HorizontalAlignment = HorizontalAlignment.Left };
        save.Click += (_, _) =>
        {
            _viewModel.UpdateSelected(width.Text, length?.Text, layers.Text);
            RefreshDetail();
        };
        _detail.Children.Add(save);
        _todo.Text = _viewModel.TodoMessage;
        _detail.Children.Add(_todo);
        AddError(_viewModel.WidthError);
        AddError(_viewModel.LengthError);
        AddError(_viewModel.LayerError);
    }

    private TextBox Field(string label, string value)
    {
        _detail.Children.Add(new TextBlock { Text = label, Foreground = AppTheme.TextMuted });
        var field = new TextBox { Text = value, Width = 180 };
        _detail.Children.Add(field);
        return field;
    }

    private void AddError(string? error)
    {
        if (error is not null)
            _detail.Children.Add(new TextBlock { Text = error, Foreground = Brushes.IndianRed });
    }

    private static Control Section(string title, Control content) => new StackPanel { Spacing = 8, Children =
    {
        new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeight.Bold }, content,
    }};

    private static Control ReadOnlyRow(string label, string value) => new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children =
    {
        new TextBlock { Text = label, MinWidth = 100, Foreground = AppTheme.TextMuted }, new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap },
    }};
}
