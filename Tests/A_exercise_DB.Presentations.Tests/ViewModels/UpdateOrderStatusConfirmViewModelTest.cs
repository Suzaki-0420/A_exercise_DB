using System.ComponentModel.DataAnnotations;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// UpdateOrderStatusConfirmViewModelの単体テスト
/// </summary>
[TestClass]
public class UpdateOrderStatusConfirmViewModelTest
{
    /// <summary>
    /// 注文ステータス更新確認ViewModelの初期値が正しいこと
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusConfirmViewModel_初期値が正しい")]
    public void UpdateOrderStatusConfirmViewModel_DefaultValuesAreCorrect()
    {
        // Act
        var viewModel = new UpdateOrderStatusConfirmViewModel();

        // Assert
        Assert.AreEqual(Guid.Empty, viewModel.OrderId);
        Assert.AreEqual(string.Empty, viewModel.OrderDate);
        Assert.AreEqual(string.Empty, viewModel.CustomerAccountName);
        Assert.AreEqual(string.Empty, viewModel.CurrentStatus);
        Assert.AreEqual(0, viewModel.NewStatusId);
        Assert.AreEqual(string.Empty, viewModel.NewStatus);
    }

    /// <summary>
    /// 注文ステータス更新確認ViewModelのプロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusConfirmViewModel_プロパティに値を設定できる")]
    public void UpdateOrderStatusConfirmViewModel_CanSetProperties()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var viewModel = new UpdateOrderStatusConfirmViewModel();

        // Act
        viewModel.OrderId = orderId;
        viewModel.OrderDate = "2026/07/10 10:30:45";
        viewModel.CustomerAccountName = "yamada01";
        viewModel.CurrentStatus = "注文受付";
        viewModel.NewStatusId = 2;
        viewModel.NewStatus = "発送準備中";

        // Assert
        Assert.AreEqual(orderId, viewModel.OrderId);
        Assert.AreEqual("2026/07/10 10:30:45", viewModel.OrderDate);
        Assert.AreEqual("yamada01", viewModel.CustomerAccountName);
        Assert.AreEqual("注文受付", viewModel.CurrentStatus);
        Assert.AreEqual(2, viewModel.NewStatusId);
        Assert.AreEqual("発送準備中", viewModel.NewStatus);
    }

    /// <summary>
    /// 必須項目の初期値では入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusConfirmViewModel_初期値では入力検証エラーにならない")]
    public void UpdateOrderStatusConfirmViewModel_DefaultValues_ReturnsNoValidationError()
    {
        // Arrange
        var viewModel = new UpdateOrderStatusConfirmViewModel();

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// 必須項目が設定されている場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "UpdateOrderStatusConfirmViewModel_必須項目が設定されている場合は入力検証エラーにならない")]
    public void UpdateOrderStatusConfirmViewModel_WhenRequiredPropertiesAreSet_ReturnsNoValidationError()
    {
        // Arrange
        var viewModel = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = Guid.NewGuid(),
            OrderDate = "2026/07/10 10:30:45",
            CustomerAccountName = "yamada01",
            CurrentStatus = "注文受付",
            NewStatusId = 2,
            NewStatus = "発送準備中"
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