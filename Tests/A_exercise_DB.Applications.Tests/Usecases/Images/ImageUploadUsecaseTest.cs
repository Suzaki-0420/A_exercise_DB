using A_exercise_DB.Applications.Interfaces;
using A_exercise_DB.Applications.Params;
using A_exercise_DB.Applications.Usecases.Images;
using A_exercise_DB.Domains.Exceptions;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
namespace A_exercise_DB.Applications.Tests.Usecases.Images;

[TestClass]
public sealed class ImageUploadUsecaseTest
{
    private Mock<IImageStorage> _imageStorageMock = null!;
    private ImageUploadUsecase _usecase = null!;

    [TestInitialize]
    public void Initialize()
    {
        _imageStorageMock = new Mock<IImageStorage>();

        _usecase = new ImageUploadUsecase(
            _imageStorageMock.Object);
    }

    [TestMethod(
        DisplayName = "入力値がnullの場合、ArgumentNullExceptionがスローされる")]
    public async Task ExecuteAsync_WhenParamIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(
                () => _usecase.ExecuteAsync(null!));

        // Assert
        Assert.AreEqual(
            "param",
            exception.ParamName);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "画像ストリームがnullの場合、ArgumentNullExceptionがスローされる")]
    public async Task ExecuteAsync_WhenContentIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var param = new ImageUploadParam(
            null!,
            "sample.png",
            "image/png",
            100);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(
                () => _usecase.ExecuteAsync(param));

        // Assert
        Assert.AreEqual(
            "param.Content",
            exception.ParamName);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "画像サイズが0の場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenLengthIsZero_ShouldThrowDomainException()
    {
        // Arrange
        using var content =
            new MemoryStream([1]);

        var param = new ImageUploadParam(
            content,
            "sample.png",
            "image/png",
            0);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(param));

        // Assert
        Assert.AreEqual(
            "画像ファイルが空です。",
            exception.Message);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "画像サイズが負数の場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenLengthIsNegative_ShouldThrowDomainException()
    {
        // Arrange
        using var content =
            new MemoryStream([1]);

        var param = new ImageUploadParam(
            content,
            "sample.png",
            "image/png",
            -1);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(param));

        // Assert
        Assert.AreEqual(
            "画像ファイルが空です。",
            exception.Message);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "画像サイズが2MBを超える場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenLengthExceedsTwoMegabytes_ShouldThrowDomainException()
    {
        // Arrange
        const long maxFileSize =
            2 * 1024 * 1024;

        using var content =
            new MemoryStream([1]);

        var param = new ImageUploadParam(
            content,
            "sample.png",
            "image/png",
            maxFileSize + 1);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(param));

        // Assert
        Assert.AreEqual(
            "画像のファイルサイズは2MB以下にしてください。",
            exception.Message);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "許可されていない拡張子の場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenExtensionIsNotAllowed_ShouldThrowDomainException()
    {
        // Arrange
        using var content =
            new MemoryStream([1]);

        var param = new ImageUploadParam(
            content,
            "sample.gif",
            "image/png",
            1);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(param));

        // Assert
        Assert.AreEqual(
            "jpg、jpeg、png、webp形式の画像を指定してください。",
            exception.Message);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "ファイル名に拡張子がない場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenFileNameHasNoExtension_ShouldThrowDomainException()
    {
        // Arrange
        using var content =
            new MemoryStream([1]);

        var param = new ImageUploadParam(
            content,
            "sample",
            "image/png",
            1);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(param));

        // Assert
        Assert.AreEqual(
            "jpg、jpeg、png、webp形式の画像を指定してください。",
            exception.Message);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "許可されていないContent-Typeの場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenContentTypeIsNotAllowed_ShouldThrowDomainException()
    {
        // Arrange
        using var content =
            new MemoryStream([1]);

        var param = new ImageUploadParam(
            content,
            "sample.png",
            "application/octet-stream",
            1);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(param));

        // Assert
        Assert.AreEqual(
            "画像ファイルの形式が正しくありません。",
            exception.Message);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "jpg画像が正常な場合、画像が保存される")]
    public async Task ExecuteAsync_WhenJpgImageIsValid_ShouldSaveImage()
    {
        await ExecuteValidImageTestAsync(
            "sample.jpg",
            "image/jpeg",
            ".jpg");
    }

    [TestMethod(
        DisplayName = "jpeg画像が正常な場合、画像が保存される")]
    public async Task ExecuteAsync_WhenJpegImageIsValid_ShouldSaveImage()
    {
        await ExecuteValidImageTestAsync(
            "sample.jpeg",
            "image/jpeg",
            ".jpeg");
    }

    [TestMethod(
        DisplayName = "png画像が正常な場合、画像が保存される")]
    public async Task ExecuteAsync_WhenPngImageIsValid_ShouldSaveImage()
    {
        await ExecuteValidImageTestAsync(
            "sample.png",
            "image/png",
            ".png");
    }

    [TestMethod(
        DisplayName = "webp画像が正常な場合、画像が保存される")]
    public async Task ExecuteAsync_WhenWebpImageIsValid_ShouldSaveImage()
    {
        await ExecuteValidImageTestAsync(
            "sample.webp",
            "image/webp",
            ".webp");
    }

    [TestMethod(
        DisplayName = "拡張子が大文字のPNG画像の場合、小文字に変換して保存される")]
    public async Task ExecuteAsync_WhenExtensionIsUpperCase_ShouldSaveWithLowerCaseExtension()
    {
        await ExecuteValidImageTestAsync(
            "sample.PNG",
            "image/png",
            ".png");
    }

    [TestMethod(
    DisplayName = "画像サイズがちょうど2MBの場合、画像が保存される")]
    public async Task ExecuteAsync_WhenLengthIsExactlyTwoMegabytes_ShouldSaveImage()
    {
        // Arrange
        const long maxFileSize =
            2 * 1024 * 1024;

        await using var content =
            await CreateImageStreamAsync(
                ".png");

        var param =
            new ImageUploadParam(
                content,
                "sample.png",
                "image/png",
                maxFileSize);

        const string expectedResult =
            "images/saved-image.png";

        _imageStorageMock
            .Setup(x => x.SaveAsync(
                content,
                It.Is<string>(
                    fileName =>
                        fileName.EndsWith(
                            ".png",
                            StringComparison.Ordinal))))
            .ReturnsAsync(
                expectedResult);

        // Act
        var result =
            await _usecase.ExecuteAsync(
                param);

        // Assert
        Assert.AreEqual(
            expectedResult,
            result);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                content,
                It.Is<string>(
                    fileName =>
                        fileName.EndsWith(
                            ".png",
                            StringComparison.Ordinal))),
            Times.Once);
    }

    [TestMethod(
    DisplayName = "画像として認識できない内容の場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenImageFormatIsUnknown_ShouldThrowDomainException()
    {
        // Arrange
        await using var content =
            new MemoryStream(
                [1, 2, 3]);

        var param =
            new ImageUploadParam(
                content,
                "sample.png",
                "image/png",
                content.Length);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(
                    param));

        // Assert
        Assert.AreEqual(
            "画像ファイルの形式が正しくありません。",
            exception.Message);

        Assert.IsNotNull(
            exception.InnerException);

        Assert.IsInstanceOfType<UnknownImageFormatException>(
            exception.InnerException);

        Assert.AreEqual(
            0,
            content.Position);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "画像の横幅が1000pxを超える場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenImageWidthExceedsLimit_ShouldThrowDomainException()
    {
        // Arrange
        await using var content =
            await CreateImageStreamAsync(
                extension: ".png",
                width: 1001,
                height: 10);

        var param =
            new ImageUploadParam(
                content,
                "sample.png",
                "image/png",
                content.Length);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(
                    param));

        // Assert
        Assert.AreEqual(
            "画像の縦横サイズは1000px以下にしてください。",
            exception.Message);

        Assert.AreEqual(
            0,
            content.Position);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "画像の縦幅が1000pxを超える場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenImageHeightExceedsLimit_ShouldThrowDomainException()
    {
        // Arrange
        await using var content =
            await CreateImageStreamAsync(
                extension: ".png",
                width: 10,
                height: 1001);

        var param =
            new ImageUploadParam(
                content,
                "sample.png",
                "image/png",
                content.Length);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(
                    param));

        // Assert
        Assert.AreEqual(
            "画像の縦横サイズは1000px以下にしてください。",
            exception.Message);

        Assert.AreEqual(
            0,
            content.Position);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod(
        DisplayName = "画像の縦横がちょうど1000pxの場合、画像が保存される")]
    public async Task ExecuteAsync_WhenImageDimensionsAreExactlyLimit_ShouldSaveImage()
    {
        // Arrange
        await using var content =
            await CreateImageStreamAsync(
                extension: ".png",
                width: 1000,
                height: 1000);

        var param =
            new ImageUploadParam(
                content,
                "sample.png",
                "image/png",
                content.Length);

        const string expectedResult =
            "images/saved-image.png";

        _imageStorageMock
            .Setup(x => x.SaveAsync(
                content,
                It.IsAny<string>()))
            .ReturnsAsync(
                expectedResult);

        // Act
        var result =
            await _usecase.ExecuteAsync(
                param);

        // Assert
        Assert.AreEqual(
            expectedResult,
            result);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                content,
                It.Is<string>(
                    fileName =>
                        fileName.EndsWith(
                            ".png",
                            StringComparison.Ordinal))),
            Times.Once);
    }

    [TestMethod(
        DisplayName = "破損した画像の場合、DomainExceptionがスローされる")]
    public async Task ExecuteAsync_WhenImageContentIsInvalid_ShouldThrowDomainException()
    {
        // Arrange
        await using var validContent =
            await CreateImageStreamAsync(
                ".png");

        var validBytes =
            validContent.ToArray();

        var brokenBytes =
            validBytes
                .Take(16)
                .ToArray();

        await using var content =
            new MemoryStream(
                brokenBytes);

        var param =
            new ImageUploadParam(
                content,
                "broken.png",
                "image/png",
                content.Length);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<DomainException>(
                () => _usecase.ExecuteAsync(
                    param));

        // Assert
        Assert.AreEqual(
            "画像ファイルが破損しているか、内容が正しくありません。",
            exception.Message);

        Assert.IsNotNull(
            exception.InnerException);

        Assert.IsInstanceOfType<InvalidImageContentException>(
            exception.InnerException);

        Assert.AreEqual(
            0,
            content.Position);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }
    private async Task ExecuteValidImageTestAsync(
    string fileName,
    string contentType,
    string expectedExtension)
    {
        // Arrange
        await using var content =
            await CreateImageStreamAsync(
                expectedExtension);

        var param =
            new ImageUploadParam(
                content,
                fileName,
                contentType,
                content.Length);

        const string expectedResult =
            "images/saved-image";

        string? actualSavedFileName = null;

        _imageStorageMock
            .Setup(x => x.SaveAsync(
                content,
                It.IsAny<string>()))
            .Callback<Stream, string>(
                (_, savedFileName) =>
                    actualSavedFileName = savedFileName)
            .ReturnsAsync(
                expectedResult);

        // Act
        var result =
            await _usecase.ExecuteAsync(
                param);

        // Assert
        Assert.AreEqual(
            expectedResult,
            result);

        Assert.IsNotNull(
            actualSavedFileName);

        Assert.AreEqual(
            expectedExtension,
            Path.GetExtension(
                actualSavedFileName));

        var uuidPart =
            Path.GetFileNameWithoutExtension(
                actualSavedFileName);

        Assert.IsTrue(
            Guid.TryParse(
                uuidPart,
                out _));

        Assert.AreEqual(
            0,
            content.Position);

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                content,
                actualSavedFileName),
            Times.Once);
    }
    private static async Task<MemoryStream> CreateImageStreamAsync(
    string extension = ".png",
    int width = 10,
    int height = 10)
    {
        var stream =
            new MemoryStream();

        using var image =
            new Image<Rgba32>(
                width,
                height);

        switch (extension.ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                await image.SaveAsJpegAsync(
                    stream);
                break;

            case ".png":
                await image.SaveAsPngAsync(
                    stream);
                break;

            case ".webp":
                await image.SaveAsWebpAsync(
                    stream);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(extension));
        }

        stream.Position = 0;

        return stream;
    }
}