using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
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
public class OrdersRepositoryTests
{
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider _provider = null!;

    private IServiceScope _scope = null!;
    private IOrdersRepository _repository = null!;
    private AppDbContext _dbContext = null!;

    private readonly Guid _customerUuid =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly Guid _orderUuid =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

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
            _scope.ServiceProvider.GetRequiredService<IOrdersRepository>();

        _dbContext =
            _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Seed();
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
            new DateTime(2026, 7, 9),
            null);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(_orderUuid, result[0].OrderUuid);
    }

    [TestMethod(DisplayName = "顧客名で購入履歴を検索できる")]
    public async Task SearchByDateOrNameAsync_WhenCustomerNameSpecified_ShouldReturnOrders()
    {
        var result = await _repository.SearchByDateOrNameAsync(
            null,
            "山田");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("山田 太郎", result[0].Customer.Name);
    }

    [TestMethod(DisplayName = "購入日と顧客名で購入履歴を検索できる")]
    public async Task SearchByDateOrNameAsync_WhenOrderDateAndCustomerNameSpecified_ShouldReturnOrders()
    {
        var result = await _repository.SearchByDateOrNameAsync(
            new DateTime(2026, 7, 9),
            "山田");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(_orderUuid, result[0].OrderUuid);
    }

    [TestMethod(DisplayName = "条件なしで購入履歴を全件取得できる")]
    public async Task SearchByDateOrNameAsync_WhenNoConditionSpecified_ShouldReturnAllOrders()
    {
        var result = await _repository.SearchByDateOrNameAsync(null, null);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod(DisplayName = "検索条件に一致しない場合は空リストを返す")]
    public async Task SearchByDateOrNameAsync_WhenNoMatchedOrders_ShouldReturnEmptyList()
    {
        var result = await _repository.SearchByDateOrNameAsync(
            new DateTime(2099, 1, 1),
            "存在しない顧客");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod(DisplayName = "購入履歴検索中にDB接続エラーが発生した場合InternalExceptionが発生する")]
    public async Task SearchByDateOrNameAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        _dbContext.Dispose();

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.SearchByDateOrNameAsync(null, null);
        });
    }

    [TestMethod(DisplayName = "注文ステータスを正常に変更できる")]
    public async Task ChangeStatusAsync_WhenValidOrder_ShouldChangeStatus()
    {
        var order = CreateOrderDomain(2);

        var result = await _repository.ChangeStatusAsync(order);

        Assert.IsTrue(result);

        var saved = await _dbContext.Orders
            .SingleAsync(o => o.OrderUuid == _orderUuid);

        Assert.AreEqual(2, saved.OrderStatusId);
    }

    [TestMethod(DisplayName = "指定されたUUIDの注文が存在しない場合falseを返す")]
    public async Task ChangeStatusAsync_WhenOrderDoesNotExist_ShouldReturnFalse()
    {
        var order = CreateOrderDomain(
            1,
            Guid.Parse("99999999-9999-9999-9999-999999999999"));

        var result = await _repository.ChangeStatusAsync(order);

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "指定された注文ステータスが存在しない場合falseを返す")]
    public async Task ChangeStatusAsync_WhenOrderStatusDoesNotExist_ShouldReturnFalse()
    {
        var order = CreateOrderDomain(999);

        var result = await _repository.ChangeStatusAsync(order);

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "注文ステータス変更中にDB接続エラーが発生した場合InternalExceptionが発生する")]
    public async Task ChangeStatusAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var order = CreateOrderDomain(1);

        _dbContext.Dispose();

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.ChangeStatusAsync(order);
        });
    }

    private void Seed()
    {
        if (_dbContext.Orders.Any(o => o.OrderUuid == _orderUuid))
        {
            return;
        }

        var customer = new CustomerEntity
        {
            CustomerUuid = _customerUuid,
            Name = "山田 太郎",
            Kana = "ヤマダ タロウ",
            Address1 = "東京都",
            Address2 = "新宿区",
            PhoneNumber = "090-1234-5678",
            MailAddress = "yamada@example.com",
            Username = "yamada_taro",
            Password = "password",
            CreatedAt = new DateTime(2026, 7, 1)
        };

        var orderStatus1 = new OrderStatusEntity
        {
            Id = 1,
            Name = "受付"
        };

        var orderStatus2 = new OrderStatusEntity
        {
            Id = 2,
            Name = "発送済み"
        };

        var paymentMethod = new PaymentMethodEntity
        {
            Id = 1,
            Name = "クレジットカード"
        };

        var productCategory = new ProductCategoryEntity
        {
            Id = 1,
            Name = "水族館グッズ"
        };

        var product = new ProductEntity
        {
            ProductUuid = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "ペンギンぬいぐるみ",
            Price = 1000,
            ProductCategory = productCategory,
            ProductCategoryId = productCategory.Id
        };

        var order = new OrdersEntity
        {
            OrderUuid = _orderUuid,
            OrderDate = new DateTime(2026, 7, 9, 10, 30, 0),
            AmountTotal = 1000,
            Customer = customer,
            OrderStatus = orderStatus1,
            OrderStatusId = orderStatus1.Id,
            PaymentMethod = paymentMethod,
            PaymentMethodId = paymentMethod.Id,
            OrderDetails = new List<OrdersDetailEntity>
            {
                new OrdersDetailEntity
                {
                    Id = 1,
                    Count = 1,
                    Product = product,
                    ProductId = product.Id
                }
            }
        };

        _dbContext.Customers.Add(customer);
        _dbContext.OrderStatuses.AddRange(orderStatus1, orderStatus2);
        _dbContext.PaymentMethods.Add(paymentMethod);
        _dbContext.ProductCategories.Add(productCategory);
        _dbContext.Products.Add(product);
        _dbContext.Orders.Add(order);

        _dbContext.SaveChanges();
    }

    private Orders CreateOrderDomain(int orderStatusId, Guid? orderUuid = null)
    {
        var customer = new Customer(
            _customerUuid,
            "山田 太郎",
            "ヤマダ タロウ",
            "東京都",
            "新宿区",
            "090-1234-5678",
            "yamada@example.com",
            "yamada_taro",
            "password",
            new DateTime(2026, 7, 1));

        var orderStatus = new OrderStatus(
            orderStatusId,
            "変更後ステータス");

        var paymentMethod = new PaymentMethod(
            1,
            "クレジットカード");

        var orderDetails = new List<OrdersDetail>();

        return new Orders(
            orderUuid ?? _orderUuid,
            new DateTime(2026, 7, 9, 10, 30, 0),
            1000,
            customer,
            orderStatus,
            paymentMethod,
            orderDetails);
    }
}