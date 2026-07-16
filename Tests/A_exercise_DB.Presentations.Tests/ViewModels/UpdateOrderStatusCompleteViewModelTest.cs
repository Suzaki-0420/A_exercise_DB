using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// UpdateOrderStatusCompleteViewModelの単体テスト
/// </summary>
[TestClass]
public class UpdateOrderStatusCompleteViewModelTest
{
    /// <summary>
    /// 注文ステータス更新完了ViewModelの初期値が正しいこと
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusCompleteViewModel_初期値が正しい")]
    public void UpdateOrderStatusCompleteViewModel_DefaultValuesAreCorrect()
    {
        // Act
        var viewModel = new UpdateOrderStatusCompleteViewModel();

        // Assert
        Assert.AreEqual("注文ステータスを更新しました", viewModel.CompleteMsg);
        Assert.AreEqual(Guid.Empty, viewModel.OrderNumber);
        Assert.AreEqual(string.Empty, viewModel.OrderStatus);
        Assert.AreEqual(string.Empty, viewModel.UpdateDate);
        Assert.AreEqual("/admin/order/search", viewModel.SearchUrl);
        Assert.AreEqual("/admin", viewModel.HomeUrl);
    }

    /// <summary>
    /// 注文ステータス更新完了ViewModelのプロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusCompleteViewModel_プロパティに値を設定できる")]
    public void UpdateOrderStatusCompleteViewModel_CanSetProperties()
    {
        // Arrange
        var orderNumber = Guid.NewGuid();
        var viewModel = new UpdateOrderStatusCompleteViewModel();

        // Act
        viewModel.CompleteMsg = "更新が完了しました";
        viewModel.OrderNumber = orderNumber;
        viewModel.OrderStatus = "発送準備中";
        viewModel.UpdateDate = "2026/07/10 10:30:45";
        viewModel.SearchUrl = "/admin/order/search/result";
        viewModel.HomeUrl = "/admin/top";

        // Assert
        Assert.AreEqual("更新が完了しました", viewModel.CompleteMsg);
        Assert.AreEqual(orderNumber, viewModel.OrderNumber);
        Assert.AreEqual("発送準備中", viewModel.OrderStatus);
        Assert.AreEqual("2026/07/10 10:30:45", viewModel.UpdateDate);
        Assert.AreEqual("/admin/order/search/result", viewModel.SearchUrl);
        Assert.AreEqual("/admin/top", viewModel.HomeUrl);
    }
}