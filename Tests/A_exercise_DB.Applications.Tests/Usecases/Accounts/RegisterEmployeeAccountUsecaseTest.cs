using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Applications.Security;
using A_exercise_DB.Applications.Usecases;
using A_exercise_DB.Applications.Usecases.Accounts;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace A_exercise_DB.Applications.Tests.Usecases.Accounts;

/// <summary>
/// RegisterEmployeeAccountUsecaseの単体テスト
/// </summary>
[TestClass]
public class RegisterEmployeeAccountUsecaseTest
{
    private Mock<IEmployeeRepository> _employeeRepositoryMock = null!;
    private Mock<IEmployeeAccountRepository> _employeeAccountRepositoryMock = null!;
    private Mock<IPasswordHashingService> _passwordHashingServiceMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private RegisterEmployeeAccountUsecase _usecase = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _employeeAccountRepositoryMock = new Mock<IEmployeeAccountRepository>();
        _passwordHashingServiceMock = new Mock<IPasswordHashingService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        _usecase = new RegisterEmployeeAccountUsecase(
            _employeeRepositoryMock.Object,
            _employeeAccountRepositoryMock.Object,
            _passwordHashingServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// アカウント未登録の社員一覧を取得できること
    /// </summary>
    [TestMethod(DisplayName = "GetUnregisteredEmployeesAsync_アカウント未登録の社員一覧を取得できる")]
    public async Task GetUnregisteredEmployeesAsync_ReturnsEmployees()
    {
        // Arrange
        var employees = new List<Employee>
        {
            CreateEmployee(Guid.NewGuid(), "山田太郎")
        };

        _employeeRepositoryMock
            .Setup(x => x.FindAllWithoutAccountAsync())
            .ReturnsAsync(employees);

        // Act
        var actual = await _usecase.GetUnregisteredEmployeesAsync();

        // Assert
        Assert.AreSame(employees, actual);

        _employeeRepositoryMock.Verify(
            x => x.FindAllWithoutAccountAsync(),
            Times.Once);
    }

    /// <summary>
    /// アカウント未登録の社員が存在しない場合、NotFoundExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "GetUnregisteredEmployeesAsync_アカウント未登録社員が存在しない場合はNotFoundExceptionをスローする")]
    public async Task GetUnregisteredEmployeesAsync_WhenEmployeesEmpty_ThrowsExactlyNotFoundException()
    {
        // Arrange
        _employeeRepositoryMock
            .Setup(x => x.FindAllWithoutAccountAsync())
            .ReturnsAsync(new List<Employee>());

        // Act
        var exception = await Assert.ThrowsExactlyAsync<NotFoundException>(async () =>
        {
            await _usecase.GetUnregisteredEmployeesAsync();
        });

        // Assert
        Assert.AreEqual("アカウント登録可能な社員が存在しません", exception.Message);
    }

    /// <summary>
    /// アカウント名が未入力の場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ExistsByAccountNameAsync_アカウント名が未入力の場合はDomainExceptionをスローする")]
    public async Task ExistsByAccountNameAsync_WhenAccountNameIsEmpty_ThrowsExactlyDomainException()
    {
        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await _usecase.ExistsByAccountNameAsync(" ");
        });

        // Assert
        Assert.AreEqual("アカウント名を入力してください", exception.Message);

        _employeeAccountRepositoryMock.Verify(
            x => x.ExistsByAccountNameAsync(It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// アカウント名が存在しない場合、例外をスローしないこと
    /// </summary>
    [TestMethod(DisplayName = "ExistsByAccountNameAsync_アカウント名が存在しない場合は例外をスローしない")]
    public async Task ExistsByAccountNameAsync_WhenAccountNameDoesNotExist_DoesNotThrow()
    {
        // Arrange
        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada"))
            .ReturnsAsync(false);

        // Act
        await _usecase.ExistsByAccountNameAsync("yamada");

        // Assert
        _employeeAccountRepositoryMock.Verify(
            x => x.ExistsByAccountNameAsync("yamada"),
            Times.Once);
    }

    /// <summary>
    /// アカウント名が既に存在する場合、ExistsExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ExistsByAccountNameAsync_アカウント名が既に存在する場合はExistsExceptionをスローする")]
    public async Task ExistsByAccountNameAsync_WhenAccountNameExists_ThrowsExactlyExistsException()
    {
        // Arrange
        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada"))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<ExistsException>(async () =>
        {
            await _usecase.ExistsByAccountNameAsync("yamada");
        });

        // Assert
        Assert.AreEqual("このアカウント名は既に使用されています", exception.Message);
    }

    /// <summary>
    /// 担当者アカウントを登録できること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountAsync_担当者アカウントを登録できる")]
    public async Task RegisterEmployeeAccountAsync_CanRegisterEmployeeAccount()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();
        var employee = CreateEmployee(employeeUuid, "山田太郎");
        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "yamada",
            "password",
            employee);

