using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Presentations.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using A_exercise_DB.Domains.Exceptions;
using Microsoft.EntityFrameworkCore;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Repositories;

namespace A_exercise_DB.Infrastructures.Tests.Repositories;

/// <summary>
/// 社員Repositoryの単体テスト
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class EmployeeRepositoryTests
{
    // MSTest v4では public・非static が必要
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider _provider = null!;

    private IServiceScope _scope = null!;
    private IEmployeeRepository _employeeRepository = null!;
    private AppDbContext _dbContext = null!;

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

        _employeeRepository =
            _scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();

        _dbContext =
            _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _scope.Dispose();
    }

    [TestMethod(DisplayName = "アカウント登録されていない社員情報をすべて取得できる")]
    public async Task FindAllWithoutAccountAsync_WhenEmployeesExist_ShouldReturnEmployeeList()
    {
        var employees = await _employeeRepository.FindAllWithoutAccountAsync();

        Assert.IsNotNull(employees);
        Assert.IsInstanceOfType(employees, typeof(List<Employee>));
        Assert.IsTrue(employees.All(e => e.Department is not null));

        Assert.AreEqual(Guid.Parse("33333333-3333-3333-3333-333333333333"), employees[0].EmployeeUuid);
        Assert.AreEqual("鈴木 一郎", employees[0].Name);
        Assert.AreEqual("スズキ イチロウ", employees[0].Kana);
        Assert.AreEqual(Guid.Parse("6e06cad7-09e6-4eae-adbe-20102ea58efc"), employees[0].Department!.DepartmentUuid);
        Assert.AreEqual("開発部", employees[0].Department!.Name);
    }

    /// <summary>
    /// DB接続エラーが発生した場合、InternalExceptionが送出される
    /// </summary>
    [TestMethod]
    public async Task FindAllAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=NotFoundDatabase;Username=postgres;Password=postgres")
            .Options;

        await using var context = new AppDbContext(options);

        var factory = _scope.ServiceProvider.GetRequiredService<EmployeeFactory>();

        var repository = new EmployeeRepository(
            context,
            factory);

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await repository.FindAllWithoutAccountAsync();
        });
    }

    [TestMethod(DisplayName = "存在する社員UUIDで社員情報を取得できる")]
    public async Task FindByIdAsync_WhenUuidExists_ShouldReturnEmployee()
    {
        // init.sqlなどに存在する社員UUIDへ置き換えてください
        var uuid = "11111111-1111-1111-1111-111111111111";

        var employee = await _employeeRepository.FindByIdAsync(uuid);

        Assert.IsNotNull(employee);
        Assert.AreEqual(uuid, employee.EmployeeUuid.ToString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(employee.Name));
        Assert.IsNotNull(employee.Department);
        Assert.AreEqual(Guid.Parse("11111111-1111-1111-1111-111111111111"), employee.EmployeeUuid);
        Assert.AreEqual("山田 太郎", employee.Name);
        Assert.AreEqual("ヤマダ タロウ", employee.Kana);
        Assert.AreEqual(Guid.Parse("e480fa43-f51c-4738-93dc-fb4fe0ecea42"), employee.Department!.DepartmentUuid);
        Assert.AreEqual("営業部", employee.Department!.Name);
    }

    [TestMethod(DisplayName = "存在しない社員UUIDの場合nullが返される")]
    public async Task FindByIdAsync_WhenUuidDoesNotExist_ShouldReturnNull()
    {
        var uuid = Guid.NewGuid().ToString();

        var employee = await _employeeRepository.FindByIdAsync(uuid);

        Assert.IsNull(employee);
    }

    [TestMethod]
    public async Task FindByIdAsync_WhenUuidIsInvalid_ShouldThrowInternalException()
    {
        // UUID形式ではない文字列
        var uuid = "invalid-uuid";

        // 実行・検証
        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _employeeRepository.FindByIdAsync(uuid);
        });
    }

    [TestMethod(DisplayName = "存在する社員UUIDの場合trueを返す")]
    public async Task ExistsByEmployeeUuidAsync_WhenEmployeeExists_ShouldReturnTrue()
    {
        var result = await _employeeRepository.ExistsByEmployeeUuidAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "存在しない社員UUIDの場合falseを返す")]
    public async Task ExistsByEmployeeUuidAsync_WhenEmployeeDoesNotExist_ShouldReturnFalse()
    {
        var result = await _employeeRepository.ExistsByEmployeeUuidAsync(
            Guid.Parse("99999999-9999-9999-9999-999999999999"));

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "社員UUID存在確認中にDB接続エラーが発生した場合InternalExceptionが発生する")]
    public async Task ExistsByEmployeeUuidAsync_WhenDatabaseConnectionError_ShouldThrowInternalException()
    {
        _dbContext.Dispose();

        await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            await _employeeRepository.ExistsByEmployeeUuidAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        });
    }
}