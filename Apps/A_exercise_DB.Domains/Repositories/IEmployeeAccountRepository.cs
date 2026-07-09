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

    /// <summary>
    /// アカウント名に一致する社員アカウントを取得する
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>社員アカウント。存在しない場合はnull</returns>
    Task<EmployeeAccount?> FindByNameAsync(string accountName);
}