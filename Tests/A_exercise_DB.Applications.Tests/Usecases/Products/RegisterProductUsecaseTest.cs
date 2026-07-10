using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Applications.Usecases;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _usecase = new RegisterProductUsecase(
            _productCategoryRepositoryMock.Object,
            _productRepositoryMock.Object,
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
}