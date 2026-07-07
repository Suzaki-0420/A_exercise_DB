using A_exercise_DB.Domains.Exceptions;
namespace A_exercise_DB.Domains.Models;
/// <summary>
/// 社員アカウントを表すドメインオブジェクト
/// </summary>
public class EmployeeAccount
{
    /// <summary>
    /// アカウント識別ID(UUID)
    /// </summary>
    public Guid? AccountUuid { get; private set; }
    /// <summary>
    /// アカウント名
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// パスワード
    /// </summary>
    public string Password { get; private set; } = string.Empty;

    /// <summary>
    /// アカウント名の最大長
    /// </summary>
    private const int MaxLengthAccountName = 20;
    /// <summary>
    /// パスワードの最大長
    /// </summary>
    private const int MaxLengthAccountPass = 255;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public EmployeeAccount(Guid? accountUuid, string accountName, string accountPass)
    {
        ValidateAccountUuid(accountUuid);
        AccountUuid = accountUuid;
        ValidateAccountName(accountName);
        Name = accountName;
        ValidateAccountPass(accountPass);
        Password = accountPass;

    }

    /// <summary>
    /// ID未定の社員を作成する場合のコンストラクタ
    /// </summary>
    public EmployeeAccount(string accountName, string accountPass)
        : this(null, accountName, accountPass) { }

    /// <summary>
    /// アカウント識別IDの検証
    /// </summary>
    private void ValidateAccountUuid(Guid? accountUuid)
    {
        if (accountUuid == Guid.Empty)
            throw new DomainException("アカウント識別IDが不正です");
    }

    /// <summary>
    /// アカウント名の検証
    /// </summary>
    private void ValidateAccountName(string accountName)
    {
        if (string.IsNullOrWhiteSpace(accountName))
            throw new DomainException("アカウント名は必須です");
        if (accountName.Length > MaxLengthAccountName)
            throw new DomainException($"アカウント名は{MaxLengthAccountName}文字以内で入力してください");
    }

    /// <summary>
    /// パスワードの検証
    /// </summary>
    private void ValidateAccountPass(string accountPass)
    {
        if (string.IsNullOrWhiteSpace(accountPass))
            throw new DomainException("パスワードは必須です");
        if (accountPass.Length > MaxLengthAccountPass)
            throw new DomainException($"パスワードは{MaxLengthAccountPass}文字以内で入力してください");
    }

    /// <summary>
    /// 等価性（IDによる比較）
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not EmployeeAccount other) return false;
        return AccountUuid == other.AccountUuid;
    }

    public override int GetHashCode() => AccountUuid?.GetHashCode() ?? 0;

    public override string ToString()
        => $"{AccountUuid?.ToString() ?? "未登録"}: {Name}";
}