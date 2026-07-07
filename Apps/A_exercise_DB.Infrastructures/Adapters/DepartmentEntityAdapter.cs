using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructure.Adapters;

/// <summary>
/// ドメインオブジェクト:DepartmentとDepartmentEntityの相互変換クラス
/// </summary>
public class DepartmentEntityAdapter :
    IConverter<Department, DepartmentEntity>,
    IRestorer<Department, DepartmentEntity>
{
    /// <summary>
    /// ドメインオブジェクト:DepartmentをDepartmentEntityに変換する
    /// </summary>
    /// <param name="domain">ドメインオブジェクト:Department</param>
    /// <returns>EFCore:DepartmentEntity</returns>
    public Task<DepartmentEntity> ConvertAsync(Department domain)
    {
        // 引数domainがnullの場合
        _ = domain ?? throw new InternalException("引数domainがnullです。");

        // ドメインオブジェクト:DepartmentをDepartmentEntityに変換する
        var entity = new DepartmentEntity();
        entity.DepartmentUuid = domain.DepartmentUuid;
        entity.Name = domain.Name;

        return Task.FromResult(entity);
    }

    /// <summary>
    /// DepartmentEntityからドメインオブジェクト:Departmentを復元する
    /// </summary>
    /// <param name="target">EFCore:DepartmentEntity</param>
    /// <returns>ドメインオブジェクト:Department</returns>
    public Task<Department> RestoreAsync(DepartmentEntity target)
    {
        // 引数targetがnullの場合
        _ = target ?? throw new InternalException("引数targetがnullです。");

        // DepartmentEntityからドメインオブジェクト:Departmentを復元する
        var domain = new Department(
            target.DepartmentUuid,
            target.Name
        );

        return Task.FromResult(domain);
    }
}