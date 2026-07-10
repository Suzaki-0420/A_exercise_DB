using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Repositories;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Shared;
using A_exercise_DB.Applications.Security;
using A_exercise_DB.Applications.Usecases;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Applications.Usecases.Categories;
using A_exercise_DB.Applications.Usecases.Orders;
using A_exercise_DB.Applications.Usecases.Accounts;
using A_exercise_DB.Presentations.Adapters;

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

        services.AddScoped<DepartmentEntityAdapter>();
        services.AddScoped<EmployeeEntityAdapter>();
        services.AddScoped<EmployeeAccountEntityAdapter>();

        services.AddScoped<CustomerEntityAdapter>();
        services.AddScoped<OrderStatusEntityAdapter>();
        services.AddScoped<PaymentMethodEntityAdapter>();

        services.AddScoped<OrdersEntityAdapter>();
        services.AddScoped<OrdersDetailEntityAdapter>();

        services.AddScoped<ProductCategoryEntityAdapter>();
        services.AddScoped<ProductStockEntityAdapter>();
        services.AddScoped<ProductEntityAdapter>();

        // Product・ProductCategory・ProductStock復元用Factory
        services.AddScoped<ProductFactory>();
        services.AddScoped<EmployeeFactory>();
        services.AddScoped<OrdersFactory>();

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeAccountRepository, EmployeeAccountRepository>();

        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();


        return services;
    }

    /// <summary>
    /// アプリケーション層の依存関係を追加
    /// </summary>
    private static IServiceCollection AddApplicationLayerDependencies(
    this IServiceCollection services, IConfiguration config)
    {
        // ASP.NET Core Identityのパスワードハッシュ化・検証機能
        services.AddScoped<IPasswordHasher<EmployeeAccount>, PasswordHasher<EmployeeAccount>>();
        // PBKDF2アルゴリズムを利用したパスワードハッシュ化・検証機能
        services.AddScoped<IPasswordHashingService, PBKDF2PasswordHashingService>();
        services.AddSingleton<ILoginAttemptTracker, InMemoryLoginAttemptTracker>();

        //services.AddScoped<IRegisterBookUsecase, RegisterBookUsecase>();
        services.AddScoped<ILoginAdminUsecase, LoginAdminUsecase>();
        services.AddScoped<IDeleteProductUsecase, DeleteProductUsecase>();
        services.AddScoped<IUpdateProductUsecase, UpdateProductUsecase>();
        services.AddScoped<IRegisterCategoryUsecase, RegisterCategoryUsecase>();
        services.AddScoped<IRegisterProductUsecase, RegisterProductUsecase>();
        services.AddScoped<IRegisterEmployeeAccountUsecase, RegisterEmployeeAccountUsecase>();
        services.AddScoped<ISearchOrdersUsecase, SearchOrdersUsecase>();
        services.AddScoped<IUpdateOrderStatusUsecase, UpdateOrderStatusUsecase>();

        services.AddScoped<ISearchProductByCategoryUsecase, SearchProductByCategoryUsecase>();
        services.AddScoped<ISearchProductByKeywordUsecase, SearchProductByKeywordUsecase>();

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

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "FullnessAdminAuth";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.LoginPath = "/admin/login";
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorization();

        // RegisterBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
        //services.AddScoped<RegisterBookViewModelAdapter>();
        services.AddScoped<ProductRegisterViewModelAdapter>();
        services.AddScoped<RegisterCategoryViewModelAdapter>();
        services.AddScoped<SearchOrdersViewModelAdapter>();
        services.AddScoped<RegisterEmployeeAccountViewModelAdapter>();
        services.AddScoped<UpdateOrderStatusViewModelAdapter>();
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
