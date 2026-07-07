using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
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
        entity.OrderId = domain.OrderId;
        entity.ProductId = domain.ProductId;
        entity.Count = domain.Count;

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
            target.OrderId,
            target.ProductId,
            target.Count
        );

        return Task.FromResult(domain);
    }
}