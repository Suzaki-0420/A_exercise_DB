using A_exercise_DB.Domains.Models;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Applications.Usecases.Orders;

/// <summary>
/// BP015: 購入履歴検索ユースケースインターフェイス
/// </summary>
public interface ISearchOrdersUsecase
{
    /// <summary>
    /// 購入日または顧客アカウント名で購入履歴を検索する
    /// </summary>
    Task<List<OrdersModel>> SearchAsync(
        DateTime? orderDate,
        string? customerName);

    /// <summary>
    /// 注文ステータス一覧を取得する
    /// </summary>
    Task<List<OrderStatus>> FindAllStatusAsync();

    /// <summary>
    /// 注文ステータスを変更する
    /// </summary>
    Task<bool> ChangeStatusAsync(OrdersModel order);
}