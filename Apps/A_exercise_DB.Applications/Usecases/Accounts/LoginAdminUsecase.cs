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

    private static readonly Regex HalfWidthAlphanumericRegex =
        new("^[a-zA-Z0-9]+$", RegexOptions.Compiled);

    private readonly IEmployeeAccountRepository _employeeAccountRepository;
    private readonly IPasswordHashingService _passwordHashingService;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeAccountRepository">社員アカウントRepository</param>
    /// <param name="passwordHashingService">パスワード検証サービス</param>
    public LoginAdminUsecase(
        IEmployeeAccountRepository employeeAccountRepository,
        IPasswordHashingService passwordHashingService)
    {
        _employeeAccountRepository = employeeAccountRepository;
        _passwordHashingService = passwordHashingService;
    }

    /// <inheritdoc />
    public async Task<AdminLoginResult> LoginAsync(AdminLoginRequest request)
    {
        ValidateRequest(request);

        var employeeAccount = await _employeeAccountRepository.FindByNameAsync(
            request.AccountName);

        if (employeeAccount is null)
        {
            throw new UnauthorizedAccessException(AuthenticationFailedMessage);
        }

        if (!_passwordHashingService.Verify(employeeAccount.Password, request.Password))
        {
            throw new UnauthorizedAccessException(AuthenticationFailedMessage);
        }

        return new AdminLoginResult(
            employeeAccount.AccountUuid,
            employeeAccount.Name,
            employeeAccount.Employee?.Name ?? string.Empty);
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
