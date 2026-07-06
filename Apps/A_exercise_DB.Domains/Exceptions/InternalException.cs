namespace RestAPI_Exercise.Application.Exceptions;
/// <summary>
/// 内部エラーを表す例外クラス
/// </summary>
public class InternalException : Exception
{
    /// <summary>
    /// 内部例外
    /// </summary>
    /// <param name="message">例外メッセージ</param>
    public InternalException(string message) : base(message) { }
    /// <summary>
    /// 内部例外
    /// </summary>
    /// <param name="message">例外メッセージ</param>
    /// <param name="innerException"></param>
    public InternalException(string message, Exception innerException) : base(message, innerException) { }
}