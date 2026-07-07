using A_exercise_DB.Domains.Exceptions;
namespace A_exercise_DB.Domains.Models;
/// <summary>
/// 注文を表すドメインオブジェクト
/// </summary>
public class Orders
{
    /// <summary>
    /// 注文識別ID(UUID)
    /// </summary>
    public Guid? OrderUuid { get; private set; }
    /// <summary>
    /// 注文日
    /// </summary>
    public DateTime OrderDate { get; private set; }
    /// <summary>
    /// 合計金額
    /// </summary>
    public int AmountTotal { get; private set; }
    /// <summary>
    /// 顧客
    /// </summary>
    public Customer? Customer { get; private set; }
    /// <summary>
    /// 注文ステータス
    /// </summary>
    public OrderStatus? OrderStatus { get; private set; }
    /// <summary>
    /// 支払い方法
    /// </summary>
    public PaymentMethod? PaymentMethod { get; private set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public Orders(Guid? orderUuid, DateTime orderDate, string amountTotal, Customer? customer, OrderStatus? orderStatus, PaymentMethod? paymentMethod)
    {
        ValidateOrderUuid(orderUuid);
        OrderUuid = orderUuid;
        ValidateOrderDate(orderDate);
        OrderDate = orderDate;
        AmountTotal = ValidateAmountTotal(amountTotal);
        Customer = customer ?? throw new DomainException("顧客は必須です。");
        OrderStatus = orderStatus ?? throw new DomainException("注文ステータスは必須です。");
        PaymentMethod = paymentMethod ?? throw new DomainException("支払い方法は必須です。");
    }

    /// <summary>
    /// ID未定の注文を作成する場合のコンストラクタ
    /// </summary>
    public Orders(DateTime orderDate, string amountTotal, Customer? customer, OrderStatus? orderStatus, PaymentMethod? paymentMethod)
        : this(null, orderDate, amountTotal, customer, orderStatus, paymentMethod) { }

    /// <summary>
    /// 注文識別IDの検証
    /// </summary>
    private void ValidateOrderUuid(Guid? orderUuid)
    {
        if (orderUuid == Guid.Empty)
            throw new DomainException("注文識別IDが不正です");
    }

    /// <summary>
    /// 注文日の検証
    /// </summary>
    private void ValidateOrderDate(DateTime orderDate)
    {
        if (orderDate == default)
            throw new DomainException("注文日が不正です");

        if (orderDate > DateTime.Now)
            throw new DomainException("注文日に未来日は指定できません");
    }

    /// <summary>
    /// 合計金額の検証
    /// </summary>
    private int ValidateAmountTotal(string? amountTotal)
    {
        if (string.IsNullOrWhiteSpace(amountTotal))
            throw new DomainException("合計金額を入力してください");

        if (!int.TryParse(amountTotal, out var parsedAmount))
            throw new DomainException("正しい合計金額形式で入力してください");

        if (parsedAmount < 0)
            throw new DomainException("合計金額は0以上で入力してください");

        return parsedAmount;
    }

    /// <summary>
    /// 等価性（IDによる比較）
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not Orders other) return false;
        return OrderUuid == other.OrderUuid;
    }

    public override int GetHashCode() => OrderUuid?.GetHashCode() ?? 0;

    public override string ToString()
        => $"{OrderUuid?.ToString() ?? "未登録"}: {OrderDate}, {AmountTotal} / {Customer?.Name} / {OrderStatus?.Name} / {PaymentMethod?.Name}";
}