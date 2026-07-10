using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Applications.Usecases.Orders;

/// <summary>
/// 注文ステータス更新Usecase
/// </summary>
public class UpdateOrderStatusUsecase : IUpdateOrderStatusUsecase
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IOrderStatusRepository _orderStatusRepository;

    public UpdateOrderStatusUsecase(
        IOrdersRepository ordersRepository,
        IOrderStatusRepository orderStatusRepository)
    {
        _ordersRepository = ordersRepository;
        _orderStatusRepository = orderStatusRepository;
    }

    /// <summary>
    /// 注文ステータス更新入力画面の表示情報を取得する
    /// </summary>
    public async Task<(OrdersModel Order, List<OrderStatus> OrderStatuses)?> GetInputAsync(
        Guid orderUuid)
    {
        if (orderUuid == Guid.Empty)
        {
            throw new DomainException("注文IDが不正です。");
        }

        var order = await _ordersRepository.FindByUuidAsync(orderUuid);

        if (order is null)
        {
            return null;
        }

        var orderStatuses = await _orderStatusRepository.FindAllAsync();

        return (order, orderStatuses);
    }

    /// <summary>
    /// 注文ステータス更新確認画面の表示情報を取得する
    /// </summary>
    public async Task<(OrdersModel Order, OrderStatus NewStatus)?> GetConfirmAsync(
        Guid orderUuid,
        int newStatusId)
    {
        if (orderUuid == Guid.Empty)
        {
            throw new DomainException("注文IDが不正です。");
        }

        if (newStatusId <= 0)
        {
            throw new DomainException("注文ステータスが不正です。");
        }

        var order = await _ordersRepository.FindByUuidAsync(orderUuid);

        if (order is null)
        {
            return null;
        }

        var newStatus = await _orderStatusRepository.FindByIdAsync(newStatusId);

        if (newStatus is null)
        {
            throw new DomainException("指定された注文ステータスは存在しません。");
        }

        return (order, newStatus);
    }

    /// <summary>
    /// 注文ステータスを更新する
    /// </summary>
    public async Task<(OrdersModel Order, OrderStatus NewStatus)> UpdateAsync(
        Guid orderUuid,
        int newStatusId)
    {
        var confirmData = await GetConfirmAsync(orderUuid, newStatusId);

        if (confirmData is null)
        {
            throw new NotFoundException("指定された注文は存在しません。");
        }

        var order = confirmData.Value.Order;
        var newStatus = confirmData.Value.NewStatus;

        order.ChangeOrderStatus(newStatus);

        var result = await _ordersRepository.ChangeStatusAsync(order);

        if (!result)
        {
            throw new InternalException("注文ステータスの更新に失敗しました。");
        }

        return (order, newStatus);
    }
}