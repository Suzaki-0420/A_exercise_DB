using System.ComponentModel.DataAnnotations;
using A_exercise_DB.Presentations.ViewModels;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// AdminLoginViewModelの単体テスト
/// </summary>
[TestClass]
public class AdminLoginViewModelTest
{
    /// <summary>
    /// 初期値が正しいこと
    /// </summary>
    [TestMethod(DisplayName = "初期値が正しい")]
    public void AdminLoginViewModel_DefaultValuesAreCorrect()
    {
        // Act
        var viewModel = new AdminLoginViewModel();

        // Assert
        Assert.AreEqual(string.Empty, viewModel.AccountName);
        Assert.AreEqual(string.Empty, viewModel.Password);
    }

    /// <summary>
    /// プロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "プロパティに値を設定できる")]
    public void AdminLoginViewModel_CanSetProperties()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel();

        // Act
        viewModel.AccountName = "admin01";
        viewModel.Password = "pass01";

        // Assert
        Assert.AreEqual("admin01", viewModel.AccountName);
        Assert.AreEqual("pass01", viewModel.Password);
    }

    /// <summary>
    /// 入力値が正しい場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "入力値が正しい場合、入力検証エラーにならない")]
    public void AdminLoginViewModel_WhenValidValues_ShouldNotReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// アカウント名が未入力の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "アカウント名が未入力の場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenAccountNameIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("アカウント名を入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// アカウント名がnullの場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "アカウント名がnullの場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenAccountNameIsNull_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = null!,
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("アカウント名を入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// アカウント名が5文字未満の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "アカウント名が5文字未満の場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenAccountNameIsTooShort_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "adm1",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("アカウント名は5～20文字で入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// アカウント名が20文字の場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "アカウント名が20文字の場合、入力検証エラーにならない")]
    public void AdminLoginViewModel_WhenAccountNameIsMaxLength_ShouldNotReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin01234567890123",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// アカウント名が20文字を超える場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "アカウント名が20文字を超える場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenAccountNameIsTooLong_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin0123456789012345",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("アカウント名は5～20文字で入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// アカウント名が半角英数字以外の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "アカウント名が半角英数字以外の場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenAccountNameFormatIsInvalid_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin_01",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("アカウント名は半角英数字で入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// パスワードが未入力の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "パスワードが未入力の場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenPasswordIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = ""
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("パスワードを入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// パスワードがnullの場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "パスワードがnullの場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenPasswordIsNull_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = null!
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("パスワードを入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// パスワードが5文字未満の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "パスワードが5文字未満の場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenPasswordIsTooShort_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = "pas1"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("パスワードは5～20文字で入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// パスワードが20文字の場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "パスワードが20文字の場合、入力検証エラーにならない")]
    public void AdminLoginViewModel_WhenPasswordIsMaxLength_ShouldNotReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = "pass01234567890123"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// パスワードが20文字を超える場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "パスワードが20文字を超える場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenPasswordIsTooLong_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = "pass01234567890123456"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("パスワードは5～20文字で入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// パスワードが半角英数字以外の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "パスワードが半角英数字以外の場合、入力検証エラーになる")]
    public void AdminLoginViewModel_WhenPasswordFormatIsInvalid_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new AdminLoginViewModel
        {
            AccountName = "admin01",
            Password = "pass_01"
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("パスワードは半角英数字で入力してください。", results[0].ErrorMessage);
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
            true
        );

        return validationResults;
    }
}