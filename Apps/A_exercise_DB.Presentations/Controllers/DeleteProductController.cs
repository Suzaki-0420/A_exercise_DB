using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace A_exercise_DB.Presentations.Controllers;

/// <summary>
/// UC013 商品削除API
/// </summary>
[ApiController]
//[AllowAnonymous]
[Authorize]
[Route("api/admin/products")]
[Tags("UC013: 商品削除")]
public class DeleteProductController : ControllerBase
{
    private readonly IDeleteProductUsecase _deleteProductUsecase;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="deleteProductUsecase">商品削除ユースケース</param>
    public DeleteProductController(IDeleteProductUsecase deleteProductUsecase)
    {
        _deleteProductUsecase = deleteProductUsecase;
    }

    /// <summary>
    /// 指定された商品を論理削除する
    /// </summary>
    /// <param name="productUuid">商品識別ID(UUID)</param>
    /// <returns>商品削除結果</returns>
    [HttpDelete("/admin/product/delete/{productId}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDeleteCompleteResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDeleteCompleteResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ProductDeleteCompleteResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ProductDeleteCompleteResult>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute(Name = "productId")] string productUuid)
    {
        try
        {
            var result = await _deleteProductUsecase.DeleteAsync(productUuid);

            return Ok(ApiResponse<ProductDeleteCompleteResult>.Ok(result));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<ProductDeleteCompleteResult>.Fail(
                "VALIDATION_ERROR",
                ex.Message,
                nameof(productUuid)));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<ProductDeleteCompleteResult>.Fail(
                "NOT_FOUND",
                ex.Message,
                nameof(productUuid)));
        }
        catch (InternalException)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<ProductDeleteCompleteResult>.Fail(
                    "INTERNAL_ERROR",
                    "商品削除中にエラーが発生しました。",
                    null));
        }
    }
}

/// <summary>
/// APIレスポンス
/// </summary>
/// <typeparam name="T">レスポンスデータの型</typeparam>
public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    IReadOnlyList<ApiError> Errors)
{
    /// <summary>
    /// 正常レスポンスを生成する
    /// </summary>
    public static ApiResponse<T> Ok(T data)
        => new(true, data, Array.Empty<ApiError>());

    /// <summary>
    /// 異常レスポンスを生成する
    /// </summary>
    public static ApiResponse<T> Fail(string code, string message, string? field)
        => new(false, default, new[] { new ApiError(code, message, field) });
}

/// <summary>
/// APIエラー
/// </summary>
public sealed record ApiError(
    string Code,
    string Message,
    string? Field);
