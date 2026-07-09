using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Presentations.ViewModels;
using OrdersModel = A_exercise_DB.Domains.Models.Orders;

namespace A_exercise_DB.Presentations.Adapters;

/// <summary>
/// 購入履歴検索ViewModelアダプタ
/// </summary>
public class SearchOrdersViewModelAdapter
{
    /// <summary>
    /// 検索条件ViewModelから購入日を復元する
    /// </summary>
    /// <param name="model">購入履歴検索条件ViewModel</param>
    /// <returns>購入日</returns>
    public DateTime? RestoreOrderDate(SearchOrdersViewModel model)
    {
        _ = model ?? throw new InternalException("引数modelがnullです。");

        if (string.IsNullOrWhiteSpace(model.OrderDate))
        {
            return null;
        }

        if (!DateTime.TryParse(model.OrderDate, out var orderDate))
        {
            throw new DomainException("購入日の形式が不正です。");
        }

        return orderDate.Date;
    }

    /// <summary>
    /// 検索条件ViewModelから顧客アカウント名を復元する
    /// </summary>
    /// <param name="model">購入履歴検索条件ViewModel</param>
    /// <returns>顧客アカウント名</returns>
    public string? RestoreCustomerAccountName(SearchOrdersViewModel model)
    {
        _ = model ?? throw new InternalException("引数modelがnullです。");

        if (string.IsNullOrWhiteSpace(model.CustomerAccountName))
        {
            return null;
        }

        return model.CustomerAccountName.Trim();
    }

    /// <summary>
    /// Ordersの一覧から検索結果ViewModelへ変換する
    /// </summary>
    /// <param name="orders">注文一覧</param>
    /// <returns>購入履歴検索結果ViewModel</returns>
    public SearchOrdersResultViewModel ConvertToResultViewModel(List<OrdersModel> orders)
    {
        _ = orders ?? throw new InternalException("引数ordersがnullです。");

        var result = new SearchOrdersResultViewModel
        {
            OrderList = orders
                .Select(ConvertToListItemViewModel)
                .ToList()
        };

        if (result.OrderList.Count == 0)
        {
            result.Message = "該当する注文が見つかりませんでした";
        }

        return result;
    }

    /// <summary>
    /// Ordersから検索結果1行分ViewModelへ変換する
    /// </summary>
    /// <param name="order">注文</param>
    /// <returns>購入履歴検索結果1行分ViewModel</returns>
    private SearchOrdersListItemViewModel ConvertToListItemViewModel(OrdersModel order)
    {
        _ = order ?? throw new InternalException("引数orderがnullです。");

        return new SearchOrdersListItemViewModel
        {
            OrderUuid = order.OrderUuid,
            OrderDate = order.OrderDate.ToString("yyyy/MM/dd HH:mm:ss"),
            CustomerAccountName = order.Customer.Username,
            OrderContent = CreateOrderContent(order),
            OrderStatus = order.OrderStatus.Name,
            StatusUpdateUrl = $"/admin/order/status/update/{order.OrderUuid}"
        };
    }

    /// <summary>
    /// 注文内容の表示文字列を作成する
    /// </summary>
    /// <param name="order">注文</param>
    /// <returns>注文内容</returns>
    private string CreateOrderContent(OrdersModel order)
    {
        if (order.OrdersDetails.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(
            "、",
            order.OrdersDetails.Select(detail =>
                $"{detail.Product.Name} × {detail.Count}"
            )
        );
    }
}