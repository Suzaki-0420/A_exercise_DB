using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Infrastructures.Entities;
using A_exercise_DB.Presentations.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A_exercise_DB.Infrastructures.Tests.Repositories;

[TestClass]
[TestCategory("Repositories")]
public class OrdersRepositoryTests
{
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider _provider = null!;

    private IServiceScope _scope = null!;
    private IOrdersRepository _repository = null!;
    private AppDbContext _dbContext = null!;
    private OrdersFactory _factory = null!;

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
    public void TestInit()
    {
        _scope = _provider.CreateScope();

        _repository =
            _scope.ServiceProvider.GetRequiredService<IOrdersRepository>();

        _dbContext =
            _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _factory =
            _scope.ServiceProvider.GetRequiredService<OrdersFactory>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _scope.Dispose();
    }

    [TestMethod(DisplayName = "購入日で購入履歴を検索できる")]
    public async Task SearchByDateOrNameAsync_WhenOrderDateSpecified_ShouldReturnOrders()
    {
        var result = await _repository.SearchByDateOrNameAsync(
            new DateTime(2026, 7, 6),
            null);

        Assert.HasCount(1, result);
        Assert.AreEqual(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), result[0].OrderUuid);
    }

    [TestMethod(DisplayName = "顧客名で購入履歴を検索できる")]
    public async Task SearchByDateOrNameAsync_WhenCustomerNameSpecified_ShouldReturnOrders()
    {
        var result = await _repository.SearchByDateOrNameAsync(
            null,
            "taro");

        Assert.HasCount(2, result);
        Assert.AreEqual("山田 太郎", result[0].Customer.Name);
        Assert.AreEqual("山田 太郎", result[1].Customer.Name);
    }

    [TestMethod(DisplayName = "購入日と顧客名で購入履歴を検索できる")]
    public async Task SearchByDateOrNameAsync_WhenOrderDateAndCustomerNameSpecified_ShouldReturnOrders()
    {
        var result = await _repository.SearchByDateOrNameAsync(
            new DateTime(2026, 7, 1),
            "taro");

        Assert.HasCount(1, result);
        Assert.AreEqual(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), result[0].OrderUuid);
    }

    [TestMethod(DisplayName = "条件なしで購入履歴を全件取得できる")]
    public async Task SearchByDateOrNameAsync_WhenNoConditionSpecified_ShouldReturnAllOrders()
    {
        var result = await _repository.SearchByDateOrNameAsync(null, null);

        Assert.HasCount(5, result);
    }

    [TestMethod(DisplayName = "検索条件に一致しない場合は空リストを返す")]
    public async Task SearchByDateOrNameAsync_WhenNoMatchedOrders_ShouldReturnEmptyList()
    {
        var result = await _repository.SearchByDateOrNameAsync(
            new DateTime(2099, 1, 1),
            "存在しない顧客");

        Assert.IsEmpty(result);
    }

    [TestMethod(DisplayName = "購入履歴検索中にDB接続エラーが発生した場合InternalExceptionが発生する")]
    public async Task SearchByDateOrNameAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        // Arrange
        _dbContext.Dispose();

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.SearchByDateOrNameAsync(null, null);
        });

        // Assert
        Assert.AreEqual(
            "購入履歴検索中に予期しないエラーが発生しました。",
            exception.Message);
    }

    [TestMethod(DisplayName = "注文ステータスを正常に変更できる")]
    public async Task ChangeStatusAsync_WhenValidOrder_ShouldChangeStatus()
    {
        var targetOrderUuid =
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        var entity = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderStatus)
            .Include(o => o.PaymentMethod)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.ProductCategory)
            .SingleAsync(o => o.OrderUuid == targetOrderUuid);

        var order = await _factory.RestoreAsync(entity);

        order.ChangeOrderStatus(new OrderStatus(2, "発送済み"));

        var result = await _repository.ChangeStatusAsync(order);

        Assert.IsTrue(result);

        var saved = await _dbContext.Orders
            .SingleAsync(o => o.OrderUuid == targetOrderUuid);

        Assert.AreEqual(2, saved.OrderStatusId);
    }

    [TestMethod(DisplayName = "指定されたUUIDの注文が存在しない場合falseを返す")]
    public async Task ChangeStatusAsync_WhenOrderDoesNotExist_ShouldReturnFalse()
    {
        var targetOrderUuid =
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        var entity = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderStatus)
            .Include(o => o.PaymentMethod)
            .SingleAsync(o => o.OrderUuid == targetOrderUuid);

        var customer = new Customer(
            entity.Customer.CustomerUuid,
            entity.Customer.Name,
            entity.Customer.Kana,
            entity.Customer.Address1,
            entity.Customer.Address2,
            entity.Customer.PhoneNumber,
            entity.Customer.MailAddress,
            entity.Customer.Username,
            entity.Customer.Password,
            entity.Customer.CreatedAt);

        var orderStatus = new OrderStatus(
            entity.OrderStatus.Id,
            entity.OrderStatus.Name);

        var paymentMethod = new PaymentMethod(
            entity.PaymentMethod.Id,
            entity.PaymentMethod.Name);

        var order = new Orders(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            entity.OrderDate,
            entity.AmountTotal,
            customer,
            orderStatus,
            paymentMethod,
            new List<OrdersDetail>());

        var result = await _repository.ChangeStatusAsync(order);

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "指定された注文ステータスが存在しない場合falseを返す")]
    public async Task ChangeStatusAsync_WhenOrderStatusDoesNotExist_ShouldReturnFalse()
    {
        var entity = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderStatus)
            .Include(o => o.PaymentMethod)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.ProductCategory)
            .SingleAsync(o => o.OrderUuid == Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"));

        var order = await _factory.RestoreAsync(entity);

        order.ChangeOrderStatus(new OrderStatus(999, "存在しないステータス"));

        var result = await _repository.ChangeStatusAsync(order);

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "注文ステータス変更中にDB接続エラーが発生した場合InternalExceptionが発生する")]
    public async Task ChangeStatusAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        // Arrange
        var entity = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderStatus)
            .Include(o => o.PaymentMethod)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.ProductCategory)
            .SingleAsync(o => o.OrderUuid == Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"));

        var order = await _factory.RestoreAsync(entity);

        order.ChangeOrderStatus(new OrderStatus(2, "発送済み"));

        _dbContext.Dispose();

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.ChangeStatusAsync(order);
        });

        // Assert
        Assert.AreEqual(
            $"Id:{order.OrderUuid}の注文ステータス変更中に予期しないエラーが発生しました。",
            exception.Message);
    }

    [TestMethod(DisplayName = "指定された注文UUIDの注文を取得できる")]
    public async Task FindByUuidAsync_WhenOrderExists_ShouldReturnOrder()
    {
        // Arrange
        var orderUuid = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        // Act
        var result = await _repository.FindByUuidAsync(orderUuid);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(orderUuid, result.OrderUuid);
        Assert.IsNotNull(result.Customer);
        Assert.IsNotNull(result.OrderStatus);
        Assert.IsNotNull(result.PaymentMethod);
        Assert.IsNotNull(result.OrdersDetails);
    }

    [TestMethod(DisplayName = "指定された注文UUIDの注文が存在しない場合nullを返す")]
    public async Task FindByUuidAsync_WhenOrderDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var orderUuid = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var result = await _repository.FindByUuidAsync(orderUuid);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod(DisplayName = "注文取得中にDB接続エラーが発生した場合InternalExceptionが発生する")]
    public async Task FindByUuidAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        // Arrange
        var orderUuid = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        _dbContext.Dispose();

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.FindByUuidAsync(orderUuid);
        });

        // Assert
        Assert.AreEqual(
            $"注文UUID:{orderUuid}の注文取得中に予期しないエラーが発生しました。",
            exception.Message);
    }


}