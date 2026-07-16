using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Presentations.Tests.Adapters;

/// <summary>
/// SearchOrdersViewModelAdapterの単体テスト
/// </summary>
[TestClass]
public class SearchOrdersViewModelAdapterTest
{
    private SearchOrdersViewModelAdapter _adapter = null!;

    /// <summary>
    /// 各テスト実行前の初期化処理
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _adapter = new SearchOrdersViewModelAdapter();
    }

    /// <summary>
    /// RestoreOrderDateで購入日を復元できること
    /// </summary>
    [TestMethod(DisplayName = "RestoreOrderDate_購入日を復元できる")]
    public void RestoreOrderDate_CanRestoreOrderDate()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            OrderDate = "2026-07-09"
        };

        // Act
        var actual = _adapter.RestoreOrderDate(model);

        // Assert
        Assert.AreEqual(new DateTime(2026, 7, 9), actual);
    }

    /// <summary>
    /// RestoreOrderDateで時刻付き購入日を日付だけにして復元できること
    /// </summary>
    [TestMethod(DisplayName = "RestoreOrderDate_時刻付き購入日を日付だけにして復元できる")]
    public void RestoreOrderDate_WhenOrderDateHasTime_ReturnsDateOnly()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            OrderDate = "2026-07-09 15:30:45"
        };

        // Act
        var actual = _adapter.RestoreOrderDate(model);

        // Assert
        Assert.AreEqual(new DateTime(2026, 7, 9), actual);
    }

    /// <summary>
    /// RestoreOrderDateでmodelがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RestoreOrderDate_modelがnullの場合はInternalExceptionをスローする")]
    public void RestoreOrderDate_WhenModelIsNull_ThrowsExactlyInternalException()
    {
        // Act
        var exception = Assert.ThrowsExactly<InternalException>(() =>
        {
            _adapter.RestoreOrderDate(null!);
        });

        // Assert
        Assert.AreEqual("引数modelがnullです。", exception.Message);
    }

    /// <summary>
    /// RestoreOrderDateで購入日がnullの場合、nullを返すこと
    /// </summary>
    [TestMethod(DisplayName = "RestoreOrderDate_購入日がnullの場合はnullを返す")]
    public void RestoreOrderDate_WhenOrderDateIsNull_ReturnsNull()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            OrderDate = null
        };

        // Act
        var actual = _adapter.RestoreOrderDate(model);

        // Assert
        Assert.IsNull(actual);
    }

    /// <summary>
    /// RestoreOrderDateで購入日が空文字の場合、nullを返すこと
    /// </summary>
    [TestMethod(DisplayName = "RestoreOrderDate_購入日が空文字の場合はnullを返す")]
    public void RestoreOrderDate_WhenOrderDateIsEmpty_ReturnsNull()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            OrderDate = string.Empty
        };

        // Act
        var actual = _adapter.RestoreOrderDate(model);

        // Assert
        Assert.IsNull(actual);
    }

    /// <summary>
    /// RestoreOrderDateで購入日が空白の場合、nullを返すこと
    /// </summary>
    [TestMethod(DisplayName = "RestoreOrderDate_購入日が空白の場合はnullを返す")]
    public void RestoreOrderDate_WhenOrderDateIsWhiteSpace_ReturnsNull()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            OrderDate = "   "
        };

        // Act
        var actual = _adapter.RestoreOrderDate(model);

        // Assert
        Assert.IsNull(actual);
    }

    /// <summary>
    /// RestoreOrderDateで購入日の形式が不正な場合、DomainExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RestoreOrderDate_購入日の形式が不正な場合はDomainExceptionをスローする")]
    public void RestoreOrderDate_WhenOrderDateIsInvalid_ThrowsExactlyDomainException()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            OrderDate = "invalid-date"
        };

        // Act
        var exception = Assert.ThrowsExactly<DomainException>(() =>
        {
            _adapter.RestoreOrderDate(model);
        });

        // Assert
        Assert.AreEqual("購入日の形式が不正です。", exception.Message);
    }

    /// <summary>
    /// RestoreCustomerAccountNameで顧客アカウント名を復元できること
    /// </summary>
    [TestMethod(DisplayName = "RestoreCustomerAccountName_顧客アカウント名を復元できる")]
    public void RestoreCustomerAccountName_CanRestoreCustomerAccountName()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            CustomerAccountName = "yamada01"
        };

        // Act
        var actual = _adapter.RestoreCustomerAccountName(model);

        // Assert
        Assert.AreEqual("yamada01", actual);
    }

    /// <summary>
    /// RestoreCustomerAccountNameで前後の空白を取り除いて顧客アカウント名を復元できること
    /// </summary>
    [TestMethod(DisplayName = "RestoreCustomerAccountName_前後の空白を取り除いて顧客アカウント名を復元できる")]
    public void RestoreCustomerAccountName_WhenCustomerAccountNameHasSpaces_ReturnsTrimmedValue()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            CustomerAccountName = "  yamada01  "
        };

        // Act
        var actual = _adapter.RestoreCustomerAccountName(model);

        // Assert
        Assert.AreEqual("yamada01", actual);
    }

    /// <summary>
    /// RestoreCustomerAccountNameでmodelがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "RestoreCustomerAccountName_modelがnullの場合はInternalExceptionをスローする")]
    public void RestoreCustomerAccountName_WhenModelIsNull_ThrowsExactlyInternalException()
    {
        // Act
        var exception = Assert.ThrowsExactly<InternalException>(() =>
        {
            _adapter.RestoreCustomerAccountName(null!);
        });

        // Assert
        Assert.AreEqual("引数modelがnullです。", exception.Message);
    }

    /// <summary>
    /// RestoreCustomerAccountNameで顧客アカウント名がnullの場合、nullを返すこと
    /// </summary>
    [TestMethod(DisplayName = "RestoreCustomerAccountName_顧客アカウント名がnullの場合はnullを返す")]
    public void RestoreCustomerAccountName_WhenCustomerAccountNameIsNull_ReturnsNull()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            CustomerAccountName = null
        };

        // Act
        var actual = _adapter.RestoreCustomerAccountName(model);

        // Assert
        Assert.IsNull(actual);
    }

    /// <summary>
    /// RestoreCustomerAccountNameで顧客アカウント名が空文字の場合、nullを返すこと
    /// </summary>
    [TestMethod(DisplayName = "RestoreCustomerAccountName_顧客アカウント名が空文字の場合はnullを返す")]
    public void RestoreCustomerAccountName_WhenCustomerAccountNameIsEmpty_ReturnsNull()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            CustomerAccountName = string.Empty
        };

        // Act
        var actual = _adapter.RestoreCustomerAccountName(model);

        // Assert
        Assert.IsNull(actual);
    }

    /// <summary>
    /// RestoreCustomerAccountNameで顧客アカウント名が空白の場合、nullを返すこと
    /// </summary>
    [TestMethod(DisplayName = "RestoreCustomerAccountName_顧客アカウント名が空白の場合はnullを返す")]
    public void RestoreCustomerAccountName_WhenCustomerAccountNameIsWhiteSpace_ReturnsNull()
    {
        // Arrange
        var model = new SearchOrdersViewModel
        {
            CustomerAccountName = "   "
        };

        // Act
        var actual = _adapter.RestoreCustomerAccountName(model);

        // Assert
        Assert.IsNull(actual);
    }

    /// <summary>
    /// ConvertToResultViewModelで注文一覧を検索結果ViewModelへ変換できること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToResultViewModel_注文一覧を検索結果ViewModelへ変換できる")]
    public void ConvertToResultViewModel_CanConvertToResultViewModel()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var order = CreateOrder(
            orderUuid,
            new DateTime(2026, 7, 9, 10, 30, 45),
            "yamada01",
            "発送準備中",
            new List<OrdersDetail>
            {
                CreateOrdersDetail("食品", 2),
                CreateOrdersDetail("飲料", 3)
            });

        var orders = new List<OrdersModel>
        {
            order
        };

        // Act
        var actual = _adapter.ConvertToResultViewModel(orders);

        // Assert
        Assert.IsNotNull(actual);
        Assert.IsNull(actual.Message);
        Assert.HasCount(1, actual.OrderList);

        var item = actual.OrderList[0];

        Assert.AreEqual(orderUuid, item.OrderUuid);
        Assert.AreEqual("2026/07/09 10:30:45", item.OrderDate);
        Assert.AreEqual("yamada01", item.CustomerAccountName);
        Assert.AreEqual("食品 × 2、飲料 × 3", item.OrderContent);
        Assert.AreEqual("発送準備中", item.OrderStatus);
        Assert.AreEqual($"/admin/order/status/update/{orderUuid}", item.StatusUpdateUrl);
    }

    /// <summary>
    /// ConvertToResultViewModelで注文一覧が空の場合、メッセージを設定すること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToResultViewModel_注文一覧が空の場合はメッセージを設定する")]
    public void ConvertToResultViewModel_WhenOrdersIsEmpty_SetsMessage()
    {
        // Arrange
        var orders = new List<OrdersModel>();

        // Act
        var actual = _adapter.ConvertToResultViewModel(orders);

        // Assert
        Assert.IsNotNull(actual);
        Assert.IsEmpty(actual.OrderList);
        Assert.AreEqual("該当する注文が見つかりませんでした", actual.Message);
    }

    /// <summary>
    /// ConvertToResultViewModelでordersがnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToResultViewModel_ordersがnullの場合はInternalExceptionをスローする")]
    public void ConvertToResultViewModel_WhenOrdersIsNull_ThrowsExactlyInternalException()
    {
        // Act
        var exception = Assert.ThrowsExactly<InternalException>(() =>
        {
            _adapter.ConvertToResultViewModel(null!);
        });

        // Assert
        Assert.AreEqual("引数ordersがnullです。", exception.Message);
    }

    /// <summary>
    /// ConvertToResultViewModelで注文がnullの場合、InternalExceptionをスローすること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToResultViewModel_注文がnullの場合はInternalExceptionをスローする")]
    public void ConvertToResultViewModel_WhenOrderIsNull_ThrowsExactlyInternalException()
    {
        // Arrange
        var orders = new List<OrdersModel>
        {
            null!
        };

        // Act
        var exception = Assert.ThrowsExactly<InternalException>(() =>
        {
            _adapter.ConvertToResultViewModel(orders);
        });

        // Assert
        Assert.AreEqual("引数orderがnullです。", exception.Message);
    }

    /// <summary>
    /// ConvertToResultViewModelで注文詳細が空の場合、注文内容が空文字になること
    /// </summary>
    [TestMethod(DisplayName = "ConvertToResultViewModel_注文詳細が空の場合は注文内容が空文字になる")]
    public void ConvertToResultViewModel_WhenOrdersDetailsIsEmpty_OrderContentIsEmpty()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var order = CreateOrder(
            orderUuid,
            new DateTime(2026, 7, 9, 10, 30, 45),
            "yamada01",
            "発送準備中",
            new List<OrdersDetail>());

        var orders = new List<OrdersModel>
        {
            order
        };

        // Act
        var actual = _adapter.ConvertToResultViewModel(orders);

        // Assert
        Assert.HasCount(1, actual.OrderList);
        Assert.AreEqual(string.Empty, actual.OrderList[0].OrderContent);
        Assert.IsNull(actual.Message);
    }

    /// <summary>
    /// テスト用のOrdersを生成する
    /// </summary>
    private static OrdersModel CreateOrder(
        Guid orderUuid,
        DateTime orderDate,
        string customerAccountName,
        string orderStatusName,
        List<OrdersDetail> ordersDetails)
    {
        var order =
            (OrdersModel)RuntimeHelpers.GetUninitializedObject(typeof(OrdersModel));

        SetPrivateProperty(order, "OrderUuid", orderUuid);
        SetPrivateProperty(order, "OrderDate", orderDate);
        SetPrivateProperty(order, "Customer", CreateCustomer(customerAccountName));
        SetPrivateProperty(order, "OrderStatus", CreateOrderStatus(orderStatusName));
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
    private static OrderStatus CreateOrderStatus(string name)
    {
        var orderStatus =
            (OrderStatus)RuntimeHelpers.GetUninitializedObject(typeof(OrderStatus));

        SetPrivateProperty(orderStatus, "Name", name);

        return orderStatus;
    }

    /// <summary>
    /// テスト用のOrdersDetailを生成する
    /// </summary>
    private static OrdersDetail CreateOrdersDetail(string productName, int count)
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