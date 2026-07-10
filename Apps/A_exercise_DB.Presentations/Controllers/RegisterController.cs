using Microsoft.AspNetCore.Mvc;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;
using A_exercise_DB.Domains.Models;
namespace A_exercise_DB.Presentations.Controllers;
/// <summary>
/// UC014: 新規商品登録
/// </summary>
[ApiController]
[Route("admin/product")]
//[SwaggerTag("UC014: 新規商品登録")]
public class RegisterProductController : ControllerBase
{
    private readonly IRegisterProductUsecase _registerProductUsecase;
    private readonly ProductRegisterViewModelAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="registerProductUsecase">ユースケース:[新規商品カテゴリを登録する]を実現するインターフェイス</param>
    /// <param name="adapter">RegisterProductViewModelからドメインオブジェクト:Productへ変換するアダプタ</param>
    public RegisterProductController(
        IRegisterProductUsecase registerProductUsecase,
        ProductRegisterViewModelAdapter adapter)
    {
        _registerProductUsecase = registerProductUsecase;
        _adapter = adapter;
    }

    /// <summary>
    /// 商品が既に存在するかを検証する
    /// </summary>
    /// <param name="ProductName">検証対象の商品カテゴリ名</param>
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
    public async Task<IActionResult> ValidateProductName([FromQuery] string ProductName)
    {
        // カテゴリ名がnullか空白
        if (string.IsNullOrWhiteSpace(ProductName))
        {
            return BadRequest(new
            { code = "INVALID_Product_NAME", message = "商品名は必須です。" });
        }
        try
        {
            // 商品名の存在有無を調べる
            await _registerProductUsecase.ExistsByProductNameAsync(ProductName);
            return Ok(new { exists = false });
        }
        catch (ExistsException ex)
        {
            // 商品が既に存在する場合
            return Conflict(new
            { code = "Product_ALREADY_EXISTS", message = ex.Message });
        }
    }

    /// <summary>
    /// 新商品カテゴリを登録する
    /// </summary>
    /// <param name="model">商品登録用ViewModel</param>
    /// <returns></returns>
    [HttpPost("register")]
    /*
    [SwaggerOperation(Summary = "商品登録",
                      Description = "新しい商品を登録する【司書のみ】")]
    [SwaggerResponse(StatusCodes.Status201Created, "商品登録成功", typeof(ProductProduct))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "入力値の検証エラー、または分類が存在しない")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "未認証、またはJWT トークン無効)")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "サーバー内部エラー")]
    */
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        // サーバーサイドバリデーション
        if (!ModelState.IsValid)
        {
            var message = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "商品名の入力内容に誤りがあります。";

            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                message
            });
        }

        try
        {
            if (!model.CategoryUuid.HasValue || model.CategoryUuid == Guid.Empty)
            {
                return BadRequest(new
                {
                    code = "CATEGORY_REQUIRED",
                    message = "商品カテゴリを選択してください。"
                });
            }

            var category = await _registerProductUsecase.GetCategoryByIdAsync(model.CategoryUuid.Value.ToString());
            model.CategoryName = category.Name;
            var product = await _adapter.RestoreAsync(model);

            await _registerProductUsecase.ExistsByProductNameAsync(product.Name);
            await _registerProductUsecase.RegisterProductAsync(product);
            return Created($"/categories/{product.ProductUuid}", product);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { code = "CATEGORY_NOT_FOUND", message = ex.Message });
        }
        catch (ExistsException ex)
        {
            // 既に存在する商品カテゴリを受信した
            return BadRequest(new { code = "Product_ALREADY_EXISTS", message = ex.Message });
        }
        catch (DomainException ex)
        {
            // 業務ルール違反のデータを受信した
            return BadRequest(new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _registerProductUsecase.GetCategoriesAsync();
        return Ok(categories);
    }
    }
