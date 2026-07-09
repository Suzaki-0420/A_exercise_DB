namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 担当者アカウント登録確認用ViewModel
/// </summary>
public class RegisterEmployeeAccountConfirmViewModel
{
    /// <summary>
    /// 社員UUID
    /// </summary>
    public Guid EmployeeUuid { get; set; }

    /// <summary>
    /// 社員名
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// アカウント名
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    public string Password { get; set; } = "********";
}