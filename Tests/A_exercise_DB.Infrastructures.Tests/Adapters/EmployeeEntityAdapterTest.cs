using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructures.Tests.Adapters;

[TestClass]
public class EmployeeEntityAdapterTests
{
    private EmployeeEntityAdapter _adapter = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _adapter = new EmployeeEntityAdapter();
    }

    [TestMethod]
    public async Task ConvertAsync_EmployeeをEmployeeEntityに変換できる()
    {
        // Arrange
        var employee = new Employee(
            Guid.NewGuid(),
            "山田太郎",
            "ヤマダタロウ"
        );

        // Act
        var result = await _adapter.ConvertAsync(employee);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(employee.EmployeeUuid, result.EmployeeUuid);
        Assert.AreEqual(employee.Name, result.Name);
        Assert.AreEqual(employee.Kana, result.Kana);
    }

    [TestMethod]
    public async Task ConvertAsync_引数がnullの場合InternalExceptionをthrowする()
    {
        // Act & Assert
        var ex = await Assert.ThrowsExactlyAsync<InternalException>(
            async () => await _adapter.ConvertAsync(null!)
        );

        Assert.AreEqual("引数domainがnullです。", ex.Message);
    }

    [TestMethod]
    public async Task RestoreAsync_EmployeeEntityからEmployeeを復元できる()
    {
        // Arrange
        var entity = new EmployeeEntity
        {
            EmployeeUuid = Guid.NewGuid(),
            Name = "山田太郎",
            Kana = "ヤマダタロウ"
        };

        // Act
        var result = await _adapter.RestoreAsync(entity);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(entity.EmployeeUuid, result.EmployeeUuid);
        Assert.AreEqual(entity.Name, result.Name);
        Assert.AreEqual(entity.Kana, result.Kana);
    }

    [TestMethod]
    public async Task RestoreAsync_引数がnullの場合InternalExceptionをthrowする()
    {
        // Act & Assert
        var ex = await Assert.ThrowsExactlyAsync<InternalException>(
            async () => await _adapter.RestoreAsync(null!)
        );

        Assert.AreEqual("引数targetがnullです。", ex.Message);
    }
}