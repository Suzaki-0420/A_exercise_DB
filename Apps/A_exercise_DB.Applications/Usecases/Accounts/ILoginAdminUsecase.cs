namespace A_exercise_DB.Applications.Usecases.Accounts;

/// <summary>
/// UC017 担当者ログインユースケースインターフェイス
/// </summary>
public interface ILoginAdminUsecase
{
    /// <summary>
    /// 担当者ログインを実行する
    /// </summary>
    /// <param name="request">担当者ログインリクエスト</param>
    /// <returns>担当者ログイン結果</returns>
    Task<AdminLoginResult> LoginAsync(AdminLoginRequest request);
}
