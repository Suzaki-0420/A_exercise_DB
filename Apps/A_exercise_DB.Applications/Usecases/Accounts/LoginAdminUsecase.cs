using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using A_exercise_DB.Applications.Security;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Repositories;

namespace A_exercise_DB.Applications.Usecases.Accounts;

/// <summary>
/// UC017 担当者ログインユースケース
/// </summary>
public class LoginAdminUsecase : ILoginAdminUsecase
{
    private const int MinLength = 5;
    private const int MaxLength = 20;
    private const string AuthenticationFailedMessage =
        "アカウント名またはパスワードが正しくありません。";
    private const string AccountLockedMessage =
        "ログインに5回失敗したため、10分間ログインできません。";

    private static readonly Regex HalfWidthAlphanumericRegex =
        new("^[a-zA-Z0-9]+$", RegexOptions.Compiled);

    private readonly IEmployeeAccountRepository _employeeAccountRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ILoginAttemptTracker _loginAttemptTracker;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeAccountRepository">社員アカウントRepository</param>
    /// <param name="passwordHashingService">パスワード検証サービス</param>
    public LoginAdminUsecase(
        IEmployeeAccountRepository employeeAccountRepository,
        IPasswordHashingService passwordHashingService)
        : this(
            employeeAccountRepository,
            passwordHashingService,
            new InMemoryLoginAttemptTracker())
    {
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeAccountRepository">社員アカウントRepository</param>
    /// <param name="passwordHashingService">パスワード検証サービス</param>
    /// <param name="loginAttemptTracker">ログイン失敗回数管理</param>
    public LoginAdminUsecase(
        IEmployeeAccountRepository employeeAccountRepository,
        IPasswordHashingService passwordHashingService,
        ILoginAttemptTracker loginAttemptTracker)
    {
        _employeeAccountRepository = employeeAccountRepository;
        _passwordHashingService = passwordHashingService;
        _loginAttemptTracker = loginAttemptTracker;
    }

    /// <inheritdoc />
    public async Task<AdminLoginResult> LoginAsync(AdminLoginRequest request)
    {
        ValidateRequest(request);

        if (_loginAttemptTracker.IsLocked(request.AccountName))
        {
            throw new AccountLockedException(AccountLockedMessage);
        }

        var employeeAccount = await _employeeAccountRepository.FindByNameAsync(
            request.AccountName);

        if (employeeAccount is null)
        {
            ThrowAuthenticationFailure(request.AccountName);
        }

        if (!_passwordHashingService.Verify(employeeAccount.Password, request.Password))
        {
            ThrowAuthenticationFailure(request.AccountName);
        }

        _loginAttemptTracker.Reset(request.AccountName);

        return new AdminLoginResult(
            employeeAccount.AccountUuid,
            employeeAccount.Name,
            employeeAccount.Employee?.Name ?? string.Empty);
    }

    /// <summary>
    /// 認証失敗を記録して対応する例外をスローする
    /// </summary>
    [DoesNotReturn]
    private void ThrowAuthenticationFailure(string accountName)
    {
        if (_loginAttemptTracker.RecordFailure(accountName))
        {
            throw new AccountLockedException(AccountLockedMessage);
        }

        throw new UnauthorizedAccessException(AuthenticationFailedMessage);
    }

    /// <summary>
    /// 担当者ログインリクエストを検証する
    /// </summary>
    private static void ValidateRequest(AdminLoginRequest request)
    {
        if (request is null)
        {
            throw new DomainException("ログイン情報を入力してください。");
        }

        ValidateAccountName(request.AccountName);
        ValidatePassword(request.Password);
    }

    /// <summary>
    /// アカウント名を検証する
    /// </summary>
    private static void ValidateAccountName(string accountName)
    {
        if (string.IsNullOrWhiteSpace(accountName))
        {
            throw new DomainException("アカウント名を入力してください。");
        }

        if (accountName.Length < MinLength || accountName.Length > MaxLength)
        {
            throw new DomainException("アカウント名は5～20文字で入力してください。");
        }

        if (!HalfWidthAlphanumericRegex.IsMatch(accountName))
        {
            throw new DomainException("アカウント名は半角英数字で入力してください。");
        }
    }

    /// <summary>
    /// パスワードを検証する
    /// </summary>
    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new DomainException("パスワードを入力してください。");
        }

        if (password.Length < MinLength || password.Length > MaxLength)
        {
            throw new DomainException("パスワードは5～20文字で入力してください。");
        }

        if (!HalfWidthAlphanumericRegex.IsMatch(password))
        {
            throw new DomainException("パスワードは半角英数字で入力してください。");
        }
    }
}
