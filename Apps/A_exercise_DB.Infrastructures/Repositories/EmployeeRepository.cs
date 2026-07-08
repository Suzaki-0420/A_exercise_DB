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
    private readonly EmployeeEntityAdapter _adapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">DBコンテキスト</param>
    /// <param name="adapter">EmployeeEntityAdapter</param>
    public EmployeeRepository(
        AppDbContext context,
        EmployeeEntityAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// 社員情報をすべて取得する
    /// </summary>
    /// <returns>Employeeのリスト</returns>
    public async Task<List<Employee>> FindAllAsync()
    {
        try
        {
            // 社員と部署を取得する
            var entities = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .ToListAsync();

            // EmployeeEntityからEmployeeを復元する
            var employees = new List<Employee>();

            foreach (var entity in entities)
            {
                var employee = await _adapter.RestoreAsync(entity);
                if (entity.Department is not null)
                {
                    var department = new Department(
                        entity.Department.DepartmentUuid,
                        entity.Department.Name
                    );

                    employee.ChangeDepartment(department);
                    employees.Add(employee);
                }

                return employees;
            }
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                "社員情報の一覧取得時に予期しないエラーが発生しました。",
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
            var employee = await _adapter.RestoreAsync(entity);

            return employee;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                $"Id:{uuid}の社員情報取得時に予期しないエラーが発生しました。",
                ex);
        }
    }
}