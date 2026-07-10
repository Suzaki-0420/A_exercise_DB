using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Applications.Usecases.Products;

namespace A_exercise_DB.Applications.Tests.Usecases.Products;

[TestClass]
public class SearchProductByCategoryUsecaseTests
{
    [TestMethod(DisplayName = "カテゴリ別に未削除の商品を取得して商品リストを返す")]
    public async Task ExecuteAsync_WhenProductsExist_ShouldReturnProductList()
    {
        // Arrange
        var repositoryMock = new Mock<IProductRepository>();

        var category = new ProductCategory("食品");
        var categoryUuid = Guid.Parse(
            "e50d978b-b73d-4afb-8e85-ace9cf1e12a7");

        const bool showDeletedOnly = false;

        var products = new List<Product>
        {
            new Product(
                Guid.NewGuid(),
                "りんご",
                100,
                "",
                category,
                new ProductStock(10),
                0),

            new Product(
                Guid.NewGuid(),
                "みかん",
                200,
                "",
                category,
                new ProductStock(5),
                0)
        };

        repositoryMock
            .Setup(r => r.SelectByProductCategoryIdAsync(
                categoryUuid,
                showDeletedOnly))
            .ReturnsAsync(products);

        var usecase =
            new SearchProductByCategoryUsecase(
                repositoryMock.Object);

        // Act
        var result = await usecase.ExecuteAsync(
            categoryUuid,
            showDeletedOnly);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.AreSame(products, result);

        repositoryMock.Verify(
            r => r.SelectByProductCategoryIdAsync(
                categoryUuid,
                showDeletedOnly),
            Times.Once);
    }

    [TestMethod(DisplayName = "カテゴリ検索で対象商品がない場合、空のリストを返す")]
    public async Task ExecuteAsync_WhenProductsDoNotExist_ShouldReturnEmptyList()
    {
        // Arrange
        var repositoryMock = new Mock<IProductRepository>();

        var categoryUuid = Guid.Parse(
            "e50d978b-b73d-4afb-8e85-ace9cf1e12a7");

        const bool showDeletedOnly = false;

        var products = new List<Product>();

        repositoryMock
            .Setup(r => r.SelectByProductCategoryIdAsync(
                categoryUuid,
                showDeletedOnly))
            .ReturnsAsync(products);

        var usecase =
            new SearchProductByCategoryUsecase(
                repositoryMock.Object);

        // Act
        var result = await usecase.ExecuteAsync(
            categoryUuid,
            showDeletedOnly);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);

        repositoryMock.Verify(
            r => r.SelectByProductCategoryIdAsync(
                categoryUuid,
                showDeletedOnly),
            Times.Once);
    }

    [TestMethod(DisplayName = "削除済み商品の検索指定をRepositoryへ渡す")]
    public async Task ExecuteAsync_WhenShowDeletedOnlyIsTrue_ShouldPassTrueToRepository()
    {
        // Arrange
        var repositoryMock = new Mock<IProductRepository>();

        var categoryUuid = Guid.NewGuid();
        const bool showDeletedOnly = true;

        var products = new List<Product>();

        repositoryMock
            .Setup(r => r.SelectByProductCategoryIdAsync(
                categoryUuid,
                showDeletedOnly))
            .ReturnsAsync(products);

        var usecase =
            new SearchProductByCategoryUsecase(
                repositoryMock.Object);

        // Act
        var result = await usecase.ExecuteAsync(
            categoryUuid,
            showDeletedOnly);

        // Assert
        Assert.IsNotNull(result);

        repositoryMock.Verify(
            r => r.SelectByProductCategoryIdAsync(
                categoryUuid,
                true),
            Times.Once);
    }
}