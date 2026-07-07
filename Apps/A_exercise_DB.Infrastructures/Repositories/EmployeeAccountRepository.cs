using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Entities;
using A_exercise_DB.Infrastructures.Contexts;
using Microsoft.EntityFrameworkCore;
using A_exercise_DB.Infrastructures.Adapters;

namespace A_exercise_DB.Infrastructure.Repositories;

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
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                "社員アカウントの永続化中に予期しないエラーが発生しました。",
                ex);
        }
    }
}