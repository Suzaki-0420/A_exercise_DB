using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace A_exercise_DB.Infrastructures.Entities;

/// <summary>
/// 社員アカウントテーブルのEntity
/// </summary>
[Table("employee_account")]
public class EmployeeAccountEntity
{
    /// <summary>
    /// 社員アカウントID（主キー）
    /// </summary>
    [Column("id")]
    [Key] // 主キーをマッピング
    public int Id { get; set; }

    /// <summary>
    /// 社員アカウント識別ID（UUID）
    /// </summary>
    [Column("account_uuid")]
    public Guid AccountUuid { get; set; }

    /// <summary>
    /// ユーザー名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 社員ID（外部キー）
    /// </summary>
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    /// <summary>
    /// 社員
    /// </summary>
    [ForeignKey("EmployeeId")]
    public EmployeeEntity Employee { get; set; } = null!;

    public override string ToString()
    {
        return $"Id={Id},AccountUuid={AccountUuid},Name={Name},Password={Password},EmployeeId={EmployeeId}";
    }
}