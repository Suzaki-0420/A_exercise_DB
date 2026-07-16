using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Applications.Security;
using A_exercise_DB.Presentations.Configs;
using Moq;

namespace A_exercise_DB.Applications.Tests.Security;

/// <summary>
///  PBKDF2アルゴリズムを利用
/// パスワードのハッシュ化と検証機能を提供するインターフェイス実装のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Security")]
public class PBKDF2PasswordHashingServiceTests
{
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;

    // スコープドサービス
    private IServiceScope? _scope;

    // テストターゲット
    private IPasswordHashingService? _service;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    /// <param name="context">テストコンテキスト</param>
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        // アプリケーション管理を生成
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // サービスプロバイダ(DIコンテナ)の生成
        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    /// <summary>
    /// テストクラスクリーンアップ
    /// </summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        // 生成したサービスプロバイダ(DIコンテナ)を破棄する
        _provider?.Dispose();
    }

    /// <summary>
    /// テストの前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();

        // テストターゲットを取得する
        _service = _scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();
    }

    /// <summary>
    /// テストメソッド実行後の後処理
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        // スコープドサービスを破棄する
        _scope!.Dispose();
    }

    [TestMethod(DisplayName = "パスワードをハッシュ化し、検証に成功する")]
    public void Hash_ShouldGenerateHash_AndVerifySuccess()
    {
        // パスワードを用意する
        var raw = "P@ssw0rd123!";

        // パスワードをハッシュ化する
        var hash = _service!.Hash(raw);

        // ハッシュが平文と異なることを検証する
        Assert.AreNotEqual(raw, hash);

        // 正しいパスワードで検証が成功することを検証
        Assert.IsTrue(_service!.Verify(hash, raw));
    }

    [TestMethod(DisplayName = "同じパスワードをハッシュ化しても異なる値になる")]
    public void Hash_SamePassword_ShouldGenerateDifferentHashes()
    {
        // パスワードを用意する
        var raw = "P@ssw0rd123!";

        // パスワードを2回ハッシュ化する
        var h1 = _service!.Hash(raw);
        var h2 = _service!.Hash(raw);

        // ソルトにより毎回異なる値であることを検証する
        Assert.AreNotEqual(h1, h2);

        // それぞれのハッシュで検証が成功することを検証する
        Assert.IsTrue(_service!.Verify(h1, raw));
        Assert.IsTrue(_service!.Verify(h2, raw));
    }

    [TestMethod(DisplayName = "間違ったパスワードの場合、検証に失敗する")]
    public void Verify_ShouldReturnFalse_WhenPasswordIsWrong()
    {
        // パスワードを用意する
        var raw = "P@ssw0rd123!";

        // ハッシュ化する
        var hash = _service!.Hash(raw);

        // 間違ったパスワードでは検証に失敗することを検証する
        Assert.IsFalse(_service!.Verify(hash, "wrong-pass"));
    }

    [TestMethod(DisplayName = "古い形式のハッシュを検証すると再ハッシュ例外がスローされる")]
    public void Verify_ShouldThrowPasswordRehashNeededException_WhenOldHashFormat()
    {
        // パスワードを用意する
        var raw = "P@ssw0rd123!";

        // IdentityV2のハッシュ生成機能を作成する
        var oldHasher = new PasswordHasher<EmployeeAccount>(
            Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2
            })
        );

        // 古い形式のハッシュ生成機能でServiceを生成する
        var oldSvc = new PBKDF2PasswordHashingService(oldHasher);

        // 古い形式でパスワードをハッシュ化する
        var oldHash = oldSvc.Hash(raw);

        // PasswordRehashNeededException例外がスローされることを検証する
        var ex = Assert.ThrowsExactly<PasswordRehashNeededException>(() =>
        {
            _service!.Verify(oldHash, raw);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("パスワードは認証されたが、再ハッシュが必要です。", ex.Message);
    }

    [TestMethod(DisplayName = "パスワードが空文字の場合、DomainExceptionがスローされる")]
    public void Hash_WhenRawPasswordIsEmpty_ShouldThrowDomainException()
    {
        // Act
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _service!.Hash("");
        });

        // Assert
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "パスワードが空白の場合、DomainExceptionがスローされる")]
    public void Hash_WhenRawPasswordIsWhiteSpace_ShouldThrowDomainException()
    {
        // Act
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _service!.Hash("   ");
        });

        // Assert
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "パスワードがnullの場合、DomainExceptionがスローされる")]
    public void Hash_WhenRawPasswordIsNull_ShouldThrowDomainException()
    {
        // Act
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _service!.Hash(null!);
        });

        // Assert
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "ハッシュ化されたパスワードが空文字の場合、DomainExceptionがスローされる")]
    public void Verify_WhenHashedPasswordIsEmpty_ShouldThrowDomainException()
    {
        // Act
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _service!.Verify("", "P@ssw0rd123!");
        });

        // Assert
        Assert.AreEqual("ハッシュ化されたパスワードは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "ハッシュ化されたパスワードが空白の場合、DomainExceptionがスローされる")]
    public void Verify_WhenHashedPasswordIsWhiteSpace_ShouldThrowDomainException()
    {
        // Act
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _service!.Verify("   ", "P@ssw0rd123!");
        });

        // Assert
        Assert.AreEqual("ハッシュ化されたパスワードは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "ハッシュ化されたパスワードがnullの場合、DomainExceptionがスローされる")]
    public void Verify_WhenHashedPasswordIsNull_ShouldThrowDomainException()
    {
        // Act
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _service!.Verify(null!, "P@ssw0rd123!");
        });

        // Assert
        Assert.AreEqual("ハッシュ化されたパスワードは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "検証用パスワードが空文字の場合、DomainExceptionがスローされる")]
    public void Verify_WhenProvidedPasswordIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var hash = _service!.Hash("P@ssw0rd123!");

        // Act
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _service!.Verify(hash, "");
        });

        // Assert
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "検証用パスワードが空白の場合、DomainExceptionがスローされる")]
    public void Verify_WhenProvidedPasswordIsWhiteSpace_ShouldThrowDomainException()
    {
        // Arrange
        var hash = _service!.Hash("P@ssw0rd123!");

        // Act
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _service!.Verify(hash, "   ");
        });

        // Assert
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "検証用パスワードがnullの場合、DomainExceptionがスローされる")]
    public void Verify_WhenProvidedPasswordIsNull_ShouldThrowDomainException()
    {
        // Arrange
        var hash = _service!.Hash("P@ssw0rd123!");

        // Act
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _service!.Verify(hash, null!);
        });

        // Assert
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "パスワード検証結果が想定外の場合、falseを返す")]
    public void Verify_WhenResultIsUnknown_ShouldReturnFalse()
    {
        // Arrange
        var passwordHasherMock = new Mock<IPasswordHasher<EmployeeAccount>>();

        passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(
                It.IsAny<EmployeeAccount>(),
                "dummy-hash",
                "P@ssw0rd123!"))
            .Returns((PasswordVerificationResult)999);

        var service = new PBKDF2PasswordHashingService(passwordHasherMock.Object);

        // Act
        var result = service.Verify("dummy-hash", "P@ssw0rd123!");

        // Assert
        Assert.IsFalse(result);
    }
}