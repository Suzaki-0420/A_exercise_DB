using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Infrastructures.Storage;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace A_exercise_DB.Infrastructures.Tests.Storage;

[TestClass]
public class LocalImageStorageTest
{
    private string _contentRootPath = null!;
    private string _rootPath = null!;
    private string _absoluteRootPath = null!;

    private LocalImageStorage _storage = null!;

    private IOptions<ImageStorageOptions> _options = null!;
    private IHostEnvironment _environment = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _contentRootPath =
            Path.Combine(
                Path.GetTempPath(),
                "LocalImageStorageTests",
                Guid.NewGuid().ToString());

        _rootPath =
            "product-images";

        _absoluteRootPath =
            Path.Combine(
                _contentRootPath,
                _rootPath);

        Directory.CreateDirectory(
            _contentRootPath);

        var imageStorageOptions =
            new ImageStorageOptions
            {
                RootPath = _rootPath,
                PublicBaseUrl = "https://example.com/",
                RequestPath = "/images"
            };

        _options =
            Options.Create(
                imageStorageOptions);

        var environmentMock =
            new Mock<IHostEnvironment>();

        environmentMock
            .SetupGet(x => x.ContentRootPath)
            .Returns(_contentRootPath);

        _environment =
            environmentMock.Object;

        _storage =
            new LocalImageStorage(
                _options,
                _environment);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_contentRootPath))
        {
            Directory.Delete(
                _contentRootPath,
                true);
        }
    }

    /// <summary>
    /// 正常な画像を保存した場合、ファイルが作成され公開URLが返されること
    /// </summary>
    [TestMethod(
        DisplayName = "正常な画像を保存した場合、ファイルが作成され公開URLが返される")]
    public async Task SaveAsync_WhenInputIsValid_ShouldSaveFileAndReturnPublicUrl()
    {
        // Arrange
        var imageBytes =
            new byte[]
            {
                1,
                2,
                3,
                4,
                5
            };

        await using var content =
            new MemoryStream(
                imageBytes);

        const string fileName =
            "sample-image.png";

        var expectedFilePath =
            Path.Combine(
                _absoluteRootPath,
                fileName);

        const string expectedUrl =
            "https://example.com/images/sample-image.png";

        Assert.IsFalse(
            Directory.Exists(_absoluteRootPath));

        // Act
        var actualUrl =
            await _storage.SaveAsync(
                content,
                fileName);

        // Assert
        Assert.AreEqual(
            expectedUrl,
            actualUrl);

        Assert.IsTrue(
            Directory.Exists(_absoluteRootPath));

        Assert.IsTrue(
            File.Exists(expectedFilePath));

        var actualBytes =
            await File.ReadAllBytesAsync(
                expectedFilePath);

        CollectionAssert.AreEqual(
            imageBytes,
            actualBytes);
    }

    /// <summary>
    /// Contentがnullの場合、ArgumentNullExceptionが発生すること
    /// </summary>
    [TestMethod(
        DisplayName = "Contentがnullの場合、ArgumentNullExceptionが発生する")]
    public async Task SaveAsync_WhenContentIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () =>
                await _storage.SaveAsync(
                    null!,
                    "sample.png"));

        // Assert
        Assert.IsFalse(
            Directory.Exists(_absoluteRootPath));
    }

    /// <summary>
    /// ファイル名がnullの場合、ArgumentNullExceptionが発生すること
    /// </summary>
    [TestMethod(
        DisplayName = "ファイル名がnullの場合、ArgumentNullExceptionが発生する")]
    public async Task SaveAsync_WhenFileNameIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        await using var content =
            new MemoryStream([1]);

        // Act
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () =>
                await _storage.SaveAsync(
                    content,
                    null!));

        // Assert
        Assert.IsFalse(
            Directory.Exists(_absoluteRootPath));
    }

    /// <summary>
    /// ファイル名が空文字または空白の場合、ArgumentExceptionが発生すること
    /// </summary>
    [TestMethod(
        DisplayName = "ファイル名が空文字または空白の場合、ArgumentExceptionが発生する")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("　")]
    public async Task SaveAsync_WhenFileNameIsEmptyOrWhiteSpace_ShouldThrowArgumentException(
        string fileName)
    {
        // Arrange
        await using var content =
            new MemoryStream([1]);

        // Act
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () =>
                await _storage.SaveAsync(
                    content,
                    fileName));

        // Assert
        Assert.IsFalse(
            Directory.Exists(_absoluteRootPath));
    }

    /// <summary>
    /// 同名ファイルが存在する場合、InternalExceptionが発生すること
    /// </summary>
    [TestMethod(
        DisplayName = "同名ファイルが存在する場合、InternalExceptionが発生する")]
    public async Task SaveAsync_WhenFileAlreadyExists_ShouldThrowInternalException()
    {
        // Arrange
        const string fileName =
            "duplicate.png";

        await using var firstContent =
            new MemoryStream([1, 2, 3]);

        await using var secondContent =
            new MemoryStream([4, 5, 6]);

        await _storage.SaveAsync(
            firstContent,
            fileName);

        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<InternalException>(
                async () =>
                    await _storage.SaveAsync(
                        secondContent,
                        fileName));

        // Assert
        Assert.AreEqual(
            "画像の保存に失敗しました。",
            exception.Message);

        Assert.IsNotNull(
            exception.InnerException);

        Assert.IsInstanceOfType<IOException>(
            exception.InnerException);

        var savedFilePath =
            Path.Combine(
                _absoluteRootPath,
                fileName);

        var savedBytes =
            await File.ReadAllBytesAsync(
                savedFilePath);

        CollectionAssert.AreEqual(
            new byte[]
            {
                1,
                2,
                3
            },
            savedBytes);
    }

    /// <summary>
    /// 書き込み時にUnauthorizedAccessExceptionが発生した場合、
    /// InternalExceptionに変換されること
    /// </summary>
    [TestMethod(
        DisplayName = "書き込み時にUnauthorizedAccessExceptionが発生した場合、InternalExceptionに変換される")]
    public async Task SaveAsync_WhenUnauthorizedAccessExceptionOccurs_ShouldThrowInternalException()
    {
        // Arrange
        var expectedException =
            new UnauthorizedAccessException(
                "書き込み権限がありません。");

        await using var content =
            new ExceptionThrowingStream(
                expectedException);

        // Act
        var actualException =
            await Assert.ThrowsExactlyAsync<InternalException>(
                async () =>
                    await _storage.SaveAsync(
                        content,
                        "unauthorized.png"));

        // Assert
        Assert.AreEqual(
            "画像の保存に失敗しました。",
            actualException.Message);

        Assert.AreSame(
            expectedException,
            actualException.InnerException);
    }

    /// <summary>
    /// IOExceptionまたはUnauthorizedAccessException以外が発生した場合、
    /// 例外が変換されずにスローされること
    /// </summary>
    [TestMethod(
        DisplayName = "対象外の例外が発生した場合、例外が変換されずにスローされる")]
    public async Task SaveAsync_WhenUnexpectedExceptionOccurs_ShouldRethrowOriginalException()
    {
        // Arrange
        var expectedException =
            new InvalidOperationException(
                "予期しないエラーです。");

        await using var content =
            new ExceptionThrowingStream(
                expectedException);

        // Act
        var actualException =
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                async () =>
                    await _storage.SaveAsync(
                        content,
                        "unexpected.png"));

        // Assert
        Assert.AreSame(
            expectedException,
            actualException);
    }

    /// <summary>
    /// 画像を削除した場合、NotImplementedExceptionが発生すること
    /// </summary>
    [TestMethod(DisplayName = "画像削除を呼び出した場合、例外なく完了する")]
    public async Task DeleteAsync_WhenCalled_ShouldComplete()
    {
        await _storage.DeleteAsync(
            "delete.png");
    }

    /// <summary>
    /// CopyToAsyncの実行時に指定された例外を発生させるストリーム
    /// </summary>
    private sealed class ExceptionThrowingStream
        : MemoryStream
    {
        private readonly Exception _exception;

        public ExceptionThrowingStream(
            Exception exception)
        {
            _exception = exception;
        }

        public override Task CopyToAsync(
            Stream destination,
            int bufferSize,
            CancellationToken cancellationToken)
        {
            return Task.FromException(
                _exception);
        }
    }
    /// <summary>
    /// 画像URLがnullの場合、ArgumentNullExceptionが発生すること
    /// </summary>
    [TestMethod(
        DisplayName = "画像URLがnullの場合、ArgumentNullExceptionが発生する")]
    public async Task DeleteAsync_WhenImageUrlIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(
                async () =>
                    await _storage.DeleteAsync(
                        null!));

        // Assert
        Assert.AreEqual(
            "imageUrl",
            exception.ParamName);
    }
    /// <summary>
    /// 画像URLが空文字または空白の場合、ArgumentExceptionが発生すること
    /// </summary>
    [TestMethod(
        DisplayName = "画像URLが空文字または空白の場合、ArgumentExceptionが発生する")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("　")]
    public async Task DeleteAsync_WhenImageUrlIsEmptyOrWhiteSpace_ShouldThrowArgumentException(
        string imageUrl)
    {
        // Act
        var exception =
            await Assert.ThrowsExactlyAsync<ArgumentException>(
                async () =>
                    await _storage.DeleteAsync(
                        imageUrl));

        // Assert
        Assert.AreEqual(
            "imageUrl",
            exception.ParamName);
    }

    /// <summary>
    /// ファイルが存在しない場合、例外なく完了すること
    /// </summary>
    [TestMethod(
        DisplayName = "ファイルが存在しない場合、例外なく完了する")]
    public async Task DeleteAsync_WhenFileDoesNotExist_ShouldComplete()
    {
        // Arrange
        var filePath =
            Path.Combine(
                _absoluteRootPath,
                "not-exist.png");

        Assert.IsFalse(
            File.Exists(filePath));

        // Act
        await _storage.DeleteAsync(
            "/images/not-exist.png");

        // Assert
        Assert.IsFalse(
            File.Exists(filePath));
    }

    /// <summary>
    /// ファイルが存在する場合、ファイルが削除されること
    /// </summary>
    [TestMethod(
        DisplayName = "ファイルが存在する場合、ファイルが削除される")]
    public async Task DeleteAsync_WhenFileExists_ShouldDeleteFile()
    {
        // Arrange
        Directory.CreateDirectory(
            _absoluteRootPath);

        const string fileName =
            "delete.png";

        var filePath =
            Path.Combine(
                _absoluteRootPath,
                fileName);

        await File.WriteAllBytesAsync(
            filePath,
            [1, 2, 3]);

        Assert.IsTrue(
            File.Exists(filePath));

        // Act
        await _storage.DeleteAsync(
            $"https://example.com/images/{fileName}");

        // Assert
        Assert.IsFalse(
            File.Exists(filePath));
    }

    /// <summary>
    /// ファイル操作時の例外を再現するためのLocalImageStorage
    /// </summary>
    private sealed class TestableLocalImageStorage
        : LocalImageStorage
    {
        private readonly Func<string, bool>? _fileExists;
        private readonly Action<string>? _deleteFile;

        public TestableLocalImageStorage(
            IOptions<ImageStorageOptions> options,
            IHostEnvironment environment,
            Func<string, bool>? fileExists = null,
            Action<string>? deleteFile = null)
            : base(
                options,
                environment)
        {
            _fileExists = fileExists;
            _deleteFile = deleteFile;
        }

        protected override bool FileExists(
            string filePath)
        {
            if (_fileExists is not null)
            {
                return _fileExists(
                    filePath);
            }

            return base.FileExists(
                filePath);
        }

        protected override void DeleteFile(
            string filePath)
        {
            if (_deleteFile is not null)
            {
                _deleteFile(
                    filePath);

                return;
            }

            base.DeleteFile(
                filePath);
        }
    }

    /// <summary>
    /// ファイル確認時にIOExceptionが発生した場合、
    /// InternalExceptionに変換されること
    /// </summary>
    [TestMethod(
        DisplayName = "削除処理でIOExceptionが発生した場合、InternalExceptionに変換される")]
    public async Task DeleteAsync_WhenIOExceptionOccurs_ShouldThrowInternalException()
    {
        // Arrange
        var expectedException =
            new IOException(
                "ファイルにアクセスできません。");

        var storage =
            new TestableLocalImageStorage(
                _options,
                _environment,
                fileExists: _ => throw expectedException);

        // Act
        var actualException =
            await Assert.ThrowsExactlyAsync<InternalException>(
                async () =>
                    await storage.DeleteAsync(
                        "/images/sample.png"));

        // Assert
        Assert.AreEqual(
            "画像の削除に失敗しました。",
            actualException.Message);

        Assert.AreSame(
            expectedException,
            actualException.InnerException);
    }
    /// <summary>
    /// ファイル削除時にUnauthorizedAccessExceptionが発生した場合、
    /// InternalExceptionに変換されること
    /// </summary>
    [TestMethod(
        DisplayName = "削除処理でUnauthorizedAccessExceptionが発生した場合、InternalExceptionに変換される")]
    public async Task DeleteAsync_WhenUnauthorizedAccessExceptionOccurs_ShouldThrowInternalException()
    {
        // Arrange
        var expectedException =
            new UnauthorizedAccessException(
                "削除権限がありません。");

        var storage =
            new TestableLocalImageStorage(
                _options,
                _environment,
                fileExists: _ => true,
                deleteFile: _ => throw expectedException);

        // Act
        var actualException =
            await Assert.ThrowsExactlyAsync<InternalException>(
                async () =>
                    await storage.DeleteAsync(
                        "/images/sample.png"));

        // Assert
        Assert.AreEqual(
            "画像の削除に失敗しました。",
            actualException.Message);

        Assert.AreSame(
            expectedException,
            actualException.InnerException);
    }
    /// <summary>
    /// 対象外の例外が発生した場合、
    /// InternalExceptionに変換されないこと
    /// </summary>
    [TestMethod(
        DisplayName = "削除処理で対象外の例外が発生した場合、元の例外がスローされる")]
    public async Task DeleteAsync_WhenUnexpectedExceptionOccurs_ShouldRethrowOriginalException()
    {
        // Arrange
        var expectedException =
            new InvalidOperationException(
                "予期しないエラーです。");

        var storage =
            new TestableLocalImageStorage(
                _options,
                _environment,
                fileExists: _ => throw expectedException);

        // Act
        var actualException =
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                async () =>
                    await storage.DeleteAsync(
                        "/images/sample.png"));

        // Assert
        Assert.AreSame(
            expectedException,
            actualException);
    }
}