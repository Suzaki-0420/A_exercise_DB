using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Repositories;
/// <summary>
///  ドメインオブジェクト:商品のCRUD操作インターフェイス
/// </summary>
public interface IProductRepository
{

    /// <summary>
    /// すべての商品を取得する
    /// </summary>
    /// <returns>商品のリスト</returns>
    Task<List<Product>> FindAllAsync();

    /// <summary>
    /// 商品UUIDを指定して商品を取得する
    /// </summary>
    /// <param name="productUuid">商品UUID</param>
    /// <returns>該当商品。存在しない場合はnull</returns>
    Task<Product?> FindByIdAsync(Guid productUuid);

    /// <summary>
    /// 商品を永続化する
    /// </summary>
    /// <param name="product">永続化する商品</param>
    /// <returns>なし</returns>
    Task CreateAsync(Product product);

    /// <summary>
    /// 指定した商品カテゴリに属する商品情報をリストで返す
    /// 削除フラグを立てたものが表示されないように.Whereで条件付けしてます
    /// </summary>
    /// <param name="productCategoryId">商品カテゴリID</param>
    /// <returns>Productのリスト</returns>
    Task<List<Product>> SelectByProductCategoryIdAsync(Guid productCategoryUuid, bool showDeletedOnly);

    /// <summary>
    /// 指定されたキーワードを商品名に含む商品情報を取得する
    /// </summary>
    /// <param name="keyword">検索キーワード</param>
    /// <returns>Productのリスト</returns>
    Task<List<Product>> SearchKeywordAsync(string keyword, bool showDeletedOnly);

    /// <summary>
    /// 商品を更新する
    /// </summary>
    /// <param name="product">更新対象の商品</param>
    /// <returns>true:更新成功 false:更新失敗</returns>
    Task<bool> UpdateByIdAsync(Product product);

    /// <summary>
    /// 商品を削除する
    /// </summary>
    /// <param name="id">削除対象の商品Id(UUID)</param>
    /// <returns>true:削除成功 false:削除失敗</returns>
    Task<bool> DeleteByIdAsync(string id);

    /// <summary>
    /// 指定された商品名の存在有無を返す
    /// </summary>
    /// <param name="name">商品名</param>
    /// <returns>true:存在する false:存在しない</returns> 
    Task<bool> ExistsByNameAsync(string name);
}