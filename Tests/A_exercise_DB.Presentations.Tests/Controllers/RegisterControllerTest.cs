using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Applications.Params;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.Controllers;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;

namespace A_exercise_DB.Presentations.Tests.Controllers;

/// <summary>
/// RegisterProductControllerの単体テスト
/// </summary>
[TestClass]
public class RegisterProductControllerTest
{
    private Mock<IRegisterProductUsecase> _registerProductUsecaseMock = null!;
    private ProductRegisterViewModelAdapter _adapter = null!;
    private RegisterProductController _controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _registerProductUsecaseMock = new Mock<IRegisterProductUsecase>();
        _adapter = new ProductRegisterViewModelAdapter();

        _controller = new RegisterProductController(
            _registerProductUsecaseMock.Object,
            _adapter
        );
    }

    /// <summary>
    /// 商品名が未入力の場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "商品名が未入力の場合、BadRequestを返す")]
    public async Task ValidateProductName_WhenProductNameIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var productName = "";

        // Act
        var result = await _controller.ValidateProductName(productName);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        Assert.AreEqual(
            "INVALID_PRODUCT_NAME",
            GetPropertyValue<string>(badRequestResult.Value!, "code")
        );

        Assert.AreEqual(
            "商品名は必須です。",
            GetPropertyValue<string>(badRequestResult.Value!, "message")
        );

        _registerProductUsecaseMock.Verify(
            x => x.ExistsByProductNameAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    /// <summary>
    /// 商品名が空白の場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "商品名が空白の場合、BadRequestを返す")]
    public async Task ValidateProductName_WhenProductNameIsWhiteSpace_ShouldReturnBadRequest()
    {
        // Arrange
        var productName = "   ";

        // Act
        var result = await _controller.ValidateProductName(productName);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        Assert.AreEqual(
            "INVALID_PRODUCT_NAME",
            GetPropertyValue<string>(badRequestResult.Value!, "code")
        );

        Assert.AreEqual(
            "商品名は必須です。",
            GetPropertyValue<string>(badRequestResult.Value!, "message")
        );

        _registerProductUsecaseMock.Verify(
            x => x.ExistsByProductNameAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    /// <summary>
    /// 商品名が存在しない場合、Okを返すこと
    /// </summary>
    [TestMethod(DisplayName = "商品名が存在しない場合、Okを返す")]
    public async Task ValidateProductName_WhenProductNameDoesNotExist_ShouldReturnOk()
    {
        // Arrange
        var productName = "りんご";

        _registerProductUsecaseMock
            .Setup(x => x.ExistsByProductNameAsync(productName))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateProductName(productName);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        Assert.IsFalse(
            GetPropertyValue<bool>(okResult.Value!, "exists")
        );

        _registerProductUsecaseMock.Verify(
            x => x.ExistsByProductNameAsync(productName),
            Times.Once
        );
    }

    /// <summary>
    /// ModelStateが不正な場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ModelStateが不正な場合、BadRequestを返す")]
    public async Task Register_WhenModelStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var model = new RegisterViewModel();

        _controller.ModelState.AddModelError(
            "Name",
            "商品名を入力してください。"
        );

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        Assert.AreEqual(
            "VALIDATION_ERROR",
            GetPropertyValue<string>(badRequestResult.Value!, "code")
        );

        Assert.AreEqual(
            "商品名を入力してください。",
            GetPropertyValue<string>(badRequestResult.Value!, "message")
        );

        _registerProductUsecaseMock.Verify(
            x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()),
            Times.Never
        );
    }

    /// <summary>
    /// 商品カテゴリが未選択の場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "商品カテゴリが未選択の場合、BadRequestを返す")]
    public async Task Register_WhenCategoryUuidIsNull_ShouldReturnBadRequest()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = null
        };

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        Assert.AreEqual(
            "CATEGORY_REQUIRED",
            GetPropertyValue<string>(badRequestResult.Value!, "code")
        );

        Assert.AreEqual(
            "商品カテゴリを選択してください。",
            GetPropertyValue<string>(badRequestResult.Value!, "message")
        );

        _registerProductUsecaseMock.Verify(
            x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()),
            Times.Never
        );
    }

    /// <summary>
    /// 商品カテゴリがGuid.Emptyの場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "商品カテゴリがGuid.Emptyの場合、BadRequestを返す")]
    public async Task Register_WhenCategoryUuidIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = Guid.Empty
        };

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        Assert.AreEqual(
            "CATEGORY_REQUIRED",
            GetPropertyValue<string>(badRequestResult.Value!, "code")
        );

        Assert.AreEqual(
            "商品カテゴリを選択してください。",
            GetPropertyValue<string>(badRequestResult.Value!, "message")
        );

        _registerProductUsecaseMock.Verify(
            x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()),
            Times.Never
        );
    }

    /// <summary>
    /// 商品登録に成功した場合、Createdを返すこと
    /// </summary>
    /// <summary>
    /// 商品登録に成功した場合、Createdを返すこと
    /// </summary>
    [TestMethod(DisplayName = "商品登録に成功した場合、Createdを返す")]
    public async Task Register_WhenRegisterSucceeds_ShouldReturnCreated()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();

        var model = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = categoryUuid,
            Image = CreateValidImage()
        };

        var category = new ProductCategory(
            categoryUuid,
            "食品"
        );

        var product = new Product(
            Guid.NewGuid(),
            "りんご",
            100);
        product.ChangeCategory(category);
        product.ChangeStock(new ProductStock(10));

        _registerProductUsecaseMock
            .Setup(x =>
                x.ExistsByProductNameAsync("りんご"))
            .ReturnsAsync(false);

        _registerProductUsecaseMock
            .Setup(x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()))
            .ReturnsAsync(product);

        // Act
        var result =
            await _controller.Register(model);

        // Assert
        var createdResult = result as ObjectResult;
        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);

        var returnedProduct = createdResult.Value as Product;
        Assert.IsNotNull(returnedProduct);
        Assert.AreEqual("りんご", returnedProduct.Name);
        Assert.AreEqual(100, returnedProduct.Price);
        Assert.IsNotNull(returnedProduct.ProductCategory);
        Assert.AreEqual("食品", returnedProduct.ProductCategory!.Name);
        Assert.IsNotNull(returnedProduct.ProductStock);
        Assert.AreEqual(10, returnedProduct.ProductStock!.Quantity);

        _registerProductUsecaseMock.Verify(
            x =>
                x.ExistsByProductNameAsync(
                    "りんご"
                ),
            Times.Once
        );

        _registerProductUsecaseMock.Verify(
            x => x.RegisterProductAsync(
                It.Is<ProductRegisterParam>(param =>
                    param.Name == "りんご" &&
                    param.Price == 100 &&
                    param.Quantity == 10 &&
                    param.CategoryId == categoryUuid &&
                    param.ImageFileName == "test.png" &&
                    param.ImageContentType == "image/png")),
            Times.Once
        );
    }
    /// <summary>
    /// カテゴリが存在しない場合、NotFoundを返すこと
    /// </summary>
    [TestMethod(DisplayName = "カテゴリが存在しない場合、NotFoundを返す")]
    public async Task Register_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();

        var model = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = categoryUuid,
            Image = CreateValidImage()
        };

        _registerProductUsecaseMock
            .Setup(x => x.ExistsByProductNameAsync("りんご"))
            .ReturnsAsync(false);

        _registerProductUsecaseMock
            .Setup(x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()))
            .ThrowsAsync(new NotFoundException("カテゴリーが存在しません。"));

        // Act
        var result =
            await _controller.Register(model);

        // Assert
        var notFoundResult =
            result as NotFoundObjectResult;

        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(
            StatusCodes.Status404NotFound,
            notFoundResult.StatusCode
        );

        Assert.AreEqual(
            "CATEGORY_NOT_FOUND",
            GetPropertyValue<string>(
                notFoundResult.Value!,
                "code"
            )
        );

        Assert.AreEqual(
            "カテゴリーが存在しません。",
            GetPropertyValue<string>(
                notFoundResult.Value!,
                "message"
            )
        );

        _registerProductUsecaseMock.Verify(
            x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()),
            Times.Once
        );
    }
    /// <summary>
    /// 商品名が既に存在する場合、Conflictを返すこと
    /// </summary>
    [TestMethod(DisplayName = "商品名が既に存在する場合、Conflictを返す")]
    public async Task Register_WhenProductNameExists_ShouldReturnConflict()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();

        var model = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = categoryUuid,
            Image = CreateValidImage()
        };

        _registerProductUsecaseMock
            .Setup(x => x.ExistsByProductNameAsync("りんご"))
            .ReturnsAsync(true);

        // Act
        var result =
            await _controller.Register(model);

        // Assert
        var conflictResult = result as ConflictObjectResult;
        Assert.IsNotNull(conflictResult);
        Assert.AreEqual(409, conflictResult.StatusCode);

        Assert.AreEqual(
            "PRODUCT_ALREADY_EXISTS",
            GetPropertyValue<string>(conflictResult.Value!, "code")
        );

        Assert.AreEqual(
            "同じ商品名の商品が既に存在します。",
            GetPropertyValue<string>(conflictResult.Value!, "message")
        );

        _registerProductUsecaseMock.Verify(
            x => x.ExistsByProductNameAsync("りんご"),
            Times.Once
        );

        _registerProductUsecaseMock.Verify(
            x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()),
            Times.Never
        );
    }
    /// <summary>
    /// ドメインルール違反の場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ドメインルール違反の場合、BadRequestを返す")]
    public async Task Register_WhenDomainExceptionThrown_ShouldReturnBadRequest()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();

        var model = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = categoryUuid,
            Image = CreateValidImage()
        };

        _registerProductUsecaseMock
            .Setup(x => x.ExistsByProductNameAsync("りんご"))
            .ReturnsAsync(false);

        _registerProductUsecaseMock
            .Setup(x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()))
            .ThrowsAsync(new DomainException("商品価格が不正です。"));

        // Act
        var result =
            await _controller.Register(model);

        // Assert
        var badRequestResult =
            result as BadRequestObjectResult;

        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(
            StatusCodes.Status400BadRequest,
            badRequestResult.StatusCode
        );

        Assert.AreEqual(
            "DOMAIN_RULE_VIOLATION",
            GetPropertyValue<string>(
                badRequestResult.Value!,
                "code"
            )
        );

        Assert.AreEqual(
            "商品価格が不正です。",
            GetPropertyValue<string>(
                badRequestResult.Value!,
                "message"
            )
        );

        _registerProductUsecaseMock.Verify(
            x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()),
            Times.Once
        );

        Assert.AreEqual(
            1,
            _registerProductUsecaseMock
                .Invocations
                .Count(invocation =>
                    invocation.Method.Name ==
                    nameof(
                        IRegisterProductUsecase
                            .RegisterProductAsync
                    ))
        );
    }

    /// <summary>
    /// カテゴリー一覧を取得できること
    /// </summary>
    [TestMethod(DisplayName = "カテゴリー一覧を取得できる")]
    public async Task GetCategories_WhenCategoriesExist_ShouldReturnOk()
    {
        // Arrange
        var categories = new List<ProductCategory>
        {
            new ProductCategory("食品"),
            new ProductCategory("飲料")
        };

        _registerProductUsecaseMock
            .Setup(x => x.GetCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var value = okResult.Value as List<ProductCategory>;
        Assert.IsNotNull(value);
        Assert.HasCount(2, value);
        Assert.AreEqual("食品", value[0].Name);
        Assert.AreEqual("飲料", value[1].Name);

        _registerProductUsecaseMock.Verify(
            x => x.GetCategoriesAsync(),
            Times.Once
        );
    }

    /// <summary>
    /// 匿名型オブジェクトから指定したプロパティの値を取得する
    /// </summary>
    private static T GetPropertyValue<T>(
        object target,
        string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName);

        if (property is null)
        {
            throw new InvalidOperationException(
                $"{propertyName}プロパティが見つかりません。");
        }

        return (T)property.GetValue(target)!;
    }

    [TestMethod(DisplayName = "商品名が既に存在する場合、Conflictを返す")]
    public async Task ValidateProductName_WhenProductNameExists_ShouldReturnConflict()
    {
        // Arrange
        var productName = "りんご";

        _registerProductUsecaseMock
            .Setup(x => x.ExistsByProductNameAsync(productName))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateProductName(productName);

        // Assert
        var conflictResult = result as ConflictObjectResult;
        Assert.IsNotNull(conflictResult);
        Assert.AreEqual(409, conflictResult.StatusCode);

        Assert.AreEqual(
            "PRODUCT_ALREADY_EXISTS",
            GetPropertyValue<string>(conflictResult.Value!, "code")
        );

        Assert.AreEqual(
            "同じ商品名の商品が既に存在します。",
            GetPropertyValue<string>(conflictResult.Value!, "message")
        );

        _registerProductUsecaseMock.Verify(
            x => x.ExistsByProductNameAsync(productName),
            Times.Once
        );
    }

    [TestMethod(DisplayName = "ModelStateのエラーメッセージが空の場合、既定メッセージを返す")]
    public async Task Register_WhenModelStateErrorMessageIsEmpty_ShouldReturnDefaultMessage()
    {
        // Arrange
        var model = new RegisterViewModel();

        _controller.ModelState.AddModelError("Name", "");

        // Act
        var result = await _controller.Register(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        Assert.AreEqual(
            "VALIDATION_ERROR",
            GetPropertyValue<string>(
                badRequestResult.Value!,
                "code")
        );

        Assert.AreEqual(
            "商品の入力内容に誤りがあります。",
            GetPropertyValue<string>(
                badRequestResult.Value!,
                "message")
        );

        _registerProductUsecaseMock.Verify(
            x => x.RegisterProductAsync(
                It.IsAny<ProductRegisterParam>()),
            Times.Never
        );
    }

    private static IFormFile CreateValidImage()
    {
        var content = new MemoryStream([1]);
        return new FormFile(
            content,
            0,
            content.Length,
            "Image",
            "test.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
    }
    /// <summary>
    /// 登録処理中に商品名の重複が判明した場合、
    /// Conflictを返すこと
    /// </summary>
    [TestMethod(
        DisplayName =
            "登録処理中にExistsExceptionが発生した場合、Conflictを返す")]
    public async Task
        Register_WhenExistsExceptionThrown_ShouldReturnConflict()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();

        var model = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = categoryUuid,
            Image = CreateValidImage()
        };

        /*
         * Controllerでの事前確認時点では、
         * 同名商品が存在しない。
         */
        _registerProductUsecaseMock
            .Setup(x =>
                x.ExistsByProductNameAsync("りんご"))
            .ReturnsAsync(false);

        /*
         * 実際の登録処理では、
         * 排他制御や再確認によって重複が判明する。
         */
        _registerProductUsecaseMock
            .Setup(x =>
                x.RegisterProductAsync(
                    It.IsAny<ProductRegisterParam>()))
            .ThrowsAsync(
                new ExistsException(
                    "同じ商品名の商品が既に存在します。"));

        // Act
        var result =
            await _controller.Register(model);

        // Assert
        var conflictResult =
            result as ConflictObjectResult;

        Assert.IsNotNull(conflictResult);

        Assert.AreEqual(
            StatusCodes.Status409Conflict,
            conflictResult.StatusCode
        );

        Assert.AreEqual(
            "PRODUCT_ALREADY_EXISTS",
            GetPropertyValue<string>(
                conflictResult.Value!,
                "code"
            )
        );

        Assert.AreEqual(
            "同じ商品名の商品が既に存在します。",
            GetPropertyValue<string>(
                conflictResult.Value!,
                "message"
            )
        );

        _registerProductUsecaseMock.Verify(
            x =>
                x.ExistsByProductNameAsync("りんご"),
            Times.Once
        );

        _registerProductUsecaseMock.Verify(
            x =>
                x.RegisterProductAsync(
                    It.Is<ProductRegisterParam>(
                        param =>
                            param.Name == "りんご" &&
                            param.CategoryId ==
                                categoryUuid)),
            Times.Once
        );
    }
    /// <summary>
    /// 画像Streamを開く処理で例外が発生した場合、
    /// 例外がそのまま伝播すること
    /// </summary>
    [TestMethod(
        DisplayName =
            "画像Streamを開けない場合、IOExceptionが伝播する")]
    public async Task
        Register_WhenOpenReadStreamThrows_ShouldPropagateIOException()
    {
        // Arrange
        var categoryUuid = Guid.NewGuid();

        var imageMock = new Mock<IFormFile>();

        imageMock
            .Setup(image =>
                image.OpenReadStream())
            .Throws(
                new IOException(
                    "画像ファイルを開けません。"));

        var model = new RegisterViewModel
        {
            Name = "りんご",
            Price = 100,
            Stock = 10,
            CategoryUuid = categoryUuid,
            Image = imageMock.Object
        };

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<IOException>(
                async () =>
                    await _controller.Register(model)
            );

        // Assert
        Assert.AreEqual(
            "画像ファイルを開けません。",
            exception.Message
        );

        imageMock.Verify(
            image =>
                image.OpenReadStream(),
            Times.Once
        );

        _registerProductUsecaseMock.Verify(
            x =>
                x.ExistsByProductNameAsync(
                    It.IsAny<string>()),
            Times.Never
        );

        _registerProductUsecaseMock.Verify(
            x =>
                x.RegisterProductAsync(
                    It.IsAny<ProductRegisterParam>()),
            Times.Never
        );
    }
}
