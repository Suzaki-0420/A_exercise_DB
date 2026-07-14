using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace A_exercise_DB.Presentations.Controllers;

/// <summary>
/// UC010：新商品登録
/// </summary>
[ApiController]
[AllowAnonymous]
// [Authorize]
[ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
[Route("admin/product")]
[Tags("UC010: 新商品登録")]
public class RegisterProductController
    : ControllerBase
{
    private readonly IRegisterProductUsecase
        _registerProductUsecase;

    private readonly ProductRegisterViewModelAdapter
        _adapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="registerProductUsecase">
    /// 新商品登録ユースケース
    /// </param>
    /// <param name="adapter">
    /// ViewModelを商品登録ユースケースの入力値へ
    /// 変換するAdapter
    /// </param>
    public RegisterProductController(
        IRegisterProductUsecase registerProductUsecase,
        ProductRegisterViewModelAdapter adapter)
    {
        _registerProductUsecase =
            registerProductUsecase;

        _adapter = adapter;
    }

    /// <summary>
    /// 商品名が既に存在するかを確認する
    /// </summary>
    /// <param name="productName">
    /// 検証対象の商品名
    /// </param>
    /// <returns>
    /// 存在しない場合は200、
    /// 存在する場合は409
    /// </returns>
    [HttpGet("validate")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult>
        ValidateProductName(
            [FromQuery] string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return BadRequest(new
            {
                code = "INVALID_PRODUCT_NAME",
                message = "商品名は必須です。"
            });
        }

        var exists =
            await _registerProductUsecase
                .ExistsByProductNameAsync(
                    productName);

        if (exists)
        {
            return Conflict(new
            {
                code = "PRODUCT_ALREADY_EXISTS",
                message =
                    "同じ商品名の商品が既に存在します。"
            });
        }

        return Ok(new
        {
            exists = false
        });
    }

    /// <summary>
    /// 新商品を登録する
    /// </summary>
    /// <param name="model">
    /// 商品登録用ViewModel
    /// </param>
    /// <returns>
    /// 登録された商品
    /// </returns>
    /// <remarks>
    /// 商品画像を含むため、
    /// multipart/form-dataで受け取る。
    /// </remarks>
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [ProducesResponseType(
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(
        [FromForm] RegisterViewModel model)
    {
        /*
         * [ApiController]が付いている場合、
         * 通常はModelStateが不正なら
         * Controller実行前に400が返されます。
         *
         * 現在の独自レスポンス形式を維持するため、
         * このチェックを残しています。
         */
        if (!ModelState.IsValid)
        {
            var message =
                ModelState.Values
                    .SelectMany(
                        value => value.Errors)
                    .Select(
                        error =>
                            error.ErrorMessage)
                    .FirstOrDefault(
                        errorMessage =>
                            !string.IsNullOrWhiteSpace(
                                errorMessage))
                ?? "商品の入力内容に誤りがあります。";

            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                message
            });
        }

        if (!model.CategoryUuid.HasValue ||
            model.CategoryUuid.Value ==
            Guid.Empty)
        {
            return BadRequest(new
            {
                code = "CATEGORY_REQUIRED",
                message =
                    "商品カテゴリを選択してください。"
            });
        }

        /*
         * Controllerが開いたStreamは、
         * Controllerで破棄する。
         */
        Stream? imageStream = null;

        try
        {
            /*
             * 画像が指定されている場合だけ
             * Streamを開く。
             */
            if (model.Image is not null)
            {
                imageStream =
                    model.Image.OpenReadStream();
            }

            /*
             * ViewModelをユースケースの入力値へ変換する。
             *
             * この時点ではまだProductを作らない。
             * 画像URLが確定していないため。
             */
            var param =
                _adapter.ToParam(
                    model,
                    imageStream);

            /*
             * 必要であれば、登録前にも
             * 商品名の重複を確認する。
             */
            var exists =
                await _registerProductUsecase
                    .ExistsByProductNameAsync(
                        param.Name);

            if (exists)
            {
                return Conflict(new
                {
                    code =
                        "PRODUCT_ALREADY_EXISTS",
                    message =
                        "同じ商品名の商品が既に存在します。"
                });
            }

            /*
             * Usecase内で以下を行う。
             *
             * 1. 画像保存
             * 2. カテゴリ取得
             * 3. 在庫生成
             * 4. Product生成
             * 5. DB登録
             */
            var product =
                await _registerProductUsecase
                    .RegisterProductAsync(param);

            /*
             * 現在、商品ID指定のGET APIがない場合、
             * CreatedAtActionは使用できないため、
             * 201と商品をそのまま返す。
             */
            return StatusCode(
                StatusCodes.Status201Created,
                product);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new
            {
                code = "CATEGORY_NOT_FOUND",
                message = ex.Message
            });
        }
        catch (ExistsException ex)
        {
            return Conflict(new
            {
                code = "PRODUCT_ALREADY_EXISTS",
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
        finally
        {
            /*
             * OpenReadStreamで開いたStreamを
             * 必ず破棄する。
             */
            if (imageStream is not null)
            {
                await imageStream.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// 商品登録画面用の商品カテゴリ一覧を取得する
    /// </summary>
    /// <returns>
    /// 商品カテゴリ一覧
    /// </returns>
    [HttpGet("categories")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    public async Task<IActionResult>
        GetCategories()
    {
        var categories =
            await _registerProductUsecase
                .GetCategoriesAsync();

        return Ok(categories);
    }
}