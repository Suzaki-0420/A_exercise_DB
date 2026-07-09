using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// RegisterEmployeeAccountConfirmViewModelの単体テスト
/// </summary>
[TestClass]
public class RegisterEmployeeAccountConfirmViewModelTest
{
    /// <summary>
    /// デフォルト値でインスタンスを生成できること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountConfirmViewModel_デフォルト値でインスタンスを生成できる")]
    public void RegisterEmployeeAccountConfirmViewModel_DefaultValue_CanCreateInstance()
    {
        // Act
        var model = new RegisterEmployeeAccountConfirmViewModel();

        // Assert
        Assert.AreEqual(Guid.Empty, model.EmployeeUuid);
        Assert.AreEqual(string.Empty, model.EmployeeName);
        Assert.AreEqual(string.Empty, model.AccountName);
        Assert.AreEqual("********", model.Password);
    }

    /// <summary>
    /// 各プロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountConfirmViewModel_各プロパティに値を設定できる")]
    public void RegisterEmployeeAccountConfirmViewModel_SetProperties_CanGetSameValues()
    {
        // Arrange
        var employeeUuid = Guid.NewGuid();

        // Act
        var model = new RegisterEmployeeAccountConfirmViewModel
        {
            EmployeeUuid = employeeUuid,
            EmployeeName = "山田太郎",
            AccountName = "yamada01",
            Password = "password01"
        };

        // Assert
        Assert.AreEqual(employeeUuid, model.EmployeeUuid);
        Assert.AreEqual("山田太郎", model.EmployeeName);
        Assert.AreEqual("yamada01", model.AccountName);
        Assert.AreEqual("password01", model.Password);
    }
}