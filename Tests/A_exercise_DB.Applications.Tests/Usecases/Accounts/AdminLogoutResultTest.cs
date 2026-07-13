using A_exercise_DB.Applications.Usecases.Accounts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Applications.Tests.Usecases.Accounts;

/// <summary>
/// AdminLogoutResultのテストクラス
/// </summary>
[TestClass]
public class AdminLogoutResultTests
{
    /// <summary>
    /// CreateLoggedOutを実行するとログアウト成功結果が生成される
    /// </summary>
    [TestMethod(DisplayName = "CreateLoggedOutを実行するとLoggedOutがtrueの結果を生成できる")]
    public void CreateLoggedOut_WhenCalled_ShouldReturnLoggedOutResult()
    {
        // Act
        var result = AdminLogoutResult.CreateLoggedOut();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.LoggedOut);
    }
}