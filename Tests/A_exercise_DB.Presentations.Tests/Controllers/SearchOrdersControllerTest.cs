using System.Reflection;
using System.Runtime.CompilerServices;
using A_exercise_DB.Applications.Usecases.Orders;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Presentations.Adapters;
using A_exercise_DB.Presentations.Controllers;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Presentations.Tests.Controllers;

/// <summary>
/// SearchOrdersControllerの単体テスト
/// </summary>
[TestClass]
public class SearchOrdersControllerTest
{
    private Mock<ISearchOrdersUsecase> _searchOrdersUsecaseMock = null!;
    private SearchOrdersViewModelAdapter _adapter = null!;
    private Mock<ILogger<SearchOrdersController>> _loggerMock = null!;
    private SearchOrdersController _controller = null!;

    /// <summary>
    /// 各テスト実行前の初期化処理
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _searchOrdersUsecaseMock = new Mock<ISearchOrdersUsecase>();
        _adapter = new SearchOrdersViewModelAdapter();
        _loggerMock = new Mock<ILogger<SearchOrdersController>>();

        _controller = new SearchOrdersController(
            _searchOrdersUsecaseMock.Object,
            _adapter,
            _loggerMock.Object);
    }

    /// <summary>
    /// Getで購入履歴検索画面の初期表示情報を取得できること
    /// </summary>
    [TestMethod(DisplayName = "Get_購入履歴検索画面の初期表示情報を取得できる")]
    public async Task Get_ReturnsOk()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var orders = new List<OrdersModel>
        {
            CreateOrder(
                orderUuid,
                new DateTime(2026, 7, 9, 10, 30, 45),
                "yamada01",
                "発送準備中",
                new List<OrdersDetail>
                {
                    CreateOrdersDetail("食品", 2)
                })
        };

        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(null, null))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var viewModel = okResult.Value as SearchOrdersResultViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("購入履歴検索", viewModel.Title);
        Assert.IsNull(viewModel.Message);
        Assert.HasCount(1, viewModel.OrderList);

        Assert.AreEqual(orderUuid, viewModel.OrderList[0].OrderUuid);
        Assert.AreEqual("2026/07/09 10:30:45", viewModel.OrderList[0].OrderDate);
        Assert.AreEqual("yamada01", viewModel.OrderList[0].CustomerAccountName);
        Assert.AreEqual("食品 × 2", viewModel.OrderList[0].OrderContent);
        Assert.AreEqual("発送準備中", viewModel.OrderList[0].OrderStatus);
        Assert.AreEqual($"/admin/order/status/update/{orderUuid}", viewModel.OrderList[0].StatusUpdateUrl);

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(null, null),
            Times.Once);
    }

    /// <summary>
    /// Getで注文履歴が0件の場合、注文未登録メッセージを設定すること
    /// </summary>
    [TestMethod(DisplayName = "Get_注文履歴が0件の場合は注文未登録メッセージを設定する")]
    public async Task Get_WhenOrdersIsEmpty_ReturnsOkWithNoOrderMessage()
    {
        // Arrange
        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(null, null))
            .ReturnsAsync(new List<OrdersModel>());

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var viewModel = okResult.Value as SearchOrdersResultViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("購入履歴検索", viewModel.Title);
        Assert.IsEmpty(viewModel.OrderList);
        Assert.AreEqual("注文が登録されていません", viewModel.Message);

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(null, null),
            Times.Once);
    }

    /// <summary>
    /// GetでInternalExceptionが発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Get_InternalExceptionが発生した場合はInternalServerErrorを返す")]
    public async Task Get_WhenInternalExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(null, null))
            .ThrowsAsync(new InternalException("注文情報の取得に失敗しました。"));

        // Act
        var result = await _controller.Get();

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("ORDER_DATA_FETCH_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("注文情報の取得に失敗しました", GetPropertyValue<string>(objectResult.Value!, "message"));

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(null, null),
            Times.Once);
    }

    /// <summary>
    /// Getで予期しない例外が発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Get_予期しない例外が発生した場合はInternalServerErrorを返す")]
    public async Task Get_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(null, null))
            .ThrowsAsync(new InvalidOperationException("DB接続エラーです。"));

        // Act
        var result = await _controller.Get();

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("SYSTEM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("注文情報の取得に失敗しました", GetPropertyValue<string>(objectResult.Value!, "message"));

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(null, null),
            Times.Once);
    }

    /// <summary>
    /// Searchで購入日と顧客アカウント名を条件に購入履歴を検索できること
    /// </summary>
    [TestMethod(DisplayName = "Search_購入日と顧客アカウント名で購入履歴を検索できる")]
    public async Task Search_WithOrderDateAndCustomerAccountName_ReturnsOk()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var orders = new List<OrdersModel>
        {
            CreateOrder(
                orderUuid,
                new DateTime(2026, 7, 9, 10, 30, 45),
                "yamada01",
                "発送準備中",
                new List<OrdersDetail>
                {
                    CreateOrdersDetail("食品", 2),
                    CreateOrdersDetail("飲料", 3)
                })
        };

        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(
                new DateTime(2026, 7, 9),
                "yamada01"))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.Search("2026-07-09", " yamada01 ");

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var viewModel = okResult.Value as SearchOrdersResultViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("購入履歴検索", viewModel.Title);
        Assert.IsNull(viewModel.Message);
        Assert.HasCount(1, viewModel.OrderList);

        Assert.AreEqual(orderUuid, viewModel.OrderList[0].OrderUuid);
        Assert.AreEqual("2026/07/09 10:30:45", viewModel.OrderList[0].OrderDate);
        Assert.AreEqual("yamada01", viewModel.OrderList[0].CustomerAccountName);
        Assert.AreEqual("食品 × 2、飲料 × 3", viewModel.OrderList[0].OrderContent);
        Assert.AreEqual("発送準備中", viewModel.OrderList[0].OrderStatus);
        Assert.AreEqual($"/admin/order/status/update/{orderUuid}", viewModel.OrderList[0].StatusUpdateUrl);

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(
                new DateTime(2026, 7, 9),
                "yamada01"),
            Times.Once);
    }

    /// <summary>
    /// Searchで検索条件なしの場合、購入履歴を検索できること
    /// </summary>
    [TestMethod(DisplayName = "Search_検索条件なしで購入履歴を検索できる")]
    public async Task Search_WithoutConditions_ReturnsOk()
    {
        // Arrange
        var orderUuid = Guid.NewGuid();

        var orders = new List<OrdersModel>
        {
            CreateOrder(
                orderUuid,
                new DateTime(2026, 7, 9, 10, 30, 45),
                "sato01",
                "注文受付",
                new List<OrdersDetail>
                {
                    CreateOrdersDetail("書籍", 1)
                })
        };

        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(null, null))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.Search(null, null);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var viewModel = okResult.Value as SearchOrdersResultViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("購入履歴検索", viewModel.Title);
        Assert.IsNull(viewModel.Message);
        Assert.HasCount(1, viewModel.OrderList);

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(null, null),
            Times.Once);
    }

    /// <summary>
    /// Searchで検索結果が0件の場合、該当なしメッセージを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Search_検索結果が0件の場合は該当なしメッセージを返す")]
    public async Task Search_WhenOrdersIsEmpty_ReturnsOkWithMessage()
    {
        // Arrange
        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(null, null))
            .ReturnsAsync(new List<OrdersModel>());

        // Act
        var result = await _controller.Search(null, null);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var viewModel = okResult.Value as SearchOrdersResultViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("購入履歴検索", viewModel.Title);
        Assert.IsEmpty(viewModel.OrderList);
        Assert.AreEqual("該当する注文が見つかりませんでした", viewModel.Message);

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(null, null),
            Times.Once);
    }

    /// <summary>
    /// Searchで購入日の形式が不正な場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Search_購入日の形式が不正な場合はBadRequestを返す")]
    public async Task Search_WhenOrderDateIsInvalid_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Search("invalid-date", "yamada01");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("購入日の形式が不正です。", GetPropertyValue<string>(badRequestResult.Value!, "message"));

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(It.IsAny<DateTime?>(), It.IsAny<string?>()),
            Times.Never);
    }

    /// <summary>
    /// SearchでUsecaseからDomainExceptionが発生した場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Search_UsecaseからDomainExceptionが発生した場合はBadRequestを返す")]
    public async Task Search_WhenDomainExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(
                new DateTime(2026, 7, 9),
                "yamada01"))
            .ThrowsAsync(new DomainException("検索条件が不正です。"));

        // Act
        var result = await _controller.Search("2026-07-09", "yamada01");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("検索条件が不正です。", GetPropertyValue<string>(badRequestResult.Value!, "message"));

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(
                new DateTime(2026, 7, 9),
                "yamada01"),
            Times.Once);
    }

    /// <summary>
    /// SearchでInternalExceptionが発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Search_InternalExceptionが発生した場合はInternalServerErrorを返す")]
    public async Task Search_WhenInternalExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(
                new DateTime(2026, 7, 9),
                "yamada01"))
            .ThrowsAsync(new InternalException("注文情報の取得に失敗しました。"));

        // Act
        var result = await _controller.Search("2026-07-09", "yamada01");

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("ORDER_DATA_FETCH_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("注文情報の取得に失敗しました", GetPropertyValue<string>(objectResult.Value!, "message"));

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(
                new DateTime(2026, 7, 9),
                "yamada01"),
            Times.Once);
    }

    /// <summary>
    /// Searchで予期しない例外が発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Search_予期しない例外が発生した場合はInternalServerErrorを返す")]
    public async Task Search_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _searchOrdersUsecaseMock
            .Setup(x => x.SearchAsync(
                new DateTime(2026, 7, 9),
                "yamada01"))
            .ThrowsAsync(new InvalidOperationException("DB接続エラーです。"));

        // Act
        var result = await _controller.Search("2026-07-09", "yamada01");

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("SYSTEM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("注文情報の取得に失敗しました", GetPropertyValue<string>(objectResult.Value!, "message"));

        _searchOrdersUsecaseMock.Verify(
            x => x.SearchAsync(
                new DateTime(2026, 7, 9),
                "yamada01"),
            Times.Once);
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

    /// <summary>
    /// 匿名型オブジェクトから指定したプロパティの値を取得する
    /// </summary>
    private static T GetPropertyValue<T>(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName);

        if (property is null)
        {
            throw new InvalidOperationException(
                $"{propertyName}プロパティが見つかりません。");
        }

        return (T)property.GetValue(target)!;
    }
}