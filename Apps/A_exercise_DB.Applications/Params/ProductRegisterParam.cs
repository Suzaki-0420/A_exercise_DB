namespace A_exercise_DB.Applications.Params;

/// <summary>
/// 商品登録ユースケースの入力値
/// </summary>
/// <remarks>
/// Applications層がIFormFileへ依存しないよう、
/// Streamと画像のメタ情報を受け取る。
/// </remarks>
public sealed record ProductRegisterParam(
    string Name,
    int Price,
    Guid CategoryId,
    int Quantity,
    Stream? ImageContent = null,
    string? ImageFileName = null,
    string? ImageContentType = null,
    long ImageLength = 0);