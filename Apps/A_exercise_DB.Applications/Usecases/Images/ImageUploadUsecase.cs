using A_exercise_DB.Applications.Interfaces;
using A_exercise_DB.Applications.Params;
using A_exercise_DB.Domains.Exceptions;

using SixLabors.ImageSharp;

namespace A_exercise_DB.Applications.Usecases.Images;

/// <summary>
/// 画像の検証と保存を行うユースケース
/// </summary>
public class ImageUploadUsecase
    : IImageUploadUsecase
{
    /// <summary>
    /// 画像ファイルの最大サイズ
    /// </summary>
    private const long MaxFileSize =
        2 * 1024 * 1024;

    /// <summary>
    /// 画像の最大横幅
    /// </summary>
    private const int MaxImageWidth = 1000;

    /// <summary>
    /// 画像の最大縦幅
    /// </summary>
    private const int MaxImageHeight = 1000;

    /// <summary>
    /// 許可する画像拡張子
    /// </summary>
    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    /// <summary>
    /// 許可するContent-Type
    /// </summary>
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

    /// <summary>
    /// 画像を検証して保存する
    /// </summary>
    /// <param name="param">画像アップロード情報</param>
    /// <returns>保存した画像のURL</returns>
    public async Task<string> ExecuteAsync(
        ImageUploadParam param)
    {
        ArgumentNullException.ThrowIfNull(param);
        ArgumentNullException.ThrowIfNull(param.Content);

        /*
         * ファイルが空でないことを確認する。
         */
        if (param.Length <= 0)
        {
            throw new DomainException(
                "画像ファイルが空です。");
        }

        /*
         * ファイルサイズを確認する。
         */
        if (param.Length > MaxFileSize)
        {
            throw new DomainException(
                "画像のファイルサイズは2MB以下にしてください。");
        }

        /*
         * 拡張子を確認する。
         */
        var extension =
            Path.GetExtension(param.FileName)
                .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new DomainException(
                "jpg、jpeg、png、webp形式の画像を指定してください。");
        }

        /*
         * Content-Typeを確認する。
         */
        if (!AllowedContentTypes.Contains(
                param.ContentType))
        {
            throw new DomainException(
                "画像ファイルの形式が正しくありません。");
        }

        /*
         * 画像の縦横サイズを確認する。
         */
        await ValidateImageDimensionsAsync(
            param.Content);

        /*
         * 画像確認によってStreamの読み取り位置が
         * 末尾へ移動する可能性があるため、
         * 保存前に先頭へ戻す。
         */
        if (param.Content.CanSeek)
        {
            param.Content.Position = 0;
        }

        var savedFileName =
            $"{Guid.NewGuid()}{extension}";

        return await _imageStorage.SaveAsync(
            param.Content,
            savedFileName);
    }

    /// <summary>
    /// 画像の縦幅と横幅を検証する
    /// </summary>
    /// <param name="content">画像内容</param>
    /// <returns>検証完了を表すタスク</returns>
    private static async Task ValidateImageDimensionsAsync(
        Stream content)
    {
        try
        {
            /*
             * 念のため読み取り位置を先頭へ戻す。
             */
            if (content.CanSeek)
            {
                content.Position = 0;
            }

            /*
             * 画像を完全展開せず、
             * 幅・高さなどの情報だけ読み取る。
             */
            var imageInfo =
                await Image.IdentifyAsync(content);

            /*
             * 横幅または縦幅のどちらかが
             * 1000pxを超えた場合はエラーにする。
             */
            if (imageInfo.Width > MaxImageWidth ||
                imageInfo.Height > MaxImageHeight)
            {
                throw new DomainException(
                    "画像の縦横サイズは1000px以下にしてください。");
            }
        }
        catch (DomainException)
        {
            /*
             * このメソッド内で発生させた
             * DomainExceptionはそのまま返す。
             */
            throw;
        }
        catch (UnknownImageFormatException ex)
        {
            throw new DomainException(
                "画像ファイルの形式が正しくありません。",
                ex);
        }
        catch (InvalidImageContentException ex)
        {
            throw new DomainException(
                "画像ファイルが破損しているか、内容が正しくありません。",
                ex);
        }
        finally
        {
            /*
             * 後続の保存処理で先頭から読み込めるようにする。
             */
            if (content.CanSeek)
            {
                content.Position = 0;
            }
        }
    }
}