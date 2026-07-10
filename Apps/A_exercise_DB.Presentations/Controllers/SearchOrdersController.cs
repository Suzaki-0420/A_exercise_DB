using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using A_exercise_DB.Applications.Usecases.Orders;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;

namespace A_exercise_DB.Presentations.Controllers;

/// <summary>
/// UC015: 購入履歴検索
/// BP015: 購入履歴検索画面
/// </summary>
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Route("admin/order/search")]
[Tags("UC015: 購入履歴検索")]
public class SearchOrdersController : ControllerBase
{
    private readonly ISearchOrdersUsecase _searchOrdersUsecase;
    private readonly SearchOrdersViewModelAdapter _adapter;
    private readonly ILogger<SearchOrdersController> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="searchOrdersUsecase">ユースケース:[購入履歴を検索する]を実現するインターフェイス</param>
    /// <param name="adapter">SearchOrdersViewModelから検索条件・検索結果ViewModelへ変換するアダプタ</param>
    /// <param name="logger">ログ出力機能</param>
    public SearchOrdersController(
        ISearchOrdersUsecase searchOrdersUsecase,
        SearchOrdersViewModelAdapter adapter,
        ILogger<SearchOrdersController> logger)
    {
        _searchOrdersUsecase = searchOrdersUsecase;
        _adapter = adapter;
        _logger = logger;
    }

    /// <summary>
    /// 購入履歴検索画面の初期表示情報を取得する
    /// </summary>
    /// <returns>
    /// 取得成功時: Ok(200)、
    /// システムエラー: InternalServerError(500)
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(SearchOrdersResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        try
        {
            // 初期表示では注文履歴を全件取得する
            var orders = await _searchOrdersUsecase.SearchAsync(null, null);

            var viewModel = _adapter.ConvertToResultViewModel(orders);

            if (viewModel.OrderList.Count == 0)
            {
                viewModel.Message = "注文が登録されていません";
            }

            return Ok(viewModel);
        }
        catch (InternalException ex)
        {
            _logger.LogError(ex, "購入履歴検索画面の初期表示中にエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "ORDER_DATA_FETCH_ERROR",
                message = "注文情報の取得に失敗しました"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "購入履歴検索画面の初期表示中に予期しないエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "注文情報の取得に失敗しました"
            });
        }
    }

    /// <summary>
    /// 購入日または顧客アカウント名で購入履歴を検索する
    /// </summary>
    /// <param name="model">購入履歴検索条件ViewModel</param>
    /// <returns>
    /// 検索成功時: Ok(200)、
    /// 入力値不正: BadRequest(400)、
    /// システムエラー: InternalServerError(500)
    /// </returns>
    [HttpGet("result")]
    [ProducesResponseType(typeof(SearchOrdersResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery(Name = "orderDate")] string? orderDate,
        [FromQuery(Name = "customerAccountName")] string? customerAccountName)
    {
        var model = new SearchOrdersViewModel
        {
            OrderDate = orderDate,
            CustomerAccountName = customerAccountName
        };

        try
        {
            var restoredOrderDate = _adapter.RestoreOrderDate(model);
            var restoredCustomerAccountName = _adapter.RestoreCustomerAccountName(model);

            var orders = await _searchOrdersUsecase.SearchAsync(
                restoredOrderDate,
                restoredCustomerAccountName
            );

            var viewModel = _adapter.ConvertToResultViewModel(orders);

            return Ok(viewModel);
        }
        catch (DomainException ex)
        {
            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                message = ex.Message
            });
        }
        catch (InternalException ex)
        {
            _logger.LogError(ex, "購入履歴検索中にエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "ORDER_DATA_FETCH_ERROR",
                message = "注文情報の取得に失敗しました"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "購入履歴検索中に予期しないエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "注文情報の取得に失敗しました"
            });
        }
    }
}
