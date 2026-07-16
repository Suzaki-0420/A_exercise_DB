using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.ViewModels;
namespace A_exercise_DB.Presentations.Adapters;
/// <summary>
/// UC014: ViewModelからドメインオブジェクトへ変換するアダプタ
/// </summary>
public class RegisterCategoryViewModelAdapter : IRestorer<ProductCategory, RegisterCategoryViewModel>
{
    /// <summary>
    /// RegisterCategoryViewModelからドメインオブジェクトCategoryを復元する
    /// </summary>
    /// <param name="target">UC014: 新規商品カテゴリ登録を実現するViewModel</param>
    /// <returns></returns>
    public Task<ProductCategory> RestoreAsync(RegisterCategoryViewModel target)
    {
        // 商品カテゴリを生成する
        var category = new ProductCategory(Guid.NewGuid(), target.CategoryName);
        return Task.FromResult(category);
    }
}