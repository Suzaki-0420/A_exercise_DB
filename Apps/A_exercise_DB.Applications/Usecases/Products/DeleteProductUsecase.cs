using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Repositories;

namespace A_exercise_DB.Applications.Usecases.Products;

/// <summary>
/// 商品削除ユースケース実装
/// </summary>
public class DeleteProductUsecase : IDeleteProductUsecase
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品Repository</param>
    /// <param name="unitOfWork">トランザクション制御</param>
    public DeleteProductUsecase(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<ProductDeleteCompleteResult> DeleteAsync(string productUuid)
    {
        if (!Guid.TryParse(productUuid, out var parsedProductUuid) ||
            parsedProductUuid == Guid.Empty)
        {
            throw new DomainException("商品識別IDが不正です。");
        }

        await _unitOfWork.BeginAsync();

        try
        {
            var deleted = await _productRepository.DeleteByIdAsync(
                parsedProductUuid.ToString());

            if (!deleted)
            {
                throw new NotFoundException("削除対象の商品が見つかりません。");
            }

            await _unitOfWork.CommitAsync();
            return ProductDeleteCompleteResult.CreateDeleted(parsedProductUuid);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
