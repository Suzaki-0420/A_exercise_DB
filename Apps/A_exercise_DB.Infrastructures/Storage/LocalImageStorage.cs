using A_exercise_DB.Applications.Interfaces;
using A_exercise_DB.Domains.Exceptions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace A_exercise_DB.Infrastructures.Storage;

/// <summary>
/// ローカルファイルシステムへ画像を保存する実装
/// </summary>
/// <remarks>
/// 第1段階の実装。第2段階では AzureBlobImageStorage へ差し替える。
/// </remarks>
public class LocalImageStorage : IImageStorage
{
    private readonly ImageStorageOptions _options;
    private readonly string _absoluteRootPath;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options">画像保存の設定</param>
    /// <param name="environment">ホスト環境</param>
    public LocalImageStorage(IOptions<ImageStorageOptions> options, IHostEnvironment environment)
    {
        _options = options.Value;
        _absoluteRootPath = Path.Combine(environment.ContentRootPath, _options.RootPath);
    }

    /// <summary>
    /// 画像を保存し、公開URLを返す
    /// </summary>
    /// <param name="content">画像の内容を読み取るストリーム</param>
    /// <param name="fileName">保存先のファイル名</param>
    /// <returns>保存された画像の公開URL</returns>
    /// <exception cref="InternalException">ファイルの書き込みに失敗した場合</exception>
    public async Task<string> SaveAsync(Stream content, string fileName)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        try
        {
            Directory.CreateDirectory(_absoluteRootPath);

            var filePath = Path.Combine(_absoluteRootPath, fileName);

            await using var fileStream = new FileStream(
                filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

            await content.CopyToAsync(fileStream);

            return $"{_options.PublicBaseUrl.TrimEnd('/')}{_options.RequestPath}/{fileName}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InternalException("画像の保存に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 指定された画像を削除する
    /// </summary>
    /// <param name="imageUrl">削除対象画像の公開URL</param>
    /// <returns>非同期処理を表すTask</returns>
    /// <exception cref="InternalException">
    /// ファイルの削除に失敗した場合
    /// </exception>
    public Task DeleteAsync(string imageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);

        try
        {
            // 公開URLからファイル名を取得する
            var fileName = Path.GetFileName(imageUrl);

            // 画像ファイルの絶対パスを作成する
            var filePath = Path.Combine(
                _absoluteRootPath,
                fileName);

            // ファイルが存在する場合のみ削除する
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
            when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InternalException(
                "画像の削除に失敗しました。",
                ex);
        }
    }

    /// <summary>
    /// ファイルが存在するか確認する
    /// </summary>
    protected virtual bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// ファイルを削除する
    /// </summary>
    protected virtual void DeleteFile(string filePath)
    {
        File.Delete(filePath);
    }
}