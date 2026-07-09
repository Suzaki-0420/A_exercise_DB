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
[Route("admin/category")]
public class RegisterCategoryController : ControllerBase
{
    private readonly IRegisterCategoryUsecase _registerCategoryUsecase;
    private readonly RegisterCategoryViewModelAdapter _adapter;
    private readonly ILogger<RegisterCategoryController> _logger;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="registerCategoryUsecase">ユースケース:[新規商品カテゴリを登録する]を実現するインターフェイス</param>
    /// <param name="adapter">RegisterCategoryViewModelからドメインオブジェクト:Categoryへ変換するアダプタ</param>
    public RegisterCategoryController(
        IRegisterCategoryUsecase registerCategoryUsecase,
        RegisterCategoryViewModelAdapter adapter,
        ILogger<RegisterCategoryController> logger)
    {
        _registerCategoryUsecase = registerCategoryUsecase;
        _adapter = adapter;
        _logger = logger;
    }

    /// <summary>
    /// 商品カテゴリ名が既に存在するかを検証する
    /// </summary>
    /// <param name="categoryName">検証対象の商品カテゴリ名</param>
    /// <returns>
    /// 存在しない場合: Ok(200)、存在する場合: Conflict(409)
    /// </returns>
    [HttpGet("validate")]
    public async Task<IActionResult> ValidateCategoryName([FromQuery] string categoryName)
    {
        try
        {
            await _registerCategoryUsecase.ExistsByCategoryAsync(categoryName);

            return Ok(new
            {
                exists = false,
                message = "使用できるカテゴリ名です"
            });
        }
        catch (ExistsException ex)
        {
            return Conflict(new
            {
                code = "CATEGORY_ALREADY_EXISTS",
                exists = true,
                message = ex.Message
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品カテゴリ名の存在確認中にエラーが発生しました。categoryName: {CategoryName}", categoryName);

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
    }

    /// <summary>
    /// 新商品カテゴリを登録する
    /// </summary>
    /// <param name="model">商品カテゴリ登録用ViewModel</param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var message = ModelState["CategoryName"]?.Errors.FirstOrDefault()?.ErrorMessage
                ?? "カテゴリ名の入力内容に誤りがあります。";

            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                message
            });
        }

        try
        {
            // RegisterCategoryViewModelからProductCategoryを復元する
            var category = await _adapter.RestoreAsync(model);

            // 商品カテゴリを登録する
            await _registerCategoryUsecase.RegisterCategoryAsync(category);

            return Created($"/admin/category/{category.CategoryUuid}", new
            {
                message = $"商品カテゴリ「{category.Name}」を登録しました",
                category
            });
        }
        catch (ExistsException ex)
        {
            return Conflict(new
            {
                code = "CATEGORY_ALREADY_EXISTS",
                message = ex.Message
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(new
            {
                code = "DOMAIN_RULE_VIOLATION",
                message = ex.Message
            });
        }
        catch (InternalException ex)
        {
            _logger.LogError(ex, "商品カテゴリ登録中に内部エラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "INTERNAL_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品カテゴリ登録中に予期しないエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
    }
}