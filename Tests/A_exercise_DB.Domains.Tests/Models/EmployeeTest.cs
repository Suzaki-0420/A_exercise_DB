using Microsoft.VisualStudio.TestTools.UnitTesting;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Tests.Models;

/// <summary>
/// Employeeクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Domains/Models")]
public class EmployeeTests
{
    /// <summary>
    /// ヘルパー：有効な部署を作成する
    /// </summary>
    private Department CreateDepartment(string departmentName = "営業部")
    {
        return new Department(departmentName);
    }

    /// <summary>
    /// ヘルパー：有効な社員を作成する
    /// </summary>
    private Employee CreateEmployee(
        Guid? employeeUuid = null,
        string employeeName = "山田太郎",
        string employeeKana = "ヤマダタロウ",
        Department? department = null)
    {
        return new Employee(
            employeeUuid ?? Guid.NewGuid(),
            employeeName,
            employeeKana,
            department ?? CreateDepartment());
    }

    [TestMethod(DisplayName = "コンストラクタに正常値を指定するとインスタンス生成される")]
    public void Constructor_WithValidValues_ShouldCreateInstance()
    {
        // データを用意する
        var employeeUuid = Guid.NewGuid();
        var employeeName = "山田太郎";
        var employeeKana = "ヤマダタロウ";
        var department = CreateDepartment();

        // インスタンスを生成する
        var employee = new Employee(
            employeeUuid,
            employeeName,
            employeeKana,
            department);

        // 社員識別IDを検証する
        Assert.AreEqual(employeeUuid, employee.EmployeeUuid);

        // 社員名を検証する
        Assert.AreEqual(employeeName, employee.Name);

        // 社員カナを検証する
        Assert.AreEqual(employeeKana, employee.Kana);

        // 部署を検証する
        Assert.AreEqual(department, employee.Department);
    }

    [TestMethod(DisplayName = "新規作成の場合UUIDが自動生成される")]
    public void NewInstance_ShouldGenerateUuidAutomatically()
    {
        // データを用意する
        var employeeName = "山田太郎";
        var employeeKana = "ヤマダタロウ";
        var department = CreateDepartment();

        // インスタンスを生成する
        var employee = new Employee(
            employeeName,
            employeeKana,
            department);

        // 社員識別IDが自動生成されていることを検証する
        Assert.AreNotEqual(Guid.Empty, employee.EmployeeUuid);

        // 社員名を検証する
        Assert.AreEqual(employeeName, employee.Name);

        // 社員カナを検証する
        Assert.AreEqual(employeeKana, employee.Kana);

        // 部署を検証する
        Assert.AreEqual(department, employee.Department);
    }

    [TestMethod(DisplayName = "再構築・復元用コンストラクタに正常値を指定するとインスタンス生成される")]
    public void RestoreConstructor_WithValidValues_ShouldCreateInstance()
    {
        // データを用意する
        var employeeUuid = Guid.NewGuid();
        var employeeName = "山田太郎";
        var employeeKana = "ヤマダタロウ";

        // インスタンスを生成する
        var employee = new Employee(
            employeeUuid,
            employeeName,
            employeeKana);

        // 社員識別IDを検証する
        Assert.AreEqual(employeeUuid, employee.EmployeeUuid);

        // 社員名を検証する
        Assert.AreEqual(employeeName, employee.Name);

        // 社員カナを検証する
        Assert.AreEqual(employeeKana, employee.Kana);

        // 部署がnullであることを検証する
        Assert.IsNull(employee.Department);
    }

