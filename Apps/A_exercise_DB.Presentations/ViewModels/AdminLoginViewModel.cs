using System.ComponentModel.DataAnnotations;

namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// UC017 担当者ログインリクエスト用ViewModel
/// </summary>
public class AdminLoginViewModel
{
    /// <summary>
    /// アカウント名
    /// </summary>
    [Required(ErrorMessage = "アカウント名を入力してください。")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "アカウント名は5～20文字で入力してください。")]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "アカウント名は半角英数字で入力してください。")]
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    [Required(ErrorMessage = "パスワードを入力してください。")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "パスワードは5～20文字で入力してください。")]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "パスワードは半角英数字で入力してください。")]
    public string Password { get; set; } = string.Empty;
}
