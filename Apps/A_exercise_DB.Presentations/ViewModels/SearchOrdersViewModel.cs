namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 購入履歴検索条件ViewModel
/// </summary>
public class SearchOrdersViewModel
{
    /// <summary>
    /// 購入日
    /// </summary>
    public string? OrderDate { get; set; }

    /// <summary>
    /// 顧客アカウント名
    /// </summary>
    public string? CustomerAccountName { get; set; }
}