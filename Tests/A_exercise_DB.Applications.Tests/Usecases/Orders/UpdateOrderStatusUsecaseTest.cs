using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Applications.Usecases;
using A_exercise_DB.Applications.Usecases.Orders;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Applications.Tests.Usecases.Orders;

/// <summary>
/// UpdateOrderStatusUsecaseの単体テスト
/// </summary>
[TestClass]
public class UpdateOrderStatusUsecaseTest
{
    private Mock<IOrdersRepository> _ordersRepositoryMock = null!;
    private Mock<IOrderStatusRepository> _orderStatusRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private UpdateOrderStatusUsecase _usecase = null!;

    /// <summary>
    /// 各テスト実行前の初期化処理
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _ordersRepositoryMock = new Mock<IOrdersRepository>();
        _orderStatusRepositoryMock = new Mock<IOrderStatusRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _usecase = new UpdateOrderStatusUsecase(
            _ordersRepositoryMock.Object,
            _orderStatusRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// GetInputAsyncで注文IDが不正な場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "GetInputAsync_注文IDが不正な場合はDomainExceptionをスローする")]
    public async Task GetInputAsync_WhenOrderUuidIsEmpty_ThrowsDomainException()
    {
        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(
            async () => await _usecase.GetInputAsync(Guid.Empty));

        // Assert
        Assert.AreEqual("注文IDが不正です。", exception.Message);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(It.IsAny<Guid>()),
            Times.Never);

        _orderStatusRepositoryMock.Verify(
            x => x.FindAllAsync(),
            Times.Never);
    }

    /// <summary>
    /// GetInputAsyncで注文が存在しない場合、nullを返すこと
    /// </summary>
    [TestMethod(DisplayName = "GetInputAsync_注文が存在しない場合はnullを返す")]
    public async Task GetInputAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        _ordersRepositoryMock
            .Setup(x => x.FindByUuidAsync(orderUuid))
            .ReturnsAsync((OrdersModel?)null);

        // Act
        var result = await _usecase.GetInputAsync(orderUuid);

