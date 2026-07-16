namespace A_exercise_DB.Applications.Security;

/// <summary>
/// 担当者ログインの失敗回数とロック状態を管理する
/// </summary>
public interface ILoginAttemptTracker
{
    /// <summary>
    /// アカウントがロック中か確認する
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>ロック中の場合true</returns>
    bool IsLocked(string accountName);

    /// <summary>
    /// ログイン失敗を記録する
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>今回の失敗でロックされた場合true</returns>
    bool RecordFailure(string accountName);

    /// <summary>
    /// ログイン失敗の記録を消去する
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    void Reset(string accountName);
}
