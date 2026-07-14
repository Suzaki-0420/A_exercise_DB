using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Applications.Params;
namespace A_exercise_DB.Applications.Usecases.Products;
/// <summary>
/// ユースケース:[新商品を登録する]を実現するインターフェイス
/// </summary>
public interface IRegisterProductUsecase
{
    /// <summary>
    /// すべてのカテゴリーを取得する
    /// クライアント側の[入力画面]で利用するプルダウンを作成するため
    /// </summary>
    /// <returns>Departmentのリスト</returns>
    Task<List<ProductCategory>> GetCategoriesAsync();
    /// <summary>
    /// 指定されたカテゴリーIdのカテゴリーを取得する
    /// クライアント側の[確認画面]、[完了画面]で利用するため
    /// </summary>
    /// <param name="id">部署Id</param>
    /// <returns>該当部署</returns>
    /// <exception cref="NotFoundException">該当データが存在しない場合にスローされる</exception>
    Task<ProductCategory> GetCategoryByIdAsync(string id);


    Task<Boolean> ExistsByProductNameAsync(string productName);
    /// <summary>
    /// 新商品を登録する
    /// </summary>
    /// <param name="product">登録対象商品</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">カテゴリーが存在しない場合にスローされる</exception>
    Task RegisterProductAsync(Product product);

    /// <summary>
    /// 画像を含む商品を登録する
    /// </summary>
    /// <param name="param">商品登録の入力値</param>
    /// <returns>登録した商品</returns>
    Task<Product> RegisterProductAsync(
        ProductRegisterParam param);
}