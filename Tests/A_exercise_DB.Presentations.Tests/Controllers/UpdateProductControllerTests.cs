using System.Reflection;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Controllers;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Http;

namespace A_exercise_DB.Presentations.Tests.Controllers;

/// <summary>
/// UpdateProductControllerの単体テスト
/// </summary>
[TestClass]
[TestCategory("Presentations/Controllers/Products")]
public class UpdateProductControllerTests
{
    private Mock<IUpdateProductUsecase> _updateProductUsecaseMock = null!;
    private UpdateProductController _controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _updateProductUsecaseMock = new Mock<IUpdateProductUsecase>(MockBehavior.Strict);
        _controller = new UpdateProductController(_updateProductUsecaseMock.Object);
    }

    [TestMethod(DisplayName = "有効な商品修正情報の場合、更新結果と200を返す")]
    public async Task UpdateAsync_WithValidRequest_ShouldReturnOk()
    {
        var productUuid = Guid.NewGuid();
        var categoryUuid = Guid.NewGuid();
        var request = CreateValidViewModel(categoryUuid);
        var expectedResult = new ProductUpdateCompleteResult(
            productUuid,
            request.Name,
            request.Price,
            request.StockQuantity,
            categoryUuid,
            null,
            true);

        _updateProductUsecaseMock
            .Setup(u => u.UpdateAsync(
                productUuid.ToString(),
                It.Is<ProductUpdateRequest>(r =>
                    r.Name == request.Name &&
                    r.Price == request.Price &&
                    r.StockQuantity == request.StockQuantity &&
                    r.CategoryUuid == request.CategoryUuid &&
                    r.ImageUrl == null)))
            .ReturnsAsync(expectedResult);

        var actionResult = await _controller.UpdateAsync(productUuid.ToString(), request);

        var okResult = actionResult as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var response = okResult.Value as ApiResponse<ProductUpdateCompleteResult>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual(expectedResult, response.Data);
        Assert.IsEmpty(response.Errors);

        _updateProductUsecaseMock.VerifyAll();
    }

    [TestMethod(DisplayName = "商品修正情報がない場合、Usecaseを呼ばず400を返す")]
    public async Task UpdateAsync_WithNullRequest_ShouldReturnBadRequest()
    {
        var actionResult = await _controller.UpdateAsync(Guid.NewGuid().ToString(), null);

        var badRequestResult = actionResult as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        AssertErrorResponse(
            badRequestResult.Value,
            "VALIDATION_ERROR",
            "商品修正情報を入力してください。",
            "request");

        _updateProductUsecaseMock.Verify(
            u => u.UpdateAsync(It.IsAny<string>(), It.IsAny<ProductUpdateRequest>()),
            Times.Never);
    }

    [TestMethod(DisplayName = "入力値が業務ルール違反の場合、400を返す")]
    public async Task UpdateAsync_WhenUsecaseThrowsDomainException_ShouldReturnBadRequest()
    {
        var productUuid = Guid.NewGuid().ToString();
        var request = CreateValidViewModel(Guid.NewGuid());

        _updateProductUsecaseMock
            .Setup(u => u.UpdateAsync(productUuid, It.IsAny<ProductUpdateRequest>()))
            .ThrowsAsync(new DomainException("商品名は2～20文字で入力してください。"));

        var actionResult = await _controller.UpdateAsync(productUuid, request);

        var badRequestResult = actionResult as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        AssertErrorResponse(
            badRequestResult.Value,
            "VALIDATION_ERROR",
            "商品名は2～20文字で入力してください。",
            null);

        _updateProductUsecaseMock.VerifyAll();
    }

    [TestMethod(DisplayName = "更新対象の商品が存在しない場合、404を返す")]
    public async Task UpdateAsync_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var productUuid = Guid.NewGuid().ToString();
        var request = CreateValidViewModel(Guid.NewGuid());

        _updateProductUsecaseMock
            .Setup(u => u.UpdateAsync(productUuid, It.IsAny<ProductUpdateRequest>()))
            .ThrowsAsync(new NotFoundException("更新対象の商品が見つかりません。"));

        var actionResult = await _controller.UpdateAsync(productUuid, request);

        var notFoundResult = actionResult as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);

        AssertErrorResponse(
            notFoundResult.Value,
            "NOT_FOUND",
            "更新対象の商品が見つかりません。",
            "productUuid");

        _updateProductUsecaseMock.VerifyAll();
    }

    [TestMethod(DisplayName = "内部エラーが発生した場合、詳細を隠して500を返す")]
    public async Task UpdateAsync_WhenInternalExceptionOccurs_ShouldReturnInternalServerError()
    {
        var productUuid = Guid.NewGuid().ToString();
        var request = CreateValidViewModel(Guid.NewGuid());

        _updateProductUsecaseMock
            .Setup(u => u.UpdateAsync(productUuid, It.IsAny<ProductUpdateRequest>()))
            .ThrowsAsync(new InternalException("DB接続情報を含む内部エラー"));

        var actionResult = await _controller.UpdateAsync(productUuid, request);

        var objectResult = actionResult as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);

        AssertErrorResponse(
            objectResult.Value,
            "INTERNAL_ERROR",
            "商品修正中にエラーが発生しました。",
            null);

        _updateProductUsecaseMock.VerifyAll();
    }

    [TestMethod(DisplayName = "商品修正Controllerは認証を必須とする")]
    public void Controller_ShouldRequireAuthorization()
    {
        var authorizeAttribute = typeof(UpdateProductController)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.IsNotNull(authorizeAttribute);
    }


    [TestMethod(
    DisplayName =
        "新しい画像が指定された場合、画像情報をUsecaseへ渡してStreamを破棄する")]
    public async Task UpdateAsync_WithImage_ShouldPassImageAndDisposeStream()
    {
        // Arrange
        var productUuid =
            Guid.NewGuid();

        var categoryUuid =
            Guid.NewGuid();

        var imageBytes =
            new byte[]
            {
            1,
            2,
            3,
            4
            };

        var imageStream =
            new TrackingMemoryStream(imageBytes);

        var imageMock =
            new Mock<IFormFile>();

        imageMock
            .Setup(x => x.OpenReadStream())
            .Returns(imageStream);

        imageMock
            .SetupGet(x => x.FileName)
            .Returns("product.png");

        imageMock
            .SetupGet(x => x.ContentType)
            .Returns("image/png");

        imageMock
            .SetupGet(x => x.Length)
            .Returns(imageBytes.Length);

        var request =
            CreateValidViewModel(categoryUuid);

        request.Image =
            imageMock.Object;

        var expectedResult =
            new ProductUpdateCompleteResult(
                productUuid,
                request.Name,
                request.Price,
                request.StockQuantity,
                categoryUuid,
                "https://example.com/product.png",
                true);

        _updateProductUsecaseMock
            .Setup(x => x.UpdateAsync(
                productUuid.ToString(),
                It.Is<ProductUpdateRequest>(
                    updateRequest =>
                        updateRequest.Name ==
                            request.Name &&
                        updateRequest.Price ==
                            request.Price &&
                        updateRequest.StockQuantity ==
                            request.StockQuantity &&
                        updateRequest.CategoryUuid ==
                            request.CategoryUuid &&
                        updateRequest.ImageUrl == null &&
                        ReferenceEquals(
                            updateRequest.ImageContent,
                            imageStream) &&
                        updateRequest.ImageFileName ==
                            "product.png" &&
                        updateRequest.ImageContentType ==
                            "image/png" &&
                        updateRequest.ImageLength ==
                            imageBytes.Length)))
            .ReturnsAsync(expectedResult);

        // Act
        var actionResult =
            await _controller.UpdateAsync(
                productUuid.ToString(),
                request);

        // Assert
        var okResult =
            actionResult as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(
            StatusCodes.Status200OK,
            okResult.StatusCode);

        var response =
            okResult.Value
                as ApiResponse<
                    ProductUpdateCompleteResult>;

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual(
            expectedResult,
            response.Data);
        Assert.IsEmpty(response.Errors);

        Assert.IsTrue(
            imageStream.IsDisposed);

        imageMock.Verify(
            x => x.OpenReadStream(),
            Times.Once);

        _updateProductUsecaseMock.VerifyAll();
    }

    private static UpdateProductViewModel CreateValidViewModel(Guid categoryUuid)
        => new()
        {
            Name = "ゲルインクペン",
            Price = 150,
            StockQuantity = 40,
            CategoryUuid = categoryUuid.ToString()
        };

    private static void AssertErrorResponse(
        object? value,
        string expectedCode,
        string expectedMessage,
        string? expectedField)
    {
        var response = value as ApiResponse<ProductUpdateCompleteResult>;
        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.IsNull(response.Data);
        Assert.HasCount(1, response.Errors);
        Assert.AreEqual(expectedCode, response.Errors[0].Code);
        Assert.AreEqual(expectedMessage, response.Errors[0].Message);
        Assert.AreEqual(expectedField, response.Errors[0].Field);
    }

    /// <summary>
    /// Streamが破棄されたことを確認するための
    /// テスト用MemoryStream
    /// </summary>
    private sealed class TrackingMemoryStream
        : MemoryStream
    {
        public bool IsDisposed
        {
            get;
            private set;
        }

        public TrackingMemoryStream(
            byte[] buffer)
            : base(buffer)
        {
        }

        public override ValueTask DisposeAsync()
        {
            IsDisposed = true;

            Dispose();

            return ValueTask.CompletedTask;
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
