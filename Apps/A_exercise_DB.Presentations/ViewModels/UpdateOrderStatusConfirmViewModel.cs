using System.ComponentModel.DataAnnotations;

namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 注文ステータス更新確認ViewModel
/// </summary>
public class UpdateOrderStatusConfirmViewModel
{
    /// <summary>
    /// 注文ID
    /// </summary>
    [Required(ErrorMessage = "注文IDは必須です。")]
    public Guid OrderId { get; set; }

    /// <summary>
    /// 購入日時
    /// </summary>
    public string OrderDate { get; set; } = string.Empty;

    /// <summary>
    /// 顧客アカウント名
    /// </summary>
    public string CustomerAccountName { get; set; } = string.Empty;

    /// <summary>
    /// 現在のステータス
    /// </summary>
    public string CurrentStatus { get; set; } = string.Empty;

    /// <summary>
    /// 新しいステータスID
    /// </summary>
    [Required(ErrorMessage = "注文ステータスは必須です。")]
    public int NewStatusId { get; set; }

    /// <summary>
    /// 新しいステータス
    /// </summary>
    public string NewStatus { get; set; } = string.Empty;
}