using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace A_exercise_DB.Presentations.ViewModels;
/// <summary>
/// ユースケース:[新商品を登録する]を実現するViewModel
/// </summary>
public class RegisterViewModel
{
    // 商品名
    [Required(ErrorMessage = "商品名は必須です。")]
    [StringLength(30, ErrorMessage = "商品名は{1}文字以内で入力してください。")]
    public string Name { get; set; } = string.Empty;
    // 単価
    [Required(ErrorMessage = "単価は必須です。")]
    [Range(0, int.MaxValue, ErrorMessage = "単価は0以上の整数を指定してください。")]
    public int Price { get; set; }
    // 在庫数
    [Required(ErrorMessage = "在庫数は必須です。")]
    [Range(0, int.MaxValue, ErrorMessage = "在庫数は0以上の整数を指定してください。")]
    public int Stock { get; set; }
    // 商品カテゴリId(UUID)
    [Required(ErrorMessage = "商品カテゴリIdは必須です。")]
    [RegularExpression(
    "^[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}$",
    ErrorMessage = "商品カテゴリIdはUUID形式で指定してください。")]
    public Guid? CategoryUuid { get; set; }
    // 商品カテゴリ名
    [Required(ErrorMessage = "商品カテゴリ名は必須です。")]
    [StringLength(20, ErrorMessage = "商品名は{1}文字以内で入力してください。")]
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 商品画像
    /// </summary>
    [Required(ErrorMessage = "商品画像を選択してください。")]
    public required IFormFile Image { get; set; }
}