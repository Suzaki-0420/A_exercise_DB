namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 購入履歴検索結果ViewModel
/// </summary>
public class SearchOrdersResultViewModel
{
    /// <summary>
    /// タイトル
    /// </summary>
    public string Title { get; set; } = "購入履歴検索";

    /// <summary>
    /// 購入履歴一覧
    /// </summary>
    public List<SearchOrdersListItemViewModel> OrderList { get; set; } = new();

    /// <summary>
    /// メッセージ
    /// </summary>
    public string? Message { get; set; }
}