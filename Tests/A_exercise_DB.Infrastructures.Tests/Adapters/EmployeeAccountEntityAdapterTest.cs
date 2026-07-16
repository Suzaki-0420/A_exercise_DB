using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;
using A_exercise_DB.Infrastructures.Adapters;
using A_exercise_DB.Infrastructures.Entities;

namespace A_exercise_DB.Infrastructures.Tests.Adapters;

[TestClass]
public class EmployeeAccountEntityAdapterTests
{
    private EmployeeAccountEntityAdapter _adapter = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _adapter = new EmployeeAccountEntityAdapter();
    }

    [TestMethod]
    public async Task ConvertAsync_EmployeeAccountをEmployeeAccountEntityに変換できる()
    {
        // Arrange
        var account = new EmployeeAccount(
            Guid.NewGuid(),
            "testuser",
            "password123"
        );

        // Act
        var result = await _adapter.ConvertAsync(account);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(account.AccountUuid, result.AccountUuid);
        Assert.AreEqual(account.Name, result.Name);
        Assert.AreEqual(account.Password, result.Password);
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
    public async Task RestoreAsync_EmployeeAccountEntityからEmployeeAccountを復元できる()
    {
        // Arrange
        var entity = new EmployeeAccountEntity
        {
            AccountUuid = Guid.NewGuid(),
            Name = "testuser",
            Password = "password123"
        };

        // Act
        var result = await _adapter.RestoreAsync(entity);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(entity.AccountUuid, result.AccountUuid);
        Assert.AreEqual(entity.Name, result.Name);
        Assert.AreEqual(entity.Password, result.Password);
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