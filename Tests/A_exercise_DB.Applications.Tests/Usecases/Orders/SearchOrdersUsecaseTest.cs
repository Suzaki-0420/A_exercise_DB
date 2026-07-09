using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Applications.Usecases.Orders;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Applications.Tests.Usecases.Orders;

/// <summary>
/// SearchOrdersUsecaseの単体テスト
/// </summary>
[TestClass]
public class SearchOrdersUsecaseTest
{
    private Mock<IOrdersRepository> _ordersRepositoryMock = null!;
    private Mock<IOrderStatusRepository> _orderStatusRepositoryMock = null!;
    private SearchOrdersUsecase _usecase = null!;

    /// <summary>
    /// 各テスト実行前の初期化処理
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _ordersRepositoryMock = new Mock<IOrdersRepository>();
        _orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();

        _usecase = new SearchOrdersUsecase(
            _ordersRepositoryMock.Object,
            _orderStatusRepositoryMock.Object);
    }

    /// <summary>
    /// SearchAsyncで購入履歴を取得できること
    /// </summary>
    [TestMethod(DisplayName = "SearchAsync_購入日または顧客名で購入履歴を取得できる")]
    public async Task SearchAsync_ReturnsOrders()
    {
        // Arrange
        var expected = new List<OrdersModel>();

        _ordersRepositoryMock
            .Setup(x => x.SearchByDateOrNameAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expected);

        // Act
        var actual = await _usecase.SearchAsync(
            new DateTime(2026, 7, 9),
            "山田");

        // Assert
        Assert.AreSame(expected, actual);

        _ordersRepositoryMock.Verify(x => x.SearchByDateOrNameAsync(
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
        var expected = new DomainException("業務ルール違反です。");

        _ordersRepositoryMock
            .Setup(x => x.SearchByDateOrNameAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<string?>()))
            .ThrowsAsync(expected);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await _usecase.SearchAsync(null, "山田");
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
        var innerException = new InvalidOperationException("DB接続エラーです。");

        _ordersRepositoryMock
            .Setup(x => x.SearchByDateOrNameAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<string?>()))
            .ThrowsAsync(innerException);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _usecase.SearchAsync(null, "山田");
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
        var expected = new List<OrderStatus>();

        _orderStatusRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ReturnsAsync(expected);

        // Act
        var actual = await _usecase.FindAllStatusAsync();

        // Assert
        Assert.AreSame(expected, actual);

        _orderStatusRepositoryMock.Verify(
            x => x.FindAllAsync(),
            Times.Once);
    }

    /// <summary>
    /// FindAllStatusAsyncでDomainExceptionが発生した場合、そのまま再スローされること
    /// </summary>
    [TestMethod(DisplayName = "FindAllStatusAsync_DomainException発生時はそのまま再スローされる")]
    public async Task FindAllStatusAsync_WhenDomainExceptionThrown_ThrowsExactlyDomainException()
    {
        // Arrange
        var expected = new DomainException("業務ルール違反です。");

        _orderStatusRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ThrowsAsync(expected);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await _usecase.FindAllStatusAsync();
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
        var innerException = new InvalidOperationException("DB接続エラーです。");

        _orderStatusRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ThrowsAsync(innerException);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _usecase.FindAllStatusAsync();
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
        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _usecase.ChangeStatusAsync(null!);
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
        var order = CreateOrder(null);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _usecase.ChangeStatusAsync(order);
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
        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ReturnsAsync((OrderStatus?)null);

        // Act
        var actual = await _usecase.ChangeStatusAsync(order);

        // Assert
        Assert.IsFalse(actual);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(1),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.ChangeStatusAsync(It.IsAny<OrdersModel>()),
            Times.Never);
    }

    /// <summary>
    /// ChangeStatusAsyncで注文ステータスを変更できること
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_注文ステータスを変更できる")]
    public async Task ChangeStatusAsync_WhenOrderStatusExists_ReturnsTrue()
    {
        // Arrange
        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ReturnsAsync(orderStatus);

        _ordersRepositoryMock
            .Setup(x => x.ChangeStatusAsync(order))
            .ReturnsAsync(true);

        // Act
        var actual = await _usecase.ChangeStatusAsync(order);

        // Assert
        Assert.IsTrue(actual);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(1),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.ChangeStatusAsync(order),
            Times.Once);
    }

    /// <summary>
    /// ChangeStatusAsyncで注文ステータス変更結果がfalseの場合、falseを返すこと
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_注文ステータス変更結果がfalseの場合はfalseを返す")]
    public async Task ChangeStatusAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
    {
        // Arrange
        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ReturnsAsync(orderStatus);

        _ordersRepositoryMock
            .Setup(x => x.ChangeStatusAsync(order))
            .ReturnsAsync(false);

        // Act
        var actual = await _usecase.ChangeStatusAsync(order);

        // Assert
        Assert.IsFalse(actual);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(1),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.ChangeStatusAsync(order),
            Times.Once);
    }

    /// <summary>
    /// ChangeStatusAsyncでDomainExceptionが発生した場合、そのまま再スローされること
    /// </summary>
    [TestMethod(DisplayName = "ChangeStatusAsync_DomainException発生時はそのまま再スローされる")]
    public async Task ChangeStatusAsync_WhenDomainExceptionThrown_ThrowsExactlyDomainException()
    {
        // Arrange
        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);
        var expected = new DomainException("業務ルール違反です。");

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ThrowsAsync(expected);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            await _usecase.ChangeStatusAsync(order);
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
        var orderStatus = CreateOrderStatus(1);
        var order = CreateOrder(orderStatus);
        var innerException = new InvalidOperationException("DB接続エラーです。");

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(1))
            .ReturnsAsync(orderStatus);

        _ordersRepositoryMock
            .Setup(x => x.ChangeStatusAsync(order))
            .ThrowsAsync(innerException);

        // Act
        var actual = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _usecase.ChangeStatusAsync(order);
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
        var order =
            (OrdersModel)RuntimeHelpers.GetUninitializedObject(typeof(OrdersModel));

        SetPrivateProperty(order, "OrderStatus", orderStatus);

        return order;
    }

    /// <summary>
    /// テスト用のOrderStatusを生成する
    /// </summary>
    private static OrderStatus CreateOrderStatus(int id)
    {
        var orderStatus =
            (OrderStatus)RuntimeHelpers.GetUninitializedObject(typeof(OrderStatus));

        SetPrivateProperty(orderStatus, "Id", id);

        return orderStatus;
    }

    /// <summary>
    /// private setのプロパティへテスト用の値を設定する
    /// </summary>
    private static void SetPrivateProperty<T>(
        T target,
        string propertyName,
        object? value)
    {
        var field = typeof(T).GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (field is null)
        {
            throw new InvalidOperationException(
                $"{propertyName}のバッキングフィールドが見つかりません。");
        }

        field.SetValue(target, value);
    }
}