using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Presentations.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A_exercise_DB.Infrastructures.Tests.Repositories;

/// <summary>
/// 社員Repositoryの単体テスト
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class EmployeeRepositoryTests
{
    private static TestContext? _testContext;
    private static AppDbContext? _dbContext;
    private static IEmployeeRepository _employeeRepository = null!;
    private static ServiceProvider? _provider;
    private IServiceScope? _scope;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _testContext = context;

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _provider?.Dispose();
    }

    [TestInitialize]
    public void TestInit()
    {
        _scope = _provider!.CreateScope();

        _employeeRepository =
            _scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();

        _dbContext =
            _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _scope!.Dispose();
    }

    [TestMethod("社員情報をすべて取得できる")]
    public async Task FindAllAsync_WhenEmployeesExist_ShouldReturnEmployeeList()
    {
        // 社員情報をすべて取得する
        var employees = await _employeeRepository.FindAllAsync();

        // nullでないことを検証する
        Assert.IsNotNull(employees);

        // List<Employee>型であることを検証する
        Assert.IsInstanceOfType(employees, typeof(List<Employee>));

        // 1件以上取得できることを検証する
        Assert.IsTrue(employees.Count > 0);

        // 取得した社員の部署情報も復元されていることを検証する
        Assert.IsTrue(employees.All(e => e.Department is not null));
    }

    [TestMethod("存在する社員UUIDで社員情報を取得できる")]
    public async Task FindByIdAsync_WhenUuidExists_ShouldReturnEmployee()
    {
        // テストデータに存在する社員UUIDを指定する
        var uuid = "ここに存在する社員UUIDを入れてください";

        // 社員情報を取得する
        var employee = await _employeeRepository.FindByIdAsync(uuid);

        // nullでないことを検証する
        Assert.IsNotNull(employee);

        // 社員UUIDを検証する
        Assert.AreEqual(uuid, employee.EmployeeUuid.ToString());

        // 社員名が取得できていることを検証する
        Assert.IsFalse(string.IsNullOrWhiteSpace(employee.Name));

        // 部署が取得できていることを検証する
        Assert.IsNotNull(employee.Department);
    }

    [TestMethod("存在しない社員UUIDの場合nullが返される")]
    public async Task FindByIdAsync_WhenUuidDoesNotExist_ShouldReturnNull()
    {
        // 存在しない社員UUIDを指定する
        var uuid = Guid.NewGuid().ToString();

        // 社員情報を取得する
        var employee = await _employeeRepository.FindByIdAsync(uuid);

        // nullであることを検証する
        Assert.IsNull(employee);
    }
}