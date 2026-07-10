using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Repositories;
using System.Reflection.Metadata.Ecma335;
namespace A_exercise_DB.Applications.Usecases.Products;
/// <summary>
/// ユースケース:[従業員を登録する]を実現するインターフェイスの実装
/// </summary>
public class RegisterProductUsecase : IRegisterProductUsecase
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productCategoryRepository">ドメインオブジェクト:productCategory(部署)のCRUD操作インターフェイス</param>
    /// <param name="productRepository">ドメインオブジェクト:product(従業員)のCRUD操作インターフェイス</param>
    /// <param name="unitOfWork">Unit of Workパターンを利用したトランザクション制御インターフェイス</param>
    public RegisterProductUsecase(
        IProductCategoryRepository productCategoryRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// すべてのカテゴリーを取得する
    /// クライアント側の[入力画面]で利用するプルダウンを作成するため
    /// </summary>
    /// <returns>productCategoryのリスト</returns>
    public async Task<List<ProductCategory>> GetCategoriesAsync()
    {
        var results = await _productCategoryRepository.FindAllAsync();
        return results;
    }

    /// <summary>
    /// 指定された部署Idの部署を取得する
    /// クライアント側の[確認画面]、[完了画面]で利用するため
    /// </summary>
    /// <param name="id">部署Id</param>
    /// <returns>該当部署</returns>
    /// <exception cref="NotFoundException">該当データが存在しない場合にスローされる</exception>
    public async Task<ProductCategory> GetCategoryByIdAsync(string id)
    {
        var result = await _productCategoryRepository.FindByIdAsync(id);
        if (result == null)
        {
            throw new NotFoundException(
                $"カテゴリーId:{id}に一致するカテゴリーは存在しません。");
        }
        return result;
    }

    /// <summary>
    /// 従業員を登録する
    /// </summary>
    /// <param name="product">登録対象従業員</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">所属部署が存在しない場合にスローされる</exception>
    public async Task<bool> ExistsByProductNameAsync(string name)
    {
        var result = await _productRepository.ExistsByNameAsync(name);
        return result;
    }
    public async Task RegisterProductAsync(Product product)
    {
        await _unitOfWork.BeginAsync();
        try
        {
            await _productRepository.CreateAsync(product);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
