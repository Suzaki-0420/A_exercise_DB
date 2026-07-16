using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace A_exercise_DB.Presentations.Tests.Controllers;

[TestClass]
public class SearchProductByCategoryControllerTests
{
    [TestMethod(DisplayName = "カテゴリ検索で未削除の商品リストを返す")]
    public async Task Search_WhenProductsExist_ShouldReturnProductList()
    {
        // Arrange
        var usecaseMock =
            new Mock<ISearchProductByCategoryUsecase>();

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

        usecaseMock
            .Setup(u => u.ExecuteAsync(
                categoryUuid,
                showDeletedOnly))
            .ReturnsAsync(products);

        var controller =
            new SearchProductByCategory(
                usecaseMock.Object);

        // Act
        var result = await controller.Search(
            categoryUuid,
            showDeletedOnly);

        // Assert
        var okResult =
            result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var actualProducts =
            okResult.Value as List<Product>;

        Assert.IsNotNull(actualProducts);
        Assert.HasCount(2, actualProducts);
        Assert.AreSame(products, actualProducts);

        usecaseMock.Verify(
            u => u.ExecuteAsync(
                categoryUuid,
                showDeletedOnly),
            Times.Once);
    }

    [TestMethod(DisplayName = "カテゴリ検索で空の商品リストを返す")]
    public async Task Search_WhenProductsDoNotExist_ShouldReturnEmptyProductList()
    {
        // Arrange
        var usecaseMock =
            new Mock<ISearchProductByCategoryUsecase>();

        var categoryUuid = Guid.Parse(
            "e50d978b-b73d-4afb-8e85-ace9cf1e12a7");

        const bool showDeletedOnly = false;

        var products = new List<Product>();

        usecaseMock
            .Setup(u => u.ExecuteAsync(
                categoryUuid,
                showDeletedOnly))
            .ReturnsAsync(products);

        var controller =
            new SearchProductByCategory(
                usecaseMock.Object);

        // Act
        var result = await controller.Search(
            categoryUuid,
            showDeletedOnly);

        // Assert
        var okResult =
            result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var actualProducts =
            okResult.Value as List<Product>;

        Assert.IsNotNull(actualProducts);
        Assert.IsEmpty(actualProducts);

        usecaseMock.Verify(
            u => u.ExecuteAsync(
                categoryUuid,
                showDeletedOnly),
            Times.Once);
    }

    [TestMethod(DisplayName = "削除済み商品を指定して検索できる")]
    public async Task Search_WhenShowDeletedOnlyIsTrue_ShouldPassTrueToUsecase()
    {
        // Arrange
        var usecaseMock =
            new Mock<ISearchProductByCategoryUsecase>();

        var categoryUuid = Guid.NewGuid();
        const bool showDeletedOnly = true;

        var products = new List<Product>();

        usecaseMock
            .Setup(u => u.ExecuteAsync(
                categoryUuid,
                showDeletedOnly))
            .ReturnsAsync(products);

        var controller =
            new SearchProductByCategory(
                usecaseMock.Object);

        // Act
        var result = await controller.Search(
            categoryUuid,
            showDeletedOnly);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(result);

        usecaseMock.Verify(
            u => u.ExecuteAsync(
                categoryUuid,
                true),
            Times.Once);
    }
}