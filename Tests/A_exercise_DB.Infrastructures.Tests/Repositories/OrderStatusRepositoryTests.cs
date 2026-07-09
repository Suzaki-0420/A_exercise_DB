using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Infrastructures.Entities;
using A_exercise_DB.Presentations.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A_exercise_DB.Infrastructures.Tests.Repositories;

[TestClass]
[TestCategory("Repositories")]
public class OrderStatusRepositoryTests
{
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider _provider = null!;

    private IServiceScope _scope = null!;
    private IOrderStatusRepository _repository = null!;
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

        _repository =
            _scope.ServiceProvider.GetRequiredService<IOrderStatusRepository>();

        _dbContext =
            _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Seed();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _scope.Dispose();
    }

    [TestMethod(DisplayName = "注文ステータスを全件取得できる")]
    public async Task FindAllAsync_WhenOrderStatusesExist_ShouldReturnAllOrderStatuses()
    {
        var result = await _repository.FindAllAsync();

        Assert.IsNotNull(result);
        Assert.IsGreaterThanOrEqualTo(3, result.Count);

        Assert.IsTrue(result.Any(s => s.Id == 1));
        Assert.IsTrue(result.Any(s => s.Id == 2));
        Assert.IsTrue(result.Any(s => s.Id == 3));
    }

    [TestMethod(DisplayName = "指定されたIDの注文ステータスを取得できる")]
    public async Task FindByIdAsync_WhenOrderStatusExists_ShouldReturnOrderStatus()
    {
        var result = await _repository.FindByIdAsync(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("受付", result.Name);
    }

    [TestMethod(DisplayName = "指定されたIDの注文ステータスが存在しない場合nullを返す")]
    public async Task FindByIdAsync_WhenOrderStatusDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.FindByIdAsync(999);

        Assert.IsNull(result);
    }

    [TestMethod(DisplayName = "注文ステータス全件取得中にDB接続エラーが発生した場合InternalExceptionが発生する")]
    public async Task FindAllAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        _dbContext.Dispose();

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.FindAllAsync();
        });
    }

    [TestMethod(DisplayName = "注文ステータスID検索中にDB接続エラーが発生した場合InternalExceptionが発生する")]
    public async Task FindByIdAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        _dbContext.Dispose();

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.FindByIdAsync(1);
        });
    }

    private void Seed()
    {
        if (_dbContext.OrderStatuses.Any())
        {
            return;
        }

        _dbContext.OrderStatuses.AddRange(
            new OrderStatusEntity
            {
                Id = 1,
                Name = "受付"
            },
            new OrderStatusEntity
            {
                Id = 2,
                Name = "発送済み"
            },
            new OrderStatusEntity
            {
                Id = 3,
                Name = "キャンセル"
            });

        _dbContext.SaveChanges();
    }
}