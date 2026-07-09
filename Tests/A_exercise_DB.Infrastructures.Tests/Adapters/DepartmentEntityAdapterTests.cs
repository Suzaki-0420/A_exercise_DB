using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructures.Tests.Adapters;

[TestClass]
public class DepartmentEntityAdapterTests
{
    private DepartmentEntityAdapter _adapter = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _adapter = new DepartmentEntityAdapter();
    }

    [TestMethod]
    public async Task ConvertAsync_DepartmentをDepartmentEntityに変換できる()
    {
        // Arrange
        var department = new Department(
            Guid.NewGuid(),
            "営業部"
        );

        // Act
        var result = await _adapter.ConvertAsync(department);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(department.DepartmentUuid, result.DepartmentUuid);
        Assert.AreEqual(department.Name, result.Name);
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
    public async Task RestoreAsync_DepartmentEntityからDepartmentを復元できる()
    {
        // Arrange
        var entity = new DepartmentEntity
        {
            DepartmentUuid = Guid.NewGuid(),
            Name = "営業部"
        };

        // Act
        var result = await _adapter.RestoreAsync(entity);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(entity.DepartmentUuid, result.DepartmentUuid);
        Assert.AreEqual(entity.Name, result.Name);
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