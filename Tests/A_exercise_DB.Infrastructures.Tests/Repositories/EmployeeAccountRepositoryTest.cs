using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Infrastructures.Entities;
using A_exercise_DB.Infrastructures.Repositories;
using A_exercise_DB.Presentations.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A_exercise_DB.Infrastructures.Tests.Repositories;

[TestClass]
[TestCategory("Repositories")]
public class EmployeeAccountRepositoryTests
{
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider _provider = null!;

    private IServiceScope _scope = null!;
    private IEmployeeAccountRepository _repository = null!;
    private AppDbContext _dbContext = null!;

    private readonly Guid _employeeUuid =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _provider.Dispose();
    }

    [TestInitialize]
    public void TestInit()
    {
        _scope = _provider.CreateScope();

        _repository =
            _scope.ServiceProvider.GetRequiredService<IEmployeeAccountRepository>();

        _dbContext =
            _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _scope.Dispose();
    }

    [TestMethod(DisplayName = "社員アカウントを正常に登録できる")]
    public async Task CreateAsync_WhenValidEmployeeAccount_ShouldCreateEmployeeAccount()
    {
        var employee = new Employee(
            _employeeUuid,
            "山田 太郎",
            "ヤマダ タロウ");

        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "yamada_taro",
            "password",
            employee);

        await _repository.CreateAsync(employeeAccount);

        var saved = await _dbContext.EmployeeAccounts
            .Include(e => e.Employee)
            .SingleOrDefaultAsync(e =>
                e.AccountUuid == employeeAccount.AccountUuid);

        Assert.IsNotNull(saved);
        Assert.AreEqual(employeeAccount.AccountUuid, saved.AccountUuid);
        Assert.AreEqual("yamada_taro", saved.Name);
        Assert.IsNotNull(saved.Employee);
        Assert.AreEqual(_employeeUuid, saved.Employee.EmployeeUuid);
    }

    [TestMethod(DisplayName = "ユーザーネームが空の場合DomainExceptionが発生する")]
    public async Task CreateAsync_WhenUserNameIsEmpty_ShouldThrowDomainException()
    {
        var employee = new Employee(
            _employeeUuid,
            "山田 太郎",
            "ヤマダ タロウ");

        await Assert.ThrowsExactlyAsync<DomainException>(async () =>
        {
            var employeeAccount = new EmployeeAccount(
                Guid.NewGuid(),
                "",
                "password",
                employee);

            await _repository.CreateAsync(employeeAccount);
        });
    }

    [TestMethod(DisplayName = "指定されたUUIDの社員が存在しない場合Exceptionが発生する")]
    public async Task CreateAsync_WhenEmployeeDoesNotExist_ShouldThrowInternalException()
    {
        var employee = new Employee(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            "存在しない社員",
            "ソンザイシナイシャイン");

        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "not_found_user",
            "password",
            employee);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _repository.CreateAsync(employeeAccount);
        });
    }

    [TestMethod(DisplayName = "DB接続エラー時にInternalExceptionが発生する")]
    public async Task CreateAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=9999;Database=All_Exercise;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var adapter =
            _scope.ServiceProvider.GetRequiredService<EmployeeAccountEntityAdapter>();

        var repository = new EmployeeAccountRepository(
            context,
            adapter);

        var employee = new Employee(
            _employeeUuid,
            "山田 太郎",
            "ヤマダ タロウ");

        var employeeAccount = new EmployeeAccount(
            Guid.NewGuid(),
            "db_error_user",
            "password",
            employee);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.CreateAsync(employeeAccount);
        });
    }
}