    [TestMethod(DisplayName = "社員識別IDが空の場合、DomainExceptionがスローされる")]
    public void EmptyEmployeeUuid_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployee(employeeUuid: Guid.Empty);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員識別IDが不正です", ex.Message);
    }

    [TestMethod(DisplayName = "社員名がnullの場合、DomainExceptionがスローされる")]
    public void NullEmployeeName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployee(employeeName: null!);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "社員名が空白の場合、DomainExceptionがスローされる")]
    public void EmptyEmployeeName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployee(employeeName: "");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "社員名が空白文字のみの場合、DomainExceptionがスローされる")]
    public void WhiteSpaceEmployeeName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployee(employeeName: "   ");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "社員名が100文字の場合、インスタンス生成される")]
    public void EmployeeName_With100Chars_ShouldCreateInstance()
    {
        // データを用意する
        var employeeName = new string('あ', 100);

        // インスタンスを生成する
        var employee = CreateEmployee(employeeName: employeeName);

        // 社員名を検証する
        Assert.AreEqual(employeeName, employee.Name);
    }

    [TestMethod(DisplayName = "社員名が101文字以上の場合、DomainExceptionがスローされる")]
    public void EmployeeName_LongerThan100Chars_ShouldThrowDomainException()
    {
        // データを用意する
        var employeeName = new string('あ', 101);

        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployee(employeeName: employeeName);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名は100文字以内で入力してください", ex.Message);
    }

    [TestMethod(DisplayName = "社員カナがnullの場合、DomainExceptionがスローされる")]
    public void NullEmployeeKana_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployee(employeeKana: null!);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名カナは必須です", ex.Message);
    }

    [TestMethod(DisplayName = "社員カナが空白の場合、DomainExceptionがスローされる")]
    public void EmptyEmployeeKana_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployee(employeeKana: "");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名カナは必須です", ex.Message);
    }

    [TestMethod(DisplayName = "社員カナが空白文字のみの場合、DomainExceptionがスローされる")]
    public void WhiteSpaceEmployeeKana_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployee(employeeKana: "   ");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名カナは必須です", ex.Message);
    }

    [TestMethod(DisplayName = "社員カナが100文字の場合、インスタンス生成される")]
    public void EmployeeKana_With100Chars_ShouldCreateInstance()
    {
        // データを用意する
        var employeeKana = new string('ア', 100);

        // インスタンスを生成する
        var employee = CreateEmployee(employeeKana: employeeKana);

        // 社員カナを検証する
        Assert.AreEqual(employeeKana, employee.Kana);
    }

    [TestMethod(DisplayName = "社員カナが101文字以上の場合、DomainExceptionがスローされる")]
    public void EmployeeKana_LongerThan100Chars_ShouldThrowDomainException()
    {
        // データを用意する
        var employeeKana = new string('ア', 101);

        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployee(employeeKana: employeeKana);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名カナは100文字以内で入力してください", ex.Message);
    }

    [TestMethod(DisplayName = "部署がnullの場合、DomainExceptionがスローされる")]
    public void NullDepartment_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new Employee(
                Guid.NewGuid(),
                "山田太郎",
                "ヤマダタロウ",
                null);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("部署は必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "再構築・復元用コンストラクタで社員識別IDが空の場合、DomainExceptionがスローされる")]
    public void RestoreConstructor_EmptyEmployeeUuid_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new Employee(
                Guid.Empty,
                "山田太郎",
                "ヤマダタロウ");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員識別IDが不正です", ex.Message);
    }

    [TestMethod(DisplayName = "再構築・復元用コンストラクタで社員名が空白の場合、DomainExceptionがスローされる")]
    public void RestoreConstructor_EmptyEmployeeName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new Employee(
                Guid.NewGuid(),
                "",
                "ヤマダタロウ");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "再構築・復元用コンストラクタで社員カナが空白の場合、DomainExceptionがスローされる")]
    public void RestoreConstructor_EmptyEmployeeKana_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new Employee(
                Guid.NewGuid(),
                "山田太郎",
                "");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員名カナは必須です", ex.Message);
    }

    [TestMethod(DisplayName = "同一インスタンスの場合、等価と判定される")]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        // インスタンスを生成する
        var employee = CreateEmployee();

        // 等価性を検証する
        var result = employee.Equals(employee);

        // 検証結果を評価する
        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "UUIDで等価と判定される")]
    public void Equals_WithSameUuid_ShouldReturnTrue()
    {
        // インスタンスを生成する
        var uuid = Guid.NewGuid();
        var employee1 = CreateEmployee(employeeUuid: uuid, employeeName: "山田太郎");
        var employee2 = CreateEmployee(employeeUuid: uuid, employeeName: "佐藤花子");

        // 等価性を検証する
        var result = employee1.Equals(employee2);

        // 検証結果を評価する
        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "異なるUUIDで非等価と判定される")]
    public void Equals_WithDifferentUuid_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var employee1 = CreateEmployee(employeeName: "山田太郎");
        var employee2 = CreateEmployee(employeeName: "佐藤花子");

        // 等価性を検証する
        var result = employee1.Equals(employee2);

        // 非等価であることを評価する
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "nullと比較した場合、非等価と判定される")]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var employee = CreateEmployee();

        // 等価性を検証する
        var result = employee.Equals(null);

        // 非等価であることを評価する
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "異なる型と比較した場合、非等価と判定される")]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var employee = CreateEmployee();

        // 等価性を検証する
        var result = employee.Equals("employee");

        // 非等価であることを評価する
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "同じUUIDの場合、同じハッシュコードが返される")]
    public void GetHashCode_WithSameUuid_ShouldReturnSameHashCode()
    {
        // インスタンスを生成する
        var uuid = Guid.NewGuid();
        var employee1 = CreateEmployee(employeeUuid: uuid, employeeName: "山田太郎");
        var employee2 = CreateEmployee(employeeUuid: uuid, employeeName: "佐藤花子");

        // ハッシュコードを取得する
        var hashCode1 = employee1.GetHashCode();
        var hashCode2 = employee2.GetHashCode();

        // ハッシュコードを検証する
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [TestMethod(DisplayName = "ToStringで社員情報が文字列化される")]
    public void ToString_ShouldContainEmployeeProperties()
    {
        // データを用意する
        var employeeUuid = Guid.NewGuid();
        var employeeName = "山田太郎";
        var employeeKana = "ヤマダタロウ";
        var department = CreateDepartment("営業部");

        // インスタンスを生成する
        var employee = CreateEmployee(
            employeeUuid: employeeUuid,
            employeeName: employeeName,
            employeeKana: employeeKana,
            department: department);

        // 文字列化する
        var result = employee.ToString();

        // 文字列に社員情報が含まれることを検証する
        StringAssert.Contains(result, employeeUuid.ToString());
        StringAssert.Contains(result, employeeName);
        StringAssert.Contains(result, employeeKana);
        StringAssert.Contains(result, "営業部");
    }

    [TestMethod(DisplayName = "部署がnullの場合でもToStringで社員情報が文字列化される")]
    public void ToString_WithNullDepartment_ShouldContainEmployeeProperties()
    {
        // データを用意する
        var employeeUuid = Guid.NewGuid();
        var employeeName = "山田太郎";
        var employeeKana = "ヤマダタロウ";

        // 再構築・復元用コンストラクタでインスタンスを生成する
        var employee = new Employee(
            employeeUuid,
            employeeName,
            employeeKana);

        // 文字列化する
        var result = employee.ToString();

        // 文字列に社員情報が含まれることを検証する
        StringAssert.Contains(result, employeeUuid.ToString());
        StringAssert.Contains(result, employeeName);
        StringAssert.Contains(result, employeeKana);
    }
}