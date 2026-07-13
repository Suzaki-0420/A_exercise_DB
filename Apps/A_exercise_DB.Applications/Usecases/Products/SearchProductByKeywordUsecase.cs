
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;

namespace A_exercise_DB.Applications.Usecases.Products;
/// <summary>
/// ユースケース:[商品をキーワード検索する]を実現するインターフェイスの実装
/// </summary>
public class SearchProductByKeywordUsecase : ISearchProductByKeywordUsecase
{
    private readonly IProductRepository _repository;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="repository">商品CRUD操作リポジトリ</param>
    public SearchProductByKeywordUsecase(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 指定されたキーワードで商品を部分一致検索した結果を返す
    /// </summary>
    /// <param name="keyword">商品キーワード</param>
    /// <returns>キーワード検索結果</returns>
    /// <exception cref="NotFoundException">該当データが存在しない場合にスローされる</exception>
    public async Task<List<Product>> ExecuteAsync(string keyword, bool showDeletedOnly)
    {
        var result = await _repository
            .SearchKeywordAsync(keyword, showDeletedOnly);
        return result;
    }
}