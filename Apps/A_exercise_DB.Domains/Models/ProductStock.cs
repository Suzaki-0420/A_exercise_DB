using A_exercise_DB.Domains.Exceptions;
namespace A_exercise_DB.Domains.Models;
/// <summary>
/// 商品在庫を表すドメインオブジェクト
/// </summary>
public class ProductStock
{
    /// <summary>
    /// 商品在庫識別ID(UUID)
    /// </summary>
    public Guid StockUuid { get; private set; }
    /// <summary>
    /// 商品在庫数
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ProductStock(Guid stockUuid, string quantity)
    {
        ValidateProductStockUuid(stockUuid);
        StockUuid = stockUuid;
        Quantity = ValidateQuantity(quantity);
    }

    /// <summary>
    /// ID未定の商品在庫を作成する場合のコンストラクタ
    /// </summary>
    public ProductStock(string quantity)
        : this(Guid.NewGuid(), quantity) { }

    /// <summary>
    /// 商品在庫識別IDの検証
    /// </summary>
    private void ValidateProductStockUuid(Guid stockUuid)
    {
        if (stockUuid == Guid.Empty)
            throw new DomainException("商品在庫識別IDが不正です");
    }

    /// <summary>
    /// 商品在庫数の検証
    /// </summary>
    private int ValidateQuantity(string? quantity)
    {
        if (string.IsNullOrWhiteSpace(quantity))
            throw new DomainException("在庫数を入力してください");

        if (!int.TryParse(quantity, out var parsedQuantity))
            throw new DomainException("正しい在庫数形式で入力してください");

        if (parsedQuantity < 0)
            throw new DomainException("在庫数は0以上で入力してください");

        return parsedQuantity;
    }
    /// <summary>
    /// 等価性（IDによる比較）
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not ProductStock other) return false;
        return StockUuid == other.StockUuid;
    }

    public override int GetHashCode() => StockUuid.GetHashCode();

    public override string ToString()
        => $"{StockUuid.ToString() ?? "未登録"}: {Quantity}";
}