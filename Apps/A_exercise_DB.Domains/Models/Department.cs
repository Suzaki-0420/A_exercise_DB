using A_exercise_DB.Domains.Exceptions;
namespace A_exercise_DB.Domains.Models;
/// <summary>
/// 部署を表すドメインオブジェクト
/// </summary>
public class Department
{
    /// <summary>
    /// 部署識別ID（UUID）
    /// </summary>
    public Guid DepartmentUuid { get; private set; }
    /// <summary>
    /// 部署名
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 部署名の最大長
    /// </summary>
    private const int MaxLengthDeptName = 100;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public Department(Guid deptUuid, string deptName)
    {
        ValidateDepartmentUuid(deptUuid);
        DepartmentUuid = deptUuid;
        ValidateDeptName(deptName);
        Name = deptName;
    }

    /// <summary>
    /// ID未定の部署を作成する場合のコンストラクタ
    /// </summary>
    public Department(string deptName)
        : this(Guid.NewGuid(), deptName) { }

    /// <summary>
    /// 部署識別IDの検証
    /// </summary>
    private void ValidateDepartmentUuid(Guid? deptUuid)
    {
        if (deptUuid == Guid.Empty)
            throw new DomainException("部署識別IDが不正です");
    }

    /// <summary>
    /// 部署名の検証
    /// </summary>
    private void ValidateDeptName(string deptName)
    {
        if (string.IsNullOrWhiteSpace(deptName))
            throw new DomainException("部署名は必須です");
        if (deptName.Length > MaxLengthDeptName)
            throw new DomainException($"部署名は{MaxLengthDeptName}文字以内で入力してください");
    }

    /// <summary>
    /// 等価性（IDによる比較）
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not Department other) return false;
        return DepartmentUuid == other.DepartmentUuid;
    }

    public override int GetHashCode() => DepartmentUuid.GetHashCode();

    public override string ToString()
        => $"{DepartmentUuid}: {Name}";
}