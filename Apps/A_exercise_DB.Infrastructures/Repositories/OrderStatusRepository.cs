using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Contexts;
using Microsoft.EntityFrameworkCore;

namespace A_exercise_DB.Infrastructures.Repositories;

/// <summary>
/// 注文ステータスRepository
/// </summary>
public class OrderStatusRepository : IOrderStatusRepository
{
    private readonly AppDbContext _context;
    private readonly OrderStatusEntityAdapter _adapter;

    public OrderStatusRepository(
        AppDbContext context,
        OrderStatusEntityAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// 注文ステータスをすべて取得する
    /// </summary>
    public async Task<List<OrderStatus>> FindAllAsync()
    {
        try
        {
            var entities = await _context.OrderStatuses
                .AsNoTracking()
                .ToListAsync();

            var orderStatuses = new List<OrderStatus>();

            foreach (var entity in entities)
            {
                orderStatuses.Add(
                    await _adapter.RestoreAsync(entity));
            }

            return orderStatuses;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                "注文ステータス一覧取得時に予期しないエラーが発生しました。",
                ex);
        }
    }

    /// <summary>
    /// 指定された注文ステータスIDの注文ステータスを取得する
    /// </summary>
    public async Task<OrderStatus?> FindByIdAsync(int id)
    {
        try
        {
            var entity = await _context.OrderStatuses
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Id == id);

            if (entity is null)
            {
                return null;
            }

            return await _adapter.RestoreAsync(entity);
        }
        catch (Exception ex)
        {
            throw new InternalException(
                $"Id:{id}の注文ステータス取得時に予期しないエラーが発生しました。",
                ex);
        }
    }
}