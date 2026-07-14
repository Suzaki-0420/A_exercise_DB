using A_exercise_DB.Applications.Interfaces;
using A_exercise_DB.Applications.Params;
using A_exercise_DB.Applications.Usecases.Images;
using A_exercise_DB.Domains.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

        using var content =
            new MemoryStream([1]);

        var param = new ImageUploadParam(
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
            .ReturnsAsync(expectedResult);

        // Act
        var result =
            await _usecase.ExecuteAsync(param);

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

    private async Task ExecuteValidImageTestAsync(
        string fileName,
        string contentType,
        string expectedExtension)
    {
        // Arrange
        using var content =
            new MemoryStream([1, 2, 3]);

        var param = new ImageUploadParam(
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
            .ReturnsAsync(expectedResult);

        // Act
        var result =
            await _usecase.ExecuteAsync(param);

        // Assert
        Assert.AreEqual(
            expectedResult,
            result);

        Assert.IsNotNull(
            actualSavedFileName);

        Assert.AreEqual(
            expectedExtension,
            Path.GetExtension(actualSavedFileName));

        var uuidPart =
            Path.GetFileNameWithoutExtension(
                actualSavedFileName);

        Assert.IsTrue(
            Guid.TryParse(
                uuidPart,
                out _));

        _imageStorageMock.Verify(
            x => x.SaveAsync(
                content,
                actualSavedFileName),
            Times.Once);
    }
}