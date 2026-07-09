using A_exercise_DB.Applications.Security;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;

namespace A_exercise_DB.Applications.Usecases.EmployeeAccounts;

/// <summary>
/// 担当者アカウント登録ユースケース
/// </summary>
public class RegisterEmployeeAccountUsecase : IRegisterEmployeeAccountUsecase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeAccountRepository _employeeAccountRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeRepository">社員リポジトリ</param>
    /// <param name="employeeAccountRepository">担当者アカウントリポジトリ</param>
    /// <param name="passwordHashingService">パスワードハッシュ化サービス</param>
    /// <param name="unitOfWork">トランザクション制御機能</param>
    public RegisterEmployeeAccountUsecase(
        IEmployeeRepository employeeRepository,
        IEmployeeAccountRepository employeeAccountRepository,
        IPasswordHashingService passwordHashingService,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _employeeAccountRepository = employeeAccountRepository;
        _passwordHashingService = passwordHashingService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// アカウント未登録の社員一覧を取得する
    /// </summary>
    /// <returns>アカウント未登録の社員一覧</returns>
    public async Task<IReadOnlyList<Employee>> GetUnregisteredEmployeesAsync()
    {
        var employees = await _employeeRepository.FindAllWithoutAccountAsync();

        if (employees.Count == 0)
        {
            throw new NotFoundException("アカウント登録可能な社員が存在しません");
        }

        return employees;
    }

    /// <summary>
    /// アカウント名が既に存在するかを検証する
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>なし</returns>
    public async Task ExistsByAccountNameAsync(string accountName)
    {
        if (string.IsNullOrWhiteSpace(accountName))
        {
            throw new DomainException("アカウント名を入力してください");
        }

        var exists = await _employeeAccountRepository.ExistsByAccountNameAsync(accountName);

        if (exists)
        {
            throw new ExistsException("このアカウント名は既に使用されています");
        }
    }

    /// <summary>
    /// 担当者アカウントを登録する
    /// </summary>
    /// <param name="employeeAccount">登録対象の担当者アカウント</param>
    /// <returns>なし</returns>
    public async Task RegisterEmployeeAccountAsync(EmployeeAccount employeeAccount)
    {
        _ = employeeAccount
            ?? throw new InternalException("引数employeeAccountがnullです。");

        if (employeeAccount.Employee is null)
        {
            throw new DomainException("社員名を選択してください");
        }

        var employeeUuid = employeeAccount.Employee.EmployeeUuid;

        await _unitOfWork.BeginAsync();

        try
        {
            // 社員が存在するか確認する
            var employeeExists = await _employeeRepository.ExistsByEmployeeUuidAsync(employeeUuid);

            if (!employeeExists)
            {
                throw new NotFoundException("選択された社員が存在しません");
            }

            // 選択された社員が既にアカウント登録済みでないか確認する
            var employeeAccountExists =
                await _employeeAccountRepository.ExistsByEmployeeUuidAsync(employeeUuid);

            if (employeeAccountExists)
            {
                throw new ExistsException("この社員のアカウントは既に登録されています");
            }

            // アカウント名重複チェック
            await ExistsByAccountNameAsync(employeeAccount.Name);

            // パスワードをハッシュ化する
            var hashedPassword = _passwordHashingService.Hash(
                employeeAccount.Password
            );

            // ハッシュ化済みパスワードの担当者アカウントを作成する
            var registeredAccount = new EmployeeAccount(
                employeeAccount.AccountUuid,
                employeeAccount.Name,
                hashedPassword,
                employeeAccount.Employee
            );

            await _employeeAccountRepository.CreateAsync(registeredAccount);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}