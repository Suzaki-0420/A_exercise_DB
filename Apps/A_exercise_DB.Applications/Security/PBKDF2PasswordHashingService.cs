using Microsoft.AspNetCore.Identity;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;

namespace A_exercise_DB.Applications.Security;

/// <summary>
/// PBKDF2アルゴリズムを利用し、
/// パスワードのハッシュ化と検証機能を提供するインターフェイスの実装
/// </summary>
public class PBKDF2PasswordHashingService : IPasswordHashingService
{
    private readonly IPasswordHasher<EmployeeAccount> _passwordHasher;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="passwordHasher">ASP.NET Core Identityのパスワードハッシュ化・検証</param>
    public PBKDF2PasswordHashingService(IPasswordHasher<EmployeeAccount> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 平文のパスワードをハッシュ化する
    /// </summary>
    /// <param name="rawPassword">平文パスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    public string Hash(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
        {
            throw new DomainException("パスワードは必須です。");
        }

        // HashPasswordはユーザー情報を参照しないためダミーを利用する
        var dummy = new EmployeeAccount(
            Guid.NewGuid(),
            "tmp",
            rawPassword
        );

        return _passwordHasher.HashPassword(dummy, rawPassword);
    }

    /// <summary>
    /// パスワードの比較結果を返す
    /// </summary>
    /// <param name="hashedPassword">ハッシュされたパスワード</param>
    /// <param name="providedPassword">平文のパスワード</param>
    /// <returns>true:一致、false:不一致</returns>
    /// <exception cref="PasswordRehashNeededException">
    /// パスワードは一致したが、ハッシュの形式や強度が古い場合にスローされる
    /// </exception>
    public bool Verify(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new DomainException("ハッシュ化されたパスワードは必須です。");
        }

        if (string.IsNullOrWhiteSpace(providedPassword))
        {
            throw new DomainException("パスワードは必須です。");
        }

        var dummy = new EmployeeAccount(
            Guid.NewGuid(),
            "tmp",
            hashedPassword
        );

        // パスワードを比較検証する
        var result = _passwordHasher.VerifyHashedPassword(
            dummy,
            hashedPassword,
            providedPassword
        );

        return result switch
        {
            // 一致したのでtrueを返す
            PasswordVerificationResult.Success => true,

            // 不一致なのでfalseを返す
            PasswordVerificationResult.Failed => false,

            // 一致したが形式や強度が古いので、PasswordRehashNeededExceptionをスローする
            PasswordVerificationResult.SuccessRehashNeeded =>
                throw new PasswordRehashNeededException("パスワードは認証されたが、再ハッシュが必要です。"),

            _ => false
        };
    }
}