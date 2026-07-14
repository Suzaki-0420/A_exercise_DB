using A_exercise_DB.Applications.Params;

namespace A_exercise_DB.Applications.Interfaces;

/// <summary>
/// 画像を検証して保存するユースケース
/// </summary>
public interface IImageUploadUsecase
{
    /// <summary>
    /// 画像を保存し、公開URLを返す
    /// </summary>
    Task<string> ExecuteAsync(
        ImageUploadParam param);
}