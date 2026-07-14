using A_exercise_DB.Applications.Interfaces;
using A_exercise_DB.Applications.Usecases;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using Moq;

namespace A_exercise_DB.Applications.Tests.Usecases.Products;

/// <summary>
/// UpdateProductUsecaseクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Applications/Usecases/Products")]
public class UpdateProductUsecaseTests
{
    /// <summary>
    /// 非同期処理で指定した例外がスローされることを検証する
    /// </summary>
    private static async Task<TException> ThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException ex)
        {
            return ex;
        }

        Assert.Fail($"{typeof(TException).Name} がスローされませんでした。");
        throw new InvalidOperationException();
    }

    [TestMethod(DisplayName = "有効な商品修正情報を指定すると商品が更新される")]
    public async Task UpdateAsync_WithValidRequest_ShouldUpdateProduct()
    {
        var productUuid = Guid.NewGuid();
        var categoryUuid = Guid.NewGuid();
        var request = CreateValidRequest(categoryUuid);
        var category = new ProductCategory(categoryUuid, "文具");
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var categoryRepositoryMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        unitOfWorkMock
            .Setup(u => u.BeginAsync())
            .Returns(Task.CompletedTask);

        productRepositoryMock
            .Setup(r => r.FindByIdAsync(productUuid))
            .ReturnsAsync(CreateExistingProduct(productUuid, category, request.ImageUrl!));

        categoryRepositoryMock
            .Setup(r => r.FindByIdAsync(categoryUuid.ToString()))
            .ReturnsAsync(category);

        productRepositoryMock
            .Setup(r => r.UpdateByIdAsync(It.Is<Product>(p =>
                p.ProductUuid == productUuid &&
                p.Name == request.Name &&
                p.Price == request.Price &&
                p.ImageUrl == request.ImageUrl &&
                p.ProductCategory!.CategoryUuid == categoryUuid &&
                p.ProductStock!.Quantity == request.StockQuantity &&
                p.DeleteFlg == 0)))
            .ReturnsAsync(true);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        var usecase = new UpdateProductUsecase(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            new Mock<IImageUploadUsecase>().Object,
            new Mock<IImageStorage>().Object,
            unitOfWorkMock.Object);

        var result = await usecase.UpdateAsync(productUuid.ToString(), request);

        Assert.AreEqual(productUuid, result.ProductUuid);
        Assert.AreEqual(request.Name, result.Name);
        Assert.AreEqual(request.Price, result.Price);
        Assert.AreEqual(request.StockQuantity, result.StockQuantity);
        Assert.AreEqual(categoryUuid, result.CategoryUuid);
        Assert.AreEqual(request.ImageUrl, result.ImageUrl);
        Assert.IsTrue(result.Updated);

        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Once);
        categoryRepositoryMock.Verify(r => r.FindByIdAsync(categoryUuid.ToString()), Times.Once);
        productRepositoryMock.Verify(r => r.UpdateByIdAsync(It.IsAny<Product>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [TestMethod(DisplayName = "商品UUIDがGuid形式でない場合、DomainExceptionがスローされる")]
    public async Task UpdateAsync_WithInvalidProductUuid_ShouldThrowDomainException()
    {
        var categoryUuid = Guid.NewGuid();
        var request = CreateValidRequest(categoryUuid);
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var categoryRepositoryMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var usecase = new UpdateProductUsecase(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            new Mock<IImageUploadUsecase>().Object,
            new Mock<IImageStorage>().Object,
            unitOfWorkMock.Object);

        var ex = await ThrowsAsync<DomainException>(() =>
            usecase.UpdateAsync("invalid-product-uuid", request));

        Assert.AreEqual("商品識別IDが不正です。", ex.Message);
        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Never);
        categoryRepositoryMock.Verify(r => r.FindByIdAsync(It.IsAny<string>()), Times.Never);
        productRepositoryMock.Verify(r => r.UpdateByIdAsync(It.IsAny<Product>()), Times.Never);
    }

    [TestMethod(DisplayName = "商品名が1文字の場合、DomainExceptionがスローされる")]
    public async Task UpdateAsync_WithTooShortProductName_ShouldThrowDomainException()
    {
        var productUuid = Guid.NewGuid();
        var categoryUuid = Guid.NewGuid();
        var request = CreateValidRequest(categoryUuid) with { Name = "A" };
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var categoryRepositoryMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var usecase = new UpdateProductUsecase(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            new Mock<IImageUploadUsecase>().Object,
            new Mock<IImageStorage>().Object,
            unitOfWorkMock.Object);

        var ex = await ThrowsAsync<DomainException>(() =>
            usecase.UpdateAsync(productUuid.ToString(), request));

        Assert.AreEqual("商品名は2～20文字で入力してください。", ex.Message);
        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Never);
        categoryRepositoryMock.Verify(r => r.FindByIdAsync(It.IsAny<string>()), Times.Never);
        productRepositoryMock.Verify(r => r.UpdateByIdAsync(It.IsAny<Product>()), Times.Never);
    }

    [TestMethod(DisplayName = "商品カテゴリが存在しない場合、RollbackしてNotFoundExceptionがスローされる")]
    public async Task UpdateAsync_WhenCategoryDoesNotExist_ShouldRollbackAndThrowNotFoundException()
    {
        var productUuid = Guid.NewGuid();
        var categoryUuid = Guid.NewGuid();
        var request = CreateValidRequest(categoryUuid);
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var categoryRepositoryMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        unitOfWorkMock
            .Setup(u => u.BeginAsync())
            .Returns(Task.CompletedTask);

        productRepositoryMock
            .Setup(r => r.FindByIdAsync(productUuid))
            .ReturnsAsync(CreateExistingProduct(productUuid, new ProductCategory(Guid.NewGuid(), "既存カテゴリ"), request.ImageUrl!));

        categoryRepositoryMock
            .Setup(r => r.FindByIdAsync(categoryUuid.ToString()))
            .ReturnsAsync((ProductCategory?)null);

        unitOfWorkMock
            .Setup(u => u.RollbackAsync())
            .Returns(Task.CompletedTask);

        var usecase = new UpdateProductUsecase(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            new Mock<IImageUploadUsecase>().Object,
            new Mock<IImageStorage>().Object,
            unitOfWorkMock.Object);

        var ex = await ThrowsAsync<NotFoundException>(() =>
            usecase.UpdateAsync(productUuid.ToString(), request));

        Assert.AreEqual("指定された商品カテゴリが見つかりません。", ex.Message);
        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Once);
        categoryRepositoryMock.Verify(r => r.FindByIdAsync(categoryUuid.ToString()), Times.Once);
        productRepositoryMock.Verify(r => r.UpdateByIdAsync(It.IsAny<Product>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [TestMethod(DisplayName = "更新対象の商品が存在しない場合、RollbackしてNotFoundExceptionがスローされる")]
    public async Task UpdateAsync_WhenProductDoesNotExist_ShouldRollbackAndThrowNotFoundException()
    {
        var productUuid = Guid.NewGuid();
        var categoryUuid = Guid.NewGuid();
        var request = CreateValidRequest(categoryUuid);
        var category = new ProductCategory(categoryUuid, "文具");
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var categoryRepositoryMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        unitOfWorkMock
            .Setup(u => u.BeginAsync())
            .Returns(Task.CompletedTask);

        productRepositoryMock
            .Setup(r => r.FindByIdAsync(productUuid))
            .ReturnsAsync((Product?)null);

        unitOfWorkMock
            .Setup(u => u.RollbackAsync())
            .Returns(Task.CompletedTask);

        var usecase = new UpdateProductUsecase(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            new Mock<IImageUploadUsecase>().Object,
            new Mock<IImageStorage>().Object,
            unitOfWorkMock.Object);

        var ex = await ThrowsAsync<NotFoundException>(() =>
            usecase.UpdateAsync(productUuid.ToString(), request));

        Assert.AreEqual("更新対象の商品が見つかりません。", ex.Message);
        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Once);
        categoryRepositoryMock.Verify(r => r.FindByIdAsync(It.IsAny<string>()), Times.Never);
        productRepositoryMock.Verify(r => r.UpdateByIdAsync(It.IsAny<Product>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [TestMethod(DisplayName = "Repositoryで例外が発生した場合、Rollbackして例外が再スローされる")]
    public async Task UpdateAsync_WhenRepositoryThrowsException_ShouldRollbackAndRethrow()
    {
        var productUuid = Guid.NewGuid();
        var categoryUuid = Guid.NewGuid();
        var request = CreateValidRequest(categoryUuid);
        var category = new ProductCategory(categoryUuid, "文具");
        var expectedException = new InvalidOperationException("Repository error");
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var categoryRepositoryMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        unitOfWorkMock
            .Setup(u => u.BeginAsync())
            .Returns(Task.CompletedTask);

        productRepositoryMock
            .Setup(r => r.FindByIdAsync(productUuid))
            .ReturnsAsync(CreateExistingProduct(productUuid, category, request.ImageUrl!));

        categoryRepositoryMock
            .Setup(r => r.FindByIdAsync(categoryUuid.ToString()))
            .ReturnsAsync(category);

        productRepositoryMock
            .Setup(r => r.UpdateByIdAsync(It.IsAny<Product>()))
            .ThrowsAsync(expectedException);

        unitOfWorkMock
            .Setup(u => u.RollbackAsync())
            .Returns(Task.CompletedTask);

        var usecase = new UpdateProductUsecase(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            new Mock<IImageUploadUsecase>().Object,
            new Mock<IImageStorage>().Object,
            unitOfWorkMock.Object);

        var ex = await ThrowsAsync<InvalidOperationException>(() =>
            usecase.UpdateAsync(productUuid.ToString(), request));

        Assert.AreSame(expectedException, ex);
        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Once);
        categoryRepositoryMock.Verify(r => r.FindByIdAsync(categoryUuid.ToString()), Times.Once);
        productRepositoryMock.Verify(r => r.UpdateByIdAsync(It.IsAny<Product>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
    }

    private static ProductUpdateRequest CreateValidRequest(Guid categoryUuid)
        => new(
            "ゲルインクペン",
            150,
            40,
            categoryUuid.ToString(),
            "https://example.com/images/gel-pen.png");

    private static Product CreateExistingProduct(
        Guid productUuid,
        ProductCategory category,
        string imageUrl)
        => new(
            productUuid,
            "既存商品",
            100,
            imageUrl,
            category,
            new ProductStock(10),
            0);
}
