using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Http;
namespace A_exercise_DB.Presentations.Tests.Adapters;

/// <summary>
/// ProductRegisterViewModelAdapterの単体テスト
/// </summary>
[TestClass]
public class ProductRegisterViewModelAdapterTest
{
    private ProductRegisterViewModelAdapter _adapter = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _adapter = new ProductRegisterViewModelAdapter();
    }

    /// <summary>
    /// RegisterViewModelからProductを復元できること
    /// </summary>
    [TestMethod(DisplayName = "RegisterViewModelからProductを復元できる")]
    public async Task RestoreAsync_WhenRegisterViewModelIsValid_ShouldReturnProduct()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();

        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = categoryUuid,
            CategoryName = "食品"
        };

        // Act
        var result = await _adapter.RestoreAsync(viewModel);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.ProductUuid);
        Assert.AreEqual("りんご", result.Name);
        Assert.AreEqual(100, result.Price);

        Assert.IsNotNull(result.ProductCategory);
        Assert.AreEqual(categoryUuid, result.ProductCategory!.CategoryUuid);
        Assert.AreEqual("食品", result.ProductCategory.Name);

        Assert.IsNotNull(result.ProductStock);
        Assert.AreEqual(10, result.ProductStock!.Quantity);
    }

    /// <summary>
    /// CategoryUuidがnullの場合、DomainExceptionが発生すること
    /// </summary>
    [TestMethod(DisplayName = "CategoryUuidがnullの場合、DomainExceptionが発生する")]
    public async Task RestoreAsync_WhenCategoryUuidIsNull_ShouldThrowDomainException()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = null,
            CategoryName = "食品"
        };

        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
            await _adapter.RestoreAsync(viewModel)
        );

        // Assert
        Assert.AreEqual("商品カテゴリ識別IDが不正です", exception.Message);
    }

    [TestMethod(DisplayName = "CategoryNameがnullの場合、DomainExceptionが発生する")]
    public async Task RestoreAsync_WhenCategoryNameIsNull_ShouldThrowDomainException()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = null!
        };

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                async () => await _adapter.RestoreAsync(viewModel)
            );

        // Assert
        Assert.AreEqual(
            "商品カテゴリ名は必須です",
            exception.Message);
    }

    [TestMethod(DisplayName = "Nameがnullの場合、DomainExceptionが発生する")]
    public async Task RestoreAsync_WhenNameIsNull_ShouldThrowDomainException()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = null!,
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品"
        };

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                async () => await _adapter.RestoreAsync(viewModel)
            );

        // Assert
        Assert.AreEqual(
            "商品名は必須です",
            exception.Message);
    }

    /// <summary>
    /// 正常なViewModelと画像を指定した場合、ProductRegisterParamへ変換できること
    /// </summary>
    [TestMethod(DisplayName = "正常なViewModelと画像を指定した場合、ProductRegisterParamへ変換できる")]
    public void ToParam_WhenViewModelAndImageAreValid_ShouldReturnProductRegisterParam()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();

        using var imageContent =
            new MemoryStream([1, 2, 3, 4]);

        var image = new FormFile(
            imageContent,
            0,
            imageContent.Length,
            "Image",
            "apple.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = categoryUuid,
            CategoryName = "食品",
            Image = image
        };

        using var imageStream =
            new MemoryStream([1, 2, 3, 4]);

        // Act
        var result =
            _adapter.ToParam(viewModel, imageStream);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("りんご", result.Name);
        Assert.AreEqual(100, result.Price);
        Assert.AreEqual(categoryUuid, result.CategoryId);
        Assert.AreEqual(10, result.Quantity);
        Assert.AreSame(imageStream, result.ImageContent);
        Assert.AreEqual("apple.png", result.ImageFileName);
        Assert.AreEqual("image/png", result.ImageContentType);
        Assert.AreEqual(4L, result.ImageLength);
    }

    /// <summary>
    /// 画像がnullの場合、画像情報にnullまたは0が設定されること
    /// </summary>
    [TestMethod(DisplayName = "画像がnullの場合、画像情報にnullまたは0が設定される")]
    public void ToParam_WhenImageIsNull_ShouldSetDefaultImageValues()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();

        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = categoryUuid,
            CategoryName = "食品",
            Image = null
        };

        // Act
        var result =
            _adapter.ToParam(viewModel, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("りんご", result.Name);
        Assert.AreEqual(100, result.Price);
        Assert.AreEqual(categoryUuid, result.CategoryId);
        Assert.AreEqual(10, result.Quantity);
        Assert.IsNull(result.ImageContent);
        Assert.IsNull(result.ImageFileName);
        Assert.IsNull(result.ImageContentType);
        Assert.AreEqual(0L, result.ImageLength);
    }

    /// <summary>
    /// targetがnullの場合、ArgumentNullExceptionがスローされること
    /// </summary>
    [TestMethod(DisplayName = "targetがnullの場合、ArgumentNullExceptionがスローされる")]
    public void ToParam_WhenTargetIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        RegisterViewModel target = null!;

        // Act
        var exception =
            Assert.ThrowsExactly<ArgumentNullException>(
                () => _adapter.ToParam(target, null)
            );

        // Assert
        Assert.AreEqual("target", exception.ParamName);
    }

    /// <summary>
    /// 商品名がnullの場合、ArgumentExceptionがスローされること
    /// </summary>
    [TestMethod(DisplayName = "商品名がnullの場合、ArgumentExceptionがスローされる")]
    public void ToParam_WhenNameIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = null!,
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品"
        };

        // Act
        var exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _adapter.ToParam(viewModel, null)
            );

        // Assert
        Assert.AreEqual(
            "商品名が指定されていません。 (Parameter 'target')",
            exception.Message);
        Assert.AreEqual("target", exception.ParamName);
    }

    /// <summary>
    /// 商品名が空白の場合、ArgumentExceptionがスローされること
    /// </summary>
    [TestMethod(DisplayName = "商品名が空白の場合、ArgumentExceptionがスローされる")]
    public void ToParam_WhenNameIsWhiteSpace_ShouldThrowArgumentException()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = " ",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.NewGuid(),
            CategoryName = "食品"
        };

        // Act
        var exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _adapter.ToParam(viewModel, null)
            );

        // Assert
        Assert.AreEqual(
            "商品名が指定されていません。 (Parameter 'target')",
            exception.Message);
        Assert.AreEqual("target", exception.ParamName);
    }

    /// <summary>
    /// CategoryUuidがnullの場合、ArgumentExceptionがスローされること
    /// </summary>
    [TestMethod(DisplayName = "CategoryUuidがnullの場合、ArgumentExceptionがスローされる")]
    public void ToParam_WhenCategoryUuidIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = null,
            CategoryName = "食品"
        };

        // Act
        var exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _adapter.ToParam(viewModel, null)
            );

        // Assert
        Assert.AreEqual(
            "商品カテゴリが指定されていません。 (Parameter 'target')",
            exception.Message);
        Assert.AreEqual("target", exception.ParamName);
    }

    /// <summary>
    /// CategoryUuidがGuid.Emptyの場合、ArgumentExceptionがスローされること
    /// </summary>
    [TestMethod(DisplayName = "CategoryUuidがGuid.Emptyの場合、ArgumentExceptionがスローされる")]
    public void ToParam_WhenCategoryUuidIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var viewModel = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.Empty,
            CategoryName = "食品"
        };

        // Act
        var exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _adapter.ToParam(viewModel, null)
            );

        // Assert
        Assert.AreEqual(
            "商品カテゴリが指定されていません。 (Parameter 'target')",
            exception.Message);
        Assert.AreEqual("target", exception.ParamName);
    }


}