using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Repositories;

namespace A_exercise_DB.Applications.Usecases.Categories;

/// <summary>
/// 商品カテゴリ登録ユースケース
/// </summary>
public class RegisterCategoryUsecase : IRegisterCateogryUsecase
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productCategoryRepository">商品カテゴリのCRUD操作リポジトリ</param>
    /// <param name="unitOfWork">トランザクション制御機能</param>
    public RegisterCategoryUsecase(
        IProductCategoryRepository productCategoryRepository,
        IUnitOfWork unitOfWork
    )
    {
        _productCategoryRepository = productCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 指定された商品カテゴリの存在有無を調べる
    /// </summary>
    /// <param name="categoryName">商品カテゴリ</param>
    /// <returns>なし</returns>
    /// <exception cref="ExistsException">同一商品カテゴリ名が存在する場合にスローされる</exception>
    public async Task ExistsByCategoryAsync(string categoryName)
    {
        var result = await _productCategoryRepository.ExistsByNameAsync(categoryName);
        if (result)
        {
            throw new ExistsException($"カテゴリ名：{categoryName}");
        }
    }

    /// <summary>
    /// 新商品カテゴリを登録する
    /// </summary>
    /// <param name="category">登録対象商品カテゴリ</param>
    /// <returns>なし</returns>
    /// <exception cref="NotFoundException">商品カテゴリが存在しない場合にスローされる</exception>
    public async Task RegisterCategoryAsync(ProductCategory productCategory)
    {
        await _unitOfWork.BeginAsync();
        try
        {
            await _productCategoryRepository.CreateAsync(productCategory);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}