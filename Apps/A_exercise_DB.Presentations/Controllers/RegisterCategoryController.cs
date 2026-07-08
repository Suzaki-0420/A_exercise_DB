using Microsoft.AspNetCore.Mvc;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Applications.Usecases.Categories;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;
namespace A_exercise_DB.Presentations.Controllers;
/// <summary>
/// UC014: 新規商品カテゴリ登録
/// </summary>
[ApiController]
[Route("categories")]
//[SwaggerTag("UC014: 新規商品カテゴリ登録")]
public class RegisterCategoryController : ControllerBase
{
    private readonly IRegisterCategoryUsecase _registerCategoryUsecase;
    private readonly RegisterCategoryViewModelAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="registerCategoryUsecase">ユースケース:[新規商品カテゴリを登録する]を実現するインターフェイス</param>
    /// <param name="adapter">RegisterCategoryViewModelからドメインオブジェクト:Categoryへ変換するアダプタ</param>
    public RegisterCategoryController(
        IRegisterCategoryUsecase registerCategoryUsecase,
        RegisterCategoryViewModelAdapter adapter)
    {
        _registerCategoryUsecase = registerCategoryUsecase;
        _adapter = adapter;
    }

    /// <summary>
    /// 商品カテゴリが既に存在するかを検証する
    /// </summary>
    /// <param name="categoryName">検証対象の商品カテゴリ名</param>
    /// <returns>
    /// 存在しない場合:Ok(200)、存在する場合:Conflict(409) 
    /// </returns>
    [HttpGet("validate")]
    /*
    [SwaggerOperation(Summary = "商品カテゴリ名の存在確認",
                      Description = "商品カテゴリ名が既に存在するかを検証する")]
    [SwaggerResponse(StatusCodes.Status200OK, "存在しない場合 { exists=false } を返す")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "商品カテゴリ名が未入力の場合")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "商品カテゴリ名が既に存在する場合")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "未認証、またはJWT トークン無効)")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "サーバー内部エラー")]
    */
    public async Task<IActionResult> ValidateCategory([FromQuery] string categoryName)
    {
        // カテゴリ名がnullか空白
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return BadRequest(new
            { code = "INVALID_CATEGORY_NAME", message = "カテゴリ名は必須です。" });
        }
        try
        {
            // カテゴリ名の存在有無を調べる
            await _registerCategoryUsecase.ExistsByCategoryAsync(categoryName);
            return Ok(new { exists = false });
        }
        catch (ExistsException ex)
        {
            // カテゴリが既に存在する場合
            return Conflict(new
            { code = "CATEGORY_ALREADY_EXISTS", message = ex.Message });
        }
    }

    /// <summary>
    /// 新商品カテゴリを登録する
    /// </summary>
    /// <param name="model">商品カテゴリ登録用ViewModel</param>
    /// <returns></returns>
    [HttpPost("register")]
    /*
    [SwaggerOperation(Summary = "商品カテゴリ登録",
                      Description = "新しい商品カテゴリを登録する【司書のみ】")]
    [SwaggerResponse(StatusCodes.Status201Created, "商品カテゴリ登録成功", typeof(ProductCategory))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "入力値の検証エラー、または分類が存在しない")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "未認証、またはJWT トークン無効)")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "サーバー内部エラー")]
    */
    public async Task<IActionResult> Register(
        RegisterCategoryViewModel model)
    {
        // サーバーサイドバリデーション
        if (!ModelState.IsValid)
        {
            var message = ModelState["Category"]?.Errors.FirstOrDefault()?.ErrorMessage
                ?? "カテゴリ名の入力内容に誤りがあります。";

            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                message
            });

        }
        try
        {
            // 既に登録済みの商品カテゴリを受信した
            await _registerCategoryUsecase.ExistsByCategoryAsync(model.CategoryName);
            // RegisterBookViewModelからcategoryを復元する
            var category = await _adapter.RestoreAsync(model);
            // 商品カテゴリを永続化する
            await _registerCategoryUsecase.RegisterCategoryAsync(category);
            return Created($"/categories/{category.CategoryUuid}", category);
        }
        catch (ExistsException ex)
        {
            // 既に存在する商品カテゴリを受信した
            return BadRequest(new { code = "CATEGORY_ALREADY_EXISTS", message = ex.Message });
        }
        catch (DomainException ex)
        {
            // 業務ルール違反のデータを受信した
            return BadRequest(new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}