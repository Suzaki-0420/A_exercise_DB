using System.Reflection;
using System.Runtime.Serialization;
using A_exercise_DB.Applications.Usecases.Orders;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using Moq;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Applications.Tests.Usecases.Orders;

/// <summary>
/// SearchOrdersUsecaseの単体テスト
/// </summary>
[TestClass]
public class SearchOrdersUsecaseTest
{
    /// <summary>
    /// SearchAsyncで購入履歴を取得できること
    /// </summary>
    [TestMethod(DisplayName = "SearchAsync_購入日または顧客名で購入履歴を取得できる")]
    public async Task SearchAsync_ReturnsOrders()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var expected = new List<OrdersModel>();

        ordersRepositoryMock
            .Setup(x => x.SearchByDateOrNameAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expected);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await usecase.SearchAsync(
            new DateTime(2026, 7, 9),
            "山田");

        // Assert
        Assert.AreSame(expected, actual);

        ordersRepositoryMock.Verify(x => x.SearchByDateOrNameAsync(
            new DateTime(2026, 7, 9),
            "山田"), Times.Once);
    }

    /// <summary>
    /// SearchAsyncでDomainExceptionが発生した場合、そのまま再スローされること
    /// </summary>
    [TestMethod(DisplayName = "SearchAsync_DomainException発生時はそのまま再スローされる")]
    public async Task SearchAsync_WhenDomainExceptionThrown_ThrowsExactlyDomainException()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var expected = new DomainException("業務ルール違反です。");

        ordersRepositoryMock
            .Setup(x => x.SearchByDateOrNameAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<string?>()))
            .ThrowsAsync(expected);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await usecase.SearchAsync(null, "山田");
        });

        // Assert
        Assert.AreSame(expected, actual);
    }

    /// <summary>
    /// SearchAsyncでDomainException以外が発生した場合、InternalExceptionに変換されること
    /// </summary>
    [TestMethod(DisplayName = "SearchAsync_DomainException以外の例外発生時はInternalExceptionに変換される")]
    public async Task SearchAsync_WhenExceptionThrown_ThrowsExactlyInternalException()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var innerException = new InvalidOperationException("DB接続エラーです。");

        ordersRepositoryMock
            .Setup(x => x.SearchByDateOrNameAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<string?>()))
            .ThrowsAsync(innerException);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await usecase.SearchAsync(null, "山田");
        });

        // Assert
        Assert.AreEqual("注文情報の取得に失敗しました。", actual.Message);
        Assert.AreSame(innerException, actual.InnerException);
    }

    /// <summary>
    /// FindAllStatusAsyncで注文ステータス一覧を取得できること
    /// </summary>
    [TestMethod(DisplayName = "FindAllStatusAsync_注文ステータス一覧を取得できる")]
    public async Task FindAllStatusAsync_ReturnsOrderStatuses()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var expected = new List<OrderStatus>();

        orderStatusRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ReturnsAsync(expected);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await usecase.FindAllStatusAsync();

        // Assert
        Assert.AreSame(expected, actual);

        orderStatusRepositoryMock.Verify(x => x.FindAllAsync(), Times.Once);
    }

    /// <summary>
    /// FindAllStatusAsyncでDomainExceptionが発生した場合、そのまま再スローされること
    /// </summary>
    [TestMethod(DisplayName = "FindAllStatusAsync_DomainException発生時はそのまま再スローされる")]
    public async Task FindAllStatusAsync_WhenDomainExceptionThrown_ThrowsExactlyDomainException()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var expected = new DomainException("業務ルール違反です。");

        orderStatusRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ThrowsAsync(expected);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await usecase.FindAllStatusAsync();
        });

        // Assert
        Assert.AreSame(expected, actual);
    }

    /// <summary>
    /// FindAllStatusAsyncでDomainException以外が発生した場合、InternalExceptionに変換されること
    /// </summary>
    [TestMethod(DisplayName = "FindAllStatusAsync_DomainException以外の例外発生時はInternalExceptionに変換される")]
    public async Task FindAllStatusAsync_WhenExceptionThrown_ThrowsExactlyInternalException()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var innerException = new InvalidOperationException("DB接続エラーです。");

        orderStatusRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ThrowsAsync(innerException);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await usecase.FindAllStatusAsync();
        });

        // Assert
        Assert.AreEqual("注文ステータス一覧の取得に失敗しました。", actual.Message);
        Assert.AreSame(innerException, actual.InnerException);
    }

    /// <summary>
    /// ChangeStatusAsyncで引数orderがnullの場合、InternalExceptionに変換されること
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_orderがnullの場合はInternalExceptionに変換される")]
    public async Task ChangeStatusAsync_WhenOrderIsNull_ThrowsExactlyInternalException()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await usecase.ChangeStatusAsync(null!);
        });

        // Assert
        Assert.AreEqual("注文ステータスの変更に失敗しました。", actual.Message);
        Assert.IsInstanceOfType(actual.InnerException, typeof(InternalException));
        Assert.AreEqual("引数orderがnullです。", actual.InnerException!.Message);
    }

    /// <summary>
    /// ChangeStatusAsyncで注文ステータスがnullの場合、InternalExceptionに変換されること
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_注文ステータスがnullの場合はInternalExceptionに変換される")]
    public async Task ChangeStatusAsync_WhenOrderStatusIsNull_ThrowsExactlyInternalException()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var order = CreateOrder(null);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await usecase.ChangeStatusAsync(order);
        });

        // Assert
        Assert.AreEqual("注文ステータスの変更に失敗しました。", actual.Message);
        Assert.IsInstanceOfType(actual.InnerException, typeof(InternalException));
        Assert.AreEqual("注文ステータスが指定されていません。", actual.InnerException!.Message);
    }

    /// <summary>
    /// ChangeStatusAsyncで指定した注文ステータスが存在しない場合、falseを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_指定した注文ステータスが存在しない場合はfalseを返す")]
    public async Task ChangeStatusAsync_WhenOrderStatusNotFound_ReturnsFalse()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);

        orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ReturnsAsync((OrderStatus?)null);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await usecase.ChangeStatusAsync(order);

        // Assert
        Assert.IsFalse(actual);

        orderStatusRepositoryMock.Verify(x => x.FindByIdAsync(1), Times.Once);
        ordersRepositoryMock.Verify(x => x.ChangeStatusAsync(It.IsAny<OrdersModel>()), Times.Never);
    }

    /// <summary>
    /// ChangeStatusAsyncで注文ステータスを変更できること
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_注文ステータスを変更できる")]
    public async Task ChangeStatusAsync_WhenOrderStatusExists_ReturnsTrue()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);

        orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ReturnsAsync(orderStatus);

        ordersRepositoryMock
            .Setup(x => x.ChangeStatusAsync(order))
            .ReturnsAsync(true);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await usecase.ChangeStatusAsync(order);

        // Assert
        Assert.IsTrue(actual);

        orderStatusRepositoryMock.Verify(x => x.FindByIdAsync(1), Times.Once);
        ordersRepositoryMock.Verify(x => x.ChangeStatusAsync(order), Times.Once);
    }

    /// <summary>
    /// ChangeStatusAsyncで注文ステータス変更結果がfalseの場合、falseを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_注文ステータス変更結果がfalseの場合はfalseを返す")]
    public async Task ChangeStatusAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);

        orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ReturnsAsync(orderStatus);

        ordersRepositoryMock
            .Setup(x => x.ChangeStatusAsync(order))
            .ReturnsAsync(false);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await usecase.ChangeStatusAsync(order);

        // Assert
        Assert.IsFalse(actual);

        orderStatusRepositoryMock.Verify(x => x.FindByIdAsync(1), Times.Once);
        ordersRepositoryMock.Verify(x => x.ChangeStatusAsync(order), Times.Once);
    }

    /// <summary>
    /// ChangeStatusAsyncでDomainExceptionが発生した場合、そのまま再スローされること
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_DomainException発生時はそのまま再スローされる")]
    public async Task ChangeStatusAsync_WhenDomainExceptionThrown_ThrowsExactlyDomainException()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);
        var expected = new DomainException("業務ルール違反です。");

        orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ThrowsAsync(expected);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await usecase.ChangeStatusAsync(order);
        });

        // Assert
        Assert.AreSame(expected, actual);
    }

    /// <summary>
    /// ChangeStatusAsyncでDomainException以外が発生した場合、InternalExceptionに変換されること
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_DomainException以外の例外発生時はInternalExceptionに変換される")]
    public async Task ChangeStatusAsync_WhenExceptionThrown_ThrowsExactlyInternalException()
    {
        // Arrange
        var ordersRepositoryMock = new Mock<IOrdersRepository>();
        var orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);
        var innerException = new InvalidOperationException("DB接続エラーです。");

        orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ReturnsAsync(orderStatus);

        ordersRepositoryMock
            .Setup(x => x.ChangeStatusAsync(order))
            .ThrowsAsync(innerException);

        var usecase = new SearchOrdersUsecase(
            ordersRepositoryMock.Object,
            orderStatusRepositoryMock.Object);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await usecase.ChangeStatusAsync(order);
        });

        // Assert
        Assert.AreEqual("注文ステータスの変更に失敗しました。", actual.Message);
        Assert.AreSame(innerException, actual.InnerException);
    }

    /// <summary>
    /// テスト用のOrdersを生成する
    /// </summary>
    private static OrdersModel CreateOrder(OrderStatus? orderStatus)
    {
        var order = (OrdersModel)RuntimeHelpers.GetUninitializedObject(typeof(OrdersModel));

        SetPrivateProperty(order, "OrderStatus", orderStatus);

        return order;
    }

    /// <summary>
    /// テスト用のOrderStatusを生成する
    /// </summary>
    private static OrderStatus CreateOrderStatus(int id)
    {
        var orderStatus = (OrderStatus)RuntimeHelpers.GetUninitializedObject(typeof(OrderStatus));

        SetPrivateProperty(orderStatus, "Id", id);

        return orderStatus;
    }

    /// <summary>
    /// private setのプロパティへテスト用の値を設定する
    /// </summary>
    private static void SetPrivateProperty<T>(T target, string propertyName, object? value)
    {
        var field = typeof(T).GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (field is null)
        {
            throw new InvalidOperationException($"{propertyName}のバッキングフィールドが見つかりません。");
        }

        field.SetValue(target, value);
    }
}