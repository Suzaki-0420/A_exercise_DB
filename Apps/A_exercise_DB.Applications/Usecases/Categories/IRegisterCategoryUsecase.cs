using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Applications.Usecases.Categories;

/// <summary>
/// 商品カテゴリ登録ユースケース
/// </summary>
public interface IRegisterCateogryUsecase
{
    /// <summary>
    /// 指定された商品カテゴリの存在有無を調べる
    /// </summary>
    /// <param name="categoryName">商品カテゴリ</param>
    /// <returns>なし</returns>
    /// <exception cref="ExistsException">同一商品カテゴリ名が存在する場合にスローされる</exception>
    Task ExistsByCategoryAsync(string categoryName);

    /// <summary>
    /// 新商品カテゴリを登録する
    /// </summary>
    /// <param name="category">登録対象商品カテゴリ</param>
    /// <returns>なし</returns>
    /// <exception cref="NotFoundException">商品カテゴリが存在しない場合にスローされる</exception>
    Task RegisterCategoryAsync(ProductCategory category);
}