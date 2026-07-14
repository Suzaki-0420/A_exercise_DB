namespace A_exercise_DB.Applications.Params;

/// <summary>
/// 商品更新ユースケースの入力値
/// </summary>
public sealed record ProductUpdateParam(
    Guid ProductUuid,
    string Name,
    int Price,
    Guid ProductCategoryUuid,
    int StockQuantity,
    Stream? ImageContent = null,
    string? ImageFileName = null,
    string? ImageContentType = null,
    long ImageLength = 0);