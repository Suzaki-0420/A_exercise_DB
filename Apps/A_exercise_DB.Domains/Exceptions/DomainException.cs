namespace A_Exercise_DB.Domains.Exceptions;
/// <summary>
/// 業務ルール違反を表す例外クラス
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// ドメイン例外
    /// </summary>
    /// <param name="message">例外メッセージ</param>
    public DomainException(string message) : base(message) { }
    /// <summary>
    /// ドメイン例外
    /// </summary>
    /// <param name="message">例外メッセージ</param>
    /// <param name="innerException"></param>
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}