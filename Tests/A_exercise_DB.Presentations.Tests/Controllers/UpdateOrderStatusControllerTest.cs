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
/// UpdateOrderStatusControllerの単体テスト
/// </summary>
[TestClass]
public class UpdateOrderStatusControllerTest
{
    private Mock<IUpdateOrderStatusUsecase> _updateOrderStatusUsecaseMock = null!;
    private UpdateOrderStatusViewModelAdapter _adapter = null!;
    private Mock<ILogger<UpdateOrderStatusController>> _loggerMock = null!;
    private UpdateOrderStatusController _controller = null!;

    /// <summary>
    /// 各テスト実行前の初期化処理
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _updateOrderStatusUsecaseMock = new Mock<IUpdateOrderStatusUsecase>();
        _adapter = new UpdateOrderStatusViewModelAdapter();
        _loggerMock = new Mock<ILogger<UpdateOrderStatusController>>();

        _controller = new UpdateOrderStatusController(
            _updateOrderStatusUsecaseMock.Object,
            _adapter,
            _loggerMock.Object);
    }

    /// <summary>
    /// GetInputで注文ステータス更新入力画面の表示情報を取得できること
    /// </summary>
    [TestMethod(DisplayName = "GetInput_注文ステータス更新入力画面の表示情報を取得できる")]
    public async Task GetInput_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var order = CreateOrder(
            orderId,
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
            CreateOrderStatus(2, "発送準備中")
        };

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetInputAsync(orderId))
            .ReturnsAsync((order, orderStatuses));

        // Act
        var result = await _controller.GetInput(orderId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var viewModel = okResult.Value as UpdateOrderStatusInputViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(orderId, viewModel.OrderId);
        Assert.AreEqual("2026/07/10 10:30:45", viewModel.OrderDate);
        Assert.AreEqual("yamada01", viewModel.CustomerAccountName);
        Assert.AreEqual("食品 × 2、飲料 × 3", viewModel.OrderContent);
        Assert.AreEqual(1, viewModel.OrderStatus);
        Assert.HasCount(2, viewModel.OrderStatusList);
        Assert.AreEqual(1, viewModel.OrderStatusList[0].Id);
        Assert.AreEqual("注文受付", viewModel.OrderStatusList[0].Name);
        Assert.AreEqual(2, viewModel.OrderStatusList[1].Id);
        Assert.AreEqual("発送準備中", viewModel.OrderStatusList[1].Name);

        _updateOrderStatusUsecaseMock.Verify(
            x => x.GetInputAsync(orderId),
            Times.Once);
    }

    /// <summary>
    /// GetInputで注文が存在しない場合、NotFoundを返すこと
    /// </summary>
    [TestMethod(DisplayName = "GetInput_注文が存在しない場合はNotFoundを返す")]
    public async Task GetInput_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetInputAsync(orderId))
            .ReturnsAsync(((OrdersModel Order, List<OrderStatus> OrderStatuses)?)null);

        // Act
        var result = await _controller.GetInput(orderId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("ORDER_NOT_FOUND", GetPropertyValue<string>(notFoundResult.Value!, "code"));
        Assert.AreEqual("指定された注文は存在しません", GetPropertyValue<string>(notFoundResult.Value!, "message"));
        Assert.AreEqual("/admin/order/search", GetPropertyValue<string>(notFoundResult.Value!, "redirectUrl"));

        _updateOrderStatusUsecaseMock.Verify(
            x => x.GetInputAsync(orderId),
            Times.Once);
    }

    /// <summary>
    /// GetInputでDomainExceptionが発生した場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "GetInput_DomainExceptionが発生した場合はBadRequestを返す")]
    public async Task GetInput_WhenDomainExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.Empty;

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetInputAsync(orderId))
            .ThrowsAsync(new DomainException("注文IDが不正です。"));

        // Act
        var result = await _controller.GetInput(orderId);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("注文IDが不正です。", GetPropertyValue<string>(badRequestResult.Value!, "message"));
    }

    /// <summary>
    /// GetInputでInternalExceptionが発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "GetInput_InternalExceptionが発生した場合はInternalServerErrorを返す")]
    public async Task GetInput_WhenInternalExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetInputAsync(orderId))
            .ThrowsAsync(new InternalException("注文情報の取得に失敗しました。"));

        // Act
        var result = await _controller.GetInput(orderId);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("ORDER_STATUS_INPUT_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("注文情報の取得に失敗しました", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// GetInputで予期しない例外が発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "GetInput_予期しない例外が発生した場合はInternalServerErrorを返す")]
    public async Task GetInput_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetInputAsync(orderId))
            .ThrowsAsync(new InvalidOperationException("DB接続エラーです。"));

        // Act
        var result = await _controller.GetInput(orderId);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("SYSTEM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// Confirmで注文ステータス更新確認画面の表示情報を取得できること
    /// </summary>
    [TestMethod(DisplayName = "Confirm_注文ステータス更新確認画面の表示情報を取得できる")]
    public async Task Confirm_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 2
        };

        var order = CreateOrder(
            orderId,
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            CreateOrderStatus(1, "注文受付"),
            new List<OrdersDetail>
            {
                CreateOrdersDetail("食品", 2)
            });

        var newStatus = CreateOrderStatus(2, "発送準備中");

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetConfirmAsync(orderId, 2))
            .ReturnsAsync((order, newStatus));

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var viewModel = okResult.Value as UpdateOrderStatusConfirmViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(orderId, viewModel.OrderId);
        Assert.AreEqual("2026/07/10 10:30:45", viewModel.OrderDate);
        Assert.AreEqual("yamada01", viewModel.CustomerAccountName);
        Assert.AreEqual("注文受付", viewModel.CurrentStatus);
        Assert.AreEqual(2, viewModel.NewStatusId);
        Assert.AreEqual("発送準備中", viewModel.NewStatus);

        _updateOrderStatusUsecaseMock.Verify(
            x => x.GetConfirmAsync(orderId, 2),
            Times.Once);
    }

    /// <summary>
    /// ConfirmでModelStateが不正な場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_ModelStateが不正な場合はBadRequestを返す")]
    public async Task Confirm_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = Guid.NewGuid(),
            NewStatusId = 2
        };

        _controller.ModelState.AddModelError("NewStatusId", "注文ステータスは必須です。");

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("入力内容が不正です", GetPropertyValue<string>(badRequestResult.Value!, "message"));

        _updateOrderStatusUsecaseMock.Verify(
            x => x.GetConfirmAsync(It.IsAny<Guid>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Confirmで注文が存在しない場合、NotFoundを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_注文が存在しない場合はNotFoundを返す")]
    public async Task Confirm_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 2
        };

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetConfirmAsync(orderId, 2))
            .ReturnsAsync(((OrdersModel Order, OrderStatus NewStatus)?)null);

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("ORDER_NOT_FOUND", GetPropertyValue<string>(notFoundResult.Value!, "code"));
        Assert.AreEqual("指定された注文は存在しません", GetPropertyValue<string>(notFoundResult.Value!, "message"));
        Assert.AreEqual("/admin/order/search", GetPropertyValue<string>(notFoundResult.Value!, "redirectUrl"));

        _updateOrderStatusUsecaseMock.Verify(
            x => x.GetConfirmAsync(orderId, 2),
            Times.Once);
    }

    /// <summary>
    /// ConfirmでDomainExceptionが発生した場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_DomainExceptionが発生した場合はBadRequestを返す")]
    public async Task Confirm_WhenDomainExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 0
        };

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetConfirmAsync(orderId, 0))
            .ThrowsAsync(new DomainException("注文ステータスが不正です。"));

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("注文ステータスが不正です。", GetPropertyValue<string>(badRequestResult.Value!, "message"));
    }

    /// <summary>
    /// ConfirmでInternalExceptionが発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_InternalExceptionが発生した場合はInternalServerErrorを返す")]
    public async Task Confirm_WhenInternalExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 2
        };

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetConfirmAsync(orderId, 2))
            .ThrowsAsync(new InternalException("注文情報の取得に失敗しました。"));

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("ORDER_STATUS_CONFIRM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// Confirmで予期しない例外が発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Confirm_予期しない例外が発生した場合はInternalServerErrorを返す")]
    public async Task Confirm_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 2
        };

        _updateOrderStatusUsecaseMock
            .Setup(x => x.GetConfirmAsync(orderId, 2))
            .ThrowsAsync(new InvalidOperationException("DB接続エラーです。"));

        // Act
        var result = await _controller.Confirm(model);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("SYSTEM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// Completeで注文ステータスを更新できること
    /// </summary>
    [TestMethod(DisplayName = "Complete_注文ステータスを更新できる")]
    public async Task Complete_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 2
        };

        var newStatus = CreateOrderStatus(2, "発送準備中");

        var order = CreateOrder(
            orderId,
            new DateTime(2026, 7, 10, 10, 30, 45),
            "yamada01",
            newStatus,
            new List<OrdersDetail>
            {
                CreateOrdersDetail("食品", 2)
            });

        _updateOrderStatusUsecaseMock
            .Setup(x => x.UpdateAsync(orderId, 2))
            .ReturnsAsync((order, newStatus));

        // Act
        var result = await _controller.Complete(model);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var viewModel = okResult.Value as UpdateOrderStatusCompleteViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("注文ステータスを更新しました", viewModel.CompleteMsg);
        Assert.AreEqual(orderId, viewModel.OrderNumber);
        Assert.AreEqual("発送準備中", viewModel.OrderStatus);
        Assert.IsFalse(string.IsNullOrEmpty(viewModel.UpdateDate));
        Assert.AreEqual("/admin/order/search", viewModel.SearchUrl);
        Assert.AreEqual("/admin", viewModel.HomeUrl);

        _updateOrderStatusUsecaseMock.Verify(
            x => x.UpdateAsync(orderId, 2),
            Times.Once);
    }

    /// <summary>
    /// CompleteでModelStateが不正な場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Complete_ModelStateが不正な場合はBadRequestを返す")]
    public async Task Complete_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = Guid.NewGuid(),
            NewStatusId = 2
        };

        _controller.ModelState.AddModelError("NewStatusId", "注文ステータスは必須です。");

        // Act
        var result = await _controller.Complete(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("入力内容が不正です", GetPropertyValue<string>(badRequestResult.Value!, "message"));

        _updateOrderStatusUsecaseMock.Verify(
            x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<int>()),
            Times.Never);
    }

    /// <summary>
    /// Completeで注文が存在しない場合、NotFoundを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Complete_注文が存在しない場合はNotFoundを返す")]
    public async Task Complete_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 2
        };

        _updateOrderStatusUsecaseMock
            .Setup(x => x.UpdateAsync(orderId, 2))
            .ThrowsAsync(new NotFoundException("指定された注文は存在しません。"));

        // Act
        var result = await _controller.Complete(model);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("ORDER_NOT_FOUND", GetPropertyValue<string>(notFoundResult.Value!, "code"));
        Assert.AreEqual("指定された注文は存在しません。", GetPropertyValue<string>(notFoundResult.Value!, "message"));
        Assert.AreEqual("/admin/order/search", GetPropertyValue<string>(notFoundResult.Value!, "redirectUrl"));
    }

    /// <summary>
    /// CompleteでDomainExceptionが発生した場合、BadRequestを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Complete_DomainExceptionが発生した場合はBadRequestを返す")]
    public async Task Complete_WhenDomainExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 0
        };

        _updateOrderStatusUsecaseMock
            .Setup(x => x.UpdateAsync(orderId, 0))
            .ThrowsAsync(new DomainException("注文ステータスが不正です。"));

        // Act
        var result = await _controller.Complete(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("VALIDATION_ERROR", GetPropertyValue<string>(badRequestResult.Value!, "code"));
        Assert.AreEqual("注文ステータスが不正です。", GetPropertyValue<string>(badRequestResult.Value!, "message"));
    }

    /// <summary>
    /// CompleteでInternalExceptionが発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Complete_InternalExceptionが発生した場合はInternalServerErrorを返す")]
    public async Task Complete_WhenInternalExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 2
        };

        _updateOrderStatusUsecaseMock
            .Setup(x => x.UpdateAsync(orderId, 2))
            .ThrowsAsync(new InternalException("注文ステータスの更新に失敗しました。"));

        // Act
        var result = await _controller.Complete(model);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("ORDER_STATUS_UPDATE_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", GetPropertyValue<string>(objectResult.Value!, "message"));
    }

    /// <summary>
    /// Completeで予期しない例外が発生した場合、InternalServerErrorを返すこと
    /// </summary>
    [TestMethod(DisplayName = "Complete_予期しない例外が発生した場合はInternalServerErrorを返す")]
    public async Task Complete_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var model = new UpdateOrderStatusConfirmViewModel
        {
            OrderId = orderId,
            NewStatusId = 2
        };

        _updateOrderStatusUsecaseMock
            .Setup(x => x.UpdateAsync(orderId, 2))
            .ThrowsAsync(new InvalidOperationException("DB接続エラーです。"));

        // Act
        var result = await _controller.Complete(model);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("SYSTEM_ERROR", GetPropertyValue<string>(objectResult.Value!, "code"));
        Assert.AreEqual("システムエラーが発生しました。管理者に連絡してください", GetPropertyValue<string>(objectResult.Value!, "message"));
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

    /// <summary>
    /// 匿名型オブジェクトから指定したプロパティの値を取得する
    /// </summary>
    private static T GetPropertyValue<T>(
        object target,
        string propertyName)
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