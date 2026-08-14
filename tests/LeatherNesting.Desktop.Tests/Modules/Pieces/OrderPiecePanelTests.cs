using Avalonia.Controls;
using Avalonia.VisualTree;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.Pieces;
using LeatherNesting.Desktop.Shell;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.Pieces;

[Collection("Avalonia UI")]
public sealed class OrderPiecePanelTests
{
    [Fact]
    public void Image_27_scenario_exposes_six_visible_cards_in_evidenced_field_order()
    {
        var state = OrderPiecePanelState.CreateImage27Demo();

        Assert.Equal(10, state.GroupPieceCount);
        Assert.Equal(
            ["205*110", "173*129", "77*169", "172*75", "104*70", "104*96"],
            state.Pieces.Take(6).Select(piece => piece.BoundingDimensions));
        Assert.All(state.Pieces.Take(6), piece =>
        {
            Assert.Equal("40", piece.Size);
            Assert.Equal("任意角度", piece.Rotation);
            Assert.Equal(1, piece.SingleSetQuantity);
            Assert.Equal(1, piece.SetCount);
            Assert.Equal(100, piece.RemainderQuantity);
            Assert.Equal(100, piece.TotalQuantity);
        });

        Assert.Equal(
            [PieceCardField.Thumbnail, PieceCardField.Size, PieceCardField.BoundingDimensions,
             PieceCardField.Rotation, PieceCardField.Completion, PieceCardField.SingleSetQuantity,
             PieceCardField.SetCount, PieceCardField.RemainderQuantity, PieceCardField.TotalQuantity],
            PieceCardView.EvidencedFieldOrder);
    }

    [Fact]
    public void Shared_edits_update_the_shell_summary_without_claiming_persistence()
    {
        var state = OrderPiecePanelState.CreateImage27Demo();
        var first = state.Pieces[0];

        state.UpdateQuantities(first.Index, singleSetQuantity: 2, setCount: 100, remainderQuantity: 200);

        Assert.Equal(200, first.TotalQuantity);
        Assert.Equal(1100, state.GroupTotalQuantity);
        Assert.Equal("900/1100", state.GroupCountSummary);
        Assert.Contains("证据缺口", state.EvidenceGapNotice);
        Assert.Contains("DEMO", state.PersistenceNotice);
    }

    [Fact]
    public void Image_13_property_scenario_has_exact_column_order_defaults_and_focus_target()
    {
        var state = OrderPiecePanelState.CreateImage27Demo();
        state.LoadImage13PropertyDemo();
        var editor = new PiecePropertiesView(state);

        Assert.Equal(
            ["图形", "名称", "尺寸", "角度", "微动", "优先级", "小片插刀", "单套", "套数",
             "总量", "余量", "附加间距", "面积", "片料耗", "片超料%"],
            PiecePropertiesView.ColumnOrder);
        Assert.Equal(10, state.Pieces.Count);
        Assert.All(state.Pieces, piece => Assert.True(piece.IsIncluded));
        Assert.Equal(2, state.Pieces[0].SingleSetQuantity);
        Assert.All(state.Pieces.Skip(1), piece => Assert.Equal(1, piece.SingleSetQuantity));
        Assert.Equal(200, state.Pieces[0].TotalQuantity);
        Assert.Equal(200, state.Pieces[0].RemainderQuantity);
        Assert.Equal("单套", editor.InitialFocusField);
        Assert.Equal("2", editor.FirstSingleSetEditor.Text);
        Assert.Equal(AppTheme.Accent, editor.FirstSingleSetEditor.BorderBrush);
        Assert.Equal("46*,54*", editor.SplitRatio);
        Assert.True(editor.AdvancedPropertiesCheckBox.IsChecked);
        Assert.False(editor.SelectAllCheckBox.IsChecked);
    }

    [Fact]
    public void Property_batch_edit_writes_back_to_the_same_shell_records()
    {
        var state = OrderPiecePanelState.CreateImage27Demo();
        state.LoadImage13PropertyDemo();
        var editor = new PiecePropertiesView(state);

        editor.ApplyBatchQuantities(singleSetQuantity: 4, setCount: 100, remainderQuantity: 0);

        Assert.All(state.Pieces, piece =>
        {
            Assert.Equal(4, piece.SingleSetQuantity);
            Assert.Equal(100, piece.SetCount);
            Assert.Equal(400, piece.TotalQuantity);
            Assert.Equal(0, piece.RemainderQuantity);
        });
        Assert.Equal("900/4000", state.GroupCountSummary);
    }

    [Fact]
    public void Shell_left_hosts_share_one_order_piece_state_and_keep_card_controls_in_the_shell()
    {
        var state = OrderPiecePanelState.CreateImage27Demo();
        var shell = new AppShellView(state);

        Assert.Same(state, shell.OrderPieceState);
        Assert.IsType<OrderGroupPanelView>(shell.OrderGroupHost.HostedContent);
        Assert.IsType<PieceCardListView>(shell.PieceListHost.HostedContent);
        Assert.IsType<ProgressSummaryView>(shell.ProgressSummaryHost.HostedContent);
        var list = Assert.IsType<PieceCardListView>(shell.PieceListHost.HostedContent);
        Assert.Equal(10, list.Cards.Count);
        Assert.Equal(6, PieceCardListView.EvidencedVisibleCardCount);
        Assert.DoesNotContain(Descendants(shell.PieceListHost), control => control is Window);
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (var child in root.GetVisualChildren().OfType<Control>())
        {
            yield return child;
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }
    }
}
