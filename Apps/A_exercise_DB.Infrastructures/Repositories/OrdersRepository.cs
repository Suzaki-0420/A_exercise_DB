using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Contexts;
using Microsoft.EntityFrameworkCore;

namespace A_exercise_DB.Infrastructures.Repositories;

/// <summary>
/// 注文Repository
/// </summary>
public class OrdersRepository : IOrdersRepository
{
    private readonly AppDbContext _context;
    private readonly OrdersFactory _factory;

    public OrdersRepository(
        AppDbContext context,
        OrdersFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// 購入日または顧客アカウント名で購入履歴を検索する
    /// </summary>
    public async Task<List<Orders>> SearchByDateOrNameAsync(
        DateTime? orderDate,
        string? customerUsername)
    {
        try
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentMethod)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(o => o.OrderDetails)
                   .ThenInclude(d => d.Product)
                       .ThenInclude(p => p.ProductStock)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.ProductCategory)
                .AsQueryable();

            if (orderDate.HasValue)
            {
                var from = orderDate.Value.Date;
                var to = from.AddDays(1);

                query = query.Where(o =>
                    o.OrderDate >= from && o.OrderDate < to);
            }

            if (!string.IsNullOrWhiteSpace(customerUsername))
            {
                query = query.Where(o =>
                    EF.Functions.Like(o.Customer.Username, $"%{customerUsername}%"));
            }
            query = query.OrderByDescending(o => o.OrderDate);

            var entities = await query.ToListAsync();

            return await _factory.RestoreAsync(entities);
        }
        catch (Exception ex)
        {
            throw new InternalException(
                "購入履歴検索中に予期しないエラーが発生しました。",
                ex);
        }
    }

    /// <summary>
    /// 注文ステータスを変更する
    /// </summary>
    public async Task<bool> ChangeStatusAsync(Orders order)
    {
        try
        {
            var entity = await _context.Orders
                .SingleOrDefaultAsync(o => o.OrderUuid == order.OrderUuid);

            if (entity is null)
            {
                return false;
            }

            var orderStatus = await _context.OrderStatuses
                .SingleOrDefaultAsync(s =>
                    s.Id == order.OrderStatus!.Id);

            if (orderStatus is null)
            {
                return false;
            }

            entity.OrderStatusId = orderStatus.Id;

            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                $"Id:{order.OrderUuid}の注文ステータス変更中に予期しないエラーが発生しました。",
                ex);
        }
    }
}