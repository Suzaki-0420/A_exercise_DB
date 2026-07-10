using A_exercise_DB.Domains.Models;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Applications.Usecases.Orders;

/// <summary>
/// 注文ステータス更新Usecaseインターフェイス
/// </summary>
public interface IUpdateOrderStatusUsecase
{
    /// <summary>
    /// 注文ステータス更新入力画面の表示情報を取得する
    /// </summary>
    Task<(OrdersModel Order, List<OrderStatus> OrderStatuses)?> GetInputAsync(Guid orderUuid);

    /// <summary>
    /// 注文ステータス更新確認画面の表示情報を取得する
    /// </summary>
    Task<(OrdersModel Order, OrderStatus NewStatus)?> GetConfirmAsync(
        Guid orderUuid,
        int newStatusId);

    /// <summary>
    /// 注文ステータスを更新する
    /// </summary>
    Task<(OrdersModel Order, OrderStatus NewStatus)> UpdateAsync(
        Guid orderUuid,
        int newStatusId);
}