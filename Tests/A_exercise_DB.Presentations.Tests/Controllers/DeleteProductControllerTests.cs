using System.Reflection;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace A_exercise_DB.Presentations.Tests.Controllers;

/// <summary>
/// DeleteProductControllerの単体テスト
/// </summary>
[TestClass]
[TestCategory("Presentations/Controllers/Products")]
public class DeleteProductControllerTests
{
    private Mock<IDeleteProductUsecase> _deleteProductUsecaseMock = null!;
    private DeleteProductController _controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _deleteProductUsecaseMock = new Mock<IDeleteProductUsecase>(MockBehavior.Strict);
        _controller = new DeleteProductController(_deleteProductUsecaseMock.Object);
    }

    [TestMethod(DisplayName = "有効な商品UUIDの場合、削除結果と200を返す")]
    public async Task DeleteAsync_WithValidProductUuid_ShouldReturnOk()
    {
        var productUuid = Guid.NewGuid();
        var expectedResult = ProductDeleteCompleteResult.CreateDeleted(productUuid);

        _deleteProductUsecaseMock
            .Setup(u => u.DeleteAsync(productUuid.ToString()))
            .ReturnsAsync(expectedResult);

        var actionResult = await _controller.DeleteAsync(productUuid.ToString());

        var okResult = actionResult as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var response = okResult.Value as ApiResponse<ProductDeleteCompleteResult>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual(expectedResult, response.Data);
        Assert.IsEmpty(response.Errors);

        _deleteProductUsecaseMock.VerifyAll();
    }

    [TestMethod(DisplayName = "商品UUIDが不正な場合、400を返す")]
    public async Task DeleteAsync_WhenUsecaseThrowsDomainException_ShouldReturnBadRequest()
    {
        const string productUuid = "invalid-product-uuid";

        _deleteProductUsecaseMock
            .Setup(u => u.DeleteAsync(productUuid))
            .ThrowsAsync(new DomainException("商品識別IDが不正です。"));

        var actionResult = await _controller.DeleteAsync(productUuid);

        var badRequestResult = actionResult as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        AssertErrorResponse(
            badRequestResult.Value,
            "VALIDATION_ERROR",
            "商品識別IDが不正です。",
            "productUuid");

        _deleteProductUsecaseMock.VerifyAll();
    }

    [TestMethod(DisplayName = "削除対象の商品が存在しない場合、404を返す")]
    public async Task DeleteAsync_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var productUuid = Guid.NewGuid().ToString();

        _deleteProductUsecaseMock
            .Setup(u => u.DeleteAsync(productUuid))
            .ThrowsAsync(new NotFoundException("削除対象の商品が見つかりません。"));

        var actionResult = await _controller.DeleteAsync(productUuid);

        var notFoundResult = actionResult as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);

        AssertErrorResponse(
            notFoundResult.Value,
            "NOT_FOUND",
            "削除対象の商品が見つかりません。",
            "productUuid");

        _deleteProductUsecaseMock.VerifyAll();
    }

    [TestMethod(DisplayName = "内部エラーが発生した場合、詳細を隠して500を返す")]
    public async Task DeleteAsync_WhenInternalExceptionOccurs_ShouldReturnInternalServerError()
    {
        var productUuid = Guid.NewGuid().ToString();

        _deleteProductUsecaseMock
            .Setup(u => u.DeleteAsync(productUuid))
            .ThrowsAsync(new InternalException("DB接続情報を含む内部エラー"));

        var actionResult = await _controller.DeleteAsync(productUuid);

        var objectResult = actionResult as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);

        AssertErrorResponse(
            objectResult.Value,
            "INTERNAL_ERROR",
            "商品削除中にエラーが発生しました。",
            null);

        _deleteProductUsecaseMock.VerifyAll();
    }

    [TestMethod(DisplayName = "商品削除Controllerは認証を必須とする")]
    public void Controller_ShouldRequireAuthorization()
    {
        var authorizeAttribute = typeof(DeleteProductController)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.IsNotNull(authorizeAttribute);
    }

    private static void AssertErrorResponse(
        object? value,
        string expectedCode,
        string expectedMessage,
        string? expectedField)
    {
        var response = value as ApiResponse<ProductDeleteCompleteResult>;
        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.IsNull(response.Data);
        Assert.HasCount(1, response.Errors);
        Assert.AreEqual(expectedCode, response.Errors[0].Code);
        Assert.AreEqual(expectedMessage, response.Errors[0].Message);
        Assert.AreEqual(expectedField, response.Errors[0].Field);
    }
}
