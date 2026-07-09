using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Repositories;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Shared;
using A_exercise_DB.Applications.Usecases;
using A_exercise_DB.Applications.Usecases.Products;
using A_exercise_DB.Applications.Usecases.Categories;
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
        //services.AddScoped<IRegisterBookUsecase, RegisterBookUsecase>();
        services.AddScoped<IDeleteProductUsecase, DeleteProductUsecase>();
        services.AddScoped<IUpdateProductUsecase, UpdateProductUsecase>();
        services.AddScoped<IRegisterCategoryUsecase, RegisterCategoryUsecase>();

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
        //services.AddScoped<RegisterBookViewModelAdapter>();
        services.AddScoped<RegisterCategoryViewModelAdapter>();
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
