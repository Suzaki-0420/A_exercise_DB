using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Applications.Usecases.Accounts;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.Controllers;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace A_exercise_DB.Presentations.Tests.Controllers;

/// <summary>
/// RegisterEmployeeAccountControllerの単体テスト
/// </summary>
[TestClass]
public class RegisterEmployeeAccountControllerTest
{
    private Mock<IRegisterEmployeeAccountUsecase> _usecaseMock = null!;
    private RegisterEmployeeAccountViewModelAdapter _adapter = null!;
    private Mock<ILogger<RegisterEmployeeAccountController>> _loggerMock = null!;
    private RegisterEmployeeAccountController _controller = null!;

    /// <summary>
    /// 各テスト実行前の初期化処理
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _usecaseMock = new Mock<IRegisterEmployeeAccountUsecase>();
        _adapter = new RegisterEmployeeAccountViewModelAdapter();
        _loggerMock = new Mock<ILogger<RegisterEmployeeAccountController>>();

        _controller = new RegisterEmployeeAccountController(
            _usecaseMock.Object,
            _adapter,
            _loggerMock.Object);
    }

    /// <summary>
    /// GetFormで担当者アカウント登録入力画面の初期表示情報を取得できること
    /// </summary>
    [TestMethod(DisplayName = "GetForm_担当者アカウント登録入力画面の初期表示情報を取得できる")]
    public async Task GetForm_ReturnsOk()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();

        IReadOnlyList<Employee> employees = new List<Employee>
        {
            CreateEmployee(employeeUuid, "山田太郎")
        };

        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ReturnsAsync(employees);

        // Act
        var result = await _controller.GetForm();

        // Assert
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        Assert.AreEqual(
            "担当者アカウント登録(入力)",
            GetPropertyValue<string>(
                okResult.Value!,
                "title"));

        var actualEmployees =
            GetPropertyValue<IEnumerable<object>>(
                okResult.Value!,
                "employees")
            .ToList();

        Assert.HasCount(1, actualEmployees);

        Assert.AreEqual(
            employeeUuid,
            GetPropertyValue<Guid>(
                actualEmployees[0],
                "employeeUuid"));

        Assert.AreEqual(
            "山田太郎",
            GetPropertyValue<string>(
                actualEmployees[0],
                "employeeName"));

        _usecaseMock.Verify(
            x => x.GetUnregisteredEmployeesAsync(),
            Times.Once);
    }

    /// <summary>
    /// GetFormで未登録社員が存在しない場合、NotFoundを返すこと
    /// </summary>
    [TestMethod(DisplayName = "GetForm_未登録社員が存在しない場合はNotFoundを返す")]
    public async Task GetForm_WhenNotFoundExceptionThrown_ReturnsNotFound()
    {
        // Arrange
        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ThrowsAsync(new NotFoundException("アカウント登録可能な社員が存在しません"));

        // Act
        var result = await _controller.GetForm();

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("NO_UNREGISTERED_EMPLOYEE", GetPropertyValue<string>(notFoundResult.Value!, "code"));
        Assert.AreEqual("アカウント登録可能な社員が存在しません", GetPropertyValue<string>(notFoundResult.Value!, "message"));
    }

    /// <summary>
    /// GetFormで予期しない例外が発生した場合、500を返すこと
    /// </summary>
    [TestMethod(DisplayName = "GetForm_予期しない例外が発生した場合はInternalServerErrorを返す")]
    public async Task GetForm_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ThrowsAsync(new InvalidOperationException("DB接続エラーです。"));

        // Act
        var result = await _controller.GetForm();

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("SYSTEM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("社員情報の取得に失敗しました", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// ValidateAccountNameで使用可能なアカウント名の場合、Okを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ValidateAccountName_使用可能なアカウント名の場合はOkを返す")]
    public async Task ValidateAccountName_WhenAccountNameCanUse_ReturnsOk()
    {
        // Arrange
        _usecaseMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada01"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ValidateAccountName("yamada01");

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.IsFalse(GetPropertyValue<bool>(okResult.Value!, "exists"));
        Assert.AreEqual("使用できるアカウント名です", GetPropertyValue<string>(okResult.Value!, "message"));
    }

    /// <summary>
    /// ValidateAccountNameでアカウント名が既に存在する場合、Conflictを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ValidateAccountName_アカウント名が既に存在する場合はConflictを返す")]
    public async Task ValidateAccountName_WhenExistsExceptionThrown_ReturnsConflict()
    {
        // Arrange
        _usecaseMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada01"))
            .ThrowsAsync(new ExistsException("このアカウント名は既に使用されています"));

        // Act
        var result = await _controller.ValidateAccountName("yamada01");

        // Assert
        var conflictResult = result as ConflictObjectResult;
        Assert.IsNotNull(conflictResult);
        Assert.AreEqual(409, conflictResult.StatusCode);
        Assert.AreEqual("ACCOUNT_NAME_ALREADY_EXISTS", GetPropertyValue<string>(conflictResult.Value!, "code"));
        Assert.IsTrue(GetPropertyValue<bool>(conflictResult.Value!, "exists"));
        Assert.AreEqual("このアカウント名は既に使用されています", GetPropertyValue<string>(conflictResult.Value!, "message"));
    }

    /// <summary>
    /// ValidateAccountNameで入力値不正の場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ValidateAccountName_入力値不正の場合はBadRequestを返す")]
    public async Task ValidateAccountName_WhenDomainExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _usecaseMock
            .Setup(x => x.ExistsByAccountNameAsync(""))
            .ThrowsAsync(new DomainException("アカウント名を入力してください"));

        // Act
        var result = await _controller.ValidateAccountName("");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("アカウント名を入力してください", GetPropertyValue<string>(badRequestResult.Value!, "message"));
    }

    /// <summary>
    /// ValidateAccountNameで予期しない例外が発生した場合、500を返すこと
    /// </summary>
    [TestMethod(DisplayName = "ValidateAccountName_予期しない例外が発生した場合はInternalServerErrorを返す")]
    public async Task ValidateAccountName_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _usecaseMock
            .Setup(x => x.ExistsByAccountNameAsync("yamada01"))
            .ThrowsAsync(new InvalidOperationException("DB接続エラーです。"));

        // Act
        var result = await _controller.ValidateAccountName("yamada01");

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("SYSTEM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// Confirmで確認画面用ViewModelを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_確認画面用ViewModelを返す")]
    public async Task Confirm_ReturnsOk()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();

        IReadOnlyList<Employee> employees = new List<Employee>
        {
            CreateEmployee(employeeUuid, "山田太郎")
        };

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = employeeUuid,
            AccountName = "yamada01",
            Password = "pass01"
        };

        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ReturnsAsync(employees);

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var actual = okResult.Value as RegisterEmployeeAccountConfirmViewModel;
        Assert.IsNotNull(actual);
        Assert.AreEqual(employeeUuid, actual.EmployeeUuid);
        Assert.AreEqual("山田太郎", actual.EmployeeName);
        Assert.AreEqual("yamada01", actual.AccountName);
        Assert.AreEqual("********", actual.Password);
    }

    /// <summary>
    /// ConfirmでModelStateが不正な場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_ModelStateが不正な場合はBadRequestを返す")]
    public async Task Confirm_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel();

        _controller.ModelState.AddModelError(
            nameof(RegisterEmployeeAccountViewModel.AccountName),
            "アカウント名を入力してください");

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));

        var messages = GetPropertyValue<List<string>>(badRequestResult.Value!, "messages");
        Assert.HasCount(1, messages);
        Assert.AreEqual("アカウント名を入力してください", messages[0]);
    }

    /// <summary>
    /// Confirmで社員UUIDが未指定の場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_社員UUIDが未指定の場合はBadRequestを返す")]
    public async Task Confirm_WhenEmployeeUuidIsNull_ReturnsBadRequest()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = null,
            AccountName = "yamada01",
            Password = "pass01"
        };

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("社員名を選択してください", GetPropertyValue<string>(badRequestResult.Value!, "message"));
    }

    /// <summary>
    /// Confirmで選択された社員が存在しない場合、NotFoundを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_選択された社員が存在しない場合はNotFoundを返す")]
    public async Task Confirm_WhenEmployeeNotFound_ReturnsNotFound()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();

        IReadOnlyList<Employee> employees = new List<Employee>();

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = employeeUuid,
            AccountName = "yamada01",
            Password = "pass01"
        };

        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ReturnsAsync(employees);

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("EMPLOYEE_NOT_FOUND", GetPropertyValue<string>(notFoundResult.Value!, "code"));
        Assert.AreEqual("選択された社員が存在しません", GetPropertyValue<string>(notFoundResult.Value!, "message"));
    }

    /// <summary>
    /// Confirmで予期しない例外が発生した場合、500を返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_予期しない例外が発生した場合はInternalServerErrorを返す")]
    public async Task Confirm_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = employeeUuid,
            AccountName = "yamada01",
            Password = "pass01"
        };

        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ThrowsAsync(new InvalidOperationException("DB接続エラーです。"));

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("SYSTEM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// Registerで担当者アカウントを登録できること
    /// </summary>
    [TestMethod(DisplayName = "Register_担当者アカウントを登録できる")]
    public async Task Register_ReturnsCreated()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();

        IReadOnlyList<Employee> employees = new List<Employee>
        {
            CreateEmployee(employeeUuid, "山田太郎")
        };

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = employeeUuid,
            AccountName = "yamada01",
            Password = "pass01"
        };

        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ReturnsAsync(employees);

        _usecaseMock
            .Setup(x => x.RegisterEmployeeAccountAsync(It.IsAny<EmployeeAccount>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Register(model);

        // Assert
        var createdResult = result as CreatedResult;
        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.StartsWith("/admin/account/", createdResult.Location);

        var actual = createdResult.Value as RegisterEmployeeAccountCompleteViewModel;
        Assert.IsNotNull(actual);
        Assert.AreEqual("アカウント登録が完了しました", actual.CompleteMessage);
        Assert.AreEqual("山田太郎", actual.EmployeeName);
        Assert.AreEqual("yamada01", actual.AccountName);
        Assert.AreNotEqual(Guid.Empty, actual.AccountUuid);

        _usecaseMock.Verify(
            x => x.RegisterEmployeeAccountAsync(It.Is<EmployeeAccount>(
                account =>
                    account.Name == "yamada01"
                    && account.Password == "pass01"
                    && account.Employee != null
                    && account.Employee.EmployeeUuid == employeeUuid)),
            Times.Once);
    }

    /// <summary>
    /// RegisterでModelStateが不正な場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Register_ModelStateが不正な場合はBadRequestを返す")]
    public async Task Register_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel();

        _controller.ModelState.AddModelError(
            nameof(RegisterEmployeeAccountViewModel.Password),
            "パスワードを入力してください");

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));

        var messages = GetPropertyValue<List<string>>(badRequestResult.Value!, "messages");
        Assert.HasCount(1, messages);
        Assert.AreEqual("パスワードを入力してください", messages[0]);
    }

    /// <summary>
    /// Registerでアカウントが既に存在する場合、Conflictを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Register_アカウントが既に存在する場合はConflictを返す")]
    public async Task Register_WhenExistsExceptionThrown_ReturnsConflict()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();

        IReadOnlyList<Employee> employees = new List<Employee>
        {
            CreateEmployee(employeeUuid, "山田太郎")
        };

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = employeeUuid,
            AccountName = "yamada01",
            Password = "pass01"
        };

        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ReturnsAsync(employees);

        _usecaseMock
            .Setup(x => x.RegisterEmployeeAccountAsync(It.IsAny<EmployeeAccount>()))
            .ThrowsAsync(new ExistsException("このアカウント名は既に使用されています"));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var conflictResult = result as ConflictObjectResult;
        Assert.IsNotNull(conflictResult);
        Assert.AreEqual(409, conflictResult.StatusCode);
        Assert.AreEqual("ACCOUNT_ALREADY_EXISTS", GetPropertyValue<string>(conflictResult.Value!, "code"));
        Assert.AreEqual("このアカウント名は既に使用されています", GetPropertyValue<string>(conflictResult.Value!, "message"));
    }

    /// <summary>
    /// Registerで選択された社員が存在しない場合、NotFoundを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Register_選択された社員が存在しない場合はNotFoundを返す")]
    public async Task Register_WhenEmployeeNotFound_ReturnsNotFound()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "yamada01",
            Password = "pass01"
        };

        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ReturnsAsync(new List<Employee>());

        // Act
        var result = await _controller.Register(model);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("EMPLOYEE_NOT_FOUND", GetPropertyValue<string>(notFoundResult.Value!, "code"));
        Assert.AreEqual("選択された社員が存在しません", GetPropertyValue<string>(notFoundResult.Value!, "message"));
    }

    /// <summary>
    /// Registerで入力値不正の場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Register_入力値不正の場合はBadRequestを返す")]
    public async Task Register_WhenDomainExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.Empty,
            AccountName = "yamada01",
            Password = "pass01"
        };

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("社員名を選択してください", GetPropertyValue<string>(badRequestResult.Value!, "message"));
    }

    /// <summary>
    /// RegisterでInternalExceptionが発生した場合、500を返すこと
    /// </summary>
    [TestMethod(DisplayName = "Register_InternalExceptionが発生した場合はInternalServerErrorを返す")]
    public async Task Register_WhenInternalExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();

        IReadOnlyList<Employee> employees = new List<Employee>
        {
            CreateEmployee(employeeUuid, "山田太郎")
        };

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = employeeUuid,
            AccountName = "yamada01",
            Password = "pass01"
        };

        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ReturnsAsync(employees);

        _usecaseMock
            .Setup(x => x.RegisterEmployeeAccountAsync(It.IsAny<EmployeeAccount>()))
            .ThrowsAsync(new InternalException("登録処理で内部エラーが発生しました。"));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("INTERNAL_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("登録処理に失敗しました。管理者に連絡してください", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// Registerで予期しない例外が発生した場合、500を返すこと
    /// </summary>
    [TestMethod(DisplayName = "Register_予期しない例外が発生した場合はInternalServerErrorを返す")]
    public async Task Register_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();

        IReadOnlyList<Employee> employees = new List<Employee>
        {
            CreateEmployee(employeeUuid, "山田太郎")
        };

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = employeeUuid,
            AccountName = "yamada01",
            Password = "pass01"
        };

        _usecaseMock
            .Setup(x => x.GetUnregisteredEmployeesAsync())
            .ReturnsAsync(employees);

        _usecaseMock
            .Setup(x => x.RegisterEmployeeAccountAsync(It.IsAny<EmployeeAccount>()))
            .ThrowsAsync(new InvalidOperationException("DB接続エラーです。"));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("SYSTEM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("登録処理に失敗しました。管理者に連絡してください", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// テスト用のEmployeeを生成する
    /// </summary>
    private static Employee CreateEmployee(Guid employeeUuid, string name)
    {
        var employee =
            (Employee)RuntimeHelpers.GetUninitializedObject(typeof(Employee));

        SetPrivateProperty(employee, "EmployeeUuid", employeeUuid);
        SetPrivateProperty(employee, "Name", name);

        return employee;
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

    /// <summary>
    /// 匿名型オブジェクトから指定したプロパティの値を取得する
    /// </summary>
    private static T GetPropertyValue<T>(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName);

        if (property is null)
        {
            throw new InvalidOperationException(
                $"{propertyName}プロパティが見つかりません。");
        }

        return (T)property.GetValue(target)!;
    }

    [TestMethod(DisplayName = "Confirm_社員UUIDが空Guidの場合はBadRequestを返す")]
public async Task Confirm_WhenEmployeeUuidIsEmpty_ShouldReturnBadRequest()
{
    // Arrange
    var model = new RegisterEmployeeAccountViewModel
    {
        EmployeeUuid = Guid.Empty,
        AccountName = "yamada01",
        Password = "pass01"
    };

    // Act
    var result = await _controller.Confirm(model);

    // Assert
    var badRequestResult = result as BadRequestObjectResult;

    Assert.IsNotNull(badRequestResult);
    Assert.AreEqual(400, badRequestResult.StatusCode);
    Assert.AreEqual(
        "VALIDATION_ERROR",
        GetPropertyValue<string>(
            badRequestResult.Value!,
            "code"));
    Assert.AreEqual(
        "社員名を選択してください",
        GetPropertyValue<string>(
            badRequestResult.Value!,
            "message"));
}

[TestMethod(DisplayName = "Register_社員UUIDがnullの場合はBadRequestを返す")]
public async Task Register_WhenEmployeeUuidIsNull_ShouldReturnBadRequest()
{
    // Arrange
    var model = new RegisterEmployeeAccountViewModel
    {
        EmployeeUuid = null,
        AccountName = "yamada01",
        Password = "pass01"
    };

    // Act
    var result = await _controller.Register(model);

    // Assert
    var badRequestResult = result as BadRequestObjectResult;

    Assert.IsNotNull(badRequestResult);
    Assert.AreEqual(400, badRequestResult.StatusCode);
    Assert.AreEqual(
        "VALIDATION_ERROR",
        GetPropertyValue<string>(
            badRequestResult.Value!,
            "code"));
    Assert.AreEqual(
        "社員名を選択してください",
        GetPropertyValue<string>(
            badRequestResult.Value!,
            "message"));
}
}