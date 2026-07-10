using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.ViewModels;

namespace A_exercise_DB.Presentations.Adapters;

/// <summary>
/// 注文ステータス更新ViewModelアダプタ
/// </summary>
public class UpdateOrderStatusViewModelAdapter
{
    /// <summary>
    /// 注文ステータス更新入力ViewModelへ変換する
    /// </summary>
    public UpdateOrderStatusInputViewModel ConvertToInputViewModel(
        Orders order,
        List<OrderStatus> orderStatuses)
    {
        _ = order ?? throw new InternalException("引数orderがnullです。");
        _ = orderStatuses ?? throw new InternalException("引数orderStatusesがnullです。");

        return new UpdateOrderStatusInputViewModel
        {
            OrderId = order.OrderUuid,
            OrderDate = order.OrderDate.ToString("yyyy/MM/dd HH:mm:ss"),
            CustomerAccountName = order.Customer.Username,
            OrderContent = CreateOrderContent(order),
            OrderStatus = order.OrderStatus.Id,
            OrderStatusList = orderStatuses
                .Select(ConvertToOrderStatusOptionViewModel)
                .ToList()
        };
    }

    /// <summary>
    /// 注文ステータス更新確認ViewModelへ変換する
    /// </summary>
    public UpdateOrderStatusConfirmViewModel ConvertToConfirmViewModel(
        Orders order,
        OrderStatus newStatus)
    {
        _ = order ?? throw new InternalException("引数orderがnullです。");
        _ = newStatus ?? throw new InternalException("引数newStatusがnullです。");

        return new UpdateOrderStatusConfirmViewModel
        {
            OrderId = order.OrderUuid,
            OrderDate = order.OrderDate.ToString("yyyy/MM/dd HH:mm:ss"),
            CustomerAccountName = order.Customer.Username,
            CurrentStatus = order.OrderStatus.Name,
            NewStatusId = newStatus.Id,
            NewStatus = newStatus.Name
        };
    }

    /// <summary>
    /// 注文ステータス更新完了ViewModelへ変換する
    /// </summary>
    public UpdateOrderStatusCompleteViewModel ConvertToCompleteViewModel(
        Orders order,
        OrderStatus newStatus)
    {
        _ = order ?? throw new InternalException("引数orderがnullです。");
        _ = newStatus ?? throw new InternalException("引数newStatusがnullです。");

        return new UpdateOrderStatusCompleteViewModel
        {
            OrderNumber = order.OrderUuid,
            OrderStatus = newStatus.Name,
            UpdateDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
        };
    }

    /// <summary>
    /// 注文ステータス選択肢ViewModelへ変換する
    /// </summary>
    private OrderStatusOptionViewModel ConvertToOrderStatusOptionViewModel(
        OrderStatus orderStatus)
    {
        _ = orderStatus ?? throw new InternalException("引数orderStatusがnullです。");

        return new OrderStatusOptionViewModel
        {
            Id = orderStatus.Id,
            Name = orderStatus.Name
        };
    }

    /// <summary>
    /// 注文内容の表示文字列を作成する
    /// </summary>
    private string CreateOrderContent(Orders order)
    {
        if (order.OrdersDetails.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(
            "、",
            order.OrdersDetails.Select(detail =>
                $"{detail.Product.Name} × {detail.Count}"
            )
        );
    }
}