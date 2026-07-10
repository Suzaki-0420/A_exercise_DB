using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Repositories;

public interface IOrdersRepository
{
    Task<List<Orders>> SearchByDateOrNameAsync(
        DateTime? orderDate,
        string? customerUsername);

    Task<bool> ChangeStatusAsync(Orders order);

    Task<Orders?> FindByUuidAsync(Guid orderUuid);
}