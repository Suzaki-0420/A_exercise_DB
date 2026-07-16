namespace A_exercise_DB.Applications.Usecases.Products;

/// <summary>
/// 商品削除ユースケース
/// </summary>
public interface IDeleteProductUsecase
{
    /// <summary>
    /// 指定された商品UUIDの商品を論理削除する
    /// </summary>
    /// <param name="productUuid">商品識別ID(UUID)</param>
    /// <returns>商品削除完了結果</returns>
    Task<ProductDeleteCompleteResult> DeleteAsync(string productUuid);
}
