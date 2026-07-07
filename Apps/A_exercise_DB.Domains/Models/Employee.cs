using A_exercise_DB.Domains.Exceptions;
namespace A_exercise_DB.Domains.Models;
/// <summary>
/// 社員を表すドメインオブジェクト
/// </summary>
public class Employee
{
    /// <summary>
    /// 社員識別ID(UUID)
    /// </summary>
    public Guid? EmployeeUuid { get; private set; }
    /// <summary>
    /// 社員名
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 社員カナ
    /// </summary>
    public string Kana { get; private set; } = string.Empty;
    /// <summary>
    /// 所属部署（null可）
    /// </summary>
    public Department? Department { get; private set; }

    /// <summary>
    /// 社員名の最大長
    /// </summary>
    private const int MaxLengthName = 100;
    /// <summary>
    /// 社員カナの最大長
    /// </summary>
    private const int MaxLengthKana = 100;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public Employee(Guid? empId, string empName, string empKana, Department? department)
    {
        ValidateEmpUuid(empId);
        EmployeeUuid = empId;
        ValidateEmpName(empName);
        Name = empName;
        ValidateEmpKana(empKana);
        Kana = empKana;
        Department = department;
    }

    /// <summary>
    /// ID未定の社員を作成する場合のコンストラクタ
    /// </summary>
    public Employee(string empName, string empKana, Department? department)
        : this(null, empName, empKana, department) { }

    /// <summary>
    /// 社員識別IDの検証
    /// </summary>
    private void ValidateEmpUuid(Guid? employeeUuid)
    {
        if (employeeUuid == Guid.Empty)
            throw new DomainException("社員識別IDが不正です");
    }

    /// <summary>
    /// 社員名の検証
    /// </summary>
    private void ValidateEmpName(string empName)
    {
        if (string.IsNullOrWhiteSpace(empName))
            throw new DomainException("社員名は必須です");
        if (empName.Length > MaxLengthName)
            throw new DomainException($"社員名は{MaxLengthName}文字以内で入力してください");
    }

    /// <summary>
    /// 社員名カナの検証
    /// </summary>
    private void ValidateEmpKana(string empKana)
    {
        if (string.IsNullOrWhiteSpace(empKana))
            throw new DomainException("社員名カナは必須です");
        if (empKana.Length > MaxLengthKana)
            throw new DomainException($"社員名カナは{MaxLengthKana}文字以内で入力してください");
    }

    /// <summary>
    /// 等価性（IDによる比較）
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not Employee other) return false;
        return EmployeeUuid == other.EmployeeUuid;
    }

    public override int GetHashCode() => EmployeeUuid?.GetHashCode() ?? 0;

    public override string ToString()
        => $"{EmployeeUuid?.ToString() ?? "未登録"}: {Name},{Kana} / {Department?.Name ?? "未配属"}";
}