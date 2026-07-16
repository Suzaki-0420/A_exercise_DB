namespace A_exercise_DB.Applications.Usecases.Products;

/// <summary>
/// 商品削除確認に表示する商品情報
/// </summary>
public sealed record ProductDeleteConfirmationResult(
    Guid ProductUuid,
    string ProductName,
    int Price,
    int Quantity,
    string CategoryName,
    string? ImageUrl);
