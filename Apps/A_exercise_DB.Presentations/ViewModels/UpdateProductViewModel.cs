using System.ComponentModel.DataAnnotations;

namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// UC012 商品修正リクエスト用ViewModel
/// </summary>
public class UpdateProductViewModel
{
    /// <summary>
    /// 商品名
    /// </summary>
    [Required(ErrorMessage = "商品名を入力してください。")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "商品名は2～20文字で入力してください。")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 価格
    /// </summary>
    [Range(0, 1000000, ErrorMessage = "価格は0以上100万円以下で入力してください。")]
    public int Price { get; set; }

    /// <summary>
    /// 在庫数
    /// </summary>
    [Range(0, 1000, ErrorMessage = "在庫数は0以上1000個以下で入力してください。")]
    public int StockQuantity { get; set; }

    /// <summary>
    /// 商品カテゴリ識別ID(UUID)
    /// </summary>
    [Required(ErrorMessage = "カテゴリを選択してください。")]
    public string CategoryUuid { get; set; } = string.Empty;

    /// <summary>
    /// 差し替える商品画像。
    /// 未指定の場合は既存画像を維持する。
    /// </summary>
    public IFormFile? Image { get; set; }
}
