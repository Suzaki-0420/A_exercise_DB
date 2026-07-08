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
[TestCategory("Repositories")]
public class ProductCategoryRepositoryTests
{
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider _provider = null!;

    private IServiceScope _scope = null!;
    private IProductCategoryRepository _repository = null!;
    private AppDbContext _dbContext = null!;

    private readonly Guid _categoryUuid =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private const string CategoryName = "テストカテゴリ";

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _provider.Dispose();
    }

    [TestInitialize]
    public void TestInit()
    {
        _scope = _provider.CreateScope();

        _repository =
            _scope.ServiceProvider.GetRequiredService<IProductCategoryRepository>();

        _dbContext =
            _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _scope.Dispose();
    }

    [TestMethod(DisplayName = "商品カテゴリをすべて取得できる")]
    public async Task FindAllAsync_WhenCategoriesExist_ShouldReturnCategoryList()
    {
        var categories = await _repository.FindAllAsync();

        Assert.IsNotNull(categories);
        Assert.IsInstanceOfType(categories, typeof(List<ProductCategory>));
        Assert.IsTrue(categories.Count > 0);
        Assert.IsTrue(categories.Any(c => c.CategoryUuid == _categoryUuid));
    }

    [TestMethod(DisplayName = "FindAllAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task FindAllAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=9999;Database=All_Exercise;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var adapter =
            _scope.ServiceProvider.GetRequiredService<ProductCategoryEntityAdapter>();

        var repository = new ProductCategoryRepository(context, adapter);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.FindAllAsync();
        });
    }

    [TestMethod(DisplayName = "存在する商品カテゴリUUIDで商品カテゴリを取得できる")]
    public async Task FindByIdAsync_WhenUuidExists_ShouldReturnCategory()
    {
        var category = await _repository.FindByIdAsync(_categoryUuid.ToString());

        Assert.IsNotNull(category);
        Assert.AreEqual(_categoryUuid, category.CategoryUuid);
        Assert.AreEqual(CategoryName, category.Name);
    }

    [TestMethod(DisplayName = "存在しない商品カテゴリUUIDの場合nullが返る")]
    public async Task FindByIdAsync_WhenUuidDoesNotExist_ShouldReturnNull()
    {
        var category = await _repository.FindByIdAsync(Guid.NewGuid().ToString());

        Assert.IsNull(category);
    }

    [TestMethod(DisplayName = "不正なUUID形式の場合InternalExceptionが発生する")]
    public async Task FindByIdAsync_WhenUuidIsInvalid_ShouldThrowInternalException()
    {
        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.FindByIdAsync("invalid-uuid");
        });
    }

    [TestMethod(DisplayName = "存在する商品カテゴリ名の場合trueが返る")]
    public async Task ExistsByNameAsync_WhenNameExists_ShouldReturnTrue()
    {
        var result = await _repository.ExistsByNameAsync(CategoryName);

        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "存在しない商品カテゴリ名の場合falseが返る")]
    public async Task ExistsByNameAsync_WhenNameDoesNotExist_ShouldReturnFalse()
    {
        var result = await _repository.ExistsByNameAsync("存在しないカテゴリ名");

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "ExistsByNameAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task ExistsByNameAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=9999;Database=All_Exercise;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var adapter =
            _scope.ServiceProvider.GetRequiredService<ProductCategoryEntityAdapter>();

        var repository = new ProductCategoryRepository(context, adapter);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.ExistsByNameAsync(CategoryName);
        });
    }

    [TestMethod(DisplayName = "商品カテゴリを正常に登録できる")]
    public async Task CreateAsync_WhenValidCategory_ShouldCreateCategory()
    {
        var categoryUuid = Guid.NewGuid();
        var categoryName = $"新規カテゴリ";

        var category = new ProductCategory(categoryUuid, categoryName);

        await _repository.CreateAsync(category);

        var saved = await _dbContext.ProductCategories
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.CategoryUuid == categoryUuid);

        Assert.IsNotNull(saved);
        Assert.AreEqual(categoryUuid, saved.CategoryUuid);
        Assert.AreEqual(categoryName, saved.Name);
    }

    [TestMethod(DisplayName = "商品カテゴリ名が空の場合DomainExceptionが発生する")]
    public async Task CreateAsync_WhenNameIsEmpty_ShouldThrowDomainException()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            var category = new ProductCategory(Guid.NewGuid(), "");

            await _repository.CreateAsync(category);
        });
    }

    [TestMethod(DisplayName = "CreateAsyncでDB接続エラー時にInternalExceptionが発生する")]
    public async Task CreateAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=9999;Database=All_Exercise;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var adapter =
            _scope.ServiceProvider.GetRequiredService<ProductCategoryEntityAdapter>();

        var repository = new ProductCategoryRepository(context, adapter);

        var category = new ProductCategory(
            Guid.NewGuid(),
            $"DBエラーカテゴリ");

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.CreateAsync(category);
        });
    }
}