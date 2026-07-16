namespace A_exercise_DB.Applications.Params;

/// <summary>
/// 画像アップロードの入力値
/// </summary>
public sealed record ImageUploadParam(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);