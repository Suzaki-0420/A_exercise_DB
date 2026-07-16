namespace A_exercise_DB.Applications.Usecases.Products;

/// <summary>
/// 商品削除完了結果
/// </summary>
public sealed record ProductDeleteCompleteResult(
    Guid ProductUuid,
    bool Deleted)
{
    /// <summary>
    /// 削除成功結果を生成する
    /// </summary>
    public static ProductDeleteCompleteResult CreateDeleted(Guid productUuid)
        => new(productUuid, true);
}
