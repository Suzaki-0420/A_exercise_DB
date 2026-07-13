using Microsoft.AspNetCore.Mvc;
using A_exercise_DB.Applications.Usecases.Accounts;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;

namespace A_exercise_DB.Presentations.Controllers;

/// <summary>
/// UC009: 担当者アカウント登録
/// BP003: 担当者アカウント登録(入力)画面
/// BP004: 担当者アカウント登録(確認)画面
/// BP005: 担当者アカウント登録(完了)画面
/// </summary>
[ApiController]
[Route("admin/account")]
[Tags("UC009: 担当者アカウント登録")]
public class RegisterEmployeeAccountController : ControllerBase
{
    private readonly IRegisterEmployeeAccountUsecase _registerEmployeeAccountUsecase;
    private readonly RegisterEmployeeAccountViewModelAdapter _adapter;
    private readonly ILogger<RegisterEmployeeAccountController> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="registerEmployeeAccountUsecase">ユースケース:[担当者アカウントを登録する]を実現するインターフェイス</param>
    /// <param name="adapter">RegisterEmployeeAccountViewModelからドメインオブジェクト:EmployeeAccountへ変換するアダプタ</param>
    /// <param name="logger">ログ出力機能</param>
    public RegisterEmployeeAccountController(
        IRegisterEmployeeAccountUsecase registerEmployeeAccountUsecase,
        RegisterEmployeeAccountViewModelAdapter adapter,
        ILogger<RegisterEmployeeAccountController> logger)
    {
        _registerEmployeeAccountUsecase = registerEmployeeAccountUsecase;
        _adapter = adapter;
        _logger = logger;
    }

    /// <summary>
    /// 担当者アカウント登録入力画面の初期表示情報を取得する
    /// </summary>
    /// <returns>
    /// 取得成功時: Ok(200)、
    /// 未登録社員なし: NotFound(404)、
    /// システムエラー: InternalServerError(500)
    /// </returns>
    [HttpGet("form")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetForm()
    {
        try
        {
            var employees = await _registerEmployeeAccountUsecase.GetUnregisteredEmployeesAsync();

            return Ok(new
            {
                title = "担当者アカウント登録(入力)",
                employees = employees.Select(employee => new
                {
                    employeeUuid = employee.EmployeeUuid,
                    employeeName = employee.Name
                })
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new
            {
                code = "NO_UNREGISTERED_EMPLOYEE",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "担当者アカウント登録入力画面の初期表示情報取得中にエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "社員情報の取得に失敗しました"
            });
        }
    }

    /// <summary>
    /// アカウント名が既に存在するかを検証する
    /// </summary>
    /// <param name="accountName">検証対象のアカウント名</param>
    /// <returns>
    /// 存在しない場合: Ok(200)、
    /// 入力値不正: BadRequest(400)、
    /// 存在する場合: Conflict(409)、
    /// システムエラー: InternalServerError(500)
    /// </returns>
    [HttpGet("validate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateAccountName([FromQuery] string accountName)
    {
        try
        {
            await _registerEmployeeAccountUsecase.ExistsByAccountNameAsync(accountName);

            return Ok(new
            {
                exists = false,
                message = "使用できるアカウント名です"
            });
        }
        catch (ExistsException ex)
        {
            return Conflict(new
            {
                code = "ACCOUNT_NAME_ALREADY_EXISTS",
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
            _logger.LogError(ex, "アカウント名の存在確認中にエラーが発生しました。accountName: {AccountName}", accountName);

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
    }

    /// <summary>
    /// 担当者アカウント登録内容を確認する
    /// </summary>
    /// <param name="model">担当者アカウント登録用ViewModel</param>
    /// <returns>
    /// 確認成功時: Ok(200)、
    /// 入力値不正: BadRequest(400)、
    /// 社員が存在しない場合: NotFound(404)、
    /// システムエラー: InternalServerError(500)
    /// </returns>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Confirm([FromBody] RegisterEmployeeAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                messages = GetModelStateErrorMessages()
            });
        }

        try
        {
            var employee = await FindUnregisteredEmployeeAsync(model.EmployeeUuid);

            var confirmViewModel = await _adapter.ToConfirmViewModelAsync(model, employee);

            return Ok(confirmViewModel);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new
            {
                code = "EMPLOYEE_NOT_FOUND",
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
            _logger.LogError(ex, "担当者アカウント登録確認処理中にエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "システムエラーが発生しました。管理者に連絡してください"
            });
        }
    }

    /// <summary>
    /// 担当者アカウントを登録する
    /// </summary>
    /// <param name="model">担当者アカウント登録用ViewModel</param>
    /// <returns>
    /// 登録成功時: Created(201)、
    /// 入力値不正: BadRequest(400)、
    /// 社員が存在しない場合: NotFound(404)、
    /// アカウント名重複: Conflict(409)、
    /// システムエラー: InternalServerError(500)
    /// </returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterEmployeeAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                messages = GetModelStateErrorMessages()
            });
        }

        try
        {
            var employee = await FindUnregisteredEmployeeAsync(model.EmployeeUuid);

            var employeeAccount = await _adapter.RestoreAsync(model, employee);

            await _registerEmployeeAccountUsecase.RegisterEmployeeAccountAsync(employeeAccount);

            var completeViewModel = await _adapter.ToCompleteViewModelAsync(employeeAccount);

            return Created($"/admin/account/{employeeAccount.AccountUuid}", completeViewModel);
        }
        catch (ExistsException ex)
        {
            return Conflict(new
            {
                code = "ACCOUNT_ALREADY_EXISTS",
                message = ex.Message
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new
            {
                code = "EMPLOYEE_NOT_FOUND",
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
        catch (InternalException ex)
        {
            _logger.LogError(ex, "担当者アカウント登録中に内部エラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "INTERNAL_ERROR",
                message = "登録処理に失敗しました。管理者に連絡してください"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "担当者アカウント登録中に予期しないエラーが発生しました。");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                code = "SYSTEM_ERROR",
                message = "登録処理に失敗しました。管理者に連絡してください"
            });
        }
    }

    /// <summary>
    /// 未登録社員一覧から指定された社員を取得する
    /// </summary>
    /// <param name="employeeUuid">社員UUID</param>
    /// <returns>社員</returns>
    private async Task<Domains.Models.Employee> FindUnregisteredEmployeeAsync(Guid? employeeUuid)
    {
        if (employeeUuid is null)
        {
            throw new DomainException("社員名を選択してください");
        }

        if (employeeUuid.Value == Guid.Empty)
        {
            throw new DomainException("社員名を選択してください");
        }

        var employees = await _registerEmployeeAccountUsecase.GetUnregisteredEmployeesAsync();

        var employee = employees.FirstOrDefault(
            x => x.EmployeeUuid == employeeUuid.Value
        );

        if (employee is null)
        {
            throw new NotFoundException("選択された社員が存在しません");
        }

        return employee;
    }

    /// <summary>
    /// ModelStateのエラーメッセージを取得する
    /// </summary>
    /// <returns>エラーメッセージ一覧</returns>
    private List<string> GetModelStateErrorMessages()
    {
        return ModelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();
    }
}