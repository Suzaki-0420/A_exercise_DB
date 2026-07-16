using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// RegisterEmployeeAccountCompleteViewModelの単体テスト
/// </summary>
[TestClass]
public class RegisterEmployeeAccountCompleteViewModelTest
{
    /// <summary>
    /// デフォルト値でインスタンスを生成できること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountCompleteViewModel_デフォルト値でインスタンスを生成できる")]
    public void RegisterEmployeeAccountCompleteViewModel_DefaultValue_CanCreateInstance()
    {
        // Act
        var model = new RegisterEmployeeAccountCompleteViewModel();

        // Assert
        Assert.AreEqual("アカウント登録が完了しました", model.CompleteMessage);
        Assert.AreEqual(Guid.Empty, model.AccountUuid);
        Assert.AreEqual(string.Empty, model.EmployeeName);
        Assert.AreEqual(string.Empty, model.AccountName);
    }

    /// <summary>
    /// 各プロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountCompleteViewModel_各プロパティに値を設定できる")]
    public void RegisterEmployeeAccountCompleteViewModel_SetProperties_CanGetSameValues()
    {
        // Arrange
        var accountUuid = Guid.NewGuid();

        // Act
        var model = new RegisterEmployeeAccountCompleteViewModel
        {
            CompleteMessage = "登録しました",
            AccountUuid = accountUuid,
            EmployeeName = "山田太郎",
            AccountName = "yamada01"
        };

        // Assert
        Assert.AreEqual("登録しました", model.CompleteMessage);
        Assert.AreEqual(accountUuid, model.AccountUuid);
        Assert.AreEqual("山田太郎", model.EmployeeName);
        Assert.AreEqual("yamada01", model.AccountName);
    }
}