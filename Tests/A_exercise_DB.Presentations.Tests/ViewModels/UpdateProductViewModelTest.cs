using System.ComponentModel.DataAnnotations;
using A_exercise_DB.Presentations.ViewModels;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// UpdateProductViewModelの入力検証テスト
/// </summary>
[TestClass]
[TestCategory("Presentations/ViewModels/Products")]
public class UpdateProductViewModelTest
{
    [TestMethod(DisplayName = "商品修正情報がすべて有効な場合、入力検証に成功する")]
    public void Validate_WithValidValues_ShouldSucceed()
    {
        var viewModel = CreateValidViewModel();

        var validationResults = Validate(viewModel);

        Assert.HasCount(0, validationResults);
    }

    [TestMethod(DisplayName = "商品名が未入力の場合、必須エラーになる")]
    public void Validate_WithEmptyName_ShouldReturnRequiredError()
    {
        var viewModel = CreateValidViewModel();
        viewModel.Name = string.Empty;

        var validationResults = Validate(viewModel);

        AssertValidationError(
            validationResults,
            nameof(UpdateProductViewModel.Name),
            "商品名を入力してください。");
    }

    [TestMethod(DisplayName = "商品名が1文字の場合、文字数エラーになる")]
    public void Validate_WithOneCharacterName_ShouldReturnLengthError()
    {
        var viewModel = CreateValidViewModel();
        viewModel.Name = "A";

        var validationResults = Validate(viewModel);

        AssertValidationError(
            validationResults,
            nameof(UpdateProductViewModel.Name),
            "商品名は2～20文字で入力してください。");
    }

    [TestMethod(DisplayName = "商品名が2文字の場合、入力検証に成功する")]
    public void Validate_WithTwoCharacterName_ShouldSucceed()
    {
        var viewModel = CreateValidViewModel();
        viewModel.Name = "AB";

        var validationResults = Validate(viewModel);

        Assert.HasCount(0, validationResults);
    }

    [TestMethod(DisplayName = "商品名が20文字の場合、入力検証に成功する")]
    public void Validate_WithTwentyCharacterName_ShouldSucceed()
    {
        var viewModel = CreateValidViewModel();
        viewModel.Name = new string('A', 20);

        var validationResults = Validate(viewModel);

        Assert.HasCount(0, validationResults);
    }

    [TestMethod(DisplayName = "商品名が21文字の場合、文字数エラーになる")]
    public void Validate_WithTwentyOneCharacterName_ShouldReturnLengthError()
    {
        var viewModel = CreateValidViewModel();
        viewModel.Name = new string('A', 21);

        var validationResults = Validate(viewModel);

        AssertValidationError(
            validationResults,
            nameof(UpdateProductViewModel.Name),
            "商品名は2～20文字で入力してください。");
    }

    [TestMethod(DisplayName = "価格が0円の場合、入力検証に成功する")]
    public void Validate_WithMinimumPrice_ShouldSucceed()
    {
        var viewModel = CreateValidViewModel();
        viewModel.Price = 0;

        var validationResults = Validate(viewModel);

        Assert.HasCount(0, validationResults);
    }

    [TestMethod(DisplayName = "価格が100万円の場合、入力検証に成功する")]
    public void Validate_WithMaximumPrice_ShouldSucceed()
    {
        var viewModel = CreateValidViewModel();
        viewModel.Price = 1_000_000;

        var validationResults = Validate(viewModel);

        Assert.HasCount(0, validationResults);
    }

    [TestMethod(DisplayName = "価格が負数の場合、範囲エラーになる")]
    public void Validate_WithNegativePrice_ShouldReturnRangeError()
    {
        var viewModel = CreateValidViewModel();
        viewModel.Price = -1;

        var validationResults = Validate(viewModel);

        AssertValidationError(
            validationResults,
            nameof(UpdateProductViewModel.Price),
            "価格は0以上100万円以下で入力してください。");
    }

    [TestMethod(DisplayName = "価格が100万円を超える場合、範囲エラーになる")]
    public void Validate_WithPriceOverMaximum_ShouldReturnRangeError()
    {
        var viewModel = CreateValidViewModel();
        viewModel.Price = 1_000_001;

        var validationResults = Validate(viewModel);

        AssertValidationError(
            validationResults,
            nameof(UpdateProductViewModel.Price),
            "価格は0以上100万円以下で入力してください。");
    }

    [TestMethod(DisplayName = "在庫数が0個の場合、入力検証に成功する")]
    public void Validate_WithMinimumStockQuantity_ShouldSucceed()
    {
        var viewModel = CreateValidViewModel();
        viewModel.StockQuantity = 0;

        var validationResults = Validate(viewModel);

        Assert.HasCount(0, validationResults);
    }

    [TestMethod(DisplayName = "在庫数が1000個の場合、入力検証に成功する")]
    public void Validate_WithMaximumStockQuantity_ShouldSucceed()
    {
        var viewModel = CreateValidViewModel();
        viewModel.StockQuantity = 1_000;

        var validationResults = Validate(viewModel);

        Assert.HasCount(0, validationResults);
    }

    [TestMethod(DisplayName = "在庫数が負数の場合、範囲エラーになる")]
    public void Validate_WithNegativeStockQuantity_ShouldReturnRangeError()
    {
        var viewModel = CreateValidViewModel();
        viewModel.StockQuantity = -1;

        var validationResults = Validate(viewModel);

        AssertValidationError(
            validationResults,
            nameof(UpdateProductViewModel.StockQuantity),
            "在庫数は0以上1000個以下で入力してください。");
    }

    [TestMethod(DisplayName = "在庫数が1000個を超える場合、範囲エラーになる")]
    public void Validate_WithStockQuantityOverMaximum_ShouldReturnRangeError()
    {
        var viewModel = CreateValidViewModel();
        viewModel.StockQuantity = 1_001;

        var validationResults = Validate(viewModel);

        AssertValidationError(
            validationResults,
            nameof(UpdateProductViewModel.StockQuantity),
            "在庫数は0以上1000個以下で入力してください。");
    }

    [TestMethod(DisplayName = "商品カテゴリが未選択の場合、必須エラーになる")]
    public void Validate_WithEmptyCategoryUuid_ShouldReturnRequiredError()
    {
        var viewModel = CreateValidViewModel();
        viewModel.CategoryUuid = string.Empty;

        var validationResults = Validate(viewModel);

        AssertValidationError(
            validationResults,
            nameof(UpdateProductViewModel.CategoryUuid),
            "カテゴリを選択してください。");
    }

    private static UpdateProductViewModel CreateValidViewModel()
        => new()
        {
            Name = "ゲルインクペン",
            Price = 150,
            StockQuantity = 40,
            CategoryUuid = Guid.NewGuid().ToString()
        };

    private static List<ValidationResult> Validate(UpdateProductViewModel viewModel)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(viewModel);

        Validator.TryValidateObject(
            viewModel,
            validationContext,
            validationResults,
            validateAllProperties: true);

        return validationResults;
    }

    private static void AssertValidationError(
        IReadOnlyCollection<ValidationResult> validationResults,
        string expectedMemberName,
        string expectedMessage)
    {
        Assert.HasCount(1, validationResults);

        var validationResult = validationResults.Single();
        Assert.AreEqual(expectedMessage, validationResult.ErrorMessage);
        CollectionAssert.Contains(
            validationResult.MemberNames.ToList(),
            expectedMemberName);
    }
}
