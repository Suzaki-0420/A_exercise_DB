using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// OrderStatusOptionViewModelの単体テスト
/// </summary>
[TestClass]
public class OrderStatusOptionViewModelTest
{
    /// <summary>
    /// 注文ステータス選択肢ViewModelのプロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "OrderStatusOptionViewModel_プロパティに値を設定できる")]
    public void OrderStatusOptionViewModel_CanSetProperties()
    {
        // Arrange
        var viewModel = new OrderStatusOptionViewModel();

        // Act
        viewModel.Id = 1;
        viewModel.Name = "注文受付";

        // Assert
        Assert.AreEqual(1, viewModel.Id);
        Assert.AreEqual("注文受付", viewModel.Name);
    }

    /// <summary>
    /// 注文ステータス名の初期値が空文字であること
    /// </summary>
    [TestMethod(DisplayName = "OrderStatusOptionViewModel_Nameの初期値は空文字である")]
    public void OrderStatusOptionViewModel_NameDefaultValueIsEmpty()
    {
        // Act
        var viewModel = new OrderStatusOptionViewModel();

        // Assert
        Assert.AreEqual(0, viewModel.Id);
        Assert.AreEqual(string.Empty, viewModel.Name);
    }
}