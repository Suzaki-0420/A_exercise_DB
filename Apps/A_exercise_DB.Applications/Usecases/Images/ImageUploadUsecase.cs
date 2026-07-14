using A_exercise_DB.Applications.Interfaces;
using A_exercise_DB.Applications.Params;
using A_exercise_DB.Domains.Exceptions;

namespace A_exercise_DB.Applications.Usecases.Images;

/// <summary>
/// 画像の検証と保存を行うユースケース
/// </summary>
public class ImageUploadUsecase
    : IImageUploadUsecase
{
    private const long MaxFileSize =
        2 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private readonly IImageStorage _imageStorage;

    public ImageUploadUsecase(
        IImageStorage imageStorage)
    {
        _imageStorage = imageStorage;
    }

    public async Task<string> ExecuteAsync(
        ImageUploadParam param)
    {
        ArgumentNullException.ThrowIfNull(param);
        ArgumentNullException.ThrowIfNull(param.Content);

        if (param.Length <= 0)
        {
            throw new DomainException(
                "画像ファイルが空です。");
        }

        if (param.Length > MaxFileSize)
        {
            throw new DomainException(
                "画像のファイルサイズは2MB以下にしてください。");
        }

        var extension =
            Path.GetExtension(param.FileName)
                .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new DomainException(
                "jpg、jpeg、png、webp形式の画像を指定してください。");
        }

        if (!AllowedContentTypes.Contains(
                param.ContentType))
        {
            throw new DomainException(
                "画像ファイルの形式が正しくありません。");
        }

        var savedFileName =
            $"{Guid.NewGuid()}{extension}";

        return await _imageStorage.SaveAsync(
            param.Content,
            savedFileName);
    }
}