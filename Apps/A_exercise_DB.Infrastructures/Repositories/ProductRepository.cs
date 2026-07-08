using Microsoft.EntityFrameworkCore;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Contexts;
namespace A_exercise_DB.Infrastructures.Repositories;
/// <summary>
///  ドメインオブジェクト:商品のCRUD操作インターフェイスの実装
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    private readonly ProductFactory _factory;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">アプリケーション用データベースコンテキスト</param>
    /// <param name="factory">商品、商品カテゴリ、商品在庫オブジェクトの相互変換Factoryクラス</param>
    public ProductRepository(AppDbContext context, ProductFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// 商品を永続化する
    /// </summary>
    /// <param name="product">永続化する商品</param>
    /// <returns>なし</returns>
    public async Task CreateAsync(Product product)
    {
        try
        {
            // 登録する商品の商品カテゴリを取得する
            var category = await _context.ProductCategories
                .SingleOrDefaultAsync(c => c.CategoryUuid == product.ProductCategory!.CategoryUuid);
            if (category is null)
            {
                throw new Exception($"Id:{product.ProductCategory!.CategoryUuid}の商品カテゴリは存在しません。");
            }
            // ProductをProductEntityに変換する
            var entity = await _factory.ConvertAsync(product);
            // 商品カテゴリの外部キーを設定する
            entity.ProductCategory = null!;
            entity.ProductCategoryId = category.Id;
            // 商品を登録する
            await _context.Products.AddAsync(entity); //プロパティにデータを足しただけ（DBには反映されていない）
            // 登録した商品をデータベースに永続化する
            await _context.SaveChangesAsync(); //ここではじめてDBに反映される
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException("商品の永続化中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 指定した商品カテゴリに属する商品情報をリストで返す
    /// 削除フラグを立てたものが表示されないように.Whereで条件付けしてます
    /// </summary>
    /// <param name="productCategoryId">商品カテゴリID</param>
    /// <returns>Productのリスト</returns>
    public async Task<List<Product>> SelectByProductCategoryIdAsync(int productCategoryId)
    {
        try
        {
            // 引数の商品カテゴリIDで商品と在庫・カテゴリを取得する
            var entities = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductStock)
                .Include(p => p.ProductCategory)
                .Where(p => p.ProductCategoryId == productCategoryId && p.DeleteFlg == 0)
                .ToListAsync();

            // List<ProductEntity>からList<Product>を復元する
            var products = await _factory.RestoreAsync(entities);

            return products;
        }
        catch (Exception ex)
        {
            throw new InternalException(
                $"商品カテゴリID:{productCategoryId}の商品取得時に予期しないエラーが発生しました。",
                ex
            );
        }
    }

    /// <summary>
    /// 商品を更新する
    /// </summary>
    /// <param name="product">更新対象の商品</param>
    /// <returns>true:更新成功 false:更新失敗</returns>
    public async Task<bool> UpdateByIdAsync(Product product)
    {
        try
        {
            var entity = await _context.Products
            .Include(p => p.ProductStock)
            .SingleOrDefaultAsync(p => p.ProductUuid == product.ProductUuid && p.DeleteFlg == 0);//UUIDが等しいものを1件取得する
            if (entity is null)
            {
                return false;
            }

            var category = await _context.ProductCategories
                .SingleOrDefaultAsync(c => c.CategoryUuid == product.ProductCategory!.CategoryUuid);
            // 商品名と単価を変更する
            entity.Name = product.Name; //引数で渡したDomain Objectの名前に変更する
            entity.Price = product.Price; //引数で渡したDomain Objectの値段に変更する
            entity.ProductCategoryId = category!.Id;
            // 在庫数を変更する
            entity.ProductStock!.Quantity = product.ProductStock!.Quantity;
            // 変更データをデータベースに永続化する
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Id:{product.ProductUuid}の商品変更中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 商品を削除する
    /// </summary>
    /// <param name="id">削除対象の商品Id(UUID)</param>
    /// <returns>true:削除成功 false:削除失敗</returns>
    public async Task<bool> DeleteByIdAsync(string id)
    {
        try
        {
            // 削除対象の商品を取得する
            // 指定したid (引数) と同じidを持つエンティティを取得する（見つからなければnull）
            var entity = await _context.Products.SingleOrDefaultAsync(p => p.ProductUuid == Guid.Parse(id));
            if (entity is null)
            {
                return false; // 該当商品が存在しない場合はfalseを返す
            }
            // 商品を削除する
            entity.DeleteFlg = 1;
            // ここではじめて削除結果がデータベースに反映される
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Id:{id}の商品削除中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 指定された商品名の存在有無を返す
    /// </summary>
    /// <param name="name">商品名</param>
    /// <returns>true:存在する false:存在しない</returns> 
    public async Task<bool> ExistsByNameAsync(string name)
    {
        try
        {
            return await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Name == name); //最低1件でも名前が完全一致するものがあればTure、なければFalseを返す
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Name:{name}の商品有無取得時に予期しないエラーが発生しました。", ex);
        }
    }
}