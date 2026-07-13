using A_exercise_DB.Applications.Usecases.Accounts;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Controllers;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace A_exercise_DB.Presentations.Tests.Controllers;

/// <summary>
/// AdminAuthControllerの単体テスト
/// </summary>
[TestClass]
public class AdminAuthControllerTest
{
    private Mock<ILoginAdminUsecase> _loginAdminUsecaseMock = null!;
    private Mock<IAuthenticationService> _authenticationServiceMock = null!;
    private AdminAuthController _controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _loginAdminUsecaseMock = new Mock<ILoginAdminUsecase>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();

        var services = new ServiceCollection();
        services.AddSingleton(_authenticationServiceMock.Object);

        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        _controller = new AdminAuthController(
            _loginAdminUsecaseMock.Object
        );

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    /// <summary>
    /// ログイン情報がnullの場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ログイン情報がnullの場合、BadRequestを返す")]
    public async Task LoginAsync_WhenRequestIsNull_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.LoginAsync(null);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        _loginAdminUsecaseMock.Verify(
            x => x.LoginAsync(It.IsAny<AdminLoginRequest>()),
            Times.Never
        );

        _authenticationServiceMock.Verify(
            x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties?>()),
            Times.Never
        );
    }

    /// <summary>
    /// ログインに成功した場合、Okを返して認証Cookieを発行すること
    /// </summary>
    [TestMethod(DisplayName = "ログインに成功した場合、Okを返して認証Cookieを発行する")]
    public async Task LoginAsync_WhenLoginSucceeds_ShouldReturnOkAndSignIn()
    {
        // Arrange
        var request = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = "pass01"
        };

        var loginResult = new AdminLoginResult(
            Guid.NewGuid(),
            "admin01",
            "山田太郎"
        );

        _loginAdminUsecaseMock
            .Setup(x => x.LoginAsync(
                It.Is<AdminLoginRequest>(r =>
                    r.AccountName == request.AccountName &&
                    r.Password == request.Password)))
            .ReturnsAsync(loginResult);

        _authenticationServiceMock
            .Setup(x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        _loginAdminUsecaseMock.Verify(
            x => x.LoginAsync(
                It.Is<AdminLoginRequest>(r =>
                    r.AccountName == request.AccountName &&
                    r.Password == request.Password)),
            Times.Once
        );

        _authenticationServiceMock.Verify(
            x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                "Cookies",
                It.Is<System.Security.Claims.ClaimsPrincipal>(p =>
                    p.Identity != null &&
                    p.Identity.IsAuthenticated &&
                    p.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value == loginResult.AccountUuid.ToString() &&
                    p.FindFirst(System.Security.Claims.ClaimTypes.Name)!.Value == loginResult.AccountName &&
                    p.FindFirst("employeeName")!.Value == loginResult.EmployeeName),
                It.Is<AuthenticationProperties>(p =>
                    p.IsPersistent == false &&
                    p.AllowRefresh == true &&
                    p.ExpiresUtc != null)),
            Times.Once
        );
    }

    /// <summary>
    /// DomainExceptionが発生した場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "DomainExceptionが発生した場合、BadRequestを返す")]
    public async Task LoginAsync_WhenDomainExceptionThrown_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new AdminLoginViewModel
        {
            AccountName = "",
            Password = "pass01"
        };

        _loginAdminUsecaseMock
            .Setup(x => x.LoginAsync(It.IsAny<AdminLoginRequest>()))
            .ThrowsAsync(new DomainException("アカウント名を入力してください。"));

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        _loginAdminUsecaseMock.Verify(
            x => x.LoginAsync(
                It.Is<AdminLoginRequest>(r =>
                    r.AccountName == request.AccountName &&
                    r.Password == request.Password)),
            Times.Once
        );

        _authenticationServiceMock.Verify(
            x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties?>()),
            Times.Never
        );
    }

    /// <summary>
    /// UnauthorizedAccessExceptionが発生した場合、Unauthorizedを返すこと
    /// </summary>
    [TestMethod(DisplayName = "UnauthorizedAccessExceptionが発生した場合、Unauthorizedを返す")]
    public async Task LoginAsync_WhenUnauthorizedAccessExceptionThrown_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = "wrongpass"
        };

        _loginAdminUsecaseMock
            .Setup(x => x.LoginAsync(It.IsAny<AdminLoginRequest>()))
            .ThrowsAsync(new UnauthorizedAccessException(
                "アカウント名またはパスワードが正しくありません。"));

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);

        _loginAdminUsecaseMock.Verify(
            x => x.LoginAsync(
                It.Is<AdminLoginRequest>(r =>
                    r.AccountName == request.AccountName &&
                    r.Password == request.Password)),
            Times.Once
        );

        _authenticationServiceMock.Verify(
            x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties?>()),
            Times.Never
        );
    }

    /// <summary>
    /// AccountLockedExceptionが発生した場合、Forbiddenを返すこと
    /// </summary>
    [TestMethod(DisplayName = "AccountLockedExceptionが発生した場合、Forbiddenを返す")]
    public async Task LoginAsync_WhenAccountLockedExceptionThrown_ShouldReturnForbidden()
    {
        // Arrange
        var request = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = "pass01"
        };

        _loginAdminUsecaseMock
            .Setup(x => x.LoginAsync(It.IsAny<AdminLoginRequest>()))
            .ThrowsAsync(new AccountLockedException(
                "ログインに5回失敗したため、10分間ログインできません。"));

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(403, objectResult.StatusCode);

        var response = objectResult.Value as ApiResponse<AdminLoginResult>;
        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.HasCount(1, response.Errors);
        Assert.AreEqual("ACCOUNT_LOCKED", response.Errors[0].Code);
        Assert.AreEqual(
            "ログインに5回失敗したため、10分間ログインできません。",
            response.Errors[0].Message);

        _authenticationServiceMock.Verify(
            x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties?>()),
            Times.Never
        );
    }

    /// <summary>
    /// InternalExceptionが発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "InternalExceptionが発生した場合、InternalServerErrorを返す")]
    public async Task LoginAsync_WhenInternalExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = "pass01"
        };

        _loginAdminUsecaseMock
            .Setup(x => x.LoginAsync(It.IsAny<AdminLoginRequest>()))
            .ThrowsAsync(new InternalException("DB接続エラー"));

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);

        _loginAdminUsecaseMock.Verify(
            x => x.LoginAsync(
                It.Is<AdminLoginRequest>(r =>
                    r.AccountName == request.AccountName &&
                    r.Password == request.Password)),
            Times.Once
        );

        _authenticationServiceMock.Verify(
            x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties?>()),
            Times.Never
        );
    }

    /// <summary>
    /// ログアウトに成功した場合、認証Cookieを破棄してOkを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ログアウトに成功した場合、認証Cookieを破棄してOkを返す")]
    public async Task LogoutAsync_ShouldSignOutAndReturnOk()
    {
        // Arrange
        _authenticationServiceMock
            .Setup(x => x.SignOutAsync(
                It.IsAny<HttpContext>(),
                "Cookies",
                It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LogoutAsync();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var response = okResult.Value as ApiResponse<AdminLogoutResult>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.IsNotNull(response.Data);
        Assert.IsTrue(response.Data.LoggedOut);
        Assert.IsEmpty(response.Errors);

        _authenticationServiceMock.Verify(
            x => x.SignOutAsync(
                It.IsAny<HttpContext>(),
                "Cookies",
                It.IsAny<AuthenticationProperties?>()),
            Times.Once
        );
    }

    /// <summary>
    /// ログアウトAPIに認証が必要であること
    /// </summary>
    [TestMethod(DisplayName = "ログアウトAPIにAuthorize属性が設定されている")]
    public void LogoutAsync_ShouldRequireAuthorization()
    {
        var method = typeof(AdminAuthController).GetMethod(
            nameof(AdminAuthController.LogoutAsync));

        Assert.IsNotNull(method);
        Assert.IsNotNull(method.GetCustomAttributes(
            typeof(AuthorizeAttribute),
            inherit: true).SingleOrDefault());
    }

    /// <summary>
    /// ログアウトに成功した場合、認証Cookieを削除してOkを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ログアウトに成功した場合、認証Cookieを削除してOkを返す")]
    public async Task LogoutAsync_WhenLogoutSucceeds_ShouldSignOutAndReturnOk()
    {
        // Arrange
        _authenticationServiceMock
            .Setup(x => x.SignOutAsync(
                It.IsAny<HttpContext>(),
                "Cookies",
                It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LogoutAsync();

        // Assert
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.IsNotNull(okResult.Value);

        _authenticationServiceMock.Verify(
            x => x.SignOutAsync(
                It.Is<HttpContext>(context =>
                    context == _controller.HttpContext),
                "Cookies",
                It.Is<AuthenticationProperties?>(properties =>
                    properties == null)),
            Times.Once);
    }
}
