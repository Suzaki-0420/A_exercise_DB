using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Applications.Usecases.Products;
using Moq;

namespace A_exercise_DB.Applications.Tests.Usecases.Products;
[TestClass]
public class SearchProductByKeywordUsecaseTests
{
    [TestMethod(DisplayName = "存在する商品キーワードで商品を取得できる")]
    public async Task ExecuteAsync_ShouldReturnProducts_WhenKeywordExists()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var category = new ProductCategory("食品");
        var products = new List<Product>
        {
            new Product(
                Guid.NewGuid(),
                "りんご缶",
                100,
                "",
                category,
                new ProductStock(10),
                0
            ),
            new Product(
                Guid.NewGuid(),
                "みかん缶",
                200,
                "",
                category,
                new ProductStock(5),
                0
            )
        };
        repositoryMock
            .Setup(r => r.SearchKeywordAsync("缶"))
            .ReturnsAsync(products);

        var usecase = new SearchProductByKeywordUsecase(repositoryMock.Object);

        var result = await usecase.ExecuteAsync("缶");

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
    }


    [TestMethod(DisplayName = "存在しない商品キーワードの場合、空のリストが返される")]
    public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenKeywordDoesNotExist()
    {
        var repositoryMock = new Mock<IProductRepository>();
        var products = new List<Product>();
        repositoryMock
           .Setup(r => r.SearchKeywordAsync("輪ゴム"))
           .ReturnsAsync(products);

        var usecase = new SearchProductByKeywordUsecase(repositoryMock.Object);

        var result = await usecase!.ExecuteAsync("輪ゴム");
        Assert.IsNotNull(result);
        // 件数が0件であることを検証する
        Assert.IsEmpty(result);
    }
}