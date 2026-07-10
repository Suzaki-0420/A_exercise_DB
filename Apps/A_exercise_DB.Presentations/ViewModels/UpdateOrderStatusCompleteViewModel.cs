namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 注文ステータス更新完了ViewModel
/// </summary>
public class UpdateOrderStatusCompleteViewModel
{
    /// <summary>
    /// 完了メッセージ
    /// </summary>
    public string CompleteMsg { get; set; } = "注文ステータスを更新しました";

    /// <summary>
    /// 注文番号
    /// </summary>
    public Guid OrderNumber { get; set; }

    /// <summary>
    /// 注文ステータス
    /// </summary>
    public string OrderStatus { get; set; } = string.Empty;

    /// <summary>
    /// 更新日時
    /// </summary>
    public string UpdateDate { get; set; } = string.Empty;

    /// <summary>
    /// 購入履歴検索画面URL
    /// </summary>
    public string SearchUrl { get; set; } = "/admin/order/search";

    /// <summary>
    /// 管理者トップ画面URL
    /// </summary>
    public string HomeUrl { get; set; } = "/admin";
}