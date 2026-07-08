using Microsoft.EntityFrameworkCore;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Contexts;
namespace A_exercise_DB.Infrastructures.Repositories;
/// <summary>
///  ドメインオブジェクト:商品カテゴリのCRUD操作インターフェイスの実装
/// </summary>
public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly AppDbContext _context;
    private readonly ProductCategoryEntityAdapter _adapter;
    /// <summary>
    /// コンストラクタ 
    /// </summary>
    /// <param name="context">アプリケーション用データベースコンテキスト</param>
    /// <param name="adapter">ドメインオブジェクト:ProductCategoryとProductCategoryEntityの相互変換クラス</param> 
    public ProductCategoryRepository(
        AppDbContext context,
        ProductCategoryEntityAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// すべての商品カテゴリを取得する
    /// </summary>
    /// <returns>ProductCategoryのリスト</returns>
    public async Task<List<ProductCategory>> FindAllAsync()
    {
        try
        {
            // すべての商品カテゴリを取得する
            var entities = await _context.ProductCategories
                .AsNoTracking().ToListAsync(); //追跡データをもらわない形でEntityが入ったリストで受けとる
            // ProductCategoryのリストを生成する
            var categories = new List<ProductCategory>();
            foreach (var entity in entities)
            {
                // ProductCategoryEntityからProductCategoryを復元する
                categories.Add(await _adapter.RestoreAsync(entity));
            }
            return categories;
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする（DBアクセス不可など）
            // 例外をもう一度,exとして投げることを「再スロー」という
            // DBにアクセスできない原因別に本当は違う例外を投げるけれど、それを全部設定するのは大変なのでラップしてスロー
            // ,ex を入れることで、開発側は何のエラーなのかを識別可能
            throw new InternalException("すべての商品カテゴリ取得時に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 指定された商品カテゴリIdの商品カテゴリを取得する
    /// </summary>
    /// <param name="id">商品カテゴリId</param>
    /// <returns>ProductCategory または null</returns>
    public async Task<ProductCategory?> FindByIdAsync(string id)
    {
        try
        {
            // 引数のUUIDで商品カテゴリを取得する
            var entity = await _context.ProductCategories
                .SingleOrDefaultAsync(c => c.CategoryUuid == Guid.Parse(id)); //見つかった1件を返す、見つからなかったらnullを返す
            if (entity is null)
            {
                return null; // 存在しない場合はnullを返す
            }
            // ProductCategoryEntityからProductCategoryを復元する
            var category = await _adapter.RestoreAsync(entity); // Entity→DomainObject
            return category;
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Id:{id}の商品カテゴリ取得時に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 指定された商品カテゴリ名の存在有無を返す
    /// </summary>
    /// <param name="name">商品カテゴリ名</param>
    /// <returns>true:存在する false:存在しない</returns> 
    public async Task<bool> ExistsByNameAsync(string name)
    {
        try
        {
            return await _context.ProductCategories
            .AsNoTracking()
            .AnyAsync(p => p.Name == name); //最低1件でも名前が完全一致するものがあればTure、なければFalseを返す
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Name:{name}の商品カテゴリ有無取得時に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 商品カテゴリを永続化する
    /// </summary>
    /// <param name="productCategory">永続化する商品カテゴリ</param>
    /// <returns>なし</returns>
    public async Task CreateAsync(ProductCategory productCategory)
    {
        try
        {
            // ProductCategoryをProductCategoryEntityに変換する
            var entity = await _adapter.ConvertAsync(productCategory);

            // 商品カテゴリを登録する
            await _context.ProductCategories.AddAsync(entity);

            // 登録した商品カテゴリをデータベースに永続化する
            await _context.SaveChangesAsync();
        }

        catch (Exception ex)
        {
            throw new InternalException("商品カテゴリの永続化中に予期しないエラーが発生しました。", ex);
        }
    }
}