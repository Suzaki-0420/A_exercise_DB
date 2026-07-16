namespace A_exercise_DB.Applications.Usecases.Products;

/// <summary>
/// 商品修正リクエスト
/// </summary>
public sealed record ProductUpdateRequest(
    string Name,
    int Price,
    int StockQuantity,
    string CategoryUuid,
    string? ImageUrl,
    Stream? ImageContent = null,
    string? ImageFileName = null,
    string? ImageContentType = null,
    long ImageLength = 0);
