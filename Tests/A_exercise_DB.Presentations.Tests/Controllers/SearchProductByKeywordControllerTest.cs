using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace A_exercise_DB.Presentations.Tests.Controllers;

[TestClass]
[TestCategory("Controllers")]
public class SearchProductByKeywordControllerTests
{
    private Mock<ISearchProductByKeywordUsecase>? _usecaseMock;
    private SearchProductByKeywordController? _controller;

    [TestInitialize]
    public void TestInit()
    {
        _usecaseMock = new Mock<ISearchProductByKeywordUsecase>();
        _controller = new SearchProductByKeywordController(_usecaseMock.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _controller = null;
        _usecaseMock = null;
    }

    /// <summary>
    /// 存在するキーワードの場合、200 OKと商品一覧を返す
    /// </summary>
    [TestMethod(DisplayName = "存在するキーワードの場合、200 OKと商品一覧を返す")]
    public async Task Search_ShouldReturnOkWithProducts_WhenKeywordExists()
    {
        // Arrange
        var category = new ProductCategory(Guid.NewGuid(), "文房具");
        const bool showDeletedOnly = false;

        var expectedProducts = new List<Product>
        {
            new Product(
                Guid.NewGuid(),
                "ペン",
                100,
                "",
                category,
                new ProductStock(Guid.NewGuid(), 10),
                0),

            new Product(
                Guid.NewGuid(),
                "赤ペン",
                120,
                "",
                category,
                new ProductStock(Guid.NewGuid(), 5),
                0)
        };

        _usecaseMock!
            .Setup(x => x.ExecuteAsync("ペン", showDeletedOnly))
            .ReturnsAsync(expectedProducts);

        // Act
        var result = await _controller!.Search("ペン", showDeletedOnly);

        // Assert
        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

        var products = ok.Value as List<Product>;
        Assert.IsNotNull(products);

        Assert.HasCount(2, products);
        CollectionAssert.AreEqual(expectedProducts, products);

        _usecaseMock.Verify(
            x => x.ExecuteAsync("ペン", showDeletedOnly),
            Times.Once);
    }

    /// <summary>
    /// キーワードがnullの場合、400 BadRequestを返す
    /// </summary>
    [TestMethod(DisplayName = "キーワードがnullの場合、400 BadRequestを返す")]
    public async Task Search_ShouldReturnBadRequest_WhenKeywordIsNull()
    {
        const bool showDeletedOnly = false;
        // Act
        var result = await _controller!.Search(null, showDeletedOnly);

        // Assert
        var badRequest = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequest);
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        _usecaseMock!.Verify(
            x => x.ExecuteAsync(It.IsAny<string>(), showDeletedOnly),
            Times.Never);
    }

    /// <summary>
    /// キーワードが空文字の場合、400 BadRequestを返す
    /// </summary>
    [TestMethod(DisplayName = "キーワードが空文字の場合、400 BadRequestを返す")]
    public async Task Search_ShouldReturnBadRequest_WhenKeywordIsEmpty()
    {
        const bool showDeletedOnly = false;
        // Act
        var result = await _controller!.Search("", showDeletedOnly);

        // Assert
        var badRequest = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequest);
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        _usecaseMock!.Verify(
            x => x.ExecuteAsync(It.IsAny<string>(), showDeletedOnly),
            Times.Never);
    }

    /// <summary>
    /// キーワードが空白のみの場合、400 BadRequestを返す
    /// </summary>
    [TestMethod(DisplayName = "キーワードが空白のみの場合、400 BadRequestを返す")]
    public async Task Search_ShouldReturnBadRequest_WhenKeywordIsWhiteSpace()
    {
        const bool showDeletedOnly = false;
        // Act
        var result = await _controller!.Search("   ", showDeletedOnly);

        // Assert
        var badRequest = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequest);
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        _usecaseMock!.Verify(
            x => x.ExecuteAsync(It.IsAny<string>(), showDeletedOnly),
            Times.Never);
    }
}