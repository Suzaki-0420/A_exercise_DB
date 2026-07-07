using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructures.Adapters;

/// <summary>
/// ドメインオブジェクト:EmployeeとEmployeeEntityの相互変換クラス
/// </summary>
public class EmployeeEntityAdapter :
    IConverter<Employee, EmployeeEntity>,
    IRestorer<Employee, EmployeeEntity>
{
    /// <summary>
    /// ドメインオブジェクト:EmployeeをEmployeeEntityに変換する
    /// </summary>
    /// <param name="domain">ドメインオブジェクト:Employee</param>
    /// <returns>EFCore:EmployeeEntity</returns>
    public Task<EmployeeEntity> ConvertAsync(Employee domain)
    {
        // 引数domainがnullの場合
        _ = domain ?? throw new InternalException("引数domainがnullです。");

        // ドメインオブジェクト:EmployeeをEmployeeEntityに変換する
        var entity = new EmployeeEntity();
        entity.EmployeeUuid = domain.EmployeeUuid;
        entity.Name = domain.Name;
        entity.Kana = domain.Kana;
        entity.DepartmentId = domain.Department.DepartmentId;

        return Task.FromResult(entity);
    }

    /// <summary>
    /// EmployeeEntityからドメインオブジェクト:Employeeを復元する
    /// </summary>
    /// <param name="target">EFCore:EmployeeEntity</param>
    /// <returns>ドメインオブジェクト:Employee</returns>
    public Task<Employee> RestoreAsync(EmployeeEntity target)
    {
        // 引数targetがnullの場合
        _ = target ?? throw new InternalException("引数targetがnullです。");

        // EmployeeEntityからドメインオブジェクト:Employeeを復元する
        var domain = new Employee(
            target.EmployeeUuid,
            target.Name,
            target.Kana,
            target.Department
        );

        return Task.FromResult(domain);
    }
}