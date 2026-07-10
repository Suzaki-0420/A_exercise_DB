using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Presentations.Tests.Adapters;

/// <summary>
/// UpdateOrderStatusViewModelAdapterの単体テスト
/// </summary>
[TestClass]
public class UpdateOrderStatusViewModelAdapterTest
{
    private UpdateOrderStatusViewModelAdapter _adapter = null!;

    /// <summary>
    /// 各テスト実行前の初期化処理
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _adapter = new UpdateOrderStatusViewModelAdapter();
    }

    /// <summary>
    /// ConvertToInputViewModelで注文ステータス更新入力ViewModelへ変換できること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToInputViewModel_注文ステータス更新入力ViewModelへ変換できる")]
    public void ConvertToInputViewModel_ReturnsInputViewModel()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var order = CreateOrder(
            orderUuid,
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            CreateOrderStatus(1, "注文受付"),
            new List<OrdersDetail>
            {
                CreateOrdersDetail("食品", 2),
                CreateOrdersDetail("飲料", 3)
            });

        var orderStatuses = new List<OrderStatus>
        {
            CreateOrderStatus(1, "注文受付"),
            CreateOrderStatus(2, "発送準備中"),
            CreateOrderStatus(3, "発送済み")
        };

        // Act
        var viewModel = _adapter.ConvertToInputViewModel(order, orderStatuses);

        // Assert
        Assert.AreEqual(orderUuid, viewModel.OrderId);
        Assert.AreEqual("2026/07/10 10:30:45", viewModel.OrderDate);
        Assert.AreEqual("yamada01", viewModel.CustomerAccountName);
        Assert.AreEqual("食品 × 2、飲料 × 3", viewModel.OrderContent);
        Assert.AreEqual(1, viewModel.OrderStatus);

        Assert.HasCount(3, viewModel.OrderStatusList);

        Assert.AreEqual(1, viewModel.OrderStatusList[0].Id);
        Assert.AreEqual("注文受付", viewModel.OrderStatusList[0].Name);

        Assert.AreEqual(2, viewModel.OrderStatusList[1].Id);
        Assert.AreEqual("発送準備中", viewModel.OrderStatusList[1].Name);

        Assert.AreEqual(3, viewModel.OrderStatusList[2].Id);
        Assert.AreEqual("発送済み", viewModel.OrderStatusList[2].Name);
    }

    /// <summary>
    /// ConvertToInputViewModelで注文詳細が0件の場合、注文内容が空文字になること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToInputViewModel_注文詳細が0件の場合は注文内容が空文字になる")]
    public void ConvertToInputViewModel_WhenOrderDetailsIsEmpty_ReturnsEmptyOrderContent()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var order = CreateOrder(
            orderUuid,
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            CreateOrderStatus(1, "注文受付"),
            new List<OrdersDetail>());

        var orderStatuses = new List<OrderStatus>
        {
            CreateOrderStatus(1, "注文受付")
        };

        // Act
        var viewModel = _adapter.ConvertToInputViewModel(order, orderStatuses);

        // Assert
        Assert.AreEqual(orderUuid, viewModel.OrderId);
        Assert.AreEqual(string.Empty, viewModel.OrderContent);
        Assert.HasCount(1, viewModel.OrderStatusList);
    }

    /// <summary>
    /// ConvertToInputViewModelでorderがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToInputViewModel_orderがnullの場合はInternalExceptionをスローする")]
    public void ConvertToInputViewModel_WhenOrderIsNull_ThrowsInternalException()
    {
        // Arrange
        var orderStatuses = new List<OrderStatus>
        {
            CreateOrderStatus(1, "注文受付")
        };

        // Act
        var exception = Assert.ThrowsExactly<InternalException>(
            () => _adapter.ConvertToInputViewModel(null!, orderStatuses));

        // Assert
        Assert.AreEqual("引数orderがnullです。", exception.Message);
    }

    /// <summary>
    /// ConvertToInputViewModelでorderStatusesがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToInputViewModel_orderStatusesがnullの場合はInternalExceptionをスローする")]
    public void ConvertToInputViewModel_WhenOrderStatusesIsNull_ThrowsInternalException()
    {
        // Arrange
        var order = CreateOrder(
            Guid.NewGuid(),
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            CreateOrderStatus(1, "注文受付"),
            new List<OrdersDetail>());

        // Act
        var exception = Assert.ThrowsExactly<InternalException>(
            () => _adapter.ConvertToInputViewModel(order, null!));

        // Assert
        Assert.AreEqual("引数orderStatusesがnullです。", exception.Message);
    }

    /// <summary>
    /// ConvertToInputViewModelで注文ステータス一覧にnullが含まれる場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToInputViewModel_注文ステータス一覧にnullが含まれる場合はInternalExceptionをスローする")]
    public void ConvertToInputViewModel_WhenOrderStatusesContainsNull_ThrowsInternalException()
    {
        // Arrange
        var order = CreateOrder(
            Guid.NewGuid(),
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            CreateOrderStatus(1, "注文受付"),
            new List<OrdersDetail>());

        var orderStatuses = new List<OrderStatus>
        {
            CreateOrderStatus(1, "注文受付"),
            null!
        };

        // Act
        var exception = Assert.ThrowsExactly<InternalException>(
            () => _adapter.ConvertToInputViewModel(order, orderStatuses));

        // Assert
        Assert.AreEqual("引数orderStatusがnullです。", exception.Message);
    }

    /// <summary>
    /// ConvertToConfirmViewModelで注文ステータス更新確認ViewModelへ変換できること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToConfirmViewModel_注文ステータス更新確認ViewModelへ変換できる")]
    public void ConvertToConfirmViewModel_ReturnsConfirmViewModel()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var order = CreateOrder(
            orderUuid,
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            CreateOrderStatus(1, "注文受付"),
            new List<OrdersDetail>
            {
                CreateOrdersDetail("食品", 2)
            });

        var newStatus = CreateOrderStatus(2, "発送準備中");

        // Act
        var viewModel = _adapter.ConvertToConfirmViewModel(order, newStatus);

        // Assert
        Assert.AreEqual(orderUuid, viewModel.OrderId);
        Assert.AreEqual("2026/07/10 10:30:45", viewModel.OrderDate);
        Assert.AreEqual("yamada01", viewModel.CustomerAccountName);
        Assert.AreEqual("注文受付", viewModel.CurrentStatus);
        Assert.AreEqual(2, viewModel.NewStatusId);
        Assert.AreEqual("発送準備中", viewModel.NewStatus);
    }

    /// <summary>
    /// ConvertToConfirmViewModelでorderがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToConfirmViewModel_orderがnullの場合はInternalExceptionをスローする")]
    public void ConvertToConfirmViewModel_WhenOrderIsNull_ThrowsInternalException()
    {
        // Arrange
        var newStatus = CreateOrderStatus(2, "発送準備中");

        // Act
        var exception = Assert.ThrowsExactly<InternalException>(
            () => _adapter.ConvertToConfirmViewModel(null!, newStatus));

        // Assert
        Assert.AreEqual("引数orderがnullです。", exception.Message);
    }

    /// <summary>
    /// ConvertToConfirmViewModelでnewStatusがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToConfirmViewModel_newStatusがnullの場合はInternalExceptionをスローする")]
    public void ConvertToConfirmViewModel_WhenNewStatusIsNull_ThrowsInternalException()
    {
        // Arrange
        var order = CreateOrder(
            Guid.NewGuid(),
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            CreateOrderStatus(1, "注文受付"),
            new List<OrdersDetail>());

        // Act
        var exception = Assert.ThrowsExactly<InternalException>(
            () => _adapter.ConvertToConfirmViewModel(order, null!));

        // Assert
        Assert.AreEqual("引数newStatusがnullです。", exception.Message);
    }

    /// <summary>
    /// ConvertToCompleteViewModelで注文ステータス更新完了ViewModelへ変換できること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToCompleteViewModel_注文ステータス更新完了ViewModelへ変換できる")]
    public void ConvertToCompleteViewModel_ReturnsCompleteViewModel()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var order = CreateOrder(
            orderUuid,
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            CreateOrderStatus(2, "発送準備中"),
            new List<OrdersDetail>
            {
                CreateOrdersDetail("食品", 2)
            });

        var newStatus = CreateOrderStatus(2, "発送準備中");

        // Act
        var viewModel = _adapter.ConvertToCompleteViewModel(order, newStatus);

        // Assert
        Assert.AreEqual("注文ステータスを更新しました", viewModel.CompleteMsg);
        Assert.AreEqual(orderUuid, viewModel.OrderNumber);
        Assert.AreEqual("発送準備中", viewModel.OrderStatus);
        Assert.IsFalse(string.IsNullOrEmpty(viewModel.UpdateDate));
        Assert.AreEqual("/admin/order/search", viewModel.SearchUrl);
        Assert.AreEqual("/admin", viewModel.HomeUrl);
    }

    /// <summary>
    /// ConvertToCompleteViewModelでorderがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToCompleteViewModel_orderがnullの場合はInternalExceptionをスローする")]
    public void ConvertToCompleteViewModel_WhenOrderIsNull_ThrowsInternalException()
    {
        // Arrange
        var newStatus = CreateOrderStatus(2, "発送準備中");

        // Act
        var exception = Assert.ThrowsExactly<InternalException>(
            () => _adapter.ConvertToCompleteViewModel(null!, newStatus));

        // Assert
        Assert.AreEqual("引数orderがnullです。", exception.Message);
    }

    /// <summary>
    /// ConvertToCompleteViewModelでnewStatusがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToCompleteViewModel_newStatusがnullの場合はInternalExceptionをスローする")]
    public void ConvertToCompleteViewModel_WhenNewStatusIsNull_ThrowsInternalException()
    {
        // Arrange
        var order = CreateOrder(
            Guid.NewGuid(),
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            CreateOrderStatus(1, "注文受付"),
            new List<OrdersDetail>());

        // Act
        var exception = Assert.ThrowsExactly<InternalException>(
            () => _adapter.ConvertToCompleteViewModel(order, null!));

        // Assert
        Assert.AreEqual("引数newStatusがnullです。", exception.Message);
    }

    /// <summary>
    /// テスト用のOrdersを生成する
    /// </summary>
    private static OrdersModel CreateOrder(
        Guid orderUuid,
        DateTime orderDate,
        string customerAccountName,
        OrderStatus orderStatus,
        List<OrdersDetail> ordersDetails)
    {
        var order =
            (OrdersModel)RuntimeHelpers.GetUninitializedObject(typeof(OrdersModel));

        SetPrivateProperty(order, "OrderUuid", orderUuid);
        SetPrivateProperty(order, "OrderDate", orderDate);
        SetPrivateProperty(order, "Customer", CreateCustomer(customerAccountName));
        SetPrivateProperty(order, "OrderStatus", orderStatus);
        SetPrivateProperty(order, "OrdersDetails", ordersDetails);

        return order;
    }

    /// <summary>
    /// テスト用のCustomerを生成する
    /// </summary>
    private static Customer CreateCustomer(string username)
    {
        var customer =
            (Customer)RuntimeHelpers.GetUninitializedObject(typeof(Customer));

        SetPrivateProperty(customer, "Username", username);

        return customer;
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
    /// テスト用のOrdersDetailを生成する
    /// </summary>
    private static OrdersDetail CreateOrdersDetail(
        string productName,
        int count)
    {
        var ordersDetail =
            (OrdersDetail)RuntimeHelpers.GetUninitializedObject(typeof(OrdersDetail));

        SetPrivateProperty(ordersDetail, "Product", CreateProduct(productName));
        SetPrivateProperty(ordersDetail, "Count", count);

        return ordersDetail;
    }

    /// <summary>
    /// テスト用のProductを生成する
    /// </summary>
    private static Product CreateProduct(string name)
    {
        var product =
            (Product)RuntimeHelpers.GetUninitializedObject(typeof(Product));

        SetPrivateProperty(product, "Name", name);

        return product;
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