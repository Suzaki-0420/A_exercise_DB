using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Applications.Params;
using System.Reflection.Metadata.Ecma335;
using A_exercise_DB.Applications.Interfaces;
namespace A_exercise_DB.Applications.Usecases.Products;
/// <summary>
/// ユースケース:[従業員を登録する]を実現するインターフェイスの実装
/// </summary>
public class RegisterProductUsecase : IRegisterProductUsecase
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IImageUploadUsecase _imageUploadUsecase;

    private readonly IImageStorage _imageStorage;
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
        IImageUploadUsecase imageUploadUsecase,
        IImageStorage imageStorage,
        IUnitOfWork unitOfWork)
    {
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
        _imageUploadUsecase = imageUploadUsecase;
        _imageStorage = imageStorage;
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

    /// <summary>
    /// 画像を含む商品を登録する
    /// </summary>
    public async Task<Product> RegisterProductAsync(
        ProductRegisterParam param)
    {
        ArgumentNullException.ThrowIfNull(param);

        string? imageUrl = null;

        /*
         * 画像はDBトランザクションで
         * ロールバックできないため、先に保存する。
         */
        if (param.ImageContent is not null)
        {
            if (string.IsNullOrWhiteSpace(
                    param.ImageFileName))
            {
                throw new DomainException(
                    "画像ファイル名が指定されていません。");
            }

            if (string.IsNullOrWhiteSpace(
                    param.ImageContentType))
            {
                throw new DomainException(
                    "画像のContent-Typeが指定されていません。");
            }

            imageUrl =
                await _imageUploadUsecase.ExecuteAsync(
                    new ImageUploadParam(
                        param.ImageContent,
                        param.ImageFileName,
                        param.ImageContentType,
                        param.ImageLength));
        }

        await _unitOfWork.BeginAsync();

        try
        {
            var category =
                await _productCategoryRepository
                    .FindByIdAsync(
                        param.CategoryId.ToString());

            if (category is null)
            {
                throw new NotFoundException(
                    $"カテゴリーId:{param.CategoryId}に一致するカテゴリーは存在しません。");
            }

            var stock = new ProductStock(
                Guid.NewGuid(),
                param.Quantity);

            var product = new Product(
                Guid.NewGuid(),
                param.Name,
                param.Price,
                imageUrl,
                category,
                stock,
                0);

            await _productRepository
                .CreateAsync(product);

            await _unitOfWork.CommitAsync();

            return product;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();

            /*
             * DB登録に失敗した場合、
             * 先に保存した画像を削除する。
             */
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                try
                {
                    await _imageStorage
                        .DeleteAsync(imageUrl);
                }
                catch
                {
                    /*
                     * 画像削除失敗によって、
                     * 元の商品登録例外を上書きしない。
                     *
                     * 後からILoggerで記録する。
                     */
                }
            }

            throw;
        }
    }
}
