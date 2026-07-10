using System.Security.Claims;
using A_exercise_DB.Applications.Usecases.Accounts;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace A_exercise_DB.Presentations.Controllers;

/// <summary>
/// UC017/UC018 担当者認証API
/// </summary>
[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly ILoginAdminUsecase _loginAdminUsecase;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="loginAdminUsecase">担当者ログインユースケース</param>
    public AdminAuthController(ILoginAdminUsecase loginAdminUsecase)
    {
        _loginAdminUsecase = loginAdminUsecase;
    }

    /// <summary>
    /// 担当者ログインを実行する
    /// </summary>
    /// <param name="request">担当者ログインリクエスト</param>
    /// <returns>担当者ログイン結果</returns>
    [HttpPost("login")]
    [Tags("UC017: 担当者ログイン")]
    [ProducesResponseType(typeof(ApiResponse<AdminLoginResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminLoginResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AdminLoginResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AdminLoginResult>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginAsync([FromBody] AdminLoginViewModel? request)
    {
        if (request is null)
        {
            return BadRequest(ApiResponse<AdminLoginResult>.Fail(
                "VALIDATION_ERROR",
                "ログイン情報を入力してください。",
                nameof(request)));
        }

        try
        {
            var loginRequest = new AdminLoginRequest(
                request.AccountName,
                request.Password);

            var result = await _loginAdminUsecase.LoginAsync(loginRequest);

            await SignInAdminAsync(result);

            return Ok(ApiResponse<AdminLoginResult>.Ok(result));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<AdminLoginResult>.Fail(
                "VALIDATION_ERROR",
                ex.Message,
                null));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AdminLoginResult>.Fail(
                "AUTHENTICATION_FAILED",
                ex.Message,
                null));
        }
        catch (InternalException)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<AdminLoginResult>.Fail(
                    "INTERNAL_ERROR",
                    "担当者ログイン中にエラーが発生しました。",
                    null));
        }
    }

    /// <summary>
    /// 担当者ログアウトを実行する
    /// </summary>
    /// <returns>担当者ログアウト結果</returns>
    [Authorize]
    [HttpPost("logout")]
    [Tags("UC018: 担当者ログアウト")]
    [ProducesResponseType(typeof(ApiResponse<AdminLogoutResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(ApiResponse<AdminLogoutResult>.Ok(
            AdminLogoutResult.CreateLoggedOut()));
    }

    /// <summary>
    /// 担当者認証Cookieを発行する
    /// </summary>
    private async Task SignInAdminAsync(AdminLoginResult result)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.AccountUuid.ToString()),
            new(ClaimTypes.Name, result.AccountName),
            new("employeeName", result.EmployeeName)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = true,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            properties);
    }
}
