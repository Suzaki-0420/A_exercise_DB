using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;

namespace A_exercise_DB.Applications.Usecases.Products;

public class SearchProductByCategoryUsecase : ISearchProductByCategoryUsecase
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="repository">商品CRUD操作リポジトリ</param>
    private readonly IProductRepository _repository;

    public SearchProductByCategoryUsecase(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 指定されたカテゴリーで商品を部分一致検索した結果を返す
    /// </summary>
    /// <param name="productCategoryId">商品カテゴリー</param>
    /// <returns>カテゴリー検索結果</returns>
    // <exception cref="NotFoundException">該当データが存在しない場合にスローされる</exception>
    public async Task<List<Product>> ExecuteAsync(Guid productCategoryId, bool showDeletedOnly)
    {
        var result = await _repository
            .SelectByProductCategoryIdAsync(productCategoryId, showDeletedOnly);
        return result;
    }
}