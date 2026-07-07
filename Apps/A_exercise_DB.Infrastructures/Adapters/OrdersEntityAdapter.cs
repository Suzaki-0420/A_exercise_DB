using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Exceptions;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructure.Adapters;

/// <summary>
/// ドメインオブジェクト:OrdersとOrdersEntityの相互変換クラス
/// </summary>
public class OrdersEntityAdapter :
    IConverter<Orders, OrdersEntity>,
    IRestorer<Orders, OrdersEntity>
{
    /// <summary>
    /// ドメインオブジェクト:OrdersをOrdersEntityに変換する
    /// </summary>
    /// <param name="domain">ドメインオブジェクト:Orders</param>
    /// <returns>EFCore:OrdersEntity</returns>
    public Task<OrdersEntity> ConvertAsync(Orders domain)
    {
        // 引数domainがnullの場合
        _ = domain ?? throw new InternalException("引数domainがnullです。");

        // ドメインオブジェクト:OrdersをOrdersEntityに変換する
        var entity = new OrdersEntity();
        entity.OrderUuid = domain.OrderUuid;
        entity.EmployeeUuid = domain.EmployeeUuid;
        entity.OrderDate = domain.OrderDate;
        entity.TotalAmount = domain.TotalAmount;

        return Task.FromResult(entity);
    }

    /// <summary>
    /// OrdersEntityからドメインオブジェクト:Ordersを復元する
    /// </summary>
    /// <param name="target">EFCore:OrdersEntity</param>
    /// <returns>ドメインオブジェクト:Orders</returns>
    public Task<Orders> RestoreAsync(OrdersEntity target)
    {
        // 引数targetがnullの場合
        _ = target ?? throw new InternalException("引数targetがnullです。");

        // OrdersEntityからドメインオブジェクト:Ordersを復元する
        var domain = new Orders(
            target.OrderUuid,
            target.EmployeeUuid,
            target.OrderDate,
            target.TotalAmount
        );

        return Task.FromResult(domain);
    }
}