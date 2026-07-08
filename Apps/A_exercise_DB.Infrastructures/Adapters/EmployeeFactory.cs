using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructures.Adapters;

/// <summary>
/// 社員、部署オブジェクトの相互変換Factoryクラス
/// ドメインオブジェクト:EmployeeとEmployeeEntityの相互変換
/// ドメインオブジェクト:DepartmentとDepartmentEntityの相互変換
/// </summary>
public class EmployeeFactory
{
    private readonly EmployeeEntityAdapter _employeeEntityAdapter;
    private readonly DepartmentEntityAdapter _departmentEntityAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeEntityAdapter">EmployeeとEmployeeEntityの相互変換</param>
    /// <param name="departmentEntityAdapter">DepartmentとDepartmentEntityの相互変換</param>
    public EmployeeFactory(
        EmployeeEntityAdapter employeeEntityAdapter,
        DepartmentEntityAdapter departmentEntityAdapter)
    {
        _employeeEntityAdapter = employeeEntityAdapter;
        _departmentEntityAdapter = departmentEntityAdapter;
    }

    /// <summary>
    /// 社員、部署の集約関係を構築したEntityを生成して返す
    /// </summary>
    /// <param name="domain">ルートドメインオブジェクト:Employee</param>
    /// <returns>集約関係を構築したEmployeeEntity</returns>
    public async Task<EmployeeEntity> ConvertAsync(Employee domain)
    {
        // EmployeeからEmployeeEntityを生成する
        var entity = await _employeeEntityAdapter.ConvertAsync(domain);

        // 部署が存在しない場合はリターンする
        if (domain.Department is null)
        {
            return entity;
        }

        // DepartmentをDepartmentEntityに変換してプロパティに設定する
        entity.Department =
            await _departmentEntityAdapter.ConvertAsync(domain.Department);

        return entity;
    }

    /// <summary>
    /// EmployeeEntityの集約関係からドメインオブジェクト:Employeeを復元する
    /// </summary>
    /// <param name="target">EmployeeEntity</param>
    /// <returns>復元したEmployee</returns>
    public async Task<Employee> RestoreAsync(EmployeeEntity target)
    {
        // EmployeeEntityからEmployeeを復元する
        var employee = await _employeeEntityAdapter.RestoreAsync(target);

        // 部署が存在しない場合はリターンする
        if (target.Department is null)
        {
            return employee;
        }

        // DepartmentEntityからDepartmentを復元してプロパティに設定する
        employee.ChangeDepartment(
            await _departmentEntityAdapter.RestoreAsync(target.Department));

        return employee;
    }
}