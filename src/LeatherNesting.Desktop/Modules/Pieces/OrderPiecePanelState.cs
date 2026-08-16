namespace LeatherNesting.Desktop.Modules.Pieces;

/// <summary>One shared, deterministic UI record set for the image 27 shell and image 13 editor.</summary>
public sealed class OrderPiecePanelState
{
    private readonly List<OrderRecord> _orders;

    private OrderPiecePanelState(List<OrderRecord> orders)
    {
        _orders = orders;
        SelectedOrder = orders[0];
    }

    public event EventHandler? Changed;

    public IReadOnlyList<OrderRecord> Orders => _orders;
    public OrderRecord SelectedOrder { get; private set; }
    public int OrderCount => _orders.Count;

    public IReadOnlyList<OrderPieceRecord> Pieces => SelectedOrder.Pieces;
    public string ChannelSummary => SelectedOrder.ChannelSummary;
    public int GroupPieceCount => SelectedOrder.PieceCount;
    public int GroupPlacedQuantity => SelectedOrder.PlacedQuantity;
    public int GroupTotalQuantity => SelectedOrder.TotalQuantity;
    public string GroupCountSummary => $"{GroupPlacedQuantity}/{GroupTotalQuantity}";
    public string GroupAreaSummary { get; } = "5.56/6.39(m²)";
    public string GroupProgress { get; } = "13.07%";
    public string OrderCountSummary { get; } = "900/12100";
    public string OrderAreaSummary { get; } = "5.56/77.23(m²)";
    public string OrderProgress { get; } = "92.81%";
    public string EvidenceGapNotice { get; } = "证据缺口：单套/套数/余量/总量的生产公式尚待现场确认。";
    public string PersistenceNotice { get; } = "DEMO · 编辑仅保存在当前内存会话，未接入生产持久化。";

    public static OrderPiecePanelState CreateImage27Demo()
    {
        string[] dimensions = ["205*110", "173*129", "77*169", "172*75", "104*70", "104*96", "99*39", "48*118", "120*104", "68*18"];
        double[] areas = [0.1649, 0.0899, 0.0794, 0.0783, 0.0735, 0.0666, 0.0418, 0.0412, 0.0391, 0.0133];
        var order1 = dimensions.Select((dimensionsValue, index) => new OrderPieceRecord(
            index + 1, "40", dimensionsValue, "任意角度", "0 | 排完", 1, 1, 100, 100, areas[index])).ToList();

        var order2 = new List<OrderPieceRecord>
        {
            new(1, "39", "210*115", "任意角度", "0 | 排完", 1, 1, 80, 80, 0.1702),
            new(2, "39", "178*132", "任意角度", "0 | 排完", 1, 1, 80, 80, 0.0941),
            new(3, "39", "80*172", "任意角度", "0 | 排完", 1, 1, 80, 80, 0.0822),
        };

        var order3 = new List<OrderPieceRecord>
        {
            new(1, "38", "176*78", "任意角度", "0 | 排完", 1, 1, 60, 60, 0.0804),
            new(2, "38", "108*72", "任意角度", "0 | 排完", 1, 1, 60, 60, 0.0766),
        };

        var orders = new List<OrderRecord>
        {
            new("贴皮测试（皮）", "P_00030; ch 0", 900, order1),
            new("鞋面-39 订单", "P_00031; ch 1", 240, order2),
            new("鞋面-38 订单", "P_00032; ch 2", 120, order3),
        };
        return new OrderPiecePanelState(orders);
    }

    public void SelectOrder(OrderRecord order)
    {
        if (ReferenceEquals(SelectedOrder, order)) return;
        SelectedOrder = order;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateQuantities(int index, int singleSetQuantity, int setCount, int remainderQuantity)
    {
        var piece = Find(index);
        piece.SingleSetQuantity = Math.Max(singleSetQuantity, 0);
        piece.SetCount = Math.Max(setCount, 0);
        piece.RemainderQuantity = Math.Max(remainderQuantity, 0);
        // This demo projection follows the only evidenced image-13 total: 单套 × 套数.
        piece.TotalQuantity = piece.SingleSetQuantity * piece.SetCount;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetIncluded(int index, bool included)
    {
        Find(index).IsIncluded = included;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void LoadImage13PropertyDemo()
    {
        foreach (var piece in SelectedOrder.Pieces)
        {
            piece.IsIncluded = true;
            piece.FineRotation = 30.0;
            piece.Priority = 0;
            piece.SmallPieceKnife = false;
            piece.SingleSetQuantity = piece.Index == 1 ? 2 : 1;
            piece.SetCount = 100;
            piece.TotalQuantity = piece.SingleSetQuantity * piece.SetCount;
            piece.RemainderQuantity = piece.TotalQuantity;
            piece.ExtraSpacing = 0.00;
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private OrderPieceRecord Find(int index) => SelectedOrder.Pieces.Single(piece => piece.Index == index);
}

/// <summary>One order (订单) with its own independent pieces. 排版是排版，订单是订单。</summary>
public sealed class OrderRecord
{
    public OrderRecord(string name, string channelSummary, int placedQuantity, IEnumerable<OrderPieceRecord> pieces)
    {
        Name = name;
        ChannelSummary = channelSummary;
        PlacedQuantity = placedQuantity;
        Pieces = pieces.ToList();
    }

    public string Name { get; }
    public string ChannelSummary { get; }
    public int PlacedQuantity { get; }
    public List<OrderPieceRecord> Pieces { get; }
    public int PieceCount => Pieces.Count;
    public int TotalQuantity => Pieces.Sum(piece => piece.TotalQuantity);
    public double Area => Pieces.Sum(piece => piece.Area);
}

public sealed class OrderPieceRecord(
    int index,
    string size,
    string boundingDimensions,
    string rotation,
    string completion,
    int singleSetQuantity,
    int setCount,
    int remainderQuantity,
    int totalQuantity,
    double area)
{
    public int Index { get; } = index;
    public string Name { get; set; } = "40";
    public string Size { get; } = size;
    public string BoundingDimensions { get; } = boundingDimensions;
    public string Rotation { get; set; } = rotation;
    public string Completion { get; } = completion;
    public bool IsIncluded { get; set; } = true;
    public double FineRotation { get; set; } = 30.0;
    public int Priority { get; set; }
    public bool SmallPieceKnife { get; set; }
    public int SingleSetQuantity { get; set; } = singleSetQuantity;
    public int SetCount { get; set; } = setCount;
    public int RemainderQuantity { get; set; } = remainderQuantity;
    public int TotalQuantity { get; set; } = totalQuantity;
    public double ExtraSpacing { get; set; }
    public double Area { get; } = area;
    public double PieceConsumption { get; } = 0.0000;
    public double PieceOveragePercent { get; } = 0.0000;
}
