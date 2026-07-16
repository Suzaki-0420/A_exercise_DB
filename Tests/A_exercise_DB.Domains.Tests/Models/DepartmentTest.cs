using Microsoft.VisualStudio.TestTools.UnitTesting;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Tests.Models;

/// <summary>
/// Departmentクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Domains/Models")]
public class DepartmentTests
{
    /// <summary>
    /// ヘルパー：有効な部署を作成する
    /// </summary>
    private Department CreateDepartment(
        Guid? departmentUuid = null,
        string departmentName = "営業部")
    {
        return new Department(
            departmentUuid ?? Guid.NewGuid(),
            departmentName);
    }

    [TestMethod(DisplayName = "コンストラクタに正常値を指定するとインスタンス生成される")]
    public void Constructor_WithValidValues_ShouldCreateInstance()
    {
        // データを用意する
        var departmentUuid = Guid.NewGuid();
        var departmentName = "営業部";

        // インスタンスを生成する
        var department = new Department(departmentUuid, departmentName);

        // 部署識別IDを検証する
        Assert.AreEqual(departmentUuid, department.DepartmentUuid);

        // 部署名を検証する
        Assert.AreEqual(departmentName, department.Name);
    }

    [TestMethod(DisplayName = "新規作成の場合UUIDが自動生成される")]
    public void NewInstance_ShouldGenerateUuidAutomatically()
    {
        // データを用意する
        var departmentName = "総務部";

        // インスタンスを生成する
        var department = new Department(departmentName);

        // 部署識別IDが自動生成されていることを検証する
        Assert.AreNotEqual(Guid.Empty, department.DepartmentUuid);

        // 部署名を検証する
        Assert.AreEqual(departmentName, department.Name);
    }

    [TestMethod(DisplayName = "部署識別IDが空の場合、DomainExceptionがスローされる")]
    public void EmptyDepartmentUuid_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateDepartment(departmentUuid: Guid.Empty);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("部署識別IDが不正です", ex.Message);
    }

    [TestMethod(DisplayName = "部署名がnullの場合、DomainExceptionがスローされる")]
    public void NullDepartmentName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateDepartment(departmentName: null!);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("部署名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "部署名が空白の場合、DomainExceptionがスローされる")]
    public void EmptyDepartmentName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateDepartment(departmentName: "");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("部署名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "部署名が空白文字のみの場合、DomainExceptionがスローされる")]
    public void WhiteSpaceDepartmentName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateDepartment(departmentName: "   ");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("部署名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "部署名が100文字の場合、インスタンス生成される")]
    public void DepartmentName_With100Chars_ShouldCreateInstance()
    {
        // データを用意する
        var departmentName = new string('あ', 100);

        // インスタンスを生成する
        var department = CreateDepartment(departmentName: departmentName);

        // 部署名を検証する
        Assert.AreEqual(departmentName, department.Name);
    }

    [TestMethod(DisplayName = "部署名が101文字以上の場合、DomainExceptionがスローされる")]
    public void DepartmentName_LongerThan100Chars_ShouldThrowDomainException()
    {
        // データを用意する
        var departmentName = new string('あ', 101);

        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateDepartment(departmentName: departmentName);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("部署名は100文字以内で入力してください", ex.Message);
    }

    [TestMethod(DisplayName = "同一インスタンスの場合、等価と判定される")]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        // インスタンスを生成する
        var department = CreateDepartment();

        // 等価性を検証する
        var result = department.Equals(department);

        // 検証結果を評価する
        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "UUIDで等価と判定される")]
    public void Equals_WithSameUuid_ShouldReturnTrue()
    {
        // インスタンスを生成する
        var uuid = Guid.NewGuid();
        var department1 = CreateDepartment(departmentUuid: uuid, departmentName: "営業部");
        var department2 = CreateDepartment(departmentUuid: uuid, departmentName: "総務部");

        // 等価性を検証する
        var result = department1.Equals(department2);

        // 検証結果を評価する
        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "異なるUUIDで非等価と判定される")]
    public void Equals_WithDifferentUuid_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var department1 = CreateDepartment(departmentName: "営業部");
        var department2 = CreateDepartment(departmentName: "総務部");

        // 等価性を検証する
        var result = department1.Equals(department2);

        // 非等価であることを評価する
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "nullと比較した場合、非等価と判定される")]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var department = CreateDepartment();

        // 等価性を検証する
        var result = department.Equals(null);

        // 非等価であることを評価する
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "異なる型と比較した場合、非等価と判定される")]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var department = CreateDepartment();

        // 等価性を検証する
        var result = department.Equals("department");

        // 非等価であることを評価する
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "同じUUIDの場合、同じハッシュコードが返される")]
    public void GetHashCode_WithSameUuid_ShouldReturnSameHashCode()
    {
        // インスタンスを生成する
        var uuid = Guid.NewGuid();
        var department1 = CreateDepartment(departmentUuid: uuid, departmentName: "営業部");
        var department2 = CreateDepartment(departmentUuid: uuid, departmentName: "総務部");

        // ハッシュコードを取得する
        var hashCode1 = department1.GetHashCode();
        var hashCode2 = department2.GetHashCode();

        // ハッシュコードを検証する
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [TestMethod(DisplayName = "ToStringで部署情報が文字列化される")]
    public void ToString_ShouldContainDepartmentProperties()
    {
        // データを用意する
        var departmentUuid = Guid.NewGuid();
        var departmentName = "営業部";

        // インスタンスを生成する
        var department = CreateDepartment(
            departmentUuid: departmentUuid,
            departmentName: departmentName);

        // 文字列化する
        var result = department.ToString();

        // 文字列に部署情報が含まれることを検証する
        StringAssert.Contains(result, departmentUuid.ToString());
        StringAssert.Contains(result, departmentName);
    }
}