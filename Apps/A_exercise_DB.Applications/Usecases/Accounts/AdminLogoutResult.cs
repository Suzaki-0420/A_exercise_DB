namespace A_exercise_DB.Applications.Usecases.Accounts;

/// <summary>
/// 担当者ログアウト結果
/// </summary>
public sealed record AdminLogoutResult(bool LoggedOut)
{
    /// <summary>
    /// ログアウト成功結果を生成する
    /// </summary>
    public static AdminLogoutResult CreateLoggedOut()
        => new(true);
}
