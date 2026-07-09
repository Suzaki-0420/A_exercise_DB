namespace A_exercise_DB.Applications.Usecases.Products;

/// <summary>
/// 商品修正完了結果
/// </summary>
public sealed record ProductUpdateCompleteResult(
    Guid ProductUuid,
    string Name,
    int Price,
    int StockQuantity,
    Guid CategoryUuid,
    string? ImageUrl,
    bool Updated)
{
    /// <summary>
    /// 更新成功結果を生成する
    /// </summary>
    public static ProductUpdateCompleteResult CreateUpdated(
        Guid productUuid,
        ProductUpdateRequest request,
        Guid categoryUuid)
        => new(
            productUuid,
            request.Name,
            request.Price,
            request.StockQuantity,
            categoryUuid,
            request.ImageUrl,
            true);
}
