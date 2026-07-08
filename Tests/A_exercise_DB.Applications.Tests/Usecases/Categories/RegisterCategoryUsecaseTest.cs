using A_exercise_DB.Applications.Usecases.Categories;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Applications.Usecases;
using Moq;

namespace A_exercise_DB.Applications.Tests.Usecases.Categories;

/// <summary>
/// RegisterCategoryUsecaseの単体テスト
/// </summary>
[TestClass]
public class RegisterCategoryUsecaseTests
{
    private Mock<IProductCategoryRepository> _productCategoryRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private RegisterCategoryUsecase _usecase = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _productCategoryRepositoryMock = new Mock<IProductCategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _usecase = new RegisterCategoryUsecase(
            _productCategoryRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    /// <summary>
    /// 同じカテゴリ名が存在しない場合、例外が発生しないこと
    /// </summary>
    [TestMethod(DisplayName = "同じカテゴリ名が存在しない場合、例外が発生しない")]
    public async Task ExistsByCategoryAsync_WhenCategoryDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var categoryName = "食品";

        _productCategoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(categoryName))
            .ReturnsAsync(false);

        // Act
        await _usecase.ExistsByCategoryAsync(categoryName);

        // Assert
        _productCategoryRepositoryMock.Verify(
            x => x.ExistsByNameAsync(categoryName),
            Times.Once
        );
    }

    /// <summary>
    /// 同じカテゴリ名が存在する場合、ExistsExceptionが発生すること
    /// </summary>
    [TestMethod(DisplayName = "同じカテゴリ名が存在する場合、ExistsExceptionが発生する")]
    public async Task ExistsByCategoryAsync_WhenCategoryExists_ShouldThrowExistsException()
    {
        // Arrange
        var categoryName = "食品";

        _productCategoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(categoryName))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<ExistsException>(async () =>
            await _usecase.ExistsByCategoryAsync(categoryName)
        );

        // Assert
        Assert.Contains(categoryName, exception.Message);

        _productCategoryRepositoryMock.Verify(
            x => x.ExistsByNameAsync(categoryName),
            Times.Once
        );
    }

    /// <summary>
    /// 登録に成功した場合、BeginAsync、CreateAsync、CommitAsyncが呼ばれること
    /// </summary>
    [TestMethod(DisplayName = "登録に成功した場合、BeginAsync、CreateAsync、CommitAsyncが呼ばれる")]
    public async Task RegisterCategoryAsync_WhenCreateSucceeds_ShouldCommitTransaction()
    {
        // Arrange
        var productCategory = new ProductCategory("食品");

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productCategoryRepositoryMock
            .Setup(x => x.CreateAsync(productCategory))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _usecase.RegisterCategoryAsync(productCategory);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once
        );

        _productCategoryRepositoryMock.Verify(
            x => x.CreateAsync(productCategory),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Never
        );
    }

    /// <summary>
    /// CreateAsyncで例外が発生した場合、RollbackAsyncが呼ばれ、例外が再スローされること
    /// </summary>
    [TestMethod(DisplayName = "CreateAsyncで例外が発生した場合、RollbackAsyncが呼ばれ、例外が再スローされる")]
    public async Task RegisterCategoryAsync_WhenCreateFails_ShouldRollbackAndRethrowException()
    {
        // Arrange
        var productCategory = new ProductCategory("食品");
        var expectedException = new Exception("登録に失敗しました。");

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productCategoryRepositoryMock
            .Setup(x => x.CreateAsync(productCategory))
            .ThrowsAsync(expectedException);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var actualException = await Assert.ThrowsExactlyAsync<Exception>(async () =>
            await _usecase.RegisterCategoryAsync(productCategory)
        );

        // Assert
        Assert.AreEqual(expectedException.Message, actualException.Message);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once
        );

        _productCategoryRepositoryMock.Verify(
            x => x.CreateAsync(productCategory),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never
        );
    }

    /// <summary>
    /// 登録成功時、RollbackAsyncが呼ばれないこと
    /// </summary>
    [TestMethod(DisplayName = "登録成功時、RollbackAsyncが呼ばれない")]
    public async Task RegisterCategoryAsync_WhenCreateSucceeds_ShouldNotRollback()
    {
        // Arrange
        var productCategory = new ProductCategory("食品");

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productCategoryRepositoryMock
            .Setup(x => x.CreateAsync(productCategory))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _usecase.RegisterCategoryAsync(productCategory);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Never
        );
    }

    /// <summary>
    /// 登録失敗時、CommitAsyncが呼ばれないこと
    /// </summary>
    [TestMethod(DisplayName = "登録失敗時、CommitAsyncが呼ばれない")]
    public async Task RegisterCategoryAsync_WhenCreateFails_ShouldNotCommit()
    {
        // Arrange
        var productCategory = new ProductCategory("食品");

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _productCategoryRepositoryMock
            .Setup(x => x.CreateAsync(productCategory))
            .ThrowsAsync(new Exception("登録エラー"));

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        await Assert.ThrowsExactlyAsync<Exception>(async () =>
            await _usecase.RegisterCategoryAsync(productCategory)
        );

        // Assert
        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never
        );
    }
}