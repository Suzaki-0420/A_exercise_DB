using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructures.Adapters;

public class OrdersFactory
{
    private readonly OrdersEntityAdapter _ordersEntityAdapter;
    private readonly OrdersDetailEntityAdapter _ordersDetailEntityAdapter;

    public OrdersFactory(
        OrdersEntityAdapter ordersEntityAdapter,
        OrdersDetailEntityAdapter ordersDetailEntityAdapter)
    {
        _ordersEntityAdapter = ordersEntityAdapter;
        _ordersDetailEntityAdapter = ordersDetailEntityAdapter;
    }

    public async Task<OrdersEntity> ConvertAsync(Orders domain)
    {
        var entity = await _ordersEntityAdapter.ConvertAsync(domain);

        if (domain.OrderDetails is not null)
        {
            entity.OrderDetails = new List<OrdersDetailEntity>();

            foreach (var detail in domain.OrderDetails)
            {
                entity.OrderDetails.Add(
                    await _ordersDetailEntityAdapter.ConvertAsync(detail));
            }
        }

        return entity;
    }

    public async Task<Orders> RestoreAsync(OrdersEntity target)
    {
        var order = await _ordersEntityAdapter.RestoreAsync(target);

        if (target.OrderDetails is not null)
        {
            foreach (var detailEntity in target.OrderDetails)
            {
                order.AddOrderDetail(
                    await _ordersDetailEntityAdapter.RestoreAsync(detailEntity));
            }
        }

        return order;
    }

    public async Task<List<Orders>> RestoreAsync(List<OrdersEntity> targets)
    {
        var orders = new List<Orders>();

        foreach (var target in targets)
        {
            orders.Add(await RestoreAsync(target));
        }

        return orders;
    }
}