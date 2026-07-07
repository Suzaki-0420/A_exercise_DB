using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Exceptions;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructure.Adapters;

/// <summary>
/// ドメインオブジェクト:OrderDetailとOrderDetailEntityの相互変換クラス
/// </summary>
public class OrdersDetailEntityAdapter :
    IConverter<OrdersDetail, OrdersDetailEntity>,
    IRestorer<OrdersDetail, OrdersDetailEntity>
{
    /// <summary>
    /// ドメインオブジェクト:OrderDetailをOrderDetailEntityに変換する
    /// </summary>
    /// <param name="domain">ドメインオブジェクト:OrderDetail</param>
    /// <returns>EFCore:OrderDetailEntity</returns>
    public Task<OrdersDetailEntity> ConvertAsync(OrdersDetail domain)
    {
        // 引数domainがnullの場合
        _ = domain ?? throw new InternalException("引数domainがnullです。");

        // ドメインオブジェクト:OrderDetailをOrderDetailEntityに変換する
        var entity = new OrdersDetailEntity();
        entity.OrdersDetailUuid = domain.OrdersDetailUuid;
        entity.OrdersUuid = domain.OrdersUuid;
        entity.ProductUuid = domain.ProductUuid;
        entity.Quantity = domain.Quantity;
        entity.UnitPrice = domain.UnitPrice;

        return Task.FromResult(entity);
    }

    /// <summary>
    /// OrderDetailEntityからドメインオブジェクト:OrderDetailを復元する
    /// </summary>
    /// <param name="target">EFCore:OrderDetailEntity</param>
    /// <returns>ドメインオブジェクト:OrderDetail</returns>
    public Task<OrdersDetail> RestoreAsync(OrdersDetailEntity target)
    {
        // 引数targetがnullの場合
        _ = target ?? throw new InternalException("引数targetがnullです。");

        // OrderDetailEntityからドメインオブジェクト:OrderDetailを復元する
        var domain = new OrdersDetail(
            target.OrdersDetailUuid,
            target.OrdersUuid,
            target.ProductUuid,
            target.Quantity,
            target.UnitPrice
        );

        return Task.FromResult(domain);
    }
}