using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace A_exercise_DB.Presentations.Controllers;

/// <summary>
/// UC012 商品修正API
/// </summary>
[ApiController]
[AllowAnonymous]
// [Authorize]
[Route("api/admin/products")]
[Tags("UC012: 商品修正")]
public class UpdateProductController : ControllerBase
{
    private readonly IUpdateProductUsecase
        _updateProductUsecase;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="updateProductUsecase">
    /// 商品修正ユースケース
    /// </param>
    public UpdateProductController(
        IUpdateProductUsecase updateProductUsecase)
    {
        _updateProductUsecase =
            updateProductUsecase;
    }

    /// <summary>
    /// 指定された商品情報を修正する
    /// </summary>
    /// <param name="productUuid">
    /// 商品識別ID(UUID)
    /// </param>
    /// <param name="request">
    /// 商品修正リクエスト
    /// </param>
    /// <returns>
    /// 商品修正結果
    /// </returns>
    [HttpPut("/admin/product/edit/{productId}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [ProducesResponseType(
        typeof(ApiResponse<ProductUpdateCompleteResult>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<ProductUpdateCompleteResult>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ApiResponse<ProductUpdateCompleteResult>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<ProductUpdateCompleteResult>),
        StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute(Name = "productId")]
        string productUuid,

        [FromForm]
        UpdateProductViewModel? request)
    {
        if (request is null)
        {
            return BadRequest(
                ApiResponse<ProductUpdateCompleteResult>
                    .Fail(
                        "VALIDATION_ERROR",
                        "商品修正情報を入力してください。",
                        nameof(request)));
        }

        /*
         * Controllerで開いたStreamは、
         * Controllerで破棄する。
         */
        Stream? imageStream = null;

        try
        {
            /*
             * 新しい画像が指定されている場合のみ
             * Streamを開く。
             */
            if (request.Image is not null)
            {
                imageStream =
                    request.Image.OpenReadStream();
            }

            /*
             * ViewModelからApplication層の
             * ProductUpdateRequestへ変換する。
             *
             * ImageUrlはユースケース内で決まるため、
             * Controllerからはnullを渡す。
             */
            var updateRequest =
                new ProductUpdateRequest(
                    Name:
                        request.Name,
                    Price:
                        request.Price,
                    StockQuantity:
                        request.StockQuantity,
                    CategoryUuid:
                        request.CategoryUuid,
                    ImageUrl:
                        null,
                    ImageContent:
                        imageStream,
                    ImageFileName:
                        request.Image?.FileName,
                    ImageContentType:
                        request.Image?.ContentType,
                    ImageLength:
                        request.Image?.Length ?? 0);

            var result =
                await _updateProductUsecase
                    .UpdateAsync(
                        productUuid,
                        updateRequest);

            return Ok(
                ApiResponse<ProductUpdateCompleteResult>
                    .Ok(result));
        }
        catch (DomainException ex)
        {
            return BadRequest(
                ApiResponse<ProductUpdateCompleteResult>
                    .Fail(
                        "VALIDATION_ERROR",
                        ex.Message,
                        null));
        }
        catch (NotFoundException ex)
        {
            return NotFound(
                ApiResponse<ProductUpdateCompleteResult>
                    .Fail(
                        "NOT_FOUND",
                        ex.Message,
                        nameof(productUuid)));
        }
        catch (InternalException)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<ProductUpdateCompleteResult>
                    .Fail(
                        "INTERNAL_ERROR",
                        "商品修正中にエラーが発生しました。",
                        null));
        }
        finally
        {
            /*
             * 成功・失敗に関係なく、
             * 開いた画像Streamを破棄する。
             */
            if (imageStream is not null)
            {
                await imageStream.DisposeAsync();
            }
        }
    }
}