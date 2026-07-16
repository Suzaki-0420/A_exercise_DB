using System.ComponentModel.DataAnnotations;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// UpdateOrderStatusInputViewModelの単体テスト
/// </summary>
[TestClass]
public class UpdateOrderStatusInputViewModelTest
{
    /// <summary>
    /// 注文ステータス更新入力ViewModelの初期値が正しいこと
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusInputViewModel_初期値が正しい")]
    public void UpdateOrderStatusInputViewModel_DefaultValuesAreCorrect()
    {
        // Act
        var viewModel = new UpdateOrderStatusInputViewModel();

        // Assert
        Assert.AreEqual(Guid.Empty, viewModel.OrderId);
        Assert.AreEqual(string.Empty, viewModel.OrderDate);
        Assert.AreEqual(string.Empty, viewModel.CustomerAccountName);
        Assert.AreEqual(string.Empty, viewModel.OrderContent);
        Assert.AreEqual(0, viewModel.OrderStatus);
        Assert.IsNotNull(viewModel.OrderStatusList);
        Assert.IsEmpty(viewModel.OrderStatusList);
    }

    /// <summary>
    /// 注文ステータス更新入力ViewModelのプロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusInputViewModel_プロパティに値を設定できる")]
    public void UpdateOrderStatusInputViewModel_CanSetProperties()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var orderStatusList = new List<OrderStatusOptionViewModel>
        {
            new OrderStatusOptionViewModel
            {
                Id = 1,
                Name = "注文受付"
            },
            new OrderStatusOptionViewModel
            {
                Id = 2,
                Name = "発送準備中"
            }
        };

        var viewModel = new UpdateOrderStatusInputViewModel();

        // Act
        viewModel.OrderId = orderId;
        viewModel.OrderDate = "2026/07/10 10:30:45";
        viewModel.CustomerAccountName = "yamada01";
        viewModel.OrderContent = "食品 × 2";
        viewModel.OrderStatus = 2;
        viewModel.OrderStatusList = orderStatusList;

        // Assert
        Assert.AreEqual(orderId, viewModel.OrderId);
        Assert.AreEqual("2026/07/10 10:30:45", viewModel.OrderDate);
        Assert.AreEqual("yamada01", viewModel.CustomerAccountName);
        Assert.AreEqual("食品 × 2", viewModel.OrderContent);
        Assert.AreEqual(2, viewModel.OrderStatus);
        Assert.AreSame(orderStatusList, viewModel.OrderStatusList);
        Assert.HasCount(2, viewModel.OrderStatusList);
        Assert.AreEqual(1, viewModel.OrderStatusList[0].Id);
        Assert.AreEqual("注文受付", viewModel.OrderStatusList[0].Name);
        Assert.AreEqual(2, viewModel.OrderStatusList[1].Id);
        Assert.AreEqual("発送準備中", viewModel.OrderStatusList[1].Name);
    }

    /// <summary>
    /// 注文ステータスが未選択の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusInputViewModel_注文ステータスが未選択の場合は入力検証エラーになる")]
    public void UpdateOrderStatusInputViewModel_WhenOrderStatusIsNotSelected_ReturnsValidationError()
    {
        // Arrange
        var viewModel = new UpdateOrderStatusInputViewModel
        {
            OrderId = Guid.NewGuid(),
            OrderDate = "2026/07/10 10:30:45",
            CustomerAccountName = "yamada01",
            OrderContent = "食品 × 2",
            OrderStatus = 0,
            OrderStatusList = new List<OrderStatusOptionViewModel>
            {
                new OrderStatusOptionViewModel
                {
                    Id = 1,
                    Name = "注文受付"
                }
            }
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// 注文ステータスが設定されている場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusInputViewModel_注文ステータスが設定されている場合は入力検証エラーにならない")]
    public void UpdateOrderStatusInputViewModel_WhenOrderStatusIsSelected_ReturnsNoValidationError()
    {
        // Arrange
        var viewModel = new UpdateOrderStatusInputViewModel
        {
            OrderId = Guid.NewGuid(),
            OrderDate = "2026/07/10 10:30:45",
            CustomerAccountName = "yamada01",
            OrderContent = "食品 × 2",
            OrderStatus = 1,
            OrderStatusList = new List<OrderStatusOptionViewModel>
            {
                new OrderStatusOptionViewModel
                {
                    Id = 1,
                    Name = "注文受付"
                }
            }
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// ViewModelの入力検証を実行する
    /// </summary>
    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);

        Validator.TryValidateObject(
            model,
            validationContext,
            validationResults,
            true);

        return validationResults;
    }
}