using System.ComponentModel.DataAnnotations;

namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 注文ステータス更新入力ViewModel
/// </summary>
public class UpdateOrderStatusInputViewModel
{
    /// <summary>
    /// 注文ID
    /// </summary>
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
    /// 注文内容
    /// </summary>
    public string OrderContent { get; set; } = string.Empty;

    /// <summary>
    /// 注文ステータス
    /// </summary>
    [Required(ErrorMessage = "注文ステータスは必須です。")]
    public int OrderStatus { get; set; }

    /// <summary>
    /// 注文ステータス選択肢
    /// </summary>
    public List<OrderStatusOptionViewModel> OrderStatusList { get; set; } = new();
}