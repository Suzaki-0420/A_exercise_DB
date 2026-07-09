using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Applications.Usecases.Products;

namespace A_exercise_DB.Tests.Usecases.Products;

[TestClass]
public class SearchProductByCategoryUsecaseTests
{
    [TestMethod(DisplayName = "カテゴリ別に商品を取得して商品リストを返す")]
    public async Task GetProductsByCategory_ReturnsProductList()
    {
        // Arrange（準備）
        var repositoryMock = new Mock<IProductRepository>();

        var category = new ProductCategory("食品");

        var products = new List<Product>
        {
            new Product(
                Guid.NewGuid(),
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

        repositoryMock
            .Setup(r => r.SelectByProductCategoryIdAsync(1))
            .ReturnsAsync(products);


        var usecase = new SearchProductByCategoryUsecase(repositoryMock.Object);


        // Act（実行）
        var result = await usecase.ExecuteAsync(1);


        // Assert（確認）
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
    }

    [TestMethod(DisplayName = "カテゴリ検索をして空のリストを返す")]
    public async Task ReturnAnEmptyProductList()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var products = new List<Product>();
        repositoryMock
           .Setup(r => r.SelectByProductCategoryIdAsync(1))
           .ReturnsAsync(products);
        var usecase = new SearchProductByCategoryUsecase(repositoryMock.Object);
        var result = await usecase.ExecuteAsync(1);
        Assert.IsNotNull(result);
        Assert.HasCount(0, result);
    }

}