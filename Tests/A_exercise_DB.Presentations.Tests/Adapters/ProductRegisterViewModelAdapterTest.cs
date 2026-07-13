using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;

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
            CategoryName = null
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
            Name = null,
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
}