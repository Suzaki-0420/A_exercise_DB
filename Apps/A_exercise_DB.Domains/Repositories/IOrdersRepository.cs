using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Repositories;

public interface IOrdersRepository
{
    Task<List<Orders>> SearchByDateOrNameAsync(
        DateTime? orderDate,
        string? customerName);

    Task<bool> ChangeStatusAsync(Orders order);
}