using A_exercise_DB.Domains.Models;
using A_exercise_DB.Domains.Repositories;
using A_exercise_DB.Infrastructures.Contexts;
using A_exercise_DB.Presentations.Configs;
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

    [TestMethod(DisplayName = "社員情報をすべて取得できる")]
    public async Task FindAllAsync_WhenEmployeesExist_ShouldReturnEmployeeList()
    {
        var employees = await _employeeRepository.FindAllAsync();

        Assert.IsNotNull(employees);
        Assert.IsInstanceOfType(employees, typeof(List<Employee>));
        Assert.IsTrue(employees.Count > 0);
        Assert.IsTrue(employees.All(e => e.Department is not null));
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
    }

    [TestMethod(DisplayName = "存在しない社員UUIDの場合nullが返される")]
    public async Task FindByIdAsync_WhenUuidDoesNotExist_ShouldReturnNull()
    {
        var uuid = Guid.NewGuid().ToString();

        var employee = await _employeeRepository.FindByIdAsync(uuid);

        Assert.IsNull(employee);
    }
}