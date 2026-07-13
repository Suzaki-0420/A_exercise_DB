using A_exercise_DB.Domains.Models;
namespace A_exercise_DB.Applications.Usecases.Products;
/// <summary>
/// ユースケース:[商品をキーワード検索する]を実現するインターフェイス
/// </summary>
public interface ISearchProductByKeywordUsecase
{
    /// <summary>
    /// 指定されたキーワードで商品を部分一致検索した結果を返す
    /// </summary>
    /// <param name="keyword">商品キーワード</param>
    /// <returns>キーワード検索結果</returns>
    Task<List<Product>> ExecuteAsync(string keyword, bool showDeletedOnly);
}