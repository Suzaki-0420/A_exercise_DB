namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 担当者アカウント登録完了用ViewModel
/// </summary>
public class RegisterEmployeeAccountCompleteViewModel
{
    /// <summary>
    /// 完了メッセージ
    /// </summary>
    public string CompleteMessage { get; set; } = "アカウント登録が完了しました";

    /// <summary>
    /// アカウントUUID
    /// </summary>
    public Guid AccountUuid { get; set; }

    /// <summary>
    /// 社員名
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// アカウント名
    /// </summary>
    public string AccountName { get; set; } = string.Empty;
}