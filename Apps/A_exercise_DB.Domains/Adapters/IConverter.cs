namespace A_Exercise_DB.Domains.Adapters;

/// <summary>
/// DomainObjectをEntityに変換するアダプターのインターフェース
/// </summary>
public interface IConverter<TDomain, TTarget>
{
    Task<TTarget> ConvertAsync(TDomain domain);
}