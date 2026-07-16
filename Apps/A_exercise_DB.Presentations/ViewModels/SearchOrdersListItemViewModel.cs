namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 購入履歴検索結果1行分ViewModel
/// </summary>
public class SearchOrdersListItemViewModel
{
    /// <summary>
    /// 注文UUID
    /// </summary>
    public Guid OrderUuid { get; set; }

    /// <summary>
    /// 購入日時
    /// </summary>
    public string OrderDate { get; set; } = string.Empty;

    /// <summary>
    /// 顧客アカウント名
    /// </summary>
    public string CustomerAccountName { get; set; } = string.Empty;

    /// <summary>
    /// 注文内容
    /// </summary>
    public string OrderContent { get; set; } = string.Empty;

    /// <summary>
    /// 注文ステータス
    /// </summary>
    public string OrderStatus { get; set; } = string.Empty;

    /// <summary>
    /// ステータス更新画面URL
    /// </summary>
    public string StatusUpdateUrl { get; set; } = string.Empty;
}