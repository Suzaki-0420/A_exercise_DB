using A_Exercise_DB.Domains.Exceptions;
namespace A_Exercise_DB.Domains.Models;
/// <summary>
/// 社員を表すドメインオブジェクト
/// </summary>
public class Employee
{
    public Guid? EmployeeUuid { get; private set; } // 社員Id
    public string EmployeeName { get; private set; } = string.Empty; // 氏名
    public string EmployeeKana { get; private set; } = string.Empty; // カナ氏名
    public Department? Department { get; private set; } // 所属部門（null可）

    // 社員名の最大長
    private const int MaxLengthName = 100;
    // 社員カナの最大長
    private const int MaxLengthKana = 100;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public Employee(Guid? empId, string empName, string empKana, Department? department)
    {
        ValidateUuid(empId);
        EmployeeUuid = empId;
        ValidateEmpName(empName);
        EmployeeName = empName;
        ValidateEmpKana(empKana);
        EmployeeKana = empKana;
        Department = department;
    }

    /// <summary>
    /// ID未定の社員を作成する場合のコンストラクタ
    /// </summary>
    public Employee(string empName, string empKana, Department? department)
        : this(null, empName, empKana, department) { }

    /// <summary>
    /// 社員UUIDの検証
    /// </summary>
    private void ValidateEmployeeUuid(Guid? employeeUuid)
    {
        if (employeeUuid == null)
            throw new DomainException("社員UUIDは必須です");

        if (employeeUuid == Guid.Empty)
            throw new DomainException("社員UUIDが不正です");
    }

    /// <summary>
    /// 氏名の検証
    /// </summary>
    private void ValidateEmpName(string empName)
    {
        if (string.IsNullOrWhiteSpace(empName))
            throw new DomainException("社員名は必須です");
        if (empName.Length > MaxLengthName)
            throw new DomainException($"社員名は{MaxLengthName}文字以内で入力してください");
    }

    /// <summary>
    /// カナ氏名の検証
    /// </summary>
    private void ValidateEmpKana(string empKana)
    {
        if (string.IsNullOrWhiteSpace(empKana))
            throw new DomainException("社員名カナは必須です");
        if (empKana.Length > MaxLengthKana)
            throw new DomainException($"社員名カナは{MaxLengthKana}文字以内で入力してください");
    }

    /// <summary>
    /// 氏名を変更する
    /// </summary>
    public void ChangeEmpName(string empName)
    {
        ValidateEmpName(empName);
        EmployeeName = empName;
    }

    /// <summary>
    /// メールアドレスを変更する
    /// </summary>
    public void changeEmpKana(string empKana)
    {
        ValidateEmpKana(empKana);
        EmployeeKana = empKana;
    }

    /// <summary>
    /// 所属部門を変更する
    /// </summary>
    public void ChangeDepartment(Department? department)
    {
        Department = department;
    }

    /// <summary>
    /// 等価性（IDによる比較）
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not Employee other) return false;
        return EmpId == other.EmpId;
    }

    public override int GetHashCode() => EmpId?.GetHashCode() ?? 0;

    public override string ToString()
        => $"{EmpId?.ToString() ?? "未登録"}: {EmpName},{EmpMailadress},{EmpPhonenumber} / {Department?.Name ?? "未配属"}";
}