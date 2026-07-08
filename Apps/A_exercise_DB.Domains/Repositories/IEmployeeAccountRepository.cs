using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Repositories;

/// <summary>
/// 社員アカウントRepositoryのインターフェース
/// </summary>
public interface IEmployeeAccountRepository
{
    /// <summary>
    /// 社員アカウントを永続化する
    /// </summary>
    /// <param name="employeeAccount">永続化する社員アカウント</param>
    /// <returns>なし</returns>
    Task CreateAsync(EmployeeAccount employeeAccount);
}