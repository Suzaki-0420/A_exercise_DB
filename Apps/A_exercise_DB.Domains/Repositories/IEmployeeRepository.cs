using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Repositories;

/// <summary>
/// 社員Repositoryのインターフェース
/// </summary>
public interface IEmployeeRepository
{
    /// <summary>
    /// 社員情報をすべて取得する
    /// </summary>
    /// <returns>Employeeのリスト</returns>
    Task<List<Employee>> FindAllWithoutAccountAsync();

    /// <summary>
    /// 指定された社員Idの社員情報を取得する
    /// </summary>
    /// <param name="id">社員Id(UUID)</param>
    /// <returns>Employee または null</returns>
    Task<Employee?> FindByIdAsync(string uuid);

    /// <summary>
    /// 社員UUIDに一致する社員が存在するかを確認する
    /// </summary>
    /// <param name="employeeUuid">社員UUID</param>
    /// <returns>存在する場合true</returns>
    Task<bool> ExistsByEmployeeUuidAsync(Guid employeeUuid);
}