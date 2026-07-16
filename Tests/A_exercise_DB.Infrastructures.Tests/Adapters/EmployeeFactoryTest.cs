using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructures.Tests.Adapters;

[TestClass]
public class EmployeeFactoryTests
{
    private EmployeeFactory _factory = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _factory = new EmployeeFactory(
            new EmployeeEntityAdapter(),
            new DepartmentEntityAdapter());
    }

    [TestMethod]
    public async Task ConvertAsync_部署ありのEmployeeをEmployeeEntityに変換できる()
    {
        // Arrange
        var department = new Department(
            Guid.NewGuid(),
            "営業部");

        var employee = new Employee(
            Guid.NewGuid(),
            "山田太郎",
            "ヤマダタロウ");

        employee.ChangeDepartment(department);

        // Act
        var result = await _factory.ConvertAsync(employee);

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(employee.EmployeeUuid, result.EmployeeUuid);
        Assert.AreEqual(employee.Name, result.Name);
        Assert.AreEqual(employee.Kana, result.Kana);

        Assert.IsNotNull(result.Department);
        Assert.AreEqual(department.DepartmentUuid, result.Department.DepartmentUuid);
        Assert.AreEqual(department.Name, result.Department.Name);
    }

    [TestMethod]
    public async Task ConvertAsync_部署なしのEmployeeをEmployeeEntityに変換できる()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "山田太郎",
            "ヤマダタロウ");

        // Act
        var result = await _factory.ConvertAsync(employee);

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(employee.EmployeeUuid, result.EmployeeUuid);
        Assert.AreEqual(employee.Name, result.Name);
        Assert.AreEqual(employee.Kana, result.Kana);

        Assert.IsNull(result.Department);
    }

    [TestMethod]
    public async Task RestoreAsync_部署ありのEmployeeEntityからEmployeeを復元できる()
    {
        // Arrange
        var entity = new EmployeeEntity
        {
            EmployeeUuid = Guid.NewGuid(),
            Name = "山田太郎",
            Kana = "ヤマダタロウ",
            Department = new DepartmentEntity
            {
                DepartmentUuid = Guid.NewGuid(),
                Name = "営業部"
            }
        };

        // Act
        var result = await _factory.RestoreAsync(entity);

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(entity.EmployeeUuid, result.EmployeeUuid);
        Assert.AreEqual(entity.Name, result.Name);
        Assert.AreEqual(entity.Kana, result.Kana);

        Assert.IsNotNull(result.Department);
        Assert.AreEqual(entity.Department.DepartmentUuid, result.Department.DepartmentUuid);
        Assert.AreEqual(entity.Department.Name, result.Department.Name);
    }

    [TestMethod]
    public async Task RestoreAsync_部署なしのEmployeeEntityからEmployeeを復元できる()
    {
        // Arrange
        var entity = new EmployeeEntity
        {
            EmployeeUuid = Guid.NewGuid(),
            Name = "山田太郎",
            Kana = "ヤマダタロウ"
        };

        // Act
        var result = await _factory.RestoreAsync(entity);

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(entity.EmployeeUuid, result.EmployeeUuid);
        Assert.AreEqual(entity.Name, result.Name);
        Assert.AreEqual(entity.Kana, result.Kana);

        Assert.IsNull(result.Department);
    }
}