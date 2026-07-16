using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.ViewModels;

namespace A_exercise_DB.Presentations.Adapters;

/// <summary>
/// 担当者アカウント登録ViewModelをドメインオブジェクトへ変換するアダプタ
/// </summary>
public class RegisterEmployeeAccountViewModelAdapter
{
    /// <summary>
    /// RegisterEmployeeAccountViewModelからEmployeeAccountを復元する
    /// </summary>
    /// <param name="model">担当者アカウント登録ViewModel</param>
    /// <param name="employee">選択された社員</param>
    /// <returns>社員アカウントドメイン</returns>
    public Task<EmployeeAccount> RestoreAsync(
        RegisterEmployeeAccountViewModel model,
        Employee employee)
    {
        _ = model ?? throw new InternalException("引数modelがnullです。");
        _ = employee ?? throw new InternalException("引数employeeがnullです。");

        if (model.EmployeeUuid is null)
        {
            throw new DomainException("社員名を選択してください");
        }

        if (model.EmployeeUuid.Value == Guid.Empty)
        {
            throw new DomainException("社員名を選択してください");
        }
        var employeeUuid = model.EmployeeUuid.Value;

        if (employee.EmployeeUuid != employeeUuid)
        {
            throw new DomainException("選択された社員が不正です");
        }

        var employeeAccount = new EmployeeAccount(
            model.AccountName,
            model.Password,
            employee
        );

        return Task.FromResult(employeeAccount);
    }

    /// <summary>
    /// 確認画面用ViewModelへ変換する
    /// </summary>
    /// <param name="model">担当者アカウント登録ViewModel</param>
    /// <param name="employee">選択された社員</param>
    /// <returns>確認画面用ViewModel</returns>
    public Task<RegisterEmployeeAccountConfirmViewModel> ToConfirmViewModelAsync(
        RegisterEmployeeAccountViewModel model,
        Employee employee)
    {
        _ = model ?? throw new InternalException("引数modelがnullです。");
        _ = employee ?? throw new InternalException("引数employeeがnullです。");

        var confirmViewModel = new RegisterEmployeeAccountConfirmViewModel
        {
            EmployeeUuid = employee.EmployeeUuid,
            EmployeeName = employee.Name,
            AccountName = model.AccountName,
            Password = "********"
        };

        return Task.FromResult(confirmViewModel);
    }

    /// <summary>
    /// 完了画面用ViewModelへ変換する
    /// </summary>
    /// <param name="employeeAccount">登録済み社員アカウント</param>
    /// <returns>完了画面用ViewModel</returns>
    public Task<RegisterEmployeeAccountCompleteViewModel> ToCompleteViewModelAsync(
        EmployeeAccount employeeAccount)
    {
        _ = employeeAccount ?? throw new InternalException("引数employeeAccountがnullです。");

        var completeViewModel = new RegisterEmployeeAccountCompleteViewModel
        {
            CompleteMessage = "アカウント登録が完了しました",
            AccountUuid = employeeAccount.AccountUuid,
            EmployeeName = employeeAccount.Employee?.Name ?? string.Empty,
            AccountName = employeeAccount.Name
        };

        return Task.FromResult(completeViewModel);
    }
}