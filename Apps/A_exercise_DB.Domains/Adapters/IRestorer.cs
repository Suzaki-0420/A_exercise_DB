namespace A_Exercise_DB.Domains.Adapters;

/// <summary>
/// EntityをDomainObjectに変換するインターフェース
/// </summary>
public interface IRestorer<TDomain, TTarget>
{
    Task<TDomain> RestoreAsync(TTarget target);
}