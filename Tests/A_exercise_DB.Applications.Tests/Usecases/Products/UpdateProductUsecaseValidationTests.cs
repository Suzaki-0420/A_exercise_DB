using A_exercise_DB.Applications.Usecases;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using Moq;

namespace A_exercise_DB.Applications.Tests.Usecases.Products;

/// <summary>
/// UpdateProductUsecaseの入力境界値テスト
/// </summary>
[TestClass]
[TestCategory("Applications/Usecases/Products")]
public class UpdateProductUsecaseValidationTests
{
    [TestMethod(DisplayName = "商品修正情報がnullの場合、DomainExceptionが発生する")]
    public async Task UpdateAsync_WithNullRequest_ShouldThrowDomainException()
    {
        await AssertValidationErrorAsync(
            Guid.NewGuid().ToString(),
            null,
            "商品修正情報を入力してください。");
    }

    [TestMethod(DisplayName = "商品UUIDがGuid.Emptyの場合、DomainExceptionが発生する")]
    public async Task UpdateAsync_WithEmptyProductUuid_ShouldThrowDomainException()
    {
        await AssertValidationErrorAsync(
            Guid.Empty.ToString(),
            CreateValidRequest(),
            "商品識別IDが不正です。");
    }

    [TestMethod(DisplayName = "商品名が空白の場合、DomainExceptionが発生する")]
    public async Task UpdateAsync_WithWhiteSpaceName_ShouldThrowDomainException()
    {
        await AssertValidationErrorAsync(
            Guid.NewGuid().ToString(),
            CreateValidRequest() with { Name = "   " },
            "商品名を入力してください。");
    }

    [TestMethod(DisplayName = "商品名が21文字の場合、DomainExceptionが発生する")]
    public async Task UpdateAsync_WithProductNameOverMaximum_ShouldThrowDomainException()
    {
        await AssertValidationErrorAsync(
            Guid.NewGuid().ToString(),
            CreateValidRequest() with { Name = new string('A', 21) },
            "商品名は2～20文字で入力してください。");
    }

    [TestMethod(DisplayName = "価格が負数の場合、DomainExceptionが発生する")]
    public async Task UpdateAsync_WithNegativePrice_ShouldThrowDomainException()
    {
        await AssertValidationErrorAsync(
            Guid.NewGuid().ToString(),
            CreateValidRequest() with { Price = -1 },
            "価格は0以上で入力してください。");
    }

    [TestMethod(DisplayName = "価格が100万円を超える場合、DomainExceptionが発生する")]
    public async Task UpdateAsync_WithPriceOverMaximum_ShouldThrowDomainException()
    {
        await AssertValidationErrorAsync(
            Guid.NewGuid().ToString(),
            CreateValidRequest() with { Price = 1_000_001 },
            "価格は100万円以下で入力してください。");
    }

    [TestMethod(DisplayName = "在庫数が負数の場合、DomainExceptionが発生する")]
    public async Task UpdateAsync_WithNegativeStockQuantity_ShouldThrowDomainException()
    {
        await AssertValidationErrorAsync(
            Guid.NewGuid().ToString(),
            CreateValidRequest() with { StockQuantity = -1 },
            "在庫数は0以上で入力してください。");
    }

    [TestMethod(DisplayName = "在庫数が1000個を超える場合、DomainExceptionが発生する")]
    public async Task UpdateAsync_WithStockQuantityOverMaximum_ShouldThrowDomainException()
    {
        await AssertValidationErrorAsync(
            Guid.NewGuid().ToString(),
            CreateValidRequest() with { StockQuantity = 1_001 },
            "在庫数は1000個以下で入力してください。");
    }

    [TestMethod(DisplayName = "カテゴリUUIDが不正な形式の場合、DomainExceptionが発生する")]
    public async Task UpdateAsync_WithInvalidCategoryUuid_ShouldThrowDomainException()
    {
        await AssertValidationErrorAsync(
            Guid.NewGuid().ToString(),
            CreateValidRequest() with { CategoryUuid = "invalid-category-uuid" },
            "カテゴリを選択してください。");
    }

    [TestMethod(DisplayName = "商品名・価格・在庫数が下限値の場合、商品を更新できる")]
    public async Task UpdateAsync_WithMinimumBoundaryValues_ShouldUpdateProduct()
    {
        var request = CreateValidRequest() with
        {
            Name = "AB",
            Price = 0,
            StockQuantity = 0
        };

        await AssertUpdateSucceedsAsync(request);
    }

    [TestMethod(DisplayName = "商品名・価格・在庫数が上限値の場合、商品を更新できる")]
    public async Task UpdateAsync_WithMaximumBoundaryValues_ShouldUpdateProduct()
    {
        var request = CreateValidRequest() with
        {
            Name = new string('A', 20),
            Price = 1_000_000,
            StockQuantity = 1_000
        };

        await AssertUpdateSucceedsAsync(request);
    }

    private static ProductUpdateRequest CreateValidRequest()
        => new(
            "ゲルインクペン",
            150,
            40,
            Guid.NewGuid().ToString(),
            null);

    private static UpdateProductUsecase CreateUsecase(
        Mock<IProductRepository> productRepositoryMock,
        Mock<IProductCategoryRepository> categoryRepositoryMock,
        Mock<IUnitOfWork> unitOfWorkMock)
        => new(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            unitOfWorkMock.Object);

    private static async Task AssertValidationErrorAsync(
        string productUuid,
        ProductUpdateRequest? request,
        string expectedMessage)
    {
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var categoryRepositoryMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var usecase = CreateUsecase(
            productRepositoryMock,
            categoryRepositoryMock,
            unitOfWorkMock);

        var exception = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
            await usecase.UpdateAsync(productUuid, request!));

        Assert.AreEqual(expectedMessage, exception.Message);
        productRepositoryMock.VerifyNoOtherCalls();
        categoryRepositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyNoOtherCalls();
    }

    private static async Task AssertUpdateSucceedsAsync(ProductUpdateRequest request)
    {
        var productUuid = Guid.NewGuid();
        var categoryUuid = Guid.Parse(request.CategoryUuid);
        var category = new ProductCategory(categoryUuid, "文具");
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var categoryRepositoryMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        unitOfWorkMock
            .Setup(u => u.BeginAsync())
            .Returns(Task.CompletedTask);
        categoryRepositoryMock
            .Setup(r => r.FindByIdAsync(request.CategoryUuid))
            .ReturnsAsync(category);
        productRepositoryMock
            .Setup(r => r.UpdateByIdAsync(It.Is<Product>(p =>
                p.ProductUuid == productUuid &&
                p.Name == request.Name &&
                p.Price == request.Price &&
                p.ProductStock!.Quantity == request.StockQuantity)))
            .ReturnsAsync(true);
        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        var usecase = CreateUsecase(
            productRepositoryMock,
            categoryRepositoryMock,
            unitOfWorkMock);

        var result = await usecase.UpdateAsync(productUuid.ToString(), request);

        Assert.IsTrue(result.Updated);
        Assert.AreEqual(productUuid, result.ProductUuid);
        Assert.AreEqual(request.Name, result.Name);
        Assert.AreEqual(request.Price, result.Price);
        Assert.AreEqual(request.StockQuantity, result.StockQuantity);
        productRepositoryMock.VerifyAll();
        categoryRepositoryMock.VerifyAll();
        unitOfWorkMock.VerifyAll();
    }
}
