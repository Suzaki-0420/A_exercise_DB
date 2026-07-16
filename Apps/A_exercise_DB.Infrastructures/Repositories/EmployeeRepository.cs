using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Entities;
using A_exercise_DB.Infrastructures.Contexts;
using Microsoft.EntityFrameworkCore;
using A_exercise_DB.Infrastructures.Adapters;

namespace A_exercise_DB.Infrastructures.Repositories;

/// <summary>
/// 社員Repository
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;
    private readonly EmployeeFactory _factory;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">DBコンテキスト</param>
    /// <param name="adapter">EmployeeEntityAdapter</param>
    public EmployeeRepository(
        AppDbContext context,
        EmployeeFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// アカウント登録がされていない社員情報をすべて取得する
    /// </summary>
    /// <returns>Employeeのリスト</returns>
    public async Task<List<Employee>> FindAllWithoutAccountAsync()
    {
        try
        {
            var entities = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.EmployeeAccount)
                .Where(e => e.EmployeeAccount == null)
                .ToListAsync();

            var employees = new List<Employee>();

            foreach (var entity in entities)
            {
                var employee = await _factory.RestoreAsync(entity);
                employees.Add(employee);
            }

            return employees;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                "アカウント未登録の社員一覧取得時に予期しないエラーが発生しました。",
                ex);
        }
    }

    /// <summary>
    /// 指定された社員Idの社員情報を取得する
    /// </summary>
    /// <param name="id">社員Id(UUID)</param>
    /// <returns>Employee または null</returns>
    public async Task<Employee?> FindByIdAsync(string uuid)
    {
        try
        {
            // 引数のUUIDで社員を取得する
            var entity = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .SingleOrDefaultAsync(e => e.EmployeeUuid == Guid.Parse(uuid));

            if (entity is null)
            {
                return null;
            }

            // EmployeeEntityからEmployeeを復元する
            var employee = await _factory.RestoreAsync(entity);

            return employee;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                $"Id:{uuid}の社員情報取得時に予期しないエラーが発生しました。",
                ex);
        }
    }

    /// <summary>
    /// 社員UUIDに一致する社員が存在するかを確認する
    /// </summary>
    /// <param name="employeeUuid">社員UUID</param>
    /// <returns>存在する場合true</returns>
    public async Task<bool> ExistsByEmployeeUuidAsync(Guid employeeUuid)
    {
        try
        {
            return await _context.Employees
                .AsNoTracking()
                .AnyAsync(e => e.EmployeeUuid == employeeUuid);
        }
        catch (Exception ex)
        {
            throw new InternalException(
                $"社員UUID:{employeeUuid}の存在確認中に予期しないエラーが発生しました。",
                ex);
        }
    }


}