        // Assert
        Assert.IsNull(result);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(orderUuid),
            Times.Once);

        _orderStatusRepositoryMock.Verify(
            x => x.FindAllAsync(),
            Times.Never);
    }

    /// <summary>
    /// GetInputAsyncで注文情報と注文ステータス一覧を取得できること
    /// </summary>
    [TestMethod(DisplayName = "GetInputAsync_注文情報と注文ステータス一覧を取得できる")]
    public async Task GetInputAsync_ReturnsOrderAndOrderStatuses()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var order = CreateOrder(orderUuid, CreateOrderStatus(1, "注文受付"));

        var orderStatuses = new List<OrderStatus>
        {
            CreateOrderStatus(1, "注文受付"),
            CreateOrderStatus(2, "発送準備中"),
            CreateOrderStatus(3, "発送済み")
        };

        _ordersRepositoryMock
            .Setup(x => x.FindByUuidAsync(orderUuid))
            .ReturnsAsync(order);

        _orderStatusRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ReturnsAsync(orderStatuses);

        // Act
        var result = await _usecase.GetInputAsync(orderUuid);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(order, result.Value.Order);
        Assert.AreSame(orderStatuses, result.Value.OrderStatuses);
        Assert.HasCount(3, result.Value.OrderStatuses);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(orderUuid),
            Times.Once);

        _orderStatusRepositoryMock.Verify(
            x => x.FindAllAsync(),
            Times.Once);
    }

    /// <summary>
    /// GetConfirmAsyncで注文IDが不正な場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "GetConfirmAsync_注文IDが不正な場合はDomainExceptionをスローする")]
    public async Task GetConfirmAsync_WhenOrderUuidIsEmpty_ThrowsDomainException()
    {
        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(
            async () => await _usecase.GetConfirmAsync(Guid.Empty, 1));

        // Assert
        Assert.AreEqual("注文IDが不正です。", exception.Message);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(It.IsAny<Guid>()),
            Times.Never);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// GetConfirmAsyncで注文ステータスIDが不正な場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "GetConfirmAsync_注文ステータスIDが不正な場合はDomainExceptionをスローする")]
    public async Task GetConfirmAsync_WhenNewStatusIdIsInvalid_ThrowsDomainException()
    {
        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(
            async () => await _usecase.GetConfirmAsync(Guid.NewGuid(), 0));

        // Assert
        Assert.AreEqual("注文ステータスが不正です。", exception.Message);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(It.IsAny<Guid>()),
            Times.Never);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// GetConfirmAsyncで注文が存在しない場合、nullを返すこと
    /// </summary>
    [TestMethod(DisplayName = "GetConfirmAsync_注文が存在しない場合はnullを返す")]
    public async Task GetConfirmAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();
        var newStatusId = 2;

        _ordersRepositoryMock
            .Setup(x => x.FindByUuidAsync(orderUuid))
            .ReturnsAsync((OrdersModel?)null);

        // Act
        var result = await _usecase.GetConfirmAsync(orderUuid, newStatusId);

        // Assert
        Assert.IsNull(result);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(orderUuid),
            Times.Once);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// GetConfirmAsyncで注文ステータスが存在しない場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "GetConfirmAsync_注文ステータスが存在しない場合はDomainExceptionをスローする")]
    public async Task GetConfirmAsync_WhenOrderStatusDoesNotExist_ThrowsDomainException()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();
        var newStatusId = 2;
        var order = CreateOrder(orderUuid, CreateOrderStatus(1, "注文受付"));

        _ordersRepositoryMock
            .Setup(x => x.FindByUuidAsync(orderUuid))
            .ReturnsAsync(order);

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(newStatusId))
            .ReturnsAsync((OrderStatus?)null);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(
            async () => await _usecase.GetConfirmAsync(orderUuid, newStatusId));

        // Assert
        Assert.AreEqual("指定された注文ステータスは存在しません。", exception.Message);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(orderUuid),
            Times.Once);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(newStatusId),
            Times.Once);
    }

    /// <summary>
    /// GetConfirmAsyncで注文情報と新しい注文ステータスを取得できること
    /// </summary>
    [TestMethod(DisplayName = "GetConfirmAsync_注文情報と新しい注文ステータスを取得できる")]
    public async Task GetConfirmAsync_ReturnsOrderAndNewStatus()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();
        var newStatusId = 2;

        var order = CreateOrder(orderUuid, CreateOrderStatus(1, "注文受付"));
        var newStatus = CreateOrderStatus(newStatusId, "発送準備中");

        _ordersRepositoryMock
            .Setup(x => x.FindByUuidAsync(orderUuid))
            .ReturnsAsync(order);

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(newStatusId))
            .ReturnsAsync(newStatus);

        // Act
        var result = await _usecase.GetConfirmAsync(orderUuid, newStatusId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(order, result.Value.Order);
        Assert.AreSame(newStatus, result.Value.NewStatus);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(orderUuid),
            Times.Once);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(newStatusId),
            Times.Once);
    }

    /// <summary>
    /// UpdateAsyncで注文ステータスを更新できること
    /// </summary>
    [TestMethod(DisplayName = "UpdateAsync_注文ステータスを更新できる")]
    public async Task UpdateAsync_ReturnsUpdatedOrderAndNewStatus()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();
        var newStatusId = 2;

        var oldStatus = CreateOrderStatus(1, "注文受付");
        var newStatus = CreateOrderStatus(newStatusId, "発送準備中");
        var order = CreateOrder(orderUuid, oldStatus);

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _ordersRepositoryMock
            .Setup(x => x.FindByUuidAsync(orderUuid))
            .ReturnsAsync(order);

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(newStatusId))
            .ReturnsAsync(newStatus);

        _ordersRepositoryMock
            .Setup(x => x.ChangeStatusAsync(order))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _usecase.UpdateAsync(orderUuid, newStatusId);

        // Assert
        Assert.AreSame(order, result.Order);
        Assert.AreSame(newStatus, result.NewStatus);
        Assert.AreSame(newStatus, result.Order.OrderStatus);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(orderUuid),
            Times.Once);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(newStatusId),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.ChangeStatusAsync(order),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Never);
    }

    /// <summary>
    /// UpdateAsyncで注文IDが不正な場合、DomainExceptionをスローしてRollbackすること
    /// </summary>
    [TestMethod(DisplayName = "UpdateAsync_注文IDが不正な場合はDomainExceptionをスローしてRollbackする")]
    public async Task UpdateAsync_WhenOrderUuidIsEmpty_ThrowsDomainExceptionAndRollback()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(
            async () => await _usecase.UpdateAsync(Guid.Empty, 1));

        // Assert
        Assert.AreEqual("注文IDが不正です。", exception.Message);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(It.IsAny<Guid>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);
    }

    /// <summary>
    /// UpdateAsyncで注文ステータスIDが不正な場合、DomainExceptionをスローしてRollbackすること
    /// </summary>
    [TestMethod(DisplayName = "UpdateAsync_注文ステータスIDが不正な場合はDomainExceptionをスローしてRollbackする")]
    public async Task UpdateAsync_WhenNewStatusIdIsInvalid_ThrowsDomainExceptionAndRollback()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(
            async () => await _usecase.UpdateAsync(Guid.NewGuid(), 0));

        // Assert
        Assert.AreEqual("注文ステータスが不正です。", exception.Message);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(It.IsAny<Guid>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);
    }

    /// <summary>
    /// UpdateAsyncで注文が存在しない場合、NotFoundExceptionをスローしてRollbackすること
    /// </summary>
    [TestMethod(DisplayName = "UpdateAsync_注文が存在しない場合はNotFoundExceptionをスローしてRollbackする")]
    public async Task UpdateAsync_WhenOrderDoesNotExist_ThrowsNotFoundExceptionAndRollback()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();
        var newStatusId = 2;

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _ordersRepositoryMock
            .Setup(x => x.FindByUuidAsync(orderUuid))
            .ReturnsAsync((OrdersModel?)null);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<NotFoundException>(
            async () => await _usecase.UpdateAsync(orderUuid, newStatusId));

        // Assert
        Assert.AreEqual("指定された注文は存在しません。", exception.Message);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(orderUuid),
            Times.Once);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(It.IsAny<int>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);
    }

    /// <summary>
    /// UpdateAsyncで注文ステータスが存在しない場合、DomainExceptionをスローしてRollbackすること
    /// </summary>
    [TestMethod(DisplayName = "UpdateAsync_注文ステータスが存在しない場合はDomainExceptionをスローしてRollbackする")]
    public async Task UpdateAsync_WhenOrderStatusDoesNotExist_ThrowsDomainExceptionAndRollback()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();
        var newStatusId = 2;
        var order = CreateOrder(orderUuid, CreateOrderStatus(1, "注文受付"));

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _ordersRepositoryMock
            .Setup(x => x.FindByUuidAsync(orderUuid))
            .ReturnsAsync(order);

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(newStatusId))
            .ReturnsAsync((OrderStatus?)null);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<DomainException>(
            async () => await _usecase.UpdateAsync(orderUuid, newStatusId));

        // Assert
        Assert.AreEqual("指定された注文ステータスは存在しません。", exception.Message);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(orderUuid),
            Times.Once);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(newStatusId),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.ChangeStatusAsync(It.IsAny<OrdersModel>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);
    }

    /// <summary>
    /// UpdateAsyncで注文ステータス更新に失敗した場合、InternalExceptionをスローしてRollbackすること
    /// </summary>
    [TestMethod(DisplayName = "UpdateAsync_注文ステータス更新に失敗した場合はInternalExceptionをスローしてRollbackする")]
    public async Task UpdateAsync_WhenChangeStatusFailed_ThrowsInternalExceptionAndRollback()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();
        var newStatusId = 2;

        var order = CreateOrder(orderUuid, CreateOrderStatus(1, "注文受付"));
        var newStatus = CreateOrderStatus(newStatusId, "発送準備中");

        _unitOfWorkMock
            .Setup(x => x.BeginAsync())
            .Returns(Task.CompletedTask);

        _ordersRepositoryMock
            .Setup(x => x.FindByUuidAsync(orderUuid))
            .ReturnsAsync(order);

        _orderStatusRepositoryMock
            .Setup(x => x.FindByIdAsync(newStatusId))
            .ReturnsAsync(newStatus);

        _ordersRepositoryMock
            .Setup(x => x.ChangeStatusAsync(order))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(
            async () => await _usecase.UpdateAsync(orderUuid, newStatusId));

        // Assert
        Assert.AreEqual("注文ステータスの更新に失敗しました。", exception.Message);
        Assert.AreSame(newStatus, order.OrderStatus);

        _unitOfWorkMock.Verify(
            x => x.BeginAsync(),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.FindByUuidAsync(orderUuid),
            Times.Once);

        _orderStatusRepositoryMock.Verify(
            x => x.FindByIdAsync(newStatusId),
            Times.Once);

        _ordersRepositoryMock.Verify(
            x => x.ChangeStatusAsync(order),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackAsync(),
            Times.Once);
    }

    /// <summary>
    /// テスト用のOrdersを生成する
    /// </summary>
    private static OrdersModel CreateOrder(
        Guid orderUuid,
        OrderStatus orderStatus)
    {
        var order =
            (OrdersModel)RuntimeHelpers.GetUninitializedObject(typeof(OrdersModel));

        SetPrivateProperty(order, "OrderUuid", orderUuid);
        SetPrivateProperty(order, "OrderStatus", orderStatus);

        return order;
    }

    /// <summary>
    /// テスト用のOrderStatusを生成する
    /// </summary>
    private static OrderStatus CreateOrderStatus(
        int id,
        string name)
    {
        var orderStatus =
            (OrderStatus)RuntimeHelpers.GetUninitializedObject(typeof(OrderStatus));

        SetPrivateProperty(orderStatus, "Id", id);
        SetPrivateProperty(orderStatus, "Name", name);

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