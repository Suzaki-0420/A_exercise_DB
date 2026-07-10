using System.ComponentModel.DataAnnotations;

namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 担当者アカウント登録用ViewModel
/// </summary>
public class RegisterEmployeeAccountViewModel
{
    /// <summary>
    /// 社員UUID
    /// </summary>
    [Required(ErrorMessage = "社員名を選択してください")]
    public Guid? EmployeeUuid { get; set; }

    /// <summary>
    /// アカウント名
    /// </summary>
    [Required(ErrorMessage = "アカウント名を入力してください")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "アカウント名は5～20文字で入力してください")]
    [RegularExpression(
        "^(?!([a-zA-Z0-9])\\1+$)[a-zA-Z0-9]+$",
        ErrorMessage = "アカウント名は半角英数字で入力し、同じ文字のみの登録はできません")]
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    [Required(ErrorMessage = "パスワードを入力してください")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "パスワードは5～20文字で入力してください")]
    [RegularExpression(
        "^(?!([a-zA-Z0-9])\\1+$)[a-zA-Z0-9]+$",
        ErrorMessage = "パスワードは半角英数字で入力し、同じ文字のみの登録はできません")]
    public string Password { get; set; } = string.Empty;
}