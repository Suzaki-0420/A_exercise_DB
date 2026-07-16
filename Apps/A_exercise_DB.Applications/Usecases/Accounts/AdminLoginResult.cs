namespace A_exercise_DB.Applications.Usecases.Accounts;

/// <summary>
/// 担当者ログイン結果
/// </summary>
public sealed record AdminLoginResult(
    Guid AccountUuid,
    string AccountName,
    string EmployeeName);
