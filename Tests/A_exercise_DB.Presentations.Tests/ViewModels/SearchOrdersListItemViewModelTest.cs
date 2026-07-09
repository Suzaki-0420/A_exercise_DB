using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// SearchOrdersListItemViewModelの単体テスト
/// </summary>
[TestClass]
public class SearchOrdersListItemViewModelTest
{
    /// <summary>
    /// デフォルト値でインスタンスを生成できること
    /// </summary>
    [TestMethod(DisplayName = "SearchOrdersListItemViewModel_デフォルト値でインスタンスを生成できる")]
    public void SearchOrdersListItemViewModel_DefaultValue_CanCreateInstance()
    {
        // Act
        var model = new SearchOrdersListItemViewModel();

        // Assert
        Assert.AreEqual(Guid.Empty, model.OrderUuid);
        Assert.AreEqual(string.Empty, model.OrderDate);
        Assert.AreEqual(string.Empty, model.CustomerAccountName);
        Assert.AreEqual(string.Empty, model.OrderContent);
        Assert.AreEqual(string.Empty, model.OrderStatus);
        Assert.AreEqual(string.Empty, model.StatusUpdateUrl);
    }

    /// <summary>
    /// 各プロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "SearchOrdersListItemViewModel_各プロパティに値を設定できる")]
    public void SearchOrdersListItemViewModel_SetProperties_CanGetSameValues()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        // Act
        var model = new SearchOrdersListItemViewModel
        {
            OrderUuid = orderUuid,
            OrderDate = "2026-07-09 10:30:00",
            CustomerAccountName = "yamada01",
            OrderContent = "食品 × 2",
            OrderStatus = "発送準備中",
            StatusUpdateUrl = "/admin/order/status/update"
        };

        // Assert
        Assert.AreEqual(orderUuid, model.OrderUuid);
        Assert.AreEqual("2026-07-09 10:30:00", model.OrderDate);
        Assert.AreEqual("yamada01", model.CustomerAccountName);
        Assert.AreEqual("食品 × 2", model.OrderContent);
        Assert.AreEqual("発送準備中", model.OrderStatus);
        Assert.AreEqual("/admin/order/status/update", model.StatusUpdateUrl);
    }
}