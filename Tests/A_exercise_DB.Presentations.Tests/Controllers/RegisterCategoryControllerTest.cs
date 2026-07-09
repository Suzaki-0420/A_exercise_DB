using A_exercise_DB.Applications.Usecases.Categories;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.Controllers;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace A_exercise_DB.Presentations.Tests.Controllers;

/// <summary>
/// RegisterCategoryControllerの単体テスト
/// </summary>
[TestClass]
public class RegisterCategoryControllerTests
{
    private Mock<IRegisterCategoryUsecase> _registerCategoryUsecaseMock = null!;
    private Mock<ILogger<RegisterCategoryController>> _loggerMock = null!;
    private RegisterCategoryViewModelAdapter _adapter = null!;
    private RegisterCategoryController _controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _registerCategoryUsecaseMock = new Mock<IRegisterCategoryUsecase>();
        _loggerMock = new Mock<ILogger<RegisterCategoryController>>();
        _adapter = new RegisterCategoryViewModelAdapter();

        _controller = new RegisterCategoryController(
            _registerCategoryUsecaseMock.Object,
            _adapter,
            _loggerMock.Object
        );
    }

    /// <summary>
    /// 匿名オブジェクトのプロパティ値を取得する
    /// </summary>
    private static object? GetPropertyValue(object target, string propertyName)
    {
        return target.GetType()
            .GetProperty(propertyName)?
            .GetValue(target);
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

        _registerCategoryUsecaseMock
            .Setup(x => x.ExistsByCategoryAsync(categoryName))
            .ThrowsAsync(new DomainException("カテゴリ名を入力してください"));

        // Act
        var result = await _controller.ValidateCategoryName(categoryName);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        var code = GetPropertyValue(badRequestResult.Value!, "code");
        var message = GetPropertyValue(badRequestResult.Value!, "message");

        Assert.AreEqual("VALIDATION_ERROR", code);
        Assert.AreEqual("カテゴリ名を入力してください", message);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(categoryName),
            Times.Once
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

        _registerCategoryUsecaseMock
            .Setup(x => x.ExistsByCategoryAsync(categoryName))
            .ThrowsAsync(new DomainException("カテゴリ名を入力してください"));

        // Act
        var result = await _controller.ValidateCategoryName(categoryName);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        var code = GetPropertyValue(badRequestResult.Value!, "code");
        var message = GetPropertyValue(badRequestResult.Value!, "message");

        Assert.AreEqual("VALIDATION_ERROR", code);
        Assert.AreEqual("カテゴリ名を入力してください", message);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(categoryName),
            Times.Once
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

        var exists = GetPropertyValue(okResult.Value!, "exists");
        var message = GetPropertyValue(okResult.Value!, "message");

        Assert.IsFalse((bool?)exists);
        Assert.AreEqual("使用できるカテゴリ名です", message);

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
            .ThrowsAsync(new ExistsException("このカテゴリ名は既に登録されています"));

        // Act
        var result = await _controller.ValidateCategoryName(categoryName);

        // Assert
        var conflictResult = result as ConflictObjectResult;

        Assert.IsNotNull(conflictResult);
        Assert.AreEqual(409, conflictResult.StatusCode);

        var code = GetPropertyValue(conflictResult.Value!, "code");
        var exists = GetPropertyValue(conflictResult.Value!, "exists");
        var message = GetPropertyValue(conflictResult.Value!, "message");

        Assert.AreEqual("CATEGORY_ALREADY_EXISTS", code);
        Assert.IsTrue((bool?)exists);
        Assert.AreEqual("このカテゴリ名は既に登録されています", message);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(categoryName),
            Times.Once
        );
    }

    /// <summary>
    /// ValidateCategoryName:
    /// 予期しない例外が発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ValidateCategoryNameで予期しない例外が発生した場合、InternalServerErrorを返す")]
    public async Task ValidateCategoryName_WhenUnexpectedExceptionOccurs_ShouldReturnInternalServerError()
    {
        // Arrange
        var categoryName = "食品";

        _registerCategoryUsecaseMock
            .Setup(x => x.ExistsByCategoryAsync(categoryName))
            .ThrowsAsync(new Exception("DB接続エラー"));

        // Act
        var result = await _controller.ValidateCategoryName(categoryName);

        // Assert
        var objectResult = result as ObjectResult;

        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);

        var code = GetPropertyValue(objectResult.Value!, "code");
        var message = GetPropertyValue(objectResult.Value!, "message");

        Assert.AreEqual("SYSTEM_ERROR", code);
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", message);

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
            "カテゴリ名を入力してください"
        );

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        var code = GetPropertyValue(badRequestResult.Value!, "code");
        var message = GetPropertyValue(badRequestResult.Value!, "message");

        Assert.AreEqual("VALIDATION_ERROR", code);
        Assert.AreEqual("カテゴリ名を入力してください", message);

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

        ProductCategory? registeredCategory = null;

        _registerCategoryUsecaseMock
            .Setup(x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()))
            .Callback<ProductCategory>(category =>
            {
                registeredCategory = category;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Register(model);

        // Assert
        var createdResult = result as CreatedResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);

        Assert.IsNotNull(registeredCategory);
        Assert.AreEqual(model.CategoryName, registeredCategory.Name);
        Assert.AreNotEqual(Guid.Empty, registeredCategory.CategoryUuid);

        var message = GetPropertyValue(createdResult.Value!, "message");
        var categoryValue = GetPropertyValue(createdResult.Value!, "category") as ProductCategory;

        Assert.AreEqual("商品カテゴリ「食品」を登録しました", message);
        Assert.IsNotNull(categoryValue);
        Assert.AreEqual(model.CategoryName, categoryValue.Name);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(It.IsAny<string>()),
            Times.Never
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
    /// カテゴリ名が既に存在する場合、Conflictを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Registerでカテゴリ名が既に存在する場合、Conflictを返す")]
    public async Task Register_WhenCategoryAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var model = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        _registerCategoryUsecaseMock
            .Setup(x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()))
            .ThrowsAsync(new ExistsException("このカテゴリ名は既に登録されています"));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var conflictResult = result as ConflictObjectResult;

        Assert.IsNotNull(conflictResult);
        Assert.AreEqual(409, conflictResult.StatusCode);

        var code = GetPropertyValue(conflictResult.Value!, "code");
        var message = GetPropertyValue(conflictResult.Value!, "message");

        Assert.AreEqual("CATEGORY_ALREADY_EXISTS", code);
        Assert.AreEqual("このカテゴリ名は既に登録されています", message);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(It.IsAny<string>()),
            Times.Never
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
            .Setup(x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()))
            .ThrowsAsync(new DomainException("カテゴリ名は2～20文字で入力してください"));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        var code = GetPropertyValue(badRequestResult.Value!, "code");
        var message = GetPropertyValue(badRequestResult.Value!, "message");

        Assert.AreEqual("DOMAIN_RULE_VIOLATION", code);
        Assert.AreEqual("カテゴリ名は2～20文字で入力してください", message);

        _registerCategoryUsecaseMock.Verify(
            x => x.ExistsByCategoryAsync(It.IsAny<string>()),
            Times.Never
        );

        _registerCategoryUsecaseMock.Verify(
            x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()),
            Times.Once
        );
    }

    /// <summary>
    /// Register:
    /// 内部例外が発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Registerで内部例外が発生した場合、InternalServerErrorを返す")]
    public async Task Register_WhenInternalExceptionOccurs_ShouldReturnInternalServerError()
    {
        // Arrange
        var model = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        _registerCategoryUsecaseMock
            .Setup(x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()))
            .ThrowsAsync(new InternalException("引数productCategoryがnullです。"));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var objectResult = result as ObjectResult;

        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);

        var code = GetPropertyValue(objectResult.Value!, "code");
        var message = GetPropertyValue(objectResult.Value!, "message");

        Assert.AreEqual("INTERNAL_ERROR", code);
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", message);

        _registerCategoryUsecaseMock.Verify(
            x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()),
            Times.Once
        );
    }

    /// <summary>
    /// Register:
    /// 予期しない例外が発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Registerで予期しない例外が発生した場合、InternalServerErrorを返す")]
    public async Task Register_WhenUnexpectedExceptionOccurs_ShouldReturnInternalServerError()
    {
        // Arrange
        var model = new RegisterCategoryViewModel
        {
            CategoryName = "食品"
        };

        _registerCategoryUsecaseMock
            .Setup(x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()))
            .ThrowsAsync(new Exception("DB接続エラー"));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var objectResult = result as ObjectResult;

        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);

        var code = GetPropertyValue(objectResult.Value!, "code");
        var message = GetPropertyValue(objectResult.Value!, "message");

        Assert.AreEqual("SYSTEM_ERROR", code);
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", message);

        _registerCategoryUsecaseMock.Verify(
            x => x.RegisterCategoryAsync(It.IsAny<ProductCategory>()),
            Times.Once
        );
    }
}