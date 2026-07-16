using A_exercise_DB.Applications.Interfaces;
using A_exercise_DB.Domains.Exceptions;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.Extensions.Options;

namespace A_exercise_DB.Infrastructures.Storage;

/// <summary>
/// Azure Blob Storageへ画像を保存する実装
/// </summary>
public class AzureBlobImageStorage : IImageStorage
{
    private readonly BlobContainerClient _containerClient;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options">Azure Blob Storageの設定</param>
    public AzureBlobImageStorage(
        IOptions<AzureBlobStorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var storageOptions = options.Value;

        if (string.IsNullOrWhiteSpace(
            storageOptions.ConnectionString))
        {
            throw new InvalidOperationException(
                "Azure Blob Storageの接続文字列が設定されていません。");
        }

        if (string.IsNullOrWhiteSpace(
            storageOptions.ContainerName))
        {
            throw new InvalidOperationException(
                "Azure Blob Storageのコンテナ名が設定されていません。");
        }

        _containerClient = new BlobContainerClient(
            storageOptions.ConnectionString,
            storageOptions.ContainerName);
    }

    /// <inheritdoc />
    public async Task<string> SaveAsync(
        Stream content,
        string fileName)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        try
        {
            await _containerClient.CreateIfNotExistsAsync(
                PublicAccessType.Blob);

            var blobClient =
                _containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(
                content,
                overwrite: false);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
            when (ex is Azure.RequestFailedException)
        {
            throw new InternalException(
                "画像の保存に失敗しました。",
                ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string imageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);

        try
        {
            var uri = new Uri(imageUrl);
            var fileName =
                Uri.UnescapeDataString(
                    Path.GetFileName(uri.AbsolutePath));

            var blobClient =
                _containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
            when (ex is Azure.RequestFailedException
                or UriFormatException)
        {
            throw new InternalException(
                "画像の削除に失敗しました。",
                ex);
        }
    }
}