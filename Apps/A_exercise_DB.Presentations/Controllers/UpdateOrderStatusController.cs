using Microsoft.AspNetCore.Mvc;
using A_exercise_DB.Applications.Usecases.Orders;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;

namespace A_exercise_DB.Presentations.Controllers;

/// <summary>
/// UC016: 注文ステータス更新
/// BP016: 注文ステータス更新入力画面
/// BP017: 注文ステータス更新確認画面
/// BP018: 注文ステータス更新完了画面
/// </summary>
[ApiController]
[Route("admin/order/status/update")]
[Tags("UC016: 注文ステータス更新")]
public class UpdateOrderStatusController : ControllerBase
{
    private readonly IUpdateOrderStatusUsecase _updateOrderStatusUsecase;
    private readonly UpdateOrderStatusViewModelAdapter _adapter;
    private readonly ILogger<UpdateOrderStatusController> _logger;

    public UpdateOrderStatusController(
        IUpdateOrderStatusUsecase updateOrderStatusUsecase,
        UpdateOrderStatusViewModelAdapter adapter,
        ILogger<UpdateOrderStatusController> logger)
    {
        _updateOrderStatusUsecase = updateOrderStatusUsecase;
        _adapter = adapter;
        _logger = logger;
    }

    /// <summary>
    /// 注文ステータス更新入力画面の表示情報を取得する
    /// </summary>
    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(UpdateOrderStatusInputViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInput(Guid orderId)
    {
        try
        {
            var inputData = await _updateOrderStatusUsecase.GetInputAsync(orderId);

            if (inputData is null)
            {
                return NotFound(new
                {
                    code = "ORDER_NOT_FOUND",
                    message = "指定された注文は存在しません",
                    redirectUrl = "/admin/order/search"
                });
            }

            var viewModel = _adapter.ConvertToInputViewModel(
                inputData.Value.Order,
                inputData.Value.OrderStatuses);

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
            _logger.LogError(ex, "注文ステータス更新入力画面の表示中にエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "ORDER_STATUS_INPUT_ERROR",
                message = "注文情報の取得に失敗しました"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注文ステータス更新入力画面の表示中に予期しないエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
    }

    /// <summary>
    /// 注文ステータス更新確認画面の表示情報を取得する
    /// </summary>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(UpdateOrderStatusConfirmViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Confirm(
        [FromBody] UpdateOrderStatusConfirmViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    code = "VALIDATION_ERROR",
                    message = "入力内容が不正です"
                });
            }

            var confirmData = await _updateOrderStatusUsecase.GetConfirmAsync(
                model.OrderId,
                model.NewStatusId);

            if (confirmData is null)
            {
                return NotFound(new
                {
                    code = "ORDER_NOT_FOUND",
                    message = "指定された注文は存在しません",
                    redirectUrl = "/admin/order/search"
                });
            }

            var viewModel = _adapter.ConvertToConfirmViewModel(
                confirmData.Value.Order,
                confirmData.Value.NewStatus);

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
            _logger.LogError(ex, "注文ステータス更新確認画面の表示中にエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "ORDER_STATUS_CONFIRM_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注文ステータス更新確認画面の表示中に予期しないエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
    }

    /// <summary>
    /// 注文ステータスを更新する
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(UpdateOrderStatusCompleteViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Complete(
        [FromBody] UpdateOrderStatusConfirmViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    code = "VALIDATION_ERROR",
                    message = "入力内容が不正です"
                });
            }

            var completeData = await _updateOrderStatusUsecase.UpdateAsync(
                model.OrderId,
                model.NewStatusId);

            var viewModel = _adapter.ConvertToCompleteViewModel(
                completeData.Order,
                completeData.NewStatus);

            return Ok(viewModel);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new
            {
                code = "ORDER_NOT_FOUND",
                message = ex.Message,
                redirectUrl = "/admin/order/search"
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
        catch (InternalException ex)
        {
            _logger.LogError(ex, "注文ステータス更新中にエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "ORDER_STATUS_UPDATE_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注文ステータス更新中に予期しないエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
    }
}