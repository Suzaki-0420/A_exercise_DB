using A_exercise_DB.Presentations.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// RegisterCategoryViewModelの単体テスト
/// </summary>
[TestClass]
public class RegisterCategoryViewModelTests
{
    /// <summary>
    /// ViewModelのバリデーションを実行する
    /// </summary>
    /// <param name="viewModel">検証対象のViewModel</param>
    /// <returns>バリデーション結果</returns>
    private static List<ValidationResult> Validate(RegisterCategoryViewModel viewModel)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(viewModel);

        Validator.TryValidateObject(
            viewModel,
            validationContext,
            validationResults,
            true
        );

        return validationResults;
    }

    /// <summary>
    /// CategoryNameが正常な場合、バリデーションエラーが発生しないこと
    /// </summary>
    [TestMethod(DisplayName = "CategoryNameが正常な場合、バリデーションエラーが発生しない")]
    public void Validate_WhenCategoryNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var viewModel = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        // Act
        var results = Validate(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// CategoryNameが空文字の場合、必須エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "CategoryNameが空文字の場合、必須エラーが発生する")]
    public void Validate_WhenCategoryNameIsEmpty_ShouldHaveRequiredError()
    {
        // Arrange
        var viewModel = new RegisterCategoryViewModel
        {
            CategoryName = string.Empty
        };

        // Act
        var results = Validate(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品カテゴリ名は必須です。", results[0].ErrorMessage);
    }

    /// <summary>
    /// CategoryNameがnullの場合、必須エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "CategoryNameがnullの場合、必須エラーが発生する")]
    public void Validate_WhenCategoryNameIsNull_ShouldHaveRequiredError()
    {
        // Arrange
        var viewModel = new RegisterCategoryViewModel
        {
            CategoryName = null!
        };

        // Act
        var results = Validate(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品カテゴリ名は必須です。", results[0].ErrorMessage);
    }

    /// <summary>
    /// CategoryNameが30文字の場合、バリデーションエラーが発生しないこと
    /// </summary>
    [TestMethod(DisplayName = "CategoryNameが30文字の場合、バリデーションエラーが発生しない")]
    public void Validate_WhenCategoryNameLengthIs30_ShouldNotHaveValidationError()
    {
        // Arrange
        var viewModel = new RegisterCategoryViewModel
        {
            CategoryName = new string('あ', 30)
        };

        // Act
        var results = Validate(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// CategoryNameが31文字の場合、文字数エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "CategoryNameが31文字の場合、文字数エラーが発生する")]
    public void Validate_WhenCategoryNameLengthIs31_ShouldHaveStringLengthError()
    {
        // Arrange
        var viewModel = new RegisterCategoryViewModel
        {
            CategoryName = new string('あ', 31)
        };

        // Act
        var results = Validate(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品カテゴリ名は30文字以内で入力してください。", results[0].ErrorMessage);
    }
}