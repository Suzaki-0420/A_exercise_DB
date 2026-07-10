using A_exercise_DB.Applications.Usecases.Products;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using A_exercise_DB.Domains.Models;
using Microsoft.AspNetCore.Mvc;

namespace A_exercise_DB.Presentations.Controllers;
/// <summary>
/// ユースケース:[商品をカテゴリー検索をする]を実現するコントローラ
/// </summary>
[ApiController]
[Route("admin/product/category")]
[Tags("UC011: 商品検索(カテゴリ)")]
public class SearchProductByCategory : ControllerBase
{
    private readonly ISearchProductByCategoryUsecase _usecase;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="usecase">ユースケース:[商品をカテゴリー検索する]を実現するインターフェイス</param>
    public SearchProductByCategory(ISearchProductByCategoryUsecase usecase)
    {
        _usecase = usecase;
    }
    /// <summary>
    /// キーワードで商品を検索する
    /// </summary>
    /// <param name="productCategoryId">検索Id</param>
    /// <returns>検索結果の商品一覧</returns>

    // [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(int productCategoryId)
    {
        // 商品キーワード検索する
        var result = await _usecase.ExecuteAsync(productCategoryId);
        return Ok(result);
    }
}