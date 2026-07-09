using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Applications.Usecases.Orders;

/// <summary>
/// BP015: 購入履歴検索ユースケース
/// </summary>
public class SearchOrdersUsecase : ISearchOrdersUsecase
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IOrderStatusRepository _orderStatusRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SearchOrdersUsecase(
        IOrdersRepository ordersRepository,
        IOrderStatusRepository orderStatusRepository)
    {
        _ordersRepository = ordersRepository;
        _orderStatusRepository = orderStatusRepository;
    }

    /// <summary>
    /// 購入日または顧客アカウント名で購入履歴を検索する
    /// </summary>
    public async Task<List<OrdersModel>> SearchAsync(
        DateTime? orderDate,
        string? customerName)
    {
        try
        {
            return await _ordersRepository.SearchByDateOrNameAsync(
                orderDate,
                customerName);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                "注文情報の取得に失敗しました。",
                ex);
        }
    }

    /// <summary>
    /// 注文ステータス一覧を取得する
    /// </summary>
    public async Task<List<OrderStatus>> FindAllStatusAsync()
    {
        try
        {
            return await _orderStatusRepository.FindAllAsync();
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                "注文ステータス一覧の取得に失敗しました。",
                ex);
        }
    }

    /// <summary>
    /// 注文ステータスを変更する
    /// </summary>
    public async Task<bool> ChangeStatusAsync(OrdersModel order)
    {
        try
        {
            _ = order ?? throw new InternalException("引数orderがnullです。");

            if (order.OrderStatus is null)
            {
                throw new InternalException("注文ステータスが指定されていません。");
            }

            var orderStatus = await _orderStatusRepository.FindByIdAsync(
                order.OrderStatus.Id);

            if (orderStatus is null)
            {
                return false;
            }

            return await _ordersRepository.ChangeStatusAsync(order);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                "注文ステータスの変更に失敗しました。",
                ex);
        }
    }
}