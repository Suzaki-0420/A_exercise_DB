using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Repositories;

namespace A_exercise_DB.Applications.Usecases.Categories;

/// <summary>
/// 商品カテゴリ登録ユースケース
/// </summary>
public class RegisterCategoryUsecase : IRegisterCategoryUsecase
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
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new DomainException("カテゴリ名を入力してください");
        }

        var exists = await _productCategoryRepository.ExistsByNameAsync(categoryName);

        if (exists)
        {
            throw new ExistsException("このカテゴリ名は既に登録されています");
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
        _ = productCategory ?? throw new InternalException("引数productCategoryがnullです。");

        await _unitOfWork.BeginAsync();

        var isCommitted = false;

        try
        {
            // 登録直前にも重複チェックを行う
            await ExistsByCategoryAsync(productCategory.Name);

            await _productCategoryRepository.CreateAsync(productCategory);

            await _unitOfWork.CommitAsync();

            isCommitted = true;
        }
        finally
        {
            if (!isCommitted)
            {
                await _unitOfWork.RollbackAsync();
            }
        }
    }
}