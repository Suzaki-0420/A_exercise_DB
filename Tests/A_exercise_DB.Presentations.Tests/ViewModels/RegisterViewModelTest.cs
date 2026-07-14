using System.ComponentModel.DataAnnotations;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Http;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// RegisterViewModelの単体テスト
/// </summary>
[TestClass]
public class RegisterViewModelTest
{
    /// <summary>
    /// 初期値が正しいこと
    /// </summary>
    [TestMethod(DisplayName = "初期値が正しい")]
    public void RegisterViewModel_DefaultValuesAreCorrect()
    {
        // Act
        var viewModel = new RegisterViewModel();

        // Assert
        Assert.AreEqual(string.Empty, viewModel.Name);
        Assert.AreEqual(0, viewModel.Price);
        Assert.AreEqual(0, viewModel.Stock);
        Assert.IsNull(viewModel.CategoryUuid);
        Assert.AreEqual(string.Empty, viewModel.CategoryName);
        Assert.IsNull(viewModel.Image);
    }

    /// <summary>
    /// プロパティに値を設定できること
    /// </summary>
    [TestMethod(DisplayName = "プロパティに値を設定できる")]
    public void RegisterViewModel_CanSetProperties()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();
        var viewModel = new RegisterViewModel();

        // Act
        viewModel.Name = "りんご";
        viewModel.Price = 100;
        viewModel.Stock = 10;
        viewModel.CategoryUuid = categoryUuid;
        viewModel.CategoryName = "食品";

        // Assert
        Assert.AreEqual("りんご", viewModel.Name);
        Assert.AreEqual(100, viewModel.Price);
        Assert.AreEqual(10, viewModel.Stock);
        Assert.AreEqual(categoryUuid, viewModel.CategoryUuid);
        Assert.AreEqual("食品", viewModel.CategoryName);
    }

    /// <summary>
    /// 入力値が正しい場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "入力値が正しい場合、入力検証エラーにならない")]
    public void RegisterViewModel_WhenValidValues_ShouldNotReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// 商品名が未入力の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "商品名が未入力の場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenNameIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品名は必須です。", results[0].ErrorMessage);
    }

    /// <summary>
    /// 商品名がnullの場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "商品名がnullの場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenNameIsNull_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = null!,
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品名は必須です。", results[0].ErrorMessage);
    }

    [TestMethod(DisplayName = "商品名が1文字の場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenNameIsOneCharacter_ShouldReturnValidationError()
    {
        var viewModel = new RegisterViewModel
        {
            Name = "A",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        var results = ValidateModel(viewModel);

        Assert.HasCount(1, results);
        Assert.AreEqual("商品名は2～20文字で入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// 商品名が20文字の場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "商品名が20文字の場合、入力検証エラーにならない")]
    public void RegisterViewModel_WhenNameIsMaxLength_ShouldNotReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "あいうえおかきくけこさしすせそたちつてと",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// 商品名が20文字を超える場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "商品名が20文字を超える場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenNameIsTooLong_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "あいうえおかきくけこさしすせそたちつてとな",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品名は2～20文字で入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// 単価が0の場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "単価が0の場合、入力検証エラーにならない")]
    public void RegisterViewModel_WhenPriceIsZero_ShouldNotReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 0,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// 単価が負数の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "単価が負数の場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenPriceIsNegative_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = -1,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("単価は0以上の整数を指定してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// 在庫数が0の場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "在庫数が0の場合、入力検証エラーにならない")]
    public void RegisterViewModel_WhenStockIsZero_ShouldNotReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 0,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// 在庫数が負数の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "在庫数が負数の場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenStockIsNegative_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = -1,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("在庫数は0以上1000個以下で入力してください。", results[0].ErrorMessage);
    }

    [TestMethod(DisplayName = "在庫数が1000個の場合、入力検証エラーにならない")]
    public void RegisterViewModel_WhenStockIsMaximum_ShouldNotReturnValidationError()
    {
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 1000,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        var results = ValidateModel(viewModel);

        Assert.IsEmpty(results);
    }

    [TestMethod(DisplayName = "在庫数が1001個の場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenStockExceedsMaximum_ShouldReturnValidationError()
    {
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 1001,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        var results = ValidateModel(viewModel);

        Assert.HasCount(1, results);
        Assert.AreEqual("在庫数は0以上1000個以下で入力してください。", results[0].ErrorMessage);
    }

    /// <summary>
    /// 商品カテゴリIdがnullの場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "商品カテゴリIdがnullの場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenCategoryUuidIsNull_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = null,
            CategoryName = "食品",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品カテゴリIdは必須です。", results[0].ErrorMessage);
    }

    /// <summary>
    /// 商品カテゴリ名が未入力の場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "商品カテゴリ名が未入力の場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenCategoryNameIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品カテゴリ名は必須です。", results[0].ErrorMessage);
    }

    /// <summary>
    /// 商品カテゴリ名がnullの場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "商品カテゴリ名がnullの場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenCategoryNameIsNull_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = null!,
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品カテゴリ名は必須です。", results[0].ErrorMessage);
    }

    /// <summary>
    /// 商品カテゴリ名が20文字の場合、入力検証エラーにならないこと
    /// </summary>
    [TestMethod(DisplayName = "商品カテゴリ名が20文字の場合、入力検証エラーにならない")]
    public void RegisterViewModel_WhenCategoryNameIsMaxLength_ShouldNotReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "あいうえおかきくけこさしすせそたちつてと",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// 商品カテゴリ名が20文字を超える場合、入力検証エラーになること
    /// </summary>
    [TestMethod(DisplayName = "商品カテゴリ名が20文字を超える場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenCategoryNameIsTooLong_ShouldReturnValidationError()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "あいうえおかきくけこさしすせそたちつてとな",
            Image = CreateValidImage()
        };

        // Act
        var results = ValidateModel(viewModel);

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("商品名は20文字以内で入力してください。", results[0].ErrorMessage);
    }

    [TestMethod(DisplayName = "商品画像が未指定の場合、入力検証エラーになる")]
    public void RegisterViewModel_WhenImageIsNull_ShouldReturnValidationError()
    {
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品",
            Image = null
        };

        var results = ValidateModel(viewModel);

        Assert.HasCount(1, results);
        Assert.AreEqual("商品画像を選択してください。", results[0].ErrorMessage);
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

    private static IFormFile CreateValidImage()
    {
        var content = new MemoryStream([1]);
        return new FormFile(content, 0, content.Length, "Image", "test.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
    }
}
