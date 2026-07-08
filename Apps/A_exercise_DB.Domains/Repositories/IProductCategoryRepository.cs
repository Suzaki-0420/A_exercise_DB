using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Repositories;
/// <summary>
///  ドメインオブジェクト:商品カテゴリのCRUD操作インターフェイスの実装
/// </summary>
public interface IProductCategoryRepository
{
    /// <summary>
    /// すべての商品カテゴリを取得する
    /// </summary>
    /// <returns>ProductCategoryのリスト</returns>
    Task<List<ProductCategory>> FindAllAsync();

    /// <summary>
    /// 指定された商品カテゴリIdの商品カテゴリを取得する
    /// </summary>
    /// <param name="id">商品カテゴリId</param>
    /// <returns>ProductCategory または null</returns>
    Task<ProductCategory?> FindByIdAsync(string id);

    /// <summary>
    /// 指定された商品カテゴリ名の存在有無を返す
    /// </summary>
    /// <param name="name">商品カテゴリ名</param>
    /// <returns>true:存在する false:存在しない</returns> 
    Task<bool> ExistsByNameAsync(string name);

    /// <summary>
    /// 商品カテゴリを永続化する
    /// </summary>
    /// <param name="productCategory">永続化する商品カテゴリ</param>
    /// <returns>なし</returns>
    Task CreateAsync(ProductCategory productCategory);
}