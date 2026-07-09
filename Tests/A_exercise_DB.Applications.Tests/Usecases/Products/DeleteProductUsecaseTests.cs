using A_exercise_DB.Applications.Usecases;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Repositories;
using Moq;

namespace A_exercise_DB.Applications.Tests.Usecases.Products;

/// <summary>
/// DeleteProductUsecaseクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Applications/Usecases/Products")]
public class DeleteProductUsecaseTests
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

    [TestMethod(DisplayName = "有効な商品UUIDを指定すると商品が論理削除される")]
    public async Task DeleteAsync_WithValidProductUuid_ShouldDeleteProduct()
    {
        var productUuid = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        unitOfWorkMock
            .Setup(u => u.BeginAsync())
            .Returns(Task.CompletedTask);

        productRepositoryMock
            .Setup(r => r.DeleteByIdAsync(productUuid.ToString()))
            .ReturnsAsync(true);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        var usecase = new DeleteProductUsecase(
            productRepositoryMock.Object,
            unitOfWorkMock.Object);

        var result = await usecase.DeleteAsync(productUuid.ToString());

        Assert.AreEqual(productUuid, result.ProductUuid);
        Assert.IsTrue(result.Deleted);

        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Once);
        productRepositoryMock.Verify(r => r.DeleteByIdAsync(productUuid.ToString()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [TestMethod(DisplayName = "商品UUIDがGuid形式でない場合、DomainExceptionがスローされる")]
    public async Task DeleteAsync_WithInvalidProductUuid_ShouldThrowDomainException()
    {
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var usecase = new DeleteProductUsecase(
            productRepositoryMock.Object,
            unitOfWorkMock.Object);

        var ex = await ThrowsAsync<DomainException>(() =>
            usecase.DeleteAsync("invalid-product-uuid"));

        Assert.AreEqual("商品識別IDが不正です。", ex.Message);
        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Never);
        productRepositoryMock.Verify(r => r.DeleteByIdAsync(It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [TestMethod(DisplayName = "商品UUIDが空Guidの場合、DomainExceptionがスローされる")]
    public async Task DeleteAsync_WithEmptyProductUuid_ShouldThrowDomainException()
    {
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var usecase = new DeleteProductUsecase(
            productRepositoryMock.Object,
            unitOfWorkMock.Object);

        var ex = await ThrowsAsync<DomainException>(() =>
            usecase.DeleteAsync(Guid.Empty.ToString()));

        Assert.AreEqual("商品識別IDが不正です。", ex.Message);
        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Never);
        productRepositoryMock.Verify(r => r.DeleteByIdAsync(It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [TestMethod(DisplayName = "削除対象の商品が存在しない場合、RollbackしてNotFoundExceptionがスローされる")]
    public async Task DeleteAsync_WhenProductDoesNotExist_ShouldRollbackAndThrowNotFoundException()
    {
        var productUuid = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        unitOfWorkMock
            .Setup(u => u.BeginAsync())
            .Returns(Task.CompletedTask);

        productRepositoryMock
            .Setup(r => r.DeleteByIdAsync(productUuid.ToString()))
            .ReturnsAsync(false);

        unitOfWorkMock
            .Setup(u => u.RollbackAsync())
            .Returns(Task.CompletedTask);

        var usecase = new DeleteProductUsecase(
            productRepositoryMock.Object,
            unitOfWorkMock.Object);

        var ex = await ThrowsAsync<NotFoundException>(() =>
            usecase.DeleteAsync(productUuid.ToString()));

        Assert.AreEqual("削除対象の商品が見つかりません。", ex.Message);
        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Once);
        productRepositoryMock.Verify(r => r.DeleteByIdAsync(productUuid.ToString()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
    }

    [TestMethod(DisplayName = "Repositoryで例外が発生した場合、Rollbackして例外が再スローされる")]
    public async Task DeleteAsync_WhenRepositoryThrowsException_ShouldRollbackAndRethrow()
    {
        var productUuid = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Repository error");
        var productRepositoryMock = new Mock<IProductRepository>(MockBehavior.Strict);
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        unitOfWorkMock
            .Setup(u => u.BeginAsync())
            .Returns(Task.CompletedTask);

        productRepositoryMock
            .Setup(r => r.DeleteByIdAsync(productUuid.ToString()))
            .ThrowsAsync(expectedException);

        unitOfWorkMock
            .Setup(u => u.RollbackAsync())
            .Returns(Task.CompletedTask);

        var usecase = new DeleteProductUsecase(
            productRepositoryMock.Object,
            unitOfWorkMock.Object);

        var ex = await ThrowsAsync<InvalidOperationException>(() =>
            usecase.DeleteAsync(productUuid.ToString()));

        Assert.AreSame(expectedException, ex);
        unitOfWorkMock.Verify(u => u.BeginAsync(), Times.Once);
        productRepositoryMock.Verify(r => r.DeleteByIdAsync(productUuid.ToString()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
    }
}
