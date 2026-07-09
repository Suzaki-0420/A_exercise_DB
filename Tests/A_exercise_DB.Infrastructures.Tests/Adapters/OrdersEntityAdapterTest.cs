using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructures.Tests.Adapters;

[TestClass]
public class OrdersEntityAdapterTests
{
    private OrdersEntityAdapter _adapter = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _adapter = new OrdersEntityAdapter();
    }

    [TestMethod]
    public async Task ConvertAsync_OrdersをOrdersEntityに変換できる()
    {
        // Arrange
        var customer = new Customer(
            Guid.NewGuid(),
            "山田太郎",
            "ヤマダタロウ",
            "東京都新宿区",
            "1-2-3",
            "090-1234-5678",
            "test@example.com",
            "yamada",
            "password",
            new DateTime(2026, 7, 9, 10, 0, 0)
        );

        var orderStatus = new OrderStatus(
            1,
            "注文受付"
        );

        var paymentMethod = new PaymentMethod(
            2,
            "クレジットカード"
        );

        var details = new List<OrdersDetail>
        {
            new OrdersDetail(3),
            new OrdersDetail(5)
        };

        var orders = new Orders(
            Guid.NewGuid(),
            new DateTime(2026, 7, 9, 12, 0, 0),
            5000,
            customer,
            orderStatus,
            paymentMethod,
            details
        );

        // Act
        var result = await _adapter.ConvertAsync(orders);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(orders.OrderUuid, result.OrderUuid);
        Assert.AreEqual(orders.OrderDate, result.OrderDate);
        Assert.AreEqual(orders.AmountTotal, result.AmountTotal);
        Assert.AreEqual(orders.OrderStatus.Id, result.OrderStatusId);
        Assert.AreEqual(orders.PaymentMethod.Id, result.PaymentMethodId);
    }

    [TestMethod]
    public async Task ConvertAsync_引数がnullの場合InternalExceptionをthrowする()
    {
        // Act & Assert
        var ex = await Assert.ThrowsExactlyAsync<InternalException>(
            async () => await _adapter.ConvertAsync(null!)
        );

        Assert.AreEqual("引数domainがnullです。", ex.Message);
    }

    [TestMethod]
    public async Task RestoreAsync_OrdersEntityからOrdersを復元できる()
    {
        // Arrange
        var customerEntity = new CustomerEntity
        {
            CustomerUuid = Guid.NewGuid(),
            Name = "山田太郎",
            Kana = "ヤマダタロウ",
            Address1 = "東京都新宿区",
            Address2 = "1-2-3",
            PhoneNumber = "090-1234-5678",
            MailAddress = "test@example.com",
            Username = "yamada",
            Password = "password",
            CreatedAt = new DateTime(2026, 7, 9, 10, 0, 0)
        };

        var orderStatusEntity = new OrderStatusEntity
        {
            Id = 1,
            Name = "注文受付"
        };

        var paymentMethodEntity = new PaymentMethodEntity
        {
            Id = 2,
            Name = "クレジットカード"
        };

        var detailEntity1 = new OrdersDetailEntity
        {
            Count = 3
        };

        var detailEntity2 = new OrdersDetailEntity
        {
            Count = 5
        };

        var entity = new OrdersEntity
        {
            OrderUuid = Guid.NewGuid(),
            OrderDate = new DateTime(2026, 7, 9, 12, 0, 0),
            AmountTotal = 5000,
            Customer = customerEntity,
            OrderStatus = orderStatusEntity,
            PaymentMethod = paymentMethodEntity,
            OrderDetails = new List<OrdersDetailEntity>
            {
                detailEntity1,
                detailEntity2
            }
        };

        // Act
        var result = await _adapter.RestoreAsync(entity);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(entity.OrderUuid, result.OrderUuid);
        Assert.AreEqual(entity.OrderDate, result.OrderDate);
        Assert.AreEqual(entity.AmountTotal, result.AmountTotal);

        Assert.IsNotNull(result.Customer);
        Assert.AreEqual(customerEntity.CustomerUuid, result.Customer.CustomerUuid);
        Assert.AreEqual(customerEntity.Name, result.Customer.Name);
        Assert.AreEqual(customerEntity.Kana, result.Customer.Kana);
        Assert.AreEqual(customerEntity.Address1, result.Customer.Address1);
        Assert.AreEqual(customerEntity.Address2, result.Customer.Address2);
        Assert.AreEqual(customerEntity.PhoneNumber, result.Customer.PhoneNumber);
        Assert.AreEqual(customerEntity.MailAddress, result.Customer.MailAddress);
        Assert.AreEqual(customerEntity.Username, result.Customer.Username);
        Assert.AreEqual(customerEntity.Password, result.Customer.Password);
        Assert.AreEqual(customerEntity.CreatedAt, result.Customer.CreatedAt);

        Assert.IsNotNull(result.OrderStatus);
        Assert.AreEqual(orderStatusEntity.Id, result.OrderStatus.Id);
        Assert.AreEqual(orderStatusEntity.Name, result.OrderStatus.Name);

        Assert.IsNotNull(result.PaymentMethod);
        Assert.AreEqual(paymentMethodEntity.Id, result.PaymentMethod.Id);
        Assert.AreEqual(paymentMethodEntity.Name, result.PaymentMethod.Name);

        Assert.IsNotNull(result.OrdersDetails);
        Assert.AreEqual(2, result.OrdersDetails.Count);
        Assert.AreEqual(detailEntity1.Count, result.OrdersDetails[0].Count);
        Assert.AreEqual(detailEntity2.Count, result.OrdersDetails[1].Count);
    }

    [TestMethod]
    public async Task RestoreAsync_注文明細が空のOrdersEntityからOrdersを復元できる()
    {
        // Arrange
        var entity = new OrdersEntity
        {
            OrderUuid = Guid.NewGuid(),
            OrderDate = new DateTime(2026, 7, 9, 12, 0, 0),
            AmountTotal = 5000,
            Customer = new CustomerEntity
            {
                CustomerUuid = Guid.NewGuid(),
                Name = "山田太郎",
                Kana = "ヤマダタロウ",
                Address1 = "東京都新宿区",
                Address2 = "1-2-3",
                PhoneNumber = "090-1234-5678",
                MailAddress = "test@example.com",
                Username = "yamada",
                Password = "password",
                CreatedAt = new DateTime(2026, 7, 9, 10, 0, 0)
            },
            OrderStatus = new OrderStatusEntity
            {
                Id = 1,
                Name = "注文受付"
            },
            PaymentMethod = new PaymentMethodEntity
            {
                Id = 2,
                Name = "クレジットカード"
            },
            OrderDetails = new List<OrdersDetailEntity>()
        };

        // Act
        var result = await _adapter.RestoreAsync(entity);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(entity.OrderUuid, result.OrderUuid);
        Assert.AreEqual(entity.OrderDate, result.OrderDate);
        Assert.AreEqual(entity.AmountTotal, result.AmountTotal);
        Assert.IsNotNull(result.OrdersDetails);
        Assert.AreEqual(0, result.OrdersDetails.Count);
    }

    [TestMethod]
    public async Task RestoreAsync_引数がnullの場合InternalExceptionをthrowする()
    {
        // Act & Assert
        var ex = await Assert.ThrowsExactlyAsync<InternalException>(
            async () => await _adapter.RestoreAsync(null!)
        );

        Assert.AreEqual("引数targetがnullです。", ex.Message);
    }
}