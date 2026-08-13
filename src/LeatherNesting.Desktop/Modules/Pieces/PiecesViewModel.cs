using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.Pieces;

public enum PieceSortField { Code, RequiredQuantity, PlacedQuantity, UnplacedQuantity, Priority }

/// <summary>Local demo state for M06. All mutation stays in memory and is explicitly labelled as TODO by the view.</summary>
public sealed class PiecesViewModel
{
    private readonly List<PieceDemoRecord> _pieces =
    [
        new("VAMP-39-L", "鞋面", "39", "左", 12, 8, "高", "0° / 180°", true, 4),
        new("VAMP-39-R", "鞋面", "39", "右", 12, 7, "高", "0° / 180°", true, 4),
        new("QUARTER-39-L", "后帮", "39", "左", 12, 12, "中", "0°", false, 3),
        new("QUARTER-39-R", "后帮", "39", "右", 12, 10, "中", "0°", false, 3),
        new("HEEL-38-L", "后跟贴", "38", "左", 6, 9, "低", "0° / 90°", true, 2),
        new("TONGUE-39", "鞋舌", "39", "单件", 12, 5, "高", "0° / 180°", false, 4),
    ];

    private string _searchText = string.Empty;
    private bool _showUnfinishedOnly;
    private PieceSortField _sortField = PieceSortField.Code;
    private bool _sortDescending;

    public IReadOnlyList<PieceDemoRecord> Pieces => _pieces;
    public string? TodoMessage { get; private set; }
    public string SearchText { get => _searchText; set => _searchText = value ?? string.Empty; }
    public bool ShowUnfinishedOnly { get => _showUnfinishedOnly; set => _showUnfinishedOnly = value; }
    public PieceSortField SortField => _sortField;
    public bool SortDescending => _sortDescending;
    public IEnumerable<string> SelectedCodes => _pieces.Where(piece => piece.IsSelected).Select(piece => piece.Code);
    public int PlannedQuantity => _pieces.Sum(piece => piece.RequiredQuantity);
    public int PlacedQuantity => _pieces.Sum(piece => piece.PlacedQuantity);
    public int UnplacedQuantity => _pieces.Sum(piece => piece.UnplacedQuantity);

    public IReadOnlyList<PieceDemoRecord> VisiblePieces
    {
        get
        {
            IEnumerable<PieceDemoRecord> records = _pieces;
            if (!string.IsNullOrWhiteSpace(SearchText))
                records = records.Where(piece => piece.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || piece.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            if (ShowUnfinishedOnly) records = records.Where(piece => piece.UnplacedQuantity > 0);
            return (_sortField switch
            {
                PieceSortField.RequiredQuantity => _sortDescending ? records.OrderByDescending(piece => piece.RequiredQuantity) : records.OrderBy(piece => piece.RequiredQuantity),
                PieceSortField.PlacedQuantity => _sortDescending ? records.OrderByDescending(piece => piece.PlacedQuantity) : records.OrderBy(piece => piece.PlacedQuantity),
                PieceSortField.UnplacedQuantity => _sortDescending ? records.OrderByDescending(piece => piece.UnplacedQuantity) : records.OrderBy(piece => piece.UnplacedQuantity),
                PieceSortField.Priority => _sortDescending ? records.OrderByDescending(piece => piece.Priority) : records.OrderBy(piece => piece.Priority),
                _ => _sortDescending ? records.OrderByDescending(piece => piece.Code) : records.OrderBy(piece => piece.Code),
            }).ToArray();
        }
    }

    public void SortBy(PieceSortField field)
    {
        _sortDescending = _sortField == field ? !_sortDescending : field is PieceSortField.RequiredQuantity or PieceSortField.PlacedQuantity or PieceSortField.UnplacedQuantity;
        _sortField = field;
    }

    public void SetSelected(string code, bool isSelected) => Find(code).IsSelected = isSelected;

    public void SetRequiredQuantity(string code, int quantity)
    {
        Find(code).RequiredQuantity = Math.Max(quantity, 0);
        TodoMessage = $"编辑订单数量：{TodoBadge.StandardText}";
    }

    public void ApplyBulkPriority(string priority) => TodoMessage = $"批量设置优先级（{SelectedCodes.Count()} 项）：{TodoBadge.StandardText}";

    public void SelectPiece(string code) => TodoMessage = $"检查器已选中 {Find(code).Code}；真实排样回写：{TodoBadge.StandardText}";

    private PieceDemoRecord Find(string code) => _pieces.Single(piece => StringComparer.Ordinal.Equals(piece.Code, code));
}
