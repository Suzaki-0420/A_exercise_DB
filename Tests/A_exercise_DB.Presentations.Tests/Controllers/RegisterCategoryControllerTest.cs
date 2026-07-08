using A_exercise_DB.Applications.Usecases.Categories;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.Controllers;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace A_exercise_DB.Presentations.Tests.Controllers;

/// <summary>
/// RegisterCategoryControllerの単体テスト
/// </summary>
[TestClass]
public class RegisterCategoryControllerTests
{
    private Mock<IRegisterCategoryUsecase> _registerCategoryUsecaseMock = null!;
    private RegisterCategoryViewModelAdapter _adapter = null!;
    private RegisterCategoryController _controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _registerCategoryUsecaseMock = new Mock<IRegisterCategoryUsecase>();
        _adapter = new RegisterCategoryViewModelAdapter();

        _controller = new RegisterCategoryController(
            _registerCategoryUsecaseMock.Object,
            _adapter
        );
    }

    /// <summary>
    /// ValidateCategoryName:
    /// categoryNameが空文字の場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ValidateCategoryNameでcategoryNameが空文字の場合、BadRequestを返す")]
    public async Task ValidateCategoryName_WhenCategoryNameIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var categoryName = string.Empty;

        // Act
        var result = await _controller.ValidateCategoryName(categoryName);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    /// <summary>
    /// ValidateCategoryName:
    /// categoryNameが空白の場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ValidateCategoryNameでcategoryNameが空白の場合、BadRequestを返す")]
    public async Task ValidateCategoryName_WhenCategoryNameIsWhiteSpace_ShouldReturnBadRequest()
    {
        // Arrange
        var categoryName = "   ";

        // Act
        var result = await _controller.ValidateCategoryName(categoryName);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    /// <summary>
    /// ValidateCategoryName:
    /// カテゴリ名が存在しない場合、Okを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ValidateCategoryNameでカテゴリ名が存在しない場合、Okを返す")]
    public async Task ValidateCategoryName_WhenCategoryDoesNotExist_ShouldReturnOk()
    {
        // Arrange
        var categoryName = "食品";

        _registerCategoryUsecaseMock
            .Setup(x => x.ExistsByCategoryAsync(categoryName))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ValidateCategoryName(categoryName);

        // Assert
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(categoryName),
            Times.Once
        );
    }

    /// <summary>
    /// ValidateCategoryName:
    /// カテゴリ名が既に存在する場合、Conflictを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ValidateCategoryNameでカテゴリ名が既に存在する場合、Conflictを返す")]
    public async Task ValidateCategoryName_WhenCategoryExists_ShouldReturnConflict()
    {
        // Arrange
        var categoryName = "食品";

        _registerCategoryUsecaseMock
            .Setup(x => x.ExistsByCategoryAsync(categoryName))
            .ThrowsAsync(new ExistsException($"カテゴリ名：{categoryName}"));

        // Act
        var result = await _controller.ValidateCategoryName(categoryName);

        // Assert
        var conflictResult = result as ConflictObjectResult;

        Assert.IsNotNull(conflictResult);
        Assert.AreEqual(409, conflictResult.StatusCode);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(categoryName),
            Times.Once
        );
    }

    /// <summary>
    /// Register:
    /// ModelStateが不正な場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "RegisterでModelStateが不正な場合、BadRequestを返す")]
    public async Task Register_WhenModelStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var model = new RegisterCategoryViewModel
        {
            CategoryName = string.Empty
        };

        _controller.ModelState.AddModelError(
            "CategoryName",
            "商品カテゴリ名は必須です。"
        );

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(It.IsAny<string>()),
            Times.Never
        );

        _registerCategoryUsecaseMock.Verify(
            x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()),
            Times.Never
        );
    }

    /// <summary>
    /// Register:
    /// 正常なViewModelの場合、Createdを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Registerで正常なViewModelの場合、Createdを返す")]
    public async Task Register_WhenValidModel_ShouldReturnCreated()
    {
        // Arrange
        var model = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        _registerCategoryUsecaseMock
            .Setup(x => x.ExistsByCategoryAsync(model.CategoryName))
            .Returns(Task.CompletedTask);

        _registerCategoryUsecaseMock
            .Setup(x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Register(model);

        // Assert
        var createdResult = result as CreatedResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);

        var category = createdResult.Value as ProductCategory;

        Assert.IsNotNull(category);
        Assert.AreEqual(model.CategoryName, category.Name);
        Assert.AreNotEqual(Guid.Empty, category.CategoryUuid);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(model.CategoryName),
            Times.Once
        );

        _registerCategoryUsecaseMock.Verify(
            x => x.RegisterCategoryAsync(It.Is<ProductCategory>(
                c => c.Name == model.CategoryName
            )),
            Times.Once
        );
    }

    /// <summary>
    /// Register:
    /// カテゴリ名が既に存在する場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Registerでカテゴリ名が既に存在する場合、BadRequestを返す")]
    public async Task Register_WhenCategoryAlreadyExists_ShouldReturnBadRequest()
    {
        // Arrange
        var model = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        _registerCategoryUsecaseMock
            .Setup(x => x.ExistsByCategoryAsync(model.CategoryName))
            .ThrowsAsync(new ExistsException($"カテゴリ名：{model.CategoryName}"));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(model.CategoryName),
            Times.Once
        );

        _registerCategoryUsecaseMock.Verify(
            x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()),
            Times.Never
        );
    }

    /// <summary>
    /// Register:
    /// ドメイン例外が発生した場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Registerでドメイン例外が発生した場合、BadRequestを返す")]
    public async Task Register_WhenDomainExceptionOccurs_ShouldReturnBadRequest()
    {
        // Arrange
        var model = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        _registerCategoryUsecaseMock
            .Setup(x => x.ExistsByCategoryAsync(model.CategoryName))
            .Returns(Task.CompletedTask);

        _registerCategoryUsecaseMock
            .Setup(x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()))
            .ThrowsAsync(new DomainException("商品カテゴリ名は不正です。"));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(model.CategoryName),
            Times.Once
        );

        _registerCategoryUsecaseMock.Verify(
            x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()),
            Times.Once
        );
    }
}