using A_exercise_DB.Domains.Exceptions;
namespace A_exercise_DB.Domains.Models;
/// <summary>
/// 注文明細を表すドメインオブジェクト
/// </summary>
public class OrdersDetail
{
    /// <summary>
    /// 注文明細ID
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// 注文
    /// </summary>
    public Orders? Orders { get; private set; }
    /// <summary>
    /// 商品
    /// </summary>
    public Product? Product { get; private set; }
    /// <summary>
    /// 合計金額
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public OrdersDetail(int detailId, Orders? orders, Product? product, string count)
    {
        ValidateDetailId(detailId);
        Id = detailId;
        Orders = orders ?? throw new DomainException("注文は必須です。");
        Product = product ?? throw new DomainException("商品は必須です。");
        Count = ValidateCount(count);
    }

    /// <summary>
    /// ID未定の注文明細を作成する場合のコンストラクタ
    /// </summary>
    public OrdersDetail(Orders? orders, Product? product, string count) { }

    /// <summary>
    /// 注文・商品なしコンストラクタ
    /// </summary>
    public OrdersDetail(string count) { }


    /// <summary>
    /// 注文明細IDの検証
    /// </summary>
    private void ValidateDetailId(int? detailId)
    {
        if (detailId is not null && detailId <= 0)
            throw new DomainException("注文明細IDが不正です。");
    }

    /// <summary>
    /// 合計金額の検証
    /// </summary>
    private int ValidateCount(string? count)
    {
        if (string.IsNullOrWhiteSpace(count))
            throw new DomainException("合計金額を入力してください");

        if (!int.TryParse(count, out var parsedCount))
            throw new DomainException("正しい合計金額形式で入力してください");

        if (parsedCount < 0)
            throw new DomainException("合計金額は0以上で入力してください");

        return parsedCount;
    }

    /// <summary>
    /// 等価性（IDによる比較）
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not OrdersDetail other) return false;

        return Id == other.Id;
    }

    public override string ToString()
        => $"{Id.ToString() ?? "未登録"}: {Count} / {Orders?.OrderUuid} / {Product?.Name}";
}