        _employeeRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(true);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(false);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada"))
            .ReturnsAsync(false);

        _passwordHashingServiceMock
            .Setup(x => x.Hash("password"))
            .Returns("hashedPassword");

        EmployeeAccount? createdAccount = null;

        _employeeAccountRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<EmployeeAccount>()))
            .Callback<EmployeeAccount>(x => createdAccount = x)
            .Returns(Task.CompletedTask);

        // Act
        await _usecase.RegisterEmployeeAccountAsync(employeeAccount);

        // Assert
        Assert.IsNotNull(createdAccount);
        Assert.AreEqual(employeeAccount.AccountUuid, createdAccount.AccountUuid);
        Assert.AreEqual("yamada", createdAccount.Name);
        Assert.AreEqual("hashedPassword", createdAccount.Password);
        Assert.AreSame(employee, createdAccount.Employee);

        _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);

        _employeeRepositoryMock.Verify(
            x => x.ExistsByEmployeeUuidAsync(employeeUuid),
            Times.Once);

        _employeeAccountRepositoryMock.Verify(
            x => x.ExistsByEmployeeUuidAsync(employeeUuid),
            Times.Once);

        _employeeAccountRepositoryMock.Verify(
            x => x.ExistsByAccountNameAsync("yamada"),
            Times.Once);

        _passwordHashingServiceMock.Verify(
            x => x.Hash("password"),
            Times.Once);

        _employeeAccountRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<EmployeeAccount>()),
            Times.Once);
    }

    /// <summary>
    /// 引数employeeAccountがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountAsync_employeeAccountがnullの場合はInternalExceptionをスローする")]
    public async Task RegisterEmployeeAccountAsync_WhenEmployeeAccountIsNull_ThrowsExactlyInternalException()
    {
        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _usecase.RegisterEmployeeAccountAsync(null!);
        });

        // Assert
        Assert.AreEqual("引数employeeAccountがnullです。", exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
    }

    /// <summary>
    /// 社員が未選択の場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountAsync_社員が未選択の場合はDomainExceptionをスローする")]
    public async Task RegisterEmployeeAccountAsync_WhenEmployeeIsNull_ThrowsExactlyDomainException()
    {
        // Arrange
        var employeeAccount = CreateEmployeeAccount(
            Guid.NewGuid(),
            "yamada",
            "password",
            null);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await _usecase.RegisterEmployeeAccountAsync(employeeAccount);
        });

        // Assert
        Assert.AreEqual("社員名を選択してください", exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
    }

    /// <summary>
    /// 選択された社員が存在しない場合、NotFoundExceptionをスローしてロールバックすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountAsync_選択された社員が存在しない場合はNotFoundExceptionをスローしてロールバックする")]
    public async Task RegisterEmployeeAccountAsync_WhenEmployeeDoesNotExist_ThrowsExactlyNotFoundExceptionAndRollback()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();
        var employee = CreateEmployee(employeeUuid, "山田太郎");
        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "yamada",
            "password",
            employee);

        _employeeRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(false);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<NotFoundException>(async () =>
        {
            await _usecase.RegisterEmployeeAccountAsync(employeeAccount);
        });

        // Assert
        Assert.AreEqual("選択された社員が存在しません", exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);

        _employeeAccountRepositoryMock.Verify(
            x => x.ExistsByEmployeeUuidAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    /// <summary>
    /// 選択された社員が既にアカウント登録済みの場合、ExistsExceptionをスローしてロールバックすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountAsync_選択された社員が登録済みの場合はExistsExceptionをスローしてロールバックする")]
    public async Task RegisterEmployeeAccountAsync_WhenEmployeeAccountAlreadyExists_ThrowsExactlyExistsExceptionAndRollback()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();
        var employee = CreateEmployee(employeeUuid, "山田太郎");
        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "yamada",
            "password",
            employee);

        _employeeRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(true);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<ExistsException>(async () =>
        {
            await _usecase.RegisterEmployeeAccountAsync(employeeAccount);
        });

        // Assert
        Assert.AreEqual("この社員のアカウントは既に登録されています", exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);

        _employeeAccountRepositoryMock.Verify(
            x => x.ExistsByAccountNameAsync(It.IsAny<string>()),
            Times.Never);

        _passwordHashingServiceMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// アカウント名が既に存在する場合、ExistsExceptionをスローしてロールバックすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountAsync_アカウント名が既に存在する場合はExistsExceptionをスローしてロールバックする")]
    public async Task RegisterEmployeeAccountAsync_WhenAccountNameAlreadyExists_ThrowsExactlyExistsExceptionAndRollback()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();
        var employee = CreateEmployee(employeeUuid, "山田太郎");
        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "yamada",
            "password",
            employee);

        _employeeRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(true);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(false);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada"))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<ExistsException>(async () =>
        {
            await _usecase.RegisterEmployeeAccountAsync(employeeAccount);
        });

        // Assert
        Assert.AreEqual("このアカウント名は既に使用されています", exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);

        _passwordHashingServiceMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never);

        _employeeAccountRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<EmployeeAccount>()),
            Times.Never);
    }

    /// <summary>
    /// パスワードハッシュ化で例外が発生した場合、例外を再スローしてロールバックすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountAsync_パスワードハッシュ化で例外が発生した場合は再スローしてロールバックする")]
    public async Task RegisterEmployeeAccountAsync_WhenHashThrowsException_ThrowsExactlyExceptionAndRollback()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();
        var employee = CreateEmployee(employeeUuid, "山田太郎");
        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "yamada",
            "password",
            employee);

        var expected = new InvalidOperationException("ハッシュ化に失敗しました。");

        _employeeRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(true);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(false);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada"))
            .ReturnsAsync(false);

        _passwordHashingServiceMock
            .Setup(x => x.Hash("password"))
            .Throws(expected);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
        {
            await _usecase.RegisterEmployeeAccountAsync(employeeAccount);
        });

        // Assert
        Assert.AreSame(expected, actual);

        _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);

        _employeeAccountRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<EmployeeAccount>()),
            Times.Never);
    }

    /// <summary>
    /// 担当者アカウント登録で例外が発生した場合、例外を再スローしてロールバックすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountAsync_担当者アカウント登録で例外が発生した場合は再スローしてロールバックする")]
    public async Task RegisterEmployeeAccountAsync_WhenCreateThrowsException_ThrowsExactlyExceptionAndRollback()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();
        var employee = CreateEmployee(employeeUuid, "山田太郎");
        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "yamada",
            "password",
            employee);

        var expected = new InvalidOperationException("登録に失敗しました。");

        _employeeRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(true);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(false);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada"))
            .ReturnsAsync(false);

        _passwordHashingServiceMock
            .Setup(x => x.Hash("password"))
            .Returns("hashedPassword");

        _employeeAccountRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<EmployeeAccount>()))
            .ThrowsAsync(expected);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
        {
            await _usecase.RegisterEmployeeAccountAsync(employeeAccount);
        });

        // Assert
        Assert.AreSame(expected, actual);

        _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);
    }

    /// <summary>
    /// コミットで例外が発生した場合、例外を再スローしてロールバックすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountAsync_コミットで例外が発生した場合は再スローしてロールバックする")]
    public async Task RegisterEmployeeAccountAsync_WhenCommitThrowsException_ThrowsExactlyExceptionAndRollback()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();
        var employee = CreateEmployee(employeeUuid, "山田太郎");
        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "yamada",
            "password",
            employee);

        var expected = new InvalidOperationException("コミットに失敗しました。");

        _employeeRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(true);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByEmployeeUuidAsync(employeeUuid))
            .ReturnsAsync(false);

        _employeeAccountRepositoryMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada"))
            .ReturnsAsync(false);

        _passwordHashingServiceMock
            .Setup(x => x.Hash("password"))
            .Returns("hashedPassword");

        _employeeAccountRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<EmployeeAccount>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync())
            .ThrowsAsync(expected);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
        {
            await _usecase.RegisterEmployeeAccountAsync(employeeAccount);
        });

        // Assert
        Assert.AreSame(expected, actual);

        _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);
    }

    /// <summary>
    /// テスト用のEmployeeを生成する
    /// </summary>
    private static Employee CreateEmployee(Guid employeeUuid, string name)
    {
        var employee = (Employee)RuntimeHelpers.GetUninitializedObject(typeof(Employee));

        SetPrivateProperty(employee, "EmployeeUuid", employeeUuid);
        SetPrivateProperty(employee, "Name", name);

        return employee;
    }

    /// <summary>
    /// テスト用のEmployeeAccountを生成する
    /// </summary>
    private static EmployeeAccount CreateEmployeeAccount(
        Guid accountUuid,
        string name,
        string password,
        Employee? employee)
    {
        var employeeAccount =
            (EmployeeAccount)RuntimeHelpers.GetUninitializedObject(typeof(EmployeeAccount));

        SetPrivateProperty(employeeAccount, "AccountUuid", accountUuid);
        SetPrivateProperty(employeeAccount, "Name", name);
        SetPrivateProperty(employeeAccount, "Password", password);
        SetPrivateProperty(employeeAccount, "Employee", employee);

        return employeeAccount;
    }

    /// <summary>
    /// private setのプロパティへテスト用の値を設定する
    /// </summary>
    private static void SetPrivateProperty<T>(
        T target,
        string propertyName,
        object? value)
    {
        var field = typeof(T).GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (field is null)
        {
            throw new InvalidOperationException(
                $"{propertyName}のバッキングフィールドが見つかりません。");
        }

        field.SetValue(target, value);
    }
}