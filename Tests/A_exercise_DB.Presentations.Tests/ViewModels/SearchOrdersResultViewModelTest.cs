using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// SearchOrdersResultViewModelの単体テスト
/// </summary>
[TestClass]
public class SearchOrdersResultViewModelTest
{
    /// <summary>
    /// デフォルト値でインスタンスを生成できること
    /// </summary>
    [TestMethod(DisplayName = "SearchOrdersResultViewModel_デフォルト値でインスタンスを生成できる")]
    public void SearchOrdersResultViewModel_DefaultValue_CanCreateInstance()
    {
        // Act
        var model = new SearchOrdersResultViewModel();

        // Assert
        Assert.AreEqual("購入履歴検索", model.Title);
        Assert.IsNotNull(model.OrderList);
        Assert.IsEmpty(model.OrderList);
        Assert.IsNull(model.Message);
    }

    /// <summary>
    /// 各プロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "SearchOrdersResultViewModel_各プロパティに値を設定できる")]
    public void SearchOrdersResultViewModel_SetProperties_CanGetSameValues()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var orderList = new List<SearchOrdersListItemViewModel>
        {
            new SearchOrdersListItemViewModel
            {
                OrderUuid = orderUuid,
                OrderDate = "2026-07-09 10:30:00",
                CustomerAccountName = "yamada01",
                OrderContent = "食品 × 2",
                OrderStatus = "発送準備中",
                StatusUpdateUrl = "/admin/order/status/update"
            }
        };

        // Act
        var model = new SearchOrdersResultViewModel
        {
            Title = "検索結果",
            OrderList = orderList,
            Message = "1件見つかりました"
        };

        // Assert
        Assert.AreEqual("検索結果", model.Title);
        Assert.AreSame(orderList, model.OrderList);
        Assert.HasCount(1, model.OrderList);
        Assert.AreEqual(orderUuid, model.OrderList[0].OrderUuid);
        Assert.AreEqual("2026-07-09 10:30:00", model.OrderList[0].OrderDate);
        Assert.AreEqual("yamada01", model.OrderList[0].CustomerAccountName);
        Assert.AreEqual("食品 × 2", model.OrderList[0].OrderContent);
        Assert.AreEqual("発送準備中", model.OrderList[0].OrderStatus);
        Assert.AreEqual("/admin/order/status/update", model.OrderList[0].StatusUpdateUrl);
        Assert.AreEqual("1件見つかりました", model.Message);
    }

    /// <summary>
    /// メッセージにnullを設定できること
    /// </summary>
    [TestMethod(DisplayName = "SearchOrdersResultViewModel_Messageにnullを設定できる")]
    public void SearchOrdersResultViewModel_SetMessageNull_CanGetNull()
    {
        // Act
        var model = new SearchOrdersResultViewModel
        {
            Message = null
        };

        // Assert
        Assert.IsNull(model.Message);
    }
}