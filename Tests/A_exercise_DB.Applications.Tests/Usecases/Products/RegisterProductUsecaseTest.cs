using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Applications.Interfaces;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Applications.Usecases;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using A_exercise_DB.Applications.Params;
using Moq;

namespace A_exercise_DB.Applications.Tests.Usecases.Products;

/// <summary>
/// RegisterProductUsecaseの単体テスト
/// </summary>
[TestClass]
[TestCategory("Applications/Usecases/Products")]
public class RegisterProductUsecaseTest
{
    private Mock<IProductCategoryRepository> _productCategoryRepositoryMock = null!;
    private Mock<IProductRepository> _productRepositoryMock = null!;
    private Mock<IImageUploadUsecase> _imageUploadUsecaseMock = null!;
    private Mock<IImageStorage> _imageStorageMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private RegisterProductUsecase _usecase = null!;

    /// <summary>
    /// 各テスト実行前の初期化処理
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _productCategoryRepositoryMock = new Mock<IProductCategoryRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _imageUploadUsecaseMock = new Mock<IImageUploadUsecase>();
        _imageStorageMock = new Mock<IImageStorage>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _usecase = new RegisterProductUsecase(
            _productCategoryRepositoryMock.Object,
            _productRepositoryMock.Object,
            _imageUploadUsecaseMock.Object,
            _imageStorageMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// GetCategoriesAsyncでカテゴリー一覧を取得できること
    /// </summary>
    [TestMethod(DisplayName = "GetCategoriesAsync_カテゴリー一覧を取得できる")]
    public async Task GetCategoriesAsync_ReturnsCategories()
    {
        // Arrange
        var categories = new List<ProductCategory>
        {
            CreateProductCategory(Guid.NewGuid(), "食品"),
            CreateProductCategory(Guid.NewGuid(), "飲料")
        };

        _productCategoryRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _usecase.GetCategoriesAsync();

        // Assert
        Assert.AreSame(categories, result);
        Assert.HasCount(2, result);
        Assert.AreEqual("食品", result[0].Name);
        Assert.AreEqual("飲料", result[1].Name);

        _productCategoryRepositoryMock.Verify(
            x => x.FindAllAsync(),
            Times.Once);
    }

    /// <summary>
    /// GetCategoriesAsyncでカテゴリーが0件の場合、空リストを返すこと
    /// </summary>
    [TestMethod(DisplayName = "GetCategoriesAsync_カテゴリーが0件の場合は空リストを返す")]
    public async Task GetCategoriesAsync_WhenCategoriesIsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var categories = new List<ProductCategory>();

        _productCategoryRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _usecase.GetCategoriesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);

        _productCategoryRepositoryMock.Verify(
            x => x.FindAllAsync(),
            Times.Once);
    }

    /// <summary>
    /// GetCategoryByIdAsyncでカテゴリーを取得できること
    /// </summary>
    [TestMethod(DisplayName = "GetCategoryByIdAsync_カテゴリーを取得できる")]
    public async Task GetCategoryByIdAsync_ReturnsCategory()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();
        var categoryId = categoryUuid.ToString();
        var category = CreateProductCategory(categoryUuid, "食品");

        _productCategoryRepositoryMock
            .Setup(x => x.FindByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        var result = await _usecase.GetCategoryByIdAsync(categoryId);

        // Assert
        Assert.AreSame(category, result);
        Assert.AreEqual(categoryUuid, result.CategoryUuid);
        Assert.AreEqual("食品", result.Name);

        _productCategoryRepositoryMock.Verify(
            x => x.FindByIdAsync(categoryId),
            Times.Once);
    }

    /// <summary>
    /// GetCategoryByIdAsyncでカテゴリーが存在しない場合、NotFoundExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "GetCategoryByIdAsync_カテゴリーが存在しない場合はNotFoundExceptionをスローする")]
    public async Task GetCategoryByIdAsync_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid().ToString();

        _productCategoryRepositoryMock
            .Setup(x => x.FindByIdAsync(categoryId))
            .ReturnsAsync((ProductCategory?)null);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<NotFoundException>(async () =>
            await _usecase.GetCategoryByIdAsync(categoryId));

        // Assert
        Assert.AreEqual(
            $"カテゴリーId:{categoryId}に一致するカテゴリーは存在しません。",
            exception.Message);

        _productCategoryRepositoryMock.Verify(
            x => x.FindByIdAsync(categoryId),
            Times.Once);
    }

    /// <summary>
    /// ExistsByProductNameAsyncで商品名が存在する場合、trueを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ExistsByProductNameAsync_商品名が存在する場合はtrueを返す")]
    public async Task ExistsByProductNameAsync_WhenProductNameExists_ReturnsTrue()
    {
        // Arrange
        var productName = "テスト商品";

        _productRepositoryMock
            .Setup(x => x.ExistsByNameAsync(productName))
            .ReturnsAsync(true);

        // Act
        var result = await _usecase.ExistsByProductNameAsync(productName);

        // Assert
        Assert.IsTrue(result);

        _productRepositoryMock.Verify(
            x => x.ExistsByNameAsync(productName),
            Times.Once);
    }

    /// <summary>
    /// ExistsByProductNameAsyncで商品名が存在しない場合、falseを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ExistsByProductNameAsync_商品名が存在しない場合はfalseを返す")]
    public async Task ExistsByProductNameAsync_WhenProductNameDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var productName = "テスト商品";

        _productRepositoryMock
            .Setup(x => x.ExistsByNameAsync(productName))
            .ReturnsAsync(false);

        // Act
        var result = await _usecase.ExistsByProductNameAsync(productName);

        // Assert
        Assert.IsFalse(result);

        _productRepositoryMock.Verify(
            x => x.ExistsByNameAsync(productName),
            Times.Once);
    }

    /// <summary>
    /// RegisterProductAsyncで商品を登録できること
    /// </summary>
    [TestMethod(DisplayName = "RegisterProductAsync_商品を登録できる")]
    public async Task RegisterProductAsync_CreatesProductAndCommit()
    {
        // Arrange
        var product = CreateProduct(
            Guid.NewGuid(),
            "テスト商品",
            1000);

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productRepositoryMock
            .Setup(x => x.CreateAsync(product))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _usecase.RegisterProductAsync(product);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.CreateAsync(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Never);
    }

    /// <summary>
    /// RegisterProductAsyncでCreateAsyncが例外をスローした場合、Rollbackして再スローすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterProductAsync_CreateAsyncが例外をスローした場合はRollbackして再スローする")]
    public async Task RegisterProductAsync_WhenCreateAsyncThrowsException_RollbackAndRethrow()
    {
        // Arrange
        var product = CreateProduct(
            Guid.NewGuid(),
            "テスト商品",
            1000);

        var expectedException = new InvalidOperationException("Repository error");

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productRepositoryMock
            .Setup(x => x.CreateAsync(product))
            .ThrowsAsync(expectedException);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await _usecase.RegisterProductAsync(product));

        // Assert
        Assert.AreSame(expectedException, exception);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.CreateAsync(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);
    }

    /// <summary>
    /// RegisterProductAsyncでCommitAsyncが例外をスローした場合、Rollbackして再スローすること
    /// </summary>
    [TestMethod(DisplayName = "RegisterProductAsync_CommitAsyncが例外をスローした場合はRollbackして再スローする")]
    public async Task RegisterProductAsync_WhenCommitAsyncThrowsException_RollbackAndRethrow()
    {
        // Arrange
        var product = CreateProduct(
            Guid.NewGuid(),
            "テスト商品",
            1000);

        var expectedException = new InvalidOperationException("Commit error");

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productRepositoryMock
            .Setup(x => x.CreateAsync(product))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync())
            .ThrowsAsync(expectedException);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await _usecase.RegisterProductAsync(product));

        // Assert
        Assert.AreSame(expectedException, exception);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.CreateAsync(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);
    }

    /// <summary>
    /// テスト用のProductCategoryを生成する
    /// </summary>
    private static ProductCategory CreateProductCategory(
        Guid categoryUuid,
        string name)
    {
        var category =
            (ProductCategory)RuntimeHelpers.GetUninitializedObject(typeof(ProductCategory));

        SetPrivateProperty(category, "CategoryUuid", categoryUuid);
        SetPrivateProperty(category, "Name", name);

        return category;
    }

    /// <summary>
    /// テスト用のProductを生成する
    /// </summary>
    private static Product CreateProduct(
        Guid productUuid,
        string name,
        int price)
    {
        var product =
            (Product)RuntimeHelpers.GetUninitializedObject(typeof(Product));

        SetPrivateProperty(product, "ProductUuid", productUuid);
        SetPrivateProperty(product, "Name", name);
        SetPrivateProperty(product, "Price", price);

        return product;
    }

    /// <summary>
    /// private setのプロパティへテスト用の値を設定する
    /// </summary>
    private static void SetPrivateProperty<T>(
        T target,
        string propertyName,
        object? value)
    {
        var field = typeof(T).GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (field is null)
        {
            throw new InvalidOperationException(
                $"{propertyName}のバッキングフィールドが見つかりません。");
        }

        field.SetValue(target, value);
    }

    /// <summary>
    /// ProductRegisterParamがnullの場合、
    /// ArgumentNullExceptionをスローすること
    /// </summary>
    [TestMethod(
        DisplayName =
            "RegisterProductAsync_入力値がnullの場合はArgumentNullExceptionをスローする")]
    public async Task RegisterProductAsync_WhenParamIsNull_ThrowsArgumentNullException()
    {
        // Act
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () =>
                await _usecase.RegisterProductAsync(
                    (ProductRegisterParam)null!));

        // Assert
        _imageUploadUsecaseMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<ImageUploadParam>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Never);

        _productRepositoryMock.Verify(
            x => x.CreateAsync(
                It.IsAny<Product>()),
            Times.Never);
    }

    /// <summary>
    /// 画像を含む商品を正常に登録できること
    /// </summary>
    [TestMethod(
        DisplayName =
            "RegisterProductAsync_画像を含む商品を正常に登録できる")]
    public async Task RegisterProductAsync_WithValidParam_CreatesProductAndCommits()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();
        var category =
            CreateProductCategory(
                categoryUuid,
                "食品");

        const string imageUrl =
            "https://example.com/images/test.png";

        await using var imageContent =
            new MemoryStream(
                new byte[]
                {
                1,
                2,
                3,
                4
                });

        var param =
            CreateProductRegisterParam(
                categoryUuid,
                imageContent);

        Product? createdProduct = null;

        _imageUploadUsecaseMock
            .Setup(x =>
                x.ExecuteAsync(
                    It.IsAny<ImageUploadParam>()))
            .ReturnsAsync(imageUrl);

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productCategoryRepositoryMock
            .Setup(x =>
                x.FindByIdAsync(
                    categoryUuid.ToString()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(x =>
                x.CreateAsync(
                    It.IsAny<Product>()))
            .Callback<Product>(
                product =>
                    createdProduct = product)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _usecase.RegisterProductAsync(
                param);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(result, createdProduct);

        Assert.AreNotEqual(
            Guid.Empty,
            result.ProductUuid);

        Assert.AreEqual(
            param.Name,
            result.Name);

        Assert.AreEqual(
            param.Price,
            result.Price);

        Assert.AreEqual(
            imageUrl,
            result.ImageUrl);

        Assert.AreSame(
            category,
            result.ProductCategory);

        Assert.IsNotNull(
            result.ProductStock);

        Assert.AreNotEqual(
            Guid.Empty,
            result.ProductStock.StockUuid);

        Assert.AreEqual(
            param.Quantity,
            result.ProductStock.Quantity);

        Assert.AreEqual(
            0,
            result.DeleteFlg);

        _imageUploadUsecaseMock.Verify(
            x => x.ExecuteAsync(
                It.Is<ImageUploadParam>(
                    upload =>
                        ReferenceEquals(
                            upload.Content,
                            param.ImageContent) &&
                        upload.FileName ==
                            param.ImageFileName &&
                        upload.ContentType ==
                            param.ImageContentType &&
                        upload.Length ==
                            param.ImageLength)),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _productCategoryRepositoryMock.Verify(
            x => x.FindByIdAsync(
                categoryUuid.ToString()),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.CreateAsync(result),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Never);

        _imageStorageMock.Verify(
            x => x.DeleteAsync(
                It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// カテゴリーが存在しない場合、
    /// Rollbackして保存済み画像を削除すること
    /// </summary>
    [TestMethod(
        DisplayName =
            "RegisterProductAsync_カテゴリーが存在しない場合はRollbackして画像を削除する")]
    public async Task RegisterProductAsync_WhenCategoryDoesNotExist_RollsBackAndDeletesImage()
    {
        // Arrange
        var categoryUuid =
            Guid.NewGuid();

        const string imageUrl =
            "https://example.com/images/test.png";

        await using var imageContent =
            new MemoryStream(
                new byte[]
                {
                1,
                2,
                3,
                4
                });

        var param =
            CreateProductRegisterParam(
                categoryUuid,
                imageContent);

        _imageUploadUsecaseMock
            .Setup(x =>
                x.ExecuteAsync(
                    It.IsAny<ImageUploadParam>()))
            .ReturnsAsync(imageUrl);

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productCategoryRepositoryMock
            .Setup(x =>
                x.FindByIdAsync(
                    categoryUuid.ToString()))
            .ReturnsAsync(
                (ProductCategory?)null);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        _imageStorageMock
            .Setup(x =>
                x.DeleteAsync(imageUrl))
            .Returns(Task.CompletedTask);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<
                NotFoundException>(
                async () =>
                    await _usecase
                        .RegisterProductAsync(
                            param));

        // Assert
        Assert.AreEqual(
            $"カテゴリーId:{categoryUuid}に一致するカテゴリーは存在しません。",
            exception.Message);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _productCategoryRepositoryMock.Verify(
            x => x.FindByIdAsync(
                categoryUuid.ToString()),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.CreateAsync(
                It.IsAny<Product>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);

        _imageStorageMock.Verify(
            x => x.DeleteAsync(imageUrl),
            Times.Once);
    }

    /// <summary>
    /// DB登録失敗後に画像削除も失敗した場合、
    /// 元の商品登録例外を再スローすること
    /// </summary>
    [TestMethod(
        DisplayName =
            "RegisterProductAsync_DB登録と画像削除が失敗した場合は元の例外を再スローする")]
    public async Task RegisterProductAsync_WhenCreateAndImageDeleteFail_RethrowsOriginalException()
    {
        // Arrange
        var categoryUuid =
            Guid.NewGuid();

        var category =
            CreateProductCategory(
                categoryUuid,
                "食品");

        const string imageUrl =
            "https://example.com/images/test.png";

        await using var imageContent =
            new MemoryStream(
                new byte[]
                {
                1,
                2,
                3,
                4
                });

        var param =
            CreateProductRegisterParam(
                categoryUuid,
                imageContent);

        var expectedException =
            new InvalidOperationException(
                "商品登録に失敗しました。");

        _imageUploadUsecaseMock
            .Setup(x =>
                x.ExecuteAsync(
                    It.IsAny<ImageUploadParam>()))
            .ReturnsAsync(imageUrl);

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productCategoryRepositoryMock
            .Setup(x =>
                x.FindByIdAsync(
                    categoryUuid.ToString()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(x =>
                x.CreateAsync(
                    It.IsAny<Product>()))
            .ThrowsAsync(expectedException);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        _imageStorageMock
            .Setup(x =>
                x.DeleteAsync(imageUrl))
            .ThrowsAsync(
                new IOException(
                    "画像削除に失敗しました。"));

        // Act
        var actualException =
            await Assert.ThrowsExactlyAsync<
                InvalidOperationException>(
                async () =>
                    await _usecase
                        .RegisterProductAsync(
                            param));

        // Assert
        Assert.AreSame(
            expectedException,
            actualException);

        _productRepositoryMock.Verify(
            x => x.CreateAsync(
                It.IsAny<Product>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);

        _imageStorageMock.Verify(
            x => x.DeleteAsync(imageUrl),
            Times.Once);
    }

    /// <summary>
    /// 画像URLが空白で登録に失敗した場合、
    /// 画像削除処理を呼ばないこと
    /// </summary>
    [TestMethod(
        DisplayName =
            "RegisterProductAsync_画像URLが空白の場合は画像削除を呼ばない")]
    public async Task RegisterProductAsync_WhenImageUrlIsWhiteSpace_DoesNotDeleteImage()
    {
        // Arrange
        var categoryUuid =
            Guid.NewGuid();

        await using var imageContent =
            new MemoryStream(
                new byte[]
                {
                1,
                2,
                3,
                4
                });

        var param =
            CreateProductRegisterParam(
                categoryUuid,
                imageContent);

        _imageUploadUsecaseMock
            .Setup(x =>
                x.ExecuteAsync(
                    It.IsAny<ImageUploadParam>()))
            .ReturnsAsync("   ");

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productCategoryRepositoryMock
            .Setup(x =>
                x.FindByIdAsync(
                    categoryUuid.ToString()))
            .ReturnsAsync(
                (ProductCategory?)null);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        await Assert.ThrowsExactlyAsync<
            NotFoundException>(
            async () =>
                await _usecase
                    .RegisterProductAsync(
                        param));

        // Assert
        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);

        _imageStorageMock.Verify(
            x => x.DeleteAsync(
                It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// テスト用のProductRegisterParamを生成する
    /// </summary>
    private static ProductRegisterParam
        CreateProductRegisterParam(
            Guid categoryId,
            Stream imageContent)
    {
        return new ProductRegisterParam(
            Name: "テスト商品",
            Price: 1000,
            CategoryId: categoryId,
            Quantity: 10,
            ImageContent: imageContent,
            ImageFileName: "test.png",
            ImageContentType: "image/png",
            ImageLength: imageContent.Length);
    }
}
