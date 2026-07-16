namespace A_exercise_DB.Infrastructures.Storage;

/// <summary>
/// Azure Blob Storageの設定
/// </summary>
public class AzureBlobStorageOptions
{
    /// <summary>
    /// 設定セクション名
    /// </summary>
    public const string SectionName =
        "AzureBlobStorage";

    /// <summary>
    /// Azure Blob Storageの接続文字列
    /// </summary>
    public string ConnectionString { get; set; } =
        string.Empty;

    /// <summary>
    /// コンテナ名
    /// </summary>
    public string ContainerName { get; set; } =
        string.Empty;
}