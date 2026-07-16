namespace A_exercise_DB.Presentations.ViewModels;

/// <summary>
/// 注文ステータス選択肢ViewModel
/// </summary>
public class OrderStatusOptionViewModel
{
    /// <summary>
    /// 注文ステータスID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 注文ステータス名
    /// </summary>
    public string Name { get; set; } = string.Empty;
}