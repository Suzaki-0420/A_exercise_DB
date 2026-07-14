using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace A_exercise_DB.Presentations.Controllers;

[ApiController]
[AllowAnonymous]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Route("admin/product/keyword")]
[Tags("UC019: 商品キーワード検索")]
/// <summary>
/// ユースケース:[商品をキーワード検索する]を実現するコントローラ
/// </summary>
public class SearchProductByKeywordController : ControllerBase
{
    private readonly ISearchProductByKeywordUsecase _usecase;
    public SearchProductByKeywordController(ISearchProductByKeywordUsecase usecase)
    {
        _usecase = usecase;
    }
    /// <summary>
    /// キーワードで商品を検索する
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns>検索結果の商品一覧</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] string? keyword, bool showDeletedOnly)
    {
        // 未入力チェック
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(
            new { code = "INVALID_KEYWORD", message = "検索キーワードを入力してください。" });
        }
        // 商品キーワード検索する
        var result = await _usecase.ExecuteAsync(keyword.Trim(), showDeletedOnly);
        return Ok(result);
    }
}
