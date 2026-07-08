using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Repositories;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Shared;
using A_exercise_DB.Application.Security;
using A_exercise_DB.Infrastructures.Security;

namespace A_exercise_DB.Presentations.Configs;
/// <summary>
/// 依存関係(DI)の設定
/// インフラストラクチャ層、アプリケーション層、プレゼンテーション層
/// をまとめて追加する拡張クラス
/// </summary>
public static class ApplicationDependencyExtensions
{
    /// <summary>
    /// アプリ全体の依存関係を一括追加する拡張メソッド
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <param name="config">構成情報</param>
    /// <returns>IServiceCollection(チェーン可能)</returns>
    public static IServiceCollection AddApplicationDependencies(
        this IServiceCollection services, IConfiguration config)
    {
        // インフラストラクチャ層の依存関係を追加
        services.AddInfrastructureDependencies(config);
        // アプリケーション層の依存関係を追加
        services.AddApplicationLayerDependencies(config);
        // プレゼンテーション層の依存関係を追加
        services.AddPresentationLayerDependencies(config);
        return services;
    }

    /// <summary>
    /// インフラストラクチャ層の依存関係を追加
    /// </summary>
    private static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services, IConfiguration config)
    {
        // DbContext の登録
        var connectstr = config.GetConnectionString("PostgreSQLConnection");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.LogTo(Console.WriteLine, LogLevel.Debug);
            options.UseNpgsql(connectstr);
        });

        // ドメインオブジェクト:BookSctockとBookStockEntityの相互変換クラス
        services.AddScoped<BookStockEntityAdapter>();
        // ドメインオブジェクト:BookCategoryとBookCategoryEntityの相互変換クラス
        services.AddScoped<BookCategoryEntityAdapter>();
        // ドメインオブジェクト:BookとBookEntityの相互変換クラス
        services.AddScoped<BookEntityAdapter>();
        // 商品、商品カテゴリ、商品在庫オブジェクトの相互変換Factoryクラス
        services.AddScoped<BookFactory>();

        services.AddScoped<UserEntityAdapter>();
        services.AddScoped<RoleEntityAdapter>();

        services.AddScoped<IRoleRepository, RoleRepository>();

        // ドメインオブジェクト:商品カテゴリのCRUD操作Repositoryインターフェイス
        services.AddScoped<IBookCategoryRepository, BookCategoryRepository>();
        // ドメインオブジェクト:商品カテゴリのCRUD操作Repositoryインターフェイス
        services.AddScoped<IBookRepository, BookRepository>();
        // Unit of Workパターンを利用したトランザクション制御インターフェイス
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();

        // JWTの発行・検証インターフェイスの実装
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();

        return services;
    }

    /// <summary>
    /// アプリケーション層の依存関係を追加
    /// </summary>
    private static IServiceCollection AddApplicationLayerDependencies(
    this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IRegisterBookUsecase, RegisterBookUsecase>();
        services.AddScoped<ISearchBookByKeywordUsecase, SearchBookByKeywordUsecase>();
        services.AddScoped<IUpdateBookUsecase, UpdateBookUsecase>();
        services.AddScoped<IGetCategoryUsecase, GetCategoryUsecase>();
        services.AddScoped<IDeleteBookUsecase, DeleteBookUsecase>();
        services.AddScoped<IRegisterUserUsecase, RegisterUserUsecase>();
        services.AddScoped<IGetRoleUsecase, GetRoleUsecase>();

        // ASP.NET Core Identityのパスワードハッシュ化・検証機能
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        // PBKDF2アルゴリズムを利用したパスワードハッシュ化・検証機能
        services.AddScoped<IPasswordHashingService, PBKDF2PasswordHashingService>();

        // JwtSettingsをバインドしてDIに登録する
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

        // ユースケース:[ログインする]を実現するインターフェイス
        services.AddScoped<IAuthenticateUserUsecase, AuthenticateUserUsecase>();
        return services;
    }

    /// <summary>
    /// プレゼンテーション層の依存関係を追加
    /// </summary>
    private static IServiceCollection AddPresentationLayerDependencies(
    this IServiceCollection services, IConfiguration config)
    {
        // コントローラをサービスコレクションに登録する
        services.AddControllers();

        // RegisterBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
        services.AddScoped<RegisterBookViewModelAdapter>();

        // UpdateBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
        services.AddScoped<UpdateBookViewModelAdapter>();

        services.AddScoped<RegisterUserViewModelAdapter>();
        return services;
    }

    /// <summary>
    /// テストプロジェクトにServiceProviderを提供するヘルパメソッド
    /// </summary>
    /// <param name="config"></param>
    /// <param name="configureServices"></param>
    /// <param name="configureLogging"></param>
    /// <returns></returns>
    public static ServiceProvider BuildAppProvider(
       IConfiguration config,
       Action<IServiceCollection>? configureServices = null,
       Action<ILoggingBuilder>? configureLogging = null)
    {
        //ServiceProvider：生成されたインスタンスを検索して返す機能（たくさんのインスタンスの中で使いたいやつだけを返してくれる）
        var services = new ServiceCollection();
        services.AddLogging(b =>
        {
            if (configureLogging is not null) configureLogging(b);
            else b.AddConsole().SetMinimumLevel(LogLevel.Warning);
        });
        services.AddApplicationDependencies(config);
        configureServices?.Invoke(services);

        return services.BuildServiceProvider(validateScopes: true);
    }
}