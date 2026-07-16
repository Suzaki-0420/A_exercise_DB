using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Repositories;

/// <summary>
/// 注文ステータスRepositoryインターフェイス
/// </summary>
public interface IOrderStatusRepository
{
    Task<List<OrderStatus>> FindAllAsync();

    Task<OrderStatus?> FindByIdAsync(int id);
}