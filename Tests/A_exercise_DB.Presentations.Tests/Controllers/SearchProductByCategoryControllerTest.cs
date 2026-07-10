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
    [TestMethod(DisplayName = "カテゴリ検索で商品リストを返す")]
    public async Task GetProductsByCategory_ReturnsProductList()
    {
        var usecaseMock = new Mock<ISearchProductByCategoryUsecase>();
        var category = new ProductCategory("食品");
        var categoryId = Guid.Parse("e50d978b-b73d-4afb-8e85-ace9cf1e12a7");

        var products = new List<Product>
        {
            new Product (Guid.NewGuid(),
                "りんご",
                100,
                "",
                category,
                new ProductStock(10),
                0
            ),
            new Product(
                Guid.NewGuid(),
                "みかん",
                200,
                "",
                category,
                new ProductStock(5),
                0
            )
        };
        usecaseMock
            .Setup(u => u.ExecuteAsync(categoryId))
            .ReturnsAsync(products);
        var controller = new SearchProductByCategory(usecaseMock.Object);



        // Act（実行）
        var result = await controller.Search(categoryId);


        // Assert（確認）
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        usecaseMock.Verify(
            u => u.ExecuteAsync(categoryId),
            Times.Once);
    }

    [TestMethod(DisplayName = "カテゴリ検索で空の商品リストを返す")]
    public async Task Search_ReturnAnEmptyProductList()
    {
        // Arrange（準備）
        var usecaseMock = new Mock<ISearchProductByCategoryUsecase>();
        var categoryId = Guid.Parse("e50d978b-b73d-4afb-8e85-ace9cf1e12a7");
        var products = new List<Product>();

        usecaseMock
            .Setup(u => u.ExecuteAsync(categoryId))
            .ReturnsAsync(products);

        var controller = new SearchProductByCategory(usecaseMock.Object);


        // Act（実行）
        var result = await controller.Search(categoryId);


        // Assert（確認）
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        usecaseMock.Verify(
            u => u.ExecuteAsync(categoryId),
            Times.Once);
    }
}