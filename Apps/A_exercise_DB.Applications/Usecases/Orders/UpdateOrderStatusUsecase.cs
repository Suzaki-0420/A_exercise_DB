using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Applications.Usecases.Orders;

/// <summary>
/// 注文ステータス更新ユースケース
/// </summary>
public class UpdateOrderStatusUsecase : IUpdateOrderStatusUsecase
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IOrderStatusRepository _orderStatusRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ordersRepository">注文Repository</param>
    /// <param name="orderStatusRepository">注文ステータスRepository</param>
    /// <param name="unitOfWork">トランザクション制御機能</param>
    public UpdateOrderStatusUsecase(
        IOrdersRepository ordersRepository,
        IOrderStatusRepository orderStatusRepository,
        IUnitOfWork unitOfWork)
    {
        _ordersRepository = ordersRepository;
        _orderStatusRepository = orderStatusRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 注文ステータス更新入力画面の表示情報を取得する
    /// </summary>
    /// <param name="orderUuid">注文UUID</param>
    /// <returns>注文情報と注文ステータス一覧</returns>
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
    /// <param name="orderUuid">注文UUID</param>
    /// <param name="newStatusId">新しい注文ステータスID</param>
    /// <returns>注文情報と新しい注文ステータス</returns>
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
    /// <param name="orderUuid">注文UUID</param>
    /// <param name="newStatusId">新しい注文ステータスID</param>
    /// <returns>更新後の注文情報と新しい注文ステータス</returns>
    /// <exception cref="NotFoundException">注文が存在しない場合にスローされる</exception>
    public async Task<(OrdersModel Order, OrderStatus NewStatus)> UpdateAsync(
        Guid orderUuid,
        int newStatusId)
    {
        await _unitOfWork.BeginAsync();

        var isCommitted = false;

        try
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
                throw new NotFoundException("指定された注文は存在しません。");
            }

            var newStatus = await _orderStatusRepository.FindByIdAsync(newStatusId);

            if (newStatus is null)
            {
                throw new DomainException("指定された注文ステータスは存在しません。");
            }

            order.ChangeOrderStatus(newStatus);

            var isUpdated = await _ordersRepository.ChangeStatusAsync(order);

            if (!isUpdated)
            {
                throw new InternalException("注文ステータスの更新に失敗しました。");
            }

            await _unitOfWork.CommitAsync();

            isCommitted = true;

            return (order, newStatus);
        }
        finally
        {
            if (!isCommitted)
            {
                await _unitOfWork.RollbackAsync();
            }
        }
    }
}