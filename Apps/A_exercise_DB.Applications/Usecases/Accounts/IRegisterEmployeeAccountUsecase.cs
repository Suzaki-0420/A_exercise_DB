using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Applications.Usecases.EmployeeAccounts;

/// <summary>
/// BP03-BP05 担当者アカウント登録ユースケースインターフェイス
/// </summary>
public interface IRegisterEmployeeAccountUsecase
{
    /// <summary>
    /// アカウント未登録の社員一覧を取得する
    /// </summary>
    /// <returns>アカウント未登録の社員一覧</returns>
    Task<IReadOnlyList<Employee>> GetUnregisteredEmployeesAsync();

    /// <summary>
    /// アカウント名が既に存在するかを検証する
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>なし</returns>
    Task ExistsByAccountNameAsync(string accountName);

    /// <summary>
    /// 担当者アカウントを登録する
    /// </summary>
    /// <param name="employeeAccount">登録対象の担当者アカウント</param>
    /// <returns>なし</returns>
    Task RegisterEmployeeAccountAsync(EmployeeAccount employeeAccount);
}