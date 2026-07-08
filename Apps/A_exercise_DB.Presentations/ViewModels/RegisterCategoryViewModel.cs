using System.ComponentModel.DataAnnotations;
namespace A_exercise_DB.Presentations.ViewModels;
/// <summary>
/// UC014: 新規商品カテゴリ登録
/// </summary>
public class RegisterCategoryViewModel
{
    /// <summary>
    /// 商品カテゴリ名
    /// テーブル一覧のVARCHAR(30)を参考に入力文字制限を30字にした
    /// </summary>
    [Required(ErrorMessage = "商品カテゴリ名は必須です。")]
    [StringLength(30, ErrorMessage = "商品カテゴリ名は{1}文字以内で入力してください。")]
    public string CategoryName { get; set; } = string.Empty;
}