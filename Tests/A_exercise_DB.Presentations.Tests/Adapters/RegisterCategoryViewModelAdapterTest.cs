using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;

namespace A_exercise_DB.Presentations.Tests.Adapters;

/// <summary>
/// RegisterCategoryViewModelAdapterの単体テスト
/// </summary>
[TestClass]
public class RegisterCategoryViewModelAdapterTests
{
    private RegisterCategoryViewModelAdapter _adapter = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _adapter = new RegisterCategoryViewModelAdapter();
    }

    /// <summary>
    /// 正常なViewModelを渡した場合、ProductCategoryが生成されること
    /// </summary>
    [TestMethod(DisplayName = "正常なViewModelを渡した場合、ProductCategoryが生成される")]
    public async Task RestoreAsync_WhenValidViewModel_ShouldReturnProductCategory()
    {
        // Arrange
        var viewModel = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        // Act
        var result = await _adapter.RestoreAsync(viewModel);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// CategoryNameに食品を指定した場合、復元後のカテゴリ名が食品になること
    /// </summary>
    [TestMethod(DisplayName = "CategoryNameに食品を指定した場合、復元後のカテゴリ名が食品になる")]
    public async Task RestoreAsync_WhenCategoryNameIsFood_ShouldSetCategoryName()
    {
        // Arrange
        var viewModel = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        // Act
        var result = await _adapter.RestoreAsync(viewModel);

        // Assert
        Assert.AreEqual("食品", result.Name);
    }

    /// <summary>
    /// ViewModelを渡した場合、カテゴリIDが空のGuidではないこと
    /// </summary>
    [TestMethod(DisplayName = "ViewModelを渡した場合、カテゴリIDが空のGuidではない")]
    public async Task RestoreAsync_WhenValidViewModel_ShouldSetNotEmptyGuid()
    {
        // Arrange
        var viewModel = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        // Act
        var result = await _adapter.RestoreAsync(viewModel);

        // Assert
        Assert.AreNotEqual(Guid.Empty, result.CategoryUuid);
    }

    /// <summary>
    /// CategoryNameが空文字の場合、DomainExceptionが発生すること
    /// </summary>
    [TestMethod(DisplayName = "CategoryNameが空文字の場合、DomainExceptionが発生する")]
    public async Task RestoreAsync_WhenCategoryNameIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var viewModel = new RegisterCategoryViewModel
        {
            CategoryName = string.Empty
        };

        // Act & Assert
        await Assert.ThrowsExactlyAsync<DomainException>(async () =>
            await _adapter.RestoreAsync(viewModel)
        );
    }
}