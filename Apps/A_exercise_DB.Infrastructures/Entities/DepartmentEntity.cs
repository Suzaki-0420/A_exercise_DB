using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace A_exercise_DB.Infrastructures.Entities;

/// <summary>
/// 部署テーブルのEntity
/// </summary>
[Table("department")]

public class DepartmentEntity
{
    /// <summary>
    /// 部署ID（主キー）
    /// </summary>
    [Column("id")]
    [Key] // 主キーをマッピング
    public int Id { get; set; }

    /// <summary>
    /// 部署識別ID（UUID）
    /// </summary>
    [Column("department_uuid")]
    public Guid DepartmentUuid { get; set; }

    /// <summary>
    /// 部署名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 所属社員一覧
    /// </summary>
    public List<EmployeeEntity> Employees { get; set; } = new();

    public override string ToString()
    {
        return $"Id={Id},DepartmentUuid={DepartmentUuid},Name={Name}";
    }
}