using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Infrastructures.Repositories;
using A_exercise_DB.Presentations.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A_exercise_DB.Infrastructures.Tests.Repositories;

[TestClass]
// RepositoryのDBテストを並列実行しない
[DoNotParallelize]
[TestCategory("Repositories")]
public class ProductRepositoryTests
{
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider _provider = null!;

    private IServiceScope _scope = null!;
    private IProductRepository _repository = null!;
    private AppDbContext _dbContext = null!;

    private ProductCategory _stationeryCategory = null!;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(
                "appsettings.Test.json",
                optional: false)
            .AddJsonFile(
                "appsettings.Test.local.json",
                optional: true)
            .AddEnvironmentVariables()
            .Build();

        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _provider.Dispose();
    }

    [TestInitialize]
    public async Task TestInit()
    {
        _scope = _provider.CreateScope();

        _repository =
            _scope.ServiceProvider.GetRequiredService<IProductRepository>();

        _dbContext =
            _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var categoryEntity = await _dbContext.ProductCategories
            .AsNoTracking()
            .SingleAsync(c => c.Name == "文房具");

        _stationeryCategory = new ProductCategory(
            categoryEntity.CategoryUuid,
            categoryEntity.Name);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _scope.Dispose();
    }

    [TestMethod(DisplayName = "商品を正常に登録できる")]
    public async Task CreateAsync_WhenValidProduct_ShouldCreateProduct()
    {
        var productUuid = Guid.NewGuid();
        var stockUuid = Guid.NewGuid();

        var stock = new ProductStock(stockUuid, 10);

        var product = new Product(
            productUuid,
            "テスト商品",
            100,
            "https://example.com/test.png",
            _stationeryCategory,
            stock,
            0);

        await _repository.CreateAsync(product);

        var saved = await _dbContext.Products
            .Include(p => p.ProductStock)
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.ProductUuid == productUuid);

        Assert.IsNotNull(saved);
        Assert.AreEqual(productUuid, saved.ProductUuid);
        Assert.AreEqual("テスト商品", saved.Name);
        Assert.AreEqual(100, saved.Price);
        Assert.AreEqual(0, saved.DeleteFlg);
        Assert.IsNotNull(saved.ProductStock);
        Assert.AreEqual(10, saved.ProductStock.Quantity);

        // テストデータ削除
        var deleteTarget = await _dbContext.Products
            .Include(p => p.ProductStock)
            .SingleAsync(p => p.ProductUuid == productUuid);

        if (deleteTarget.ProductStock is not null)
        {
            _dbContext.ProductStocks.Remove(deleteTarget.ProductStock);
        }

        _dbContext.Products.Remove(deleteTarget);
        await _dbContext.SaveChangesAsync();
    }

    [TestMethod(DisplayName = "商品カテゴリが存在しない場合InternalExceptionが発生する")]
    public async Task CreateAsync_WhenCategoryDoesNotExist_ShouldThrowInternalException()
    {
        var notExistCategory = new ProductCategory(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            "存在なし");

        var stock = new ProductStock(Guid.NewGuid(), 10);

        var product = new Product(
            Guid.NewGuid(),
            "カテゴリなし商品",
            100,
            "https://example.com/test.png",
            notExistCategory,
            stock,
            0);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.CreateAsync(product);
        });
    }

    [TestMethod(DisplayName = "商品名が空の場合DomainExceptionが発生する")]
    public async Task CreateAsync_WhenProductNameIsEmpty_ShouldThrowDomainException()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            var stock = new ProductStock(Guid.NewGuid(), 10);

            var product = new Product(
                Guid.NewGuid(),
                "",
                100,
                "https://example.com/test.png",
                _stationeryCategory,
                stock,
                0);

            await _repository.CreateAsync(product);
        });
    }

    [TestMethod(DisplayName = "CreateAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task CreateAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=9999;Database=All_Exercise;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var factory =
            _scope.ServiceProvider.GetRequiredService<ProductFactory>();

        var repository = new ProductRepository(context, factory);

        var stock = new ProductStock(Guid.NewGuid(), 10);

        var product = new Product(
            Guid.NewGuid(),
            "DBテスト商品",
            100,
            "https://example.com/test.png",
            _stationeryCategory,
            stock,
            0);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.CreateAsync(product);
        });
    }

    [TestMethod(DisplayName = "指定カテゴリの商品一覧を取得できる")]
    public async Task SelectByProductCategoryIdAsync_WhenProductsExist_ShouldReturnProductList()
    {
        var categoryEntity = await _dbContext.ProductCategories
            .AsNoTracking()
            .SingleAsync(c => c.Name == "文房具");

        var products = await _repository.SelectByProductCategoryIdAsync(
        categoryEntity.CategoryUuid,
        false);

        Assert.IsNotNull(products);
        Assert.IsNotEmpty(products);
        Assert.IsTrue(products.All(p => p.ProductCategory is not null));
        Assert.IsTrue(products.All(p => p.ProductStock is not null));
        Assert.IsTrue(products.All(p => p.DeleteFlg == 0));
    }

    [TestMethod(DisplayName = "SelectByProductCategoryIdAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task SelectByProductCategoryIdAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=9999;Database=All_Exercise;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var factory =
            _scope.ServiceProvider.GetRequiredService<ProductFactory>();

        var repository = new ProductRepository(context, factory);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.SelectByProductCategoryIdAsync(
            Guid.NewGuid(),
            false);
        });
    }
    [TestMethod(DisplayName = "（削除済み）キーワードに一致する商品一覧を取得できる")]
    public async Task SearchKeywordAsync_WhenProductsExist_ShouldReturnProducts()
    {
        var products = await _repository.SearchKeywordAsync("ボールペン", true);

        Assert.IsNotNull(products);
        Assert.IsTrue(products.All(p => p.Name.Contains("ボールペン")));
        Assert.IsTrue(products.All(p => p.DeleteFlg == 1));
    }

    [TestMethod(DisplayName = "（売りもの）キーワードに一致する商品一覧を取得できる")]
    public async Task SearchKeywordAsync_WhenProductsExist_ShouldReturnProducts_false()
    {
        var products = await _repository.SearchKeywordAsync("ボールペン", false);

        Assert.IsNotNull(products);
        Assert.IsNotEmpty(products);
        Assert.IsTrue(products.All(p => p.Name.Contains("ボールペン")));
        Assert.IsTrue(products.All(p => p.DeleteFlg == 0));
    }

    [TestMethod(DisplayName = "(削除済み）キーワードに一致する商品が存在しない場合は空リストを返す")]
    public async Task SearchKeywordAsync_WhenProductsDoNotExist_ShouldReturnEmptyList()
    {
        var products = await _repository.SearchKeywordAsync("存在しない商品", true);

        Assert.IsNotNull(products);
        Assert.IsEmpty(products);
    }

    [TestMethod(DisplayName = "（売り物）キーワードに一致する商品が存在しない場合は空リストを返す")]
    public async Task SearchKeywordAsync_WhenProductsDoNotExist_ShouldReturnEmptyList_false()
    {
        var products = await _repository.SearchKeywordAsync("存在しない商品", false);

        Assert.IsNotNull(products);
        Assert.IsEmpty(products);
    }

    [TestMethod(DisplayName = "SearchKeywordAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task SearchKeywordAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=9999;Database=All_Exercise;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var factory =
            _scope.ServiceProvider.GetRequiredService<ProductFactory>();

        var repository = new ProductRepository(context, factory);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.SearchKeywordAsync("ボールペン", true);
        });
    }

    [TestMethod(DisplayName = "商品を正常に更新できる")]
    public async Task UpdateByIdAsync_WhenProductExists_ShouldReturnTrue()
    {
        // 登録用データを作成
        var productUuid = Guid.NewGuid();

        var stock = new ProductStock(
            Guid.NewGuid(),
            10);

        var product = new Product(
            productUuid,
            "更新前商品",
            100,
            "https://example.com/before.png",
            _stationeryCategory,
            stock,
            0);

        await _repository.CreateAsync(product);

        // 更新用データを作成
        var updatedStock = new ProductStock(
            stock.StockUuid,
            99);

        var updatedProduct = new Product(
            productUuid,
            "更新後商品",
            500,
            "https://example.com/after.png",
            _stationeryCategory,
            updatedStock,
            0);

        // 実行
        var result = await _repository.UpdateByIdAsync(updatedProduct);

        // 検証
        Assert.IsTrue(result);

        var saved = await _dbContext.Products
            .Include(p => p.ProductStock)
            .AsNoTracking()
            .SingleAsync(p => p.ProductUuid == productUuid);

        Assert.AreEqual("更新後商品", saved.Name);
        Assert.AreEqual(500, saved.Price);
        Assert.IsNotNull(saved.ProductStock);
        Assert.AreEqual(99, saved.ProductStock.Quantity);

        // クリーンアップ
        var deleteTarget = await _dbContext.Products
            .Include(p => p.ProductStock)
            .SingleAsync(p => p.ProductUuid == productUuid);

        if (deleteTarget.ProductStock is not null)
        {
            _dbContext.ProductStocks.Remove(deleteTarget.ProductStock);
        }

        _dbContext.Products.Remove(deleteTarget);

        await _dbContext.SaveChangesAsync();
    }

    [TestMethod(DisplayName = "存在しない商品を更新した場合falseが返る")]
    public async Task UpdateByIdAsync_WhenProductDoesNotExist_ShouldReturnFalse()
    {
        var stock = new ProductStock(Guid.NewGuid(), 10);

        var product = new Product(
            Guid.NewGuid(),
            "存在しない商品",
            100,
            "https://example.com/test.png",
            _stationeryCategory,
            stock,
            0);

        var result = await _repository.UpdateByIdAsync(product);

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "UpdateByIdAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task UpdateByIdAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=9999;Database=All_Exercise;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var factory =
            _scope.ServiceProvider.GetRequiredService<ProductFactory>();

        var repository = new ProductRepository(context, factory);

        var stock = new ProductStock(Guid.NewGuid(), 10);

        var product = new Product(
            Guid.NewGuid(),
            "DB更新商品",
            100,
            "https://example.com/test.png",
            _stationeryCategory,
            stock,
            0);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.UpdateByIdAsync(product);
        });
    }

    [TestMethod(DisplayName = "商品を正常に削除できる")]
    public async Task DeleteByIdAsync_WhenProductExists_ShouldReturnTrue()
    {
        var stock = new ProductStock(Guid.NewGuid(), 5);

        var product = new Product(
            Guid.NewGuid(),
            "削除テスト",
            100,
            "https://example.com/delete.png",
            _stationeryCategory,
            stock,
            0);

        await _repository.CreateAsync(product);

        var result = await _repository.DeleteByIdAsync(product.ProductUuid.ToString());

        Assert.IsTrue(result);

        var saved = await _dbContext.Products
            .AsNoTracking()
            .SingleAsync(p => p.ProductUuid == product.ProductUuid);

        Assert.AreEqual(1, saved.DeleteFlg);

        // クリーンアップ
        var deleteTarget = await _dbContext.Products
            .Include(p => p.ProductStock)
            .SingleAsync(p => p.ProductUuid == product.ProductUuid);

        if (deleteTarget.ProductStock is not null)
        {
            _dbContext.ProductStocks.Remove(deleteTarget.ProductStock);
        }

        _dbContext.Products.Remove(deleteTarget);

        await _dbContext.SaveChangesAsync();
    }

    [TestMethod(DisplayName = "存在しない商品を削除した場合falseが返る")]
    public async Task DeleteByIdAsync_WhenProductDoesNotExist_ShouldReturnFalse()
    {
        var result = await _repository.DeleteByIdAsync(Guid.NewGuid().ToString());

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "不正なUUIDで削除した場合InternalExceptionが発生する")]
    public async Task DeleteByIdAsync_WhenUuidIsInvalid_ShouldThrowInternalException()
    {
        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.DeleteByIdAsync("invalid-uuid");
        });
    }

    [TestMethod(DisplayName = "存在する商品名の場合trueが返る")]
    public async Task ExistsByNameAsync_WhenNameExists_ShouldReturnTrue()
    {
        var result = await _repository.ExistsByNameAsync("水性ボールペン(黒)");

        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "存在しない商品名の場合falseが返る")]
    public async Task ExistsByNameAsync_WhenNameDoesNotExist_ShouldReturnFalse()
    {
        var result = await _repository.ExistsByNameAsync("存在しない商品名");

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "ExistsByNameAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task ExistsByNameAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=9999;Database=All_Exercise;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var factory =
            _scope.ServiceProvider.GetRequiredService<ProductFactory>();

        var repository = new ProductRepository(context, factory);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.ExistsByNameAsync("水性ボールペン(黒)");
        });
    }

    [TestMethod(DisplayName = "削除済みの商品をカテゴリで取得できる")]
    public async Task SelectByProductCategoryIdAsync_WhenShowDeletedOnlyIsTrue_ShouldReturnDeletedProducts()
    {
        // Arrange
        var categoryEntity = await _dbContext.ProductCategories
            .AsNoTracking()
            .SingleAsync(c => c.Name == "文房具");

        // Act
        var products = await _repository.SelectByProductCategoryIdAsync(
            categoryEntity.CategoryUuid,
            true);

        // Assert
        Assert.IsNotNull(products);
        Assert.IsTrue(products.All(p => p.DeleteFlg == 1));
        Assert.IsTrue(products.All(
            p => p.ProductCategory is not null
                && p.ProductCategory.CategoryUuid == categoryEntity.CategoryUuid));
    }

    /// <summary>
    /// 商品一覧を正常に取得できる
    /// </summary>
    [TestMethod(DisplayName = "商品一覧を正常に取得できる")]
    public async Task FindAllAsync_WhenProductsExist_ShouldReturnProducts()
    {
        // Act
        var products = await _repository.FindAllAsync();

        // Assert
        Assert.IsNotNull(products);
        Assert.IsNotEmpty(products);

        Assert.IsTrue(
            products.All(p => p.ProductCategory is not null));

        Assert.IsTrue(
            products.All(p => p.ProductStock is not null));

        Assert.IsTrue(
            products.All(p => p.DeleteFlg == 0));
    }

    /// <summary>
    /// FindAllAsyncでDB接続エラーが発生した場合のテスト
    /// </summary>
    [TestMethod(
        DisplayName =
            "FindAllAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task FindAllAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        // Arrange
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(
                    "Host=localhost;" +
                    "Port=9999;" +
                    "Database=All_Exercise;" +
                    "Username=postgres;" +
                    "Password=postgres")
                .Options;

        await using var context =
            new AppDbContext(options);

        var factory =
            _scope.ServiceProvider
                .GetRequiredService<ProductFactory>();

        var repository =
            new ProductRepository(
                context,
                factory);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InternalException>(
            async () =>
            {
                await repository.FindAllAsync();
            });
    }

    [TestMethod(DisplayName = "存在する商品UUIDを指定した場合、商品を取得できる")]
    public async Task FindByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        var productUuid = Guid.NewGuid();
        var stockUuid = Guid.NewGuid();

        var stock = new ProductStock(
            stockUuid,
            10);

        var product = new Product(
            productUuid,
            "FindByIdテスト商品",
            100,
            "https://example.com/test.png",
            _stationeryCategory,
            stock,
            0);

        try
        {
            await _repository.CreateAsync(product);

            // Act
            var result =
                await _repository.FindByIdAsync(productUuid);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(
                productUuid,
                result.ProductUuid);

            Assert.AreEqual(
                "FindByIdテスト商品",
                result.Name);

            Assert.AreEqual(
                100,
                result.Price);

            Assert.AreEqual(
                "https://example.com/test.png",
                result.ImageUrl);

            Assert.IsNotNull(result.ProductCategory);

            Assert.AreEqual(
                _stationeryCategory.CategoryUuid,
                result.ProductCategory.CategoryUuid);

            Assert.IsNotNull(result.ProductStock);

            Assert.AreEqual(
                10,
                result.ProductStock.Quantity);
        }
        finally
        {
            var deleteTarget = await _dbContext.Products
                .Include(p => p.ProductStock)
                .SingleOrDefaultAsync(
                    p => p.ProductUuid == productUuid);

            if (deleteTarget is not null)
            {
                _dbContext.Products.Remove(deleteTarget);

                await _dbContext.SaveChangesAsync();
            }
        }
    }

    [TestMethod(DisplayName = "存在しない商品UUIDを指定した場合、nullを返す")]
    public async Task FindByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var productUuid = Guid.NewGuid();

        // Act
        var result =
            await _repository.FindByIdAsync(productUuid);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod(DisplayName = "FindByIdAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task FindByIdAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        // Arrange
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(
                    "Host=localhost;" +
                    "Port=9999;" +
                    "Database=All_Exercise;" +
                    "Username=postgres;" +
                    "Password=postgres")
                .Options;

        await using var context =
            new AppDbContext(options);

        var factory =
            _scope.ServiceProvider
                .GetRequiredService<ProductFactory>();

        var repository =
            new ProductRepository(
                context,
                factory);

        var productUuid = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InternalException>(
            async () =>
            {
                await repository.FindByIdAsync(productUuid);
            });
    }
}