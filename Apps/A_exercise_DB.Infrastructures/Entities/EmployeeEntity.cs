using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace A_exercise_DB.Infrastructures.Entities;

/// <summary>
/// 社員テーブルのEntity
/// </summary>
[Table("employee")]
public class EmployeeEntity
{
    /// <summary>
    /// 社員ID（主キー）
    /// </summary>
    [Column("id")]
    [Key] // 主キーをマッピング
    public int Id { get; set; }

    /// <summary>
    /// 社員識別ID（UUID）
    /// </summary>
    [Column("employee_uuid")]
    public Guid EmployeeUuid { get; set; }

    /// <summary>
    /// 社員名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 社員名カナ
    /// </summary>
    [Column("kana")]
    public string Kana { get; set; } = string.Empty;

    /// <summary>
    /// 部署ID（外部キー）
    /// </summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>
    /// 部署
    /// </summary>
    [ForeignKey("DepartmentId")]
    public DepartmentEntity Department { get; set; } = null!;

    /// <summary>
    /// 社員アカウント
    /// </summary>
    public EmployeeAccountEntity? EmployeeAccount { get; set; }

}