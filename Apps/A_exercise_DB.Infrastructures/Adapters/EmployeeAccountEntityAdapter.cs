using A_exercise_DB.Domains.Adapters;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructures.Adapters;

/// <summary>
/// ドメインオブジェクト:EmployeeAccountとEmployeeAccountEntityの相互変換クラス
/// </summary>
public class EmployeeAccountEntityAdapter :
    IConverter<EmployeeAccount, EmployeeAccountEntity>,
    IRestorer<EmployeeAccount, EmployeeAccountEntity>
{
    /// <summary>
    /// ドメインオブジェクト:EmployeeAccountをEmployeeAccountEntityに変換する
    /// </summary>
    /// <param name="domain">ドメインオブジェクト:EmployeeAccount</param>
    /// <returns>EFCore:EmployeeAccountEntity</returns>
    public Task<EmployeeAccountEntity> ConvertAsync(EmployeeAccount domain)
    {
        // 引数domainがnullの場合
        _ = domain ?? throw new InternalException("引数domainがnullです。");

        // ドメインオブジェクト:EmployeeAccountをEmployeeAccountEntityに変換する
        var entity = new EmployeeAccountEntity();
        entity.AccountUuid = domain.AccountUuid;
        entity.Name = domain.Name;
        entity.Password = domain.Password;
        entity.EmployeeId = domain.EmployeeId;

        return Task.FromResult(entity);
    }

    /// <summary>
    /// EmployeeAccountEntityからドメインオブジェクト:EmployeeAccountを復元する
    /// </summary>
    /// <param name="target">EFCore:EmployeeAccountEntity</param>
    /// <returns>ドメインオブジェクト:EmployeeAccount</returns>
    public Task<EmployeeAccount> RestoreAsync(EmployeeAccountEntity target)
    {
        // 引数targetがnullの場合
        _ = target ?? throw new InternalException("引数targetがnullです。");

        // EmployeeAccountEntityからドメインオブジェクト:EmployeeAccountを復元する
        var domain = new EmployeeAccount(
            target.AccountUuid,
            target.Name,
            target.Password,
            target.EmployeeId
        );

        return Task.FromResult(domain);
    }
}