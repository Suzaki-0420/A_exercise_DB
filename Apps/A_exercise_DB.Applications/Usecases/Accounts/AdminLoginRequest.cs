namespace A_exercise_DB.Applications.Usecases.EmployeeAccounts;

/// <summary>
/// 担当者ログインリクエスト
/// </summary>
public sealed record AdminLoginRequest(
    string AccountName,
    string Password);
