using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Applications.Interfaces;
using A_exercise_DB.Applications.Params;

namespace A_exercise_DB.Applications.Usecases.Products;

/// <summary>
/// 商品修正ユースケース実装
/// </summary>
public class UpdateProductUsecase : IUpdateProductUsecase
{
    private const int MinProductNameLength = 2;
    private const int MaxProductNameLength = 20;
    private const int MaxPrice = 1000000;
    private const int MaxStockQuantity = 1000;

    private readonly IProductRepository _productRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品Repository</param>
    /// <param name="productCategoryRepository">商品カテゴリRepository</param>
    /// <param name="unitOfWork">トランザクション制御</param>
    public UpdateProductUsecase(
        IProductRepository productRepository,
        IProductCategoryRepository productCategoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<ProductUpdateCompleteResult> UpdateAsync(
        string productUuid,
        ProductUpdateRequest request)
    {
        var parsedProductUuid = ValidateProductUuid(productUuid);
        var parsedCategoryUuid = ValidateRequest(request);

        string? newImageUrl = null;
        string? oldImageUrl = null;

        await _unitOfWork.BeginAsync();

        try
        {
            var category = await _productCategoryRepository.FindByIdAsync(
                parsedCategoryUuid.ToString());

            if (category is null)
            {
                throw new NotFoundException("指定された商品カテゴリが見つかりません。");
            }

            var product = new Product(
                parsedProductUuid,
                request.Name,
                request.Price,
                request.ImageUrl ?? string.Empty,
                category,
                new ProductStock(request.StockQuantity),
                0);

            var updated = await _productRepository.UpdateByIdAsync(product);

            if (!updated)
            {
                throw new NotFoundException("更新対象の商品が見つかりません。");
            }

            await _unitOfWork.CommitAsync();

            return ProductUpdateCompleteResult.CreateUpdated(
                parsedProductUuid,
                request,
                parsedCategoryUuid);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// 商品UUIDを検証する
    /// </summary>
    private static Guid ValidateProductUuid(string productUuid)
    {
        if (!Guid.TryParse(productUuid, out var parsedProductUuid) ||
            parsedProductUuid == Guid.Empty)
        {
            throw new DomainException("商品識別IDが不正です。");
        }

        return parsedProductUuid;
    }

    /// <summary>
    /// 商品修正リクエストを検証する
    /// </summary>
    private static Guid ValidateRequest(ProductUpdateRequest request)
    {
        if (request is null)
        {
            throw new DomainException("商品修正情報を入力してください。");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new DomainException("商品名を入力してください。");
        }

        if (request.Name.Length < MinProductNameLength ||
            request.Name.Length > MaxProductNameLength)
        {
            throw new DomainException("商品名は2～20文字で入力してください。");
        }

        if (request.Price < 0)
        {
            throw new DomainException("価格は0以上で入力してください。");
        }

        if (request.Price > MaxPrice)
        {
            throw new DomainException("価格は100万円以下で入力してください。");
        }

        if (request.StockQuantity < 0)
        {
            throw new DomainException("在庫数は0以上で入力してください。");
        }

        if (request.StockQuantity > MaxStockQuantity)
        {
            throw new DomainException("在庫数は1000個以下で入力してください。");
        }

        if (!Guid.TryParse(request.CategoryUuid, out var parsedCategoryUuid) ||
            parsedCategoryUuid == Guid.Empty)
        {
            throw new DomainException("カテゴリを選択してください。");
        }

        return parsedCategoryUuid;
    }

}
