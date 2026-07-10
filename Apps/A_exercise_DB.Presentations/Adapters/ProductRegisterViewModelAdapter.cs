using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Presentations.ViewModels;
namespace A_exercise_DB.Presentations.Adapters;
/// <summary>
/// RegisterProductViewModelからドメインオブジェクト:Productへ変換するアダプタ
/// </summary>
public class ProductRegisterViewModelAdapter : IRestorer<Product, RegisterViewModel>
{
    /// <summary>
    /// RegisterProductViewModelからドメインオブジェクト:Productを復元する
    /// </summary>
    /// <param name="target">ユースケース:[新商品を登録する]を実現するViewModel</param>
    /// <returns></returns>
    public Task<Product> RestoreAsync(RegisterViewModel target)
    {
        // 商品カテゴリを生成する
        var category = new ProductCategory(target.CategoryUuid ?? Guid.Empty, target.CategoryName ?? string.Empty);
        // 商品在庫を生成する
        var productStock = new ProductStock(target.Stock);
        // 商品を生成する   
        var product = new Product(Guid.NewGuid(), target.Name ?? string.Empty, target.Price);
        // 商品カテゴリと商品在庫を設定する
        product.ChangeCategory(category);
        product.ChangeStock(productStock);
        return Task.FromResult(product);
    }
}