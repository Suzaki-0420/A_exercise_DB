using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Infrastructures.Entities;
using A_exercise_DB.Presentations.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A_exercise_DB.Infrastructures.Tests.Contexts;

/// <summary>
/// アプリケーション用DbContextの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Contexts")]
public class AddDbContextTests
{
    // MSTest v4では public・非static のTestContextプロパティが必要
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider _provider = null!;
    private IServiceScope _scope = null!;
    private AppDbContext _dbContext = null!;

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
        _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _scope.Dispose();
    }

    [TestMethod(DisplayName = "データベース接続ができる")]
    public void DbConnect_ShouldSucceed()
    {
        try
        {
            _dbContext.Database.OpenConnection();

            TestContext.WriteLine("DB接続成功しました。");

            Assert.IsTrue(true);
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"例外が発生しました: {ex.Message}");
            TestContext.WriteLine($"スタックトレース:\n{ex.StackTrace}");

            Assert.Fail("接続に失敗しました。");
        }
        finally
        {
            _dbContext.Database.CloseConnection();
        }
    }

    [TestMethod(DisplayName = "DbSetプロパティにアクセスできる")]
    public void DbSet_Properties_ShouldBeAccessible()
    {
        Assert.IsNotNull(_dbContext.Products, "Products DbSet にアクセスできません。");
        Assert.IsNotNull(_dbContext.ProductCategories, "ProductCategories DbSet にアクセスできません。");
        Assert.IsNotNull(_dbContext.ProductStocks, "ProductStocks DbSet にアクセスできません。");

        Assert.IsInstanceOfType(_dbContext.Products, typeof(DbSet<ProductEntity>));
        Assert.IsInstanceOfType(_dbContext.ProductCategories, typeof(DbSet<ProductCategoryEntity>));
        Assert.IsInstanceOfType(_dbContext.ProductStocks, typeof(DbSet<ProductStockEntity>));

        try
        {
            _ = _dbContext.Products.Count();
            _ = _dbContext.ProductCategories.Count();
            _ = _dbContext.ProductStocks.Count();

            Assert.IsTrue(true);
        }
        catch (Exception ex)
        {
            Assert.Fail($"DbSetに対する基本的なクエリ実行に失敗: {ex.Message}");
        }
    }

    [TestMethod(
    DisplayName = "BuildAppProviderで追加のサービス設定を実行できる")]
    public void BuildAppProvider_WhenConfigureServicesIsSpecified_ShouldInvokeAction()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var configureServicesWasCalled = false;

        // Act
        using var provider =
            ApplicationDependencyExtensions.BuildAppProvider(
                config,
                services =>
                {
                    configureServicesWasCalled = true;
                    services.AddSingleton<TestService>();
                });

        // Assert
        Assert.IsTrue(configureServicesWasCalled);

        var service = provider.GetService<TestService>();

        Assert.IsNotNull(service);
    }
    private sealed class TestService
    {
    }
}