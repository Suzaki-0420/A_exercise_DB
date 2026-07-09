using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// SearchOrdersViewModelの単体テスト
/// </summary>
[TestClass]
public class SearchOrdersViewModelTest
{
    /// <summary>
    /// デフォルト値でインスタンスを生成できること
    /// </summary>
    [TestMethod(DisplayName = "SearchOrdersViewModel_デフォルト値でインスタンスを生成できる")]
    public void SearchOrdersViewModel_DefaultValue_CanCreateInstance()
    {
        // Act
        var model = new SearchOrdersViewModel();

        // Assert
        Assert.IsNull(model.OrderDate);
        Assert.IsNull(model.CustomerAccountName);
    }

    /// <summary>
    /// 各プロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "SearchOrdersViewModel_各プロパティに値を設定できる")]
    public void SearchOrdersViewModel_SetProperties_CanGetSameValues()
    {
        // Act
        var model = new SearchOrdersViewModel
        {
            OrderDate = "2026-07-09",
            CustomerAccountName = "yamada01"
        };

        // Assert
        Assert.AreEqual("2026-07-09", model.OrderDate);
        Assert.AreEqual("yamada01", model.CustomerAccountName);
    }
}