namespace A_exercise_DB.Applications.Usecases.Products;

/// <summary>
/// 商品修正ユースケース
/// </summary>
public interface IUpdateProductUsecase
{
    /// <summary>
    /// 指定された商品UUIDの商品情報を修正する
    /// </summary>
    /// <param name="productUuid">商品識別ID(UUID)</param>
    /// <param name="request">商品修正リクエスト</param>
    /// <returns>商品修正完了結果</returns>
    Task<ProductUpdateCompleteResult> UpdateAsync(
        string productUuid,
        ProductUpdateRequest request);
}
