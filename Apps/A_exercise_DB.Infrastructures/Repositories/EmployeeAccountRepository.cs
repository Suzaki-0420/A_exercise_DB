using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Entities;
using A_exercise_DB.Infrastructures.Contexts;
using Microsoft.EntityFrameworkCore;
using A_exercise_DB.Infrastructures.Adapters;

namespace A_exercise_DB.Infrastructures.Repositories;

/// <summary>
/// 社員アカウントRepository
/// </summary>
public class EmployeeAccountRepository : IEmployeeAccountRepository
{
    private readonly AppDbContext _context;
    private readonly EmployeeAccountEntityAdapter _adapter;

    public EmployeeAccountRepository(
        AppDbContext context,
        EmployeeAccountEntityAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// 社員アカウントを永続化する
    /// </summary>
    /// <param name="employeeAccount">永続化する社員アカウント</param>
    /// <returns>なし</returns>
    public async Task CreateAsync(EmployeeAccount employeeAccount)
    {
        try
        {
            // 登録するアカウントに紐づく社員を取得する
            var employee = await _context.Employees
                .SingleOrDefaultAsync(e =>
                    e.EmployeeUuid == employeeAccount.Employee!.EmployeeUuid);

            if (employee is null)
            {
                throw new Exception(
                    $"Id:{employeeAccount.Employee!.EmployeeUuid}の社員は存在しません。");
            }

            // EmployeeAccountをEmployeeAccountEntityに変換する
            var entity = await _adapter.ConvertAsync(employeeAccount);

            // 社員の外部キーを設定する
            entity.Employee = null!;
            entity.EmployeeId = employee.Id;

            // 社員アカウントを登録する
            await _context.EmployeeAccounts.AddAsync(entity);

            // データベースに反映する
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InternalException(
                "社員アカウントの永続化中に予期しないエラーが発生しました。",
                ex);
        }
    }

    /// <summary>
    /// アカウント名に一致する社員アカウントを取得する
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>社員アカウント。存在しない場合はnull</returns>
    public async Task<EmployeeAccount?> FindByNameAsync(string accountName)
    {
        try
        {
            var entity = await _context.EmployeeAccounts
                .AsNoTracking()
                .Include(a => a.Employee)
                .SingleOrDefaultAsync(a => a.Name == accountName);

            if (entity is null)
            {
                return null;
            }

            var employee = new Employee(
                entity.Employee.EmployeeUuid,
                entity.Employee.Name,
                entity.Employee.Kana);

            return new EmployeeAccount(
                entity.AccountUuid,
                entity.Name,
                entity.Password,
                employee);
        }
        catch (Exception ex)
        {
            throw new InternalException(
                $"アカウント名:{accountName}の社員アカウント取得中に予期しないエラーが発生しました。",
                ex);
        }
    }

    /// <summary>
    /// アカウント名が既に存在するかを確認する
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>存在する場合true</returns>
    public async Task<bool> ExistsByAccountNameAsync(string accountName)
    {
        try
        {
            return await _context.EmployeeAccounts
                .AsNoTracking()
                .AnyAsync(a => a.Name == accountName);
        }
        catch (Exception ex)
        {
            throw new InternalException(
                $"アカウント名:{accountName}の存在確認中に予期しないエラーが発生しました。",
                ex);
        }
    }

    /// <summary>
    /// 社員UUIDに紐づく担当者アカウントが既に存在するかを確認する
    /// </summary>
    /// <param name="employeeUuid">社員UUID</param>
    /// <returns>存在する場合true</returns>
    public async Task<bool> ExistsByEmployeeUuidAsync(Guid employeeUuid)
    {
        try
        {
            return await _context.EmployeeAccounts
                .AsNoTracking()
                .Include(a => a.Employee)
                .AnyAsync(a => a.Employee.EmployeeUuid == employeeUuid);
        }
        catch (Exception ex)
        {
            throw new InternalException(
                $"社員UUID:{employeeUuid}に紐づくアカウントの存在確認中に予期しないエラーが発生しました。",
                ex);
        }
    }
}