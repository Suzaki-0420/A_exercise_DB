using A_exercise_DB.Applications.Params;

namespace A_exercise_DB.Applications.Tests.Params;

/// <summary>
/// ImageUploadParamの単体テスト
/// </summary>
[TestClass]
public class ImageUploadParamTest
{
    /// <summary>
    /// コンストラクタで渡した値を
    /// 各プロパティから取得できること
    /// </summary>
    [TestMethod(
        DisplayName =
            "コンストラクタで渡した画像情報を保持できる")]
    public void Constructor_WhenValuesAreProvided_ShouldSetProperties()
    {
        // Arrange
        using var content =
            new MemoryStream(
                new byte[]
                {
                    1,
                    2,
                    3,
                    4
                });

        const string fileName =
            "product.png";

        const string contentType =
            "image/png";

        const long length = 4;

        // Act
        var result =
            new ImageUploadParam(
                content,
                fileName,
                contentType,
                length);

        // Assert
        Assert.AreSame(
            content,
            result.Content);

        Assert.AreEqual(
            fileName,
            result.FileName);

        Assert.AreEqual(
            contentType,
            result.ContentType);

        Assert.AreEqual(
            length,
            result.Length);
    }
}