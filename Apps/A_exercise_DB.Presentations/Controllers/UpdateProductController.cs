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
[Authorize]
[Route("api/admin/products")]
[Tags("UC012: 商品修正")]
public class UpdateProductController : ControllerBase
{
    private readonly IUpdateProductUsecase _updateProductUsecase;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="updateProductUsecase">商品修正ユースケース</param>
    public UpdateProductController(IUpdateProductUsecase updateProductUsecase)
    {
        _updateProductUsecase = updateProductUsecase;
    }

    /// <summary>
    /// 指定された商品情報を修正する
    /// </summary>
    /// <param name="productUuid">商品識別ID(UUID)</param>
    /// <param name="request">商品修正リクエスト</param>
    /// <returns>商品修正結果</returns>
    [HttpPut("{productUuid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductUpdateCompleteResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductUpdateCompleteResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ProductUpdateCompleteResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ProductUpdateCompleteResult>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync(
        string productUuid,
        [FromBody] UpdateProductViewModel? request)
    {
        if (request is null)
        {
            return BadRequest(ApiResponse<ProductUpdateCompleteResult>.Fail(
                "VALIDATION_ERROR",
                "商品修正情報を入力してください。",
                nameof(request)));
        }

        try
        {
            var updateRequest = new ProductUpdateRequest(
                request.Name,
                request.Price,
                request.StockQuantity,
                request.CategoryUuid,
                null);

            var result = await _updateProductUsecase.UpdateAsync(productUuid, updateRequest);

            return Ok(ApiResponse<ProductUpdateCompleteResult>.Ok(result));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<ProductUpdateCompleteResult>.Fail(
                "VALIDATION_ERROR",
                ex.Message,
                null));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<ProductUpdateCompleteResult>.Fail(
                "NOT_FOUND",
                ex.Message,
                nameof(productUuid)));
        }
        catch (InternalException)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<ProductUpdateCompleteResult>.Fail(
                    "INTERNAL_ERROR",
                    "商品修正中にエラーが発生しました。",
                    null));
        }
    }
}
