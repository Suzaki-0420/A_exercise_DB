using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.Adapters;

/// <summary>
/// RegisterEmployeeAccountViewModelAdapterの単体テスト
/// </summary>
[TestClass]
public class RegisterEmployeeAccountViewModelAdapterTest
{
    private RegisterEmployeeAccountViewModelAdapter _adapter = null!;

    /// <summary>
    /// 各テスト実行前の初期化処理
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _adapter = new RegisterEmployeeAccountViewModelAdapter();
    }

    /// <summary>
    /// RestoreAsyncでEmployeeAccountを復元できること
    /// </summary>
    [TestMethod(DisplayName = "RestoreAsync_EmployeeAccountを復元できる")]
    public async Task RestoreAsync_CanRestoreEmployeeAccount()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();
        var employee = CreateEmployee(employeeUuid, "山田太郎");

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = employeeUuid,
            AccountName = "yamada01",
            Password = "pass01"
        };

        // Act
        var actual = await _adapter.RestoreAsync(model, employee);

        // Assert
        Assert.IsNotNull(actual);
        Assert.AreEqual("yamada01", actual.Name);
        Assert.AreEqual("pass01", actual.Password);
        Assert.AreSame(employee, actual.Employee);
        Assert.AreNotEqual(Guid.Empty, actual.AccountUuid);
    }

    /// <summary>
    /// RestoreAsyncでmodelがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RestoreAsync_modelがnullの場合はInternalExceptionをスローする")]
    public async Task RestoreAsync_WhenModelIsNull_ThrowsExactlyInternalException()
    {
        // Arrange
        var employee = CreateEmployee(Guid.NewGuid(), "山田太郎");

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _adapter.RestoreAsync(null!, employee);
        });

        // Assert
        Assert.AreEqual("引数modelがnullです。", exception.Message);
    }

    /// <summary>
    /// RestoreAsyncでemployeeがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RestoreAsync_employeeがnullの場合はInternalExceptionをスローする")]
    public async Task RestoreAsync_WhenEmployeeIsNull_ThrowsExactlyInternalException()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "yamada01",
            Password = "pass01"
        };

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _adapter.RestoreAsync(model, null!);
        });

        // Assert
        Assert.AreEqual("引数employeeがnullです。", exception.Message);
    }

    /// <summary>
    /// RestoreAsyncで社員UUIDがnullの場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RestoreAsync_社員UUIDがnullの場合はDomainExceptionをスローする")]
    public async Task RestoreAsync_WhenEmployeeUuidIsNull_ThrowsExactlyDomainException()
    {
        // Arrange
        var employee = CreateEmployee(Guid.NewGuid(), "山田太郎");

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = null,
            AccountName = "yamada01",
            Password = "pass01"
        };

        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await _adapter.RestoreAsync(model, employee);
        });

        // Assert
        Assert.AreEqual("社員名を選択してください", exception.Message);
    }

    /// <summary>
    /// RestoreAsyncで社員UUIDがGuid.Emptyの場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RestoreAsync_社員UUIDがGuidEmptyの場合はDomainExceptionをスローする")]
    public async Task RestoreAsync_WhenEmployeeUuidIsGuidEmpty_ThrowsExactlyDomainException()
    {
        // Arrange
        var employee = CreateEmployee(Guid.NewGuid(), "山田太郎");

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.Empty,
            AccountName = "yamada01",
            Password = "pass01"
        };

        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await _adapter.RestoreAsync(model, employee);
        });

        // Assert
        Assert.AreEqual("社員名を選択してください", exception.Message);
    }

    /// <summary>
    /// RestoreAsyncでViewModelの社員UUIDと社員のUUIDが一致しない場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RestoreAsync_ViewModelの社員UUIDと社員のUUIDが一致しない場合はDomainExceptionをスローする")]
    public async Task RestoreAsync_WhenEmployeeUuidDoesNotMatch_ThrowsExactlyDomainException()
    {
        // Arrange
        var employee = CreateEmployee(Guid.NewGuid(), "山田太郎");

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "yamada01",
            Password = "pass01"
        };

        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await _adapter.RestoreAsync(model, employee);
        });

        // Assert
        Assert.AreEqual("選択された社員が不正です", exception.Message);
    }

    /// <summary>
    /// ToConfirmViewModelAsyncで確認画面用ViewModelへ変換できること
    /// </summary>
    [TestMethod(DisplayName = "ToConfirmViewModelAsync_確認画面用ViewModelへ変換できる")]
    public async Task ToConfirmViewModelAsync_CanConvertToConfirmViewModel()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();
        var employee = CreateEmployee(employeeUuid, "山田太郎");

        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = employeeUuid,
            AccountName = "yamada01",
            Password = "pass01"
        };

        // Act
        var actual = await _adapter.ToConfirmViewModelAsync(model, employee);

        // Assert
        Assert.IsNotNull(actual);
        Assert.AreEqual(employeeUuid, actual.EmployeeUuid);
        Assert.AreEqual("山田太郎", actual.EmployeeName);
        Assert.AreEqual("yamada01", actual.AccountName);
        Assert.AreEqual("********", actual.Password);
    }

    /// <summary>
    /// ToConfirmViewModelAsyncでmodelがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ToConfirmViewModelAsync_modelがnullの場合はInternalExceptionをスローする")]
    public async Task ToConfirmViewModelAsync_WhenModelIsNull_ThrowsExactlyInternalException()
    {
        // Arrange
        var employee = CreateEmployee(Guid.NewGuid(), "山田太郎");

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _adapter.ToConfirmViewModelAsync(null!, employee);
        });

        // Assert
        Assert.AreEqual("引数modelがnullです。", exception.Message);
    }

    /// <summary>
    /// ToConfirmViewModelAsyncでemployeeがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ToConfirmViewModelAsync_employeeがnullの場合はInternalExceptionをスローする")]
    public async Task ToConfirmViewModelAsync_WhenEmployeeIsNull_ThrowsExactlyInternalException()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "yamada01",
            Password = "pass01"
        };

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _adapter.ToConfirmViewModelAsync(model, null!);
        });

        // Assert
        Assert.AreEqual("引数employeeがnullです。", exception.Message);
    }

    /// <summary>
    /// ToCompleteViewModelAsyncで完了画面用ViewModelへ変換できること
    /// </summary>
    [TestMethod(DisplayName = "ToCompleteViewModelAsync_完了画面用ViewModelへ変換できる")]
    public async Task ToCompleteViewModelAsync_CanConvertToCompleteViewModel()
    {
        // Arrange
        var accountUuid = Guid.NewGuid();
        var employee = CreateEmployee(Guid.NewGuid(), "山田太郎");

        var employeeAccount = new EmployeeAccount(
            accountUuid,
            "yamada01",
            "hashedPassword",
            employee);

        // Act
        var actual = await _adapter.ToCompleteViewModelAsync(employeeAccount);

        // Assert
        Assert.IsNotNull(actual);
        Assert.AreEqual("アカウント登録が完了しました", actual.CompleteMessage);
        Assert.AreEqual(accountUuid, actual.AccountUuid);
        Assert.AreEqual("山田太郎", actual.EmployeeName);
        Assert.AreEqual("yamada01", actual.AccountName);
    }

    /// <summary>
    /// ToCompleteViewModelAsyncでemployeeAccountがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ToCompleteViewModelAsync_employeeAccountがnullの場合はInternalExceptionをスローする")]
    public async Task ToCompleteViewModelAsync_WhenEmployeeAccountIsNull_ThrowsExactlyInternalException()
    {
        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _adapter.ToCompleteViewModelAsync(null!);
        });

        // Assert
        Assert.AreEqual("引数employeeAccountがnullです。", exception.Message);
    }

    /// <summary>
    /// ToCompleteViewModelAsyncでEmployeeがnullの場合、社員名が空文字になること
    /// </summary>
    [TestMethod(DisplayName = "ToCompleteViewModelAsync_Employeeがnullの場合は社員名が空文字になる")]
    public async Task ToCompleteViewModelAsync_WhenEmployeeIsNull_EmployeeNameIsEmpty()
    {
        // Arrange
        var accountUuid = Guid.NewGuid();

        var employeeAccount = new EmployeeAccount(
            accountUuid,
            "yamada01",
            "hashedPassword");

        // Act
        var actual = await _adapter.ToCompleteViewModelAsync(employeeAccount);

        // Assert
        Assert.AreEqual("アカウント登録が完了しました", actual.CompleteMessage);
        Assert.AreEqual(accountUuid, actual.AccountUuid);
        Assert.AreEqual(string.Empty, actual.EmployeeName);
        Assert.AreEqual("yamada01", actual.AccountName);
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
}