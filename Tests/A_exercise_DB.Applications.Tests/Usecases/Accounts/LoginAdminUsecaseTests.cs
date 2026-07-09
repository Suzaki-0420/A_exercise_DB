using A_exercise_DB.Applications.Security;
using A_exercise_DB.Applications.Usecases.Accounts;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using Moq;

namespace A_exercise_DB.Applications.Tests.Usecases.Accounts;

/// <summary>
/// LoginAdminUsecaseクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Applications/Usecases/Accounts")]
public class LoginAdminUsecaseTests
{
    /// <summary>
    /// 非同期処理で指定した例外がスローされることを検証する
    /// </summary>
    private static async Task<TException> ThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException ex)
        {
            return ex;
        }

        Assert.Fail($"{typeof(TException).Name} がスローされませんでした。");
        throw new InvalidOperationException();
    }

    [TestMethod(DisplayName = "正しいアカウント名とパスワードの場合、ログイン結果を返す")]
    public async Task LoginAsync_WithValidCredential_ShouldReturnLoginResult()
    {
        var accountUuid = Guid.NewGuid();
        var request = new AdminLoginRequest("admin01", "pass01");
        var employee = new Employee(
            Guid.NewGuid(),
            "山田 太郎",
            "ヤマダ タロウ");
        var employeeAccount = new EmployeeAccount(
            accountUuid,
            request.AccountName,
            "hashed-password",
            employee);
        var repositoryMock = new Mock<IEmployeeAccountRepository>(MockBehavior.Strict);
        var passwordHashingServiceMock = new Mock<IPasswordHashingService>(MockBehavior.Strict);

        repositoryMock
            .Setup(r => r.FindByNameAsync(request.AccountName))
            .ReturnsAsync(employeeAccount);

        passwordHashingServiceMock
            .Setup(s => s.Verify(employeeAccount.Password, request.Password))
            .Returns(true);

        var usecase = new LoginAdminUsecase(
            repositoryMock.Object,
            passwordHashingServiceMock.Object);

        var result = await usecase.LoginAsync(request);

        Assert.AreEqual(accountUuid, result.AccountUuid);
        Assert.AreEqual(request.AccountName, result.AccountName);
        Assert.AreEqual(employee.Name, result.EmployeeName);
        repositoryMock.Verify(r => r.FindByNameAsync(request.AccountName), Times.Once);
        passwordHashingServiceMock.Verify(
            s => s.Verify(employeeAccount.Password, request.Password),
            Times.Once);
    }

    [TestMethod(DisplayName = "アカウント名が未入力の場合、DomainExceptionがスローされる")]
    public async Task LoginAsync_WithEmptyAccountName_ShouldThrowDomainException()
    {
        var request = new AdminLoginRequest("", "pass01");
        var repositoryMock = new Mock<IEmployeeAccountRepository>(MockBehavior.Strict);
        var passwordHashingServiceMock = new Mock<IPasswordHashingService>(MockBehavior.Strict);
        var usecase = new LoginAdminUsecase(
            repositoryMock.Object,
            passwordHashingServiceMock.Object);

        var ex = await ThrowsAsync<DomainException>(() =>
            usecase.LoginAsync(request));

        Assert.AreEqual("アカウント名を入力してください。", ex.Message);
        repositoryMock.Verify(r => r.FindByNameAsync(It.IsAny<string>()), Times.Never);
        passwordHashingServiceMock.Verify(
            s => s.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(DisplayName = "パスワードが5文字未満の場合、DomainExceptionがスローされる")]
    public async Task LoginAsync_WithTooShortPassword_ShouldThrowDomainException()
    {
        var request = new AdminLoginRequest("admin01", "p001");
        var repositoryMock = new Mock<IEmployeeAccountRepository>(MockBehavior.Strict);
        var passwordHashingServiceMock = new Mock<IPasswordHashingService>(MockBehavior.Strict);
        var usecase = new LoginAdminUsecase(
            repositoryMock.Object,
            passwordHashingServiceMock.Object);

        var ex = await ThrowsAsync<DomainException>(() =>
            usecase.LoginAsync(request));

        Assert.AreEqual("パスワードは5～20文字で入力してください。", ex.Message);
        repositoryMock.Verify(r => r.FindByNameAsync(It.IsAny<string>()), Times.Never);
        passwordHashingServiceMock.Verify(
            s => s.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(DisplayName = "アカウント名が半角英数字以外の場合、DomainExceptionがスローされる")]
    public async Task LoginAsync_WithInvalidAccountNameFormat_ShouldThrowDomainException()
    {
        var request = new AdminLoginRequest("admin_01", "pass01");
        var repositoryMock = new Mock<IEmployeeAccountRepository>(MockBehavior.Strict);
        var passwordHashingServiceMock = new Mock<IPasswordHashingService>(MockBehavior.Strict);
        var usecase = new LoginAdminUsecase(
            repositoryMock.Object,
            passwordHashingServiceMock.Object);

        var ex = await ThrowsAsync<DomainException>(() =>
            usecase.LoginAsync(request));

        Assert.AreEqual("アカウント名は半角英数字で入力してください。", ex.Message);
        repositoryMock.Verify(r => r.FindByNameAsync(It.IsAny<string>()), Times.Never);
        passwordHashingServiceMock.Verify(
            s => s.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(DisplayName = "アカウント名が存在しない場合、UnauthorizedAccessExceptionがスローされる")]
    public async Task LoginAsync_WhenAccountDoesNotExist_ShouldThrowUnauthorizedAccessException()
    {
        var request = new AdminLoginRequest("admin01", "pass01");
        var repositoryMock = new Mock<IEmployeeAccountRepository>(MockBehavior.Strict);
        var passwordHashingServiceMock = new Mock<IPasswordHashingService>(MockBehavior.Strict);

        repositoryMock
            .Setup(r => r.FindByNameAsync(request.AccountName))
            .ReturnsAsync((EmployeeAccount?)null);

        var usecase = new LoginAdminUsecase(
            repositoryMock.Object,
            passwordHashingServiceMock.Object);

        var ex = await ThrowsAsync<UnauthorizedAccessException>(() =>
            usecase.LoginAsync(request));

        Assert.AreEqual("アカウント名またはパスワードが正しくありません。", ex.Message);
        repositoryMock.Verify(r => r.FindByNameAsync(request.AccountName), Times.Once);
        passwordHashingServiceMock.Verify(
            s => s.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(DisplayName = "パスワードが一致しない場合、UnauthorizedAccessExceptionがスローされる")]
    public async Task LoginAsync_WhenPasswordDoesNotMatch_ShouldThrowUnauthorizedAccessException()
    {
        var request = new AdminLoginRequest("admin01", "pass01");
        var employee = new Employee(
            Guid.NewGuid(),
            "山田 太郎",
            "ヤマダ タロウ");
        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            request.AccountName,
            "hashed-password",
            employee);
        var repositoryMock = new Mock<IEmployeeAccountRepository>(MockBehavior.Strict);
        var passwordHashingServiceMock = new Mock<IPasswordHashingService>(MockBehavior.Strict);

        repositoryMock
            .Setup(r => r.FindByNameAsync(request.AccountName))
            .ReturnsAsync(employeeAccount);

        passwordHashingServiceMock
            .Setup(s => s.Verify(employeeAccount.Password, request.Password))
            .Returns(false);

        var usecase = new LoginAdminUsecase(
            repositoryMock.Object,
            passwordHashingServiceMock.Object);

        var ex = await ThrowsAsync<UnauthorizedAccessException>(() =>
            usecase.LoginAsync(request));

        Assert.AreEqual("アカウント名またはパスワードが正しくありません。", ex.Message);
        repositoryMock.Verify(r => r.FindByNameAsync(request.AccountName), Times.Once);
        passwordHashingServiceMock.Verify(
            s => s.Verify(employeeAccount.Password, request.Password),
            Times.Once);
    }
}
