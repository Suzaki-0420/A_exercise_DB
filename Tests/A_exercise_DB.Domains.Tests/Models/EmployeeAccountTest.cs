using Microsoft.VisualStudio.TestTools.UnitTesting;
using A_exercise_DB.Domains.Exceptions;
using A_exercise_DB.Domains.Models;

namespace A_exercise_DB.Domains.Tests.Models;

/// <summary>
/// EmployeeAccountクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Domains/Models")]
public class EmployeeAccountTests
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
        string employeeName = "山田太郎",
        string employeeKana = "ヤマダタロウ")
    {
        return new Employee(
            employeeName,
            employeeKana,
            CreateDepartment());
    }

    /// <summary>
    /// ヘルパー：有効な社員アカウントを作成する
    /// </summary>
    private EmployeeAccount CreateEmployeeAccount(
        Guid? accountUuid = null,
        string accountName = "yamada",
        string accountPass = "password123",
        Employee? employee = null)
    {
        return new EmployeeAccount(
            accountUuid ?? Guid.NewGuid(),
            accountName,
            accountPass,
            employee ?? CreateEmployee());
    }

    [TestMethod(DisplayName = "コンストラクタに正常値を指定するとインスタンス生成される")]
    public void Constructor_WithValidValues_ShouldCreateInstance()
    {
        // データを用意する
        var accountUuid = Guid.NewGuid();
        var accountName = "yamada";
        var accountPass = "password123";
        var employee = CreateEmployee();

        // インスタンスを生成する
        var employeeAccount = new EmployeeAccount(
            accountUuid,
            accountName,
            accountPass,
            employee);

        // アカウント識別IDを検証する
        Assert.AreEqual(accountUuid, employeeAccount.AccountUuid);

        // アカウント名を検証する
        Assert.AreEqual(accountName, employeeAccount.Name);

        // パスワードを検証する
        Assert.AreEqual(accountPass, employeeAccount.Password);

        // 社員を検証する
        Assert.AreEqual(employee, employeeAccount.Employee);
    }

    [TestMethod(DisplayName = "新規作成の場合UUIDが自動生成される")]
    public void NewInstance_ShouldGenerateUuidAutomatically()
    {
        // データを用意する
        var accountName = "yamada";
        var accountPass = "password123";
        var employee = CreateEmployee();

        // インスタンスを生成する
        var employeeAccount = new EmployeeAccount(
            accountName,
            accountPass,
            employee);

        // アカウント識別IDが自動生成されていることを検証する
        Assert.AreNotEqual(Guid.Empty, employeeAccount.AccountUuid);

        // アカウント名を検証する
        Assert.AreEqual(accountName, employeeAccount.Name);

        // パスワードを検証する
        Assert.AreEqual(accountPass, employeeAccount.Password);

        // 社員を検証する
        Assert.AreEqual(employee, employeeAccount.Employee);
    }

    [TestMethod(DisplayName = "再構築・復元用コンストラクタに正常値を指定するとインスタンス生成される")]
    public void RestoreConstructor_WithValidValues_ShouldCreateInstance()
    {
        // データを用意する
        var accountUuid = Guid.NewGuid();
        var accountName = "yamada";
        var accountPass = "password123";

        // インスタンスを生成する
        var employeeAccount = new EmployeeAccount(
            accountUuid,
            accountName,
            accountPass);

        // アカウント識別IDを検証する
        Assert.AreEqual(accountUuid, employeeAccount.AccountUuid);

        // アカウント名を検証する
        Assert.AreEqual(accountName, employeeAccount.Name);

        // パスワードを検証する
        Assert.AreEqual(accountPass, employeeAccount.Password);

        // 社員がnullであることを検証する
        Assert.IsNull(employeeAccount.Employee);
    }

    [TestMethod(DisplayName = "アカウント識別IDが空の場合、DomainExceptionがスローされる")]
    public void EmptyAccountUuid_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployeeAccount(accountUuid: Guid.Empty);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("アカウント識別IDが不正です", ex.Message);
    }

    [TestMethod(DisplayName = "アカウント名がnullの場合、DomainExceptionがスローされる")]
    public void NullAccountName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployeeAccount(accountName: null!);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("アカウント名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "アカウント名が空白の場合、DomainExceptionがスローされる")]
    public void EmptyAccountName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployeeAccount(accountName: "");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("アカウント名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "アカウント名が空白文字のみの場合、DomainExceptionがスローされる")]
    public void WhiteSpaceAccountName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployeeAccount(accountName: "   ");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("アカウント名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "アカウント名が20文字の場合、インスタンス生成される")]
    public void AccountName_With20Chars_ShouldCreateInstance()
    {
        // データを用意する
        var accountName = new string('a', 20);

        // インスタンスを生成する
        var employeeAccount = CreateEmployeeAccount(accountName: accountName);

        // アカウント名を検証する
        Assert.AreEqual(accountName, employeeAccount.Name);
    }

    [TestMethod(DisplayName = "アカウント名が21文字以上の場合、DomainExceptionがスローされる")]
    public void AccountName_LongerThan20Chars_ShouldThrowDomainException()
    {
        // データを用意する
        var accountName = new string('a', 21);

        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployeeAccount(accountName: accountName);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("アカウント名は20文字以内で入力してください", ex.Message);
    }

    [TestMethod(DisplayName = "パスワードがnullの場合、DomainExceptionがスローされる")]
    public void NullAccountPass_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployeeAccount(accountPass: null!);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("パスワードは必須です", ex.Message);
    }

    [TestMethod(DisplayName = "パスワードが空白の場合、DomainExceptionがスローされる")]
    public void EmptyAccountPass_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployeeAccount(accountPass: "");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("パスワードは必須です", ex.Message);
    }

    [TestMethod(DisplayName = "パスワードが空白文字のみの場合、DomainExceptionがスローされる")]
    public void WhiteSpaceAccountPass_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployeeAccount(accountPass: "   ");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("パスワードは必須です", ex.Message);
    }

    [TestMethod(DisplayName = "パスワードが255文字の場合、インスタンス生成される")]
    public void AccountPass_With255Chars_ShouldCreateInstance()
    {
        // データを用意する
        var accountPass = new string('a', 255);

        // インスタンスを生成する
        var employeeAccount = CreateEmployeeAccount(accountPass: accountPass);

        // パスワードを検証する
        Assert.AreEqual(accountPass, employeeAccount.Password);
    }

    [TestMethod(DisplayName = "パスワードが256文字以上の場合、DomainExceptionがスローされる")]
    public void AccountPass_LongerThan255Chars_ShouldThrowDomainException()
    {
        // データを用意する
        var accountPass = new string('a', 256);

        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = CreateEmployeeAccount(accountPass: accountPass);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("パスワードは255文字以内で入力してください", ex.Message);
    }

    [TestMethod(DisplayName = "社員がnullの場合、DomainExceptionがスローされる")]
    public void NullEmployee_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new EmployeeAccount(
                Guid.NewGuid(),
                "yamada",
                "password123",
                null);
        });

        // 例外メッセージを検証する
        Assert.AreEqual("社員は必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "再構築・復元用コンストラクタでアカウント識別IDが空の場合、DomainExceptionがスローされる")]
    public void RestoreConstructor_EmptyAccountUuid_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new EmployeeAccount(
                Guid.Empty,
                "yamada",
                "password123");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("アカウント識別IDが不正です", ex.Message);
    }

    [TestMethod(DisplayName = "再構築・復元用コンストラクタでアカウント名が空白の場合、DomainExceptionがスローされる")]
    public void RestoreConstructor_EmptyAccountName_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new EmployeeAccount(
                Guid.NewGuid(),
                "",
                "password123");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("アカウント名は必須です", ex.Message);
    }

    [TestMethod(DisplayName = "再構築・復元用コンストラクタでパスワードが空白の場合、DomainExceptionがスローされる")]
    public void RestoreConstructor_EmptyAccountPass_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new EmployeeAccount(
                Guid.NewGuid(),
                "yamada",
                "");
        });

        // 例外メッセージを検証する
        Assert.AreEqual("パスワードは必須です", ex.Message);
    }

    [TestMethod(DisplayName = "同一インスタンスの場合、等価と判定される")]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        // インスタンスを生成する
        var employeeAccount = CreateEmployeeAccount();

        // 等価性を検証する
        var result = employeeAccount.Equals(employeeAccount);

        // 検証結果を評価する
        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "UUIDで等価と判定される")]
    public void Equals_WithSameUuid_ShouldReturnTrue()
    {
        // インスタンスを生成する
        var uuid = Guid.NewGuid();
        var employeeAccount1 = CreateEmployeeAccount(accountUuid: uuid, accountName: "yamada");
        var employeeAccount2 = CreateEmployeeAccount(accountUuid: uuid, accountName: "sato");

        // 等価性を検証する
        var result = employeeAccount1.Equals(employeeAccount2);

        // 検証結果を評価する
        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "異なるUUIDで非等価と判定される")]
    public void Equals_WithDifferentUuid_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var employeeAccount1 = CreateEmployeeAccount(accountName: "yamada");
        var employeeAccount2 = CreateEmployeeAccount(accountName: "sato");

        // 等価性を検証する
        var result = employeeAccount1.Equals(employeeAccount2);

        // 非等価であることを評価する
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "nullと比較した場合、非等価と判定される")]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var employeeAccount = CreateEmployeeAccount();

        // 等価性を検証する
        var result = employeeAccount.Equals(null);

        // 非等価であることを評価する
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "異なる型と比較した場合、非等価と判定される")]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var employeeAccount = CreateEmployeeAccount();

        // 等価性を検証する
        var result = employeeAccount.Equals("employeeAccount");

        // 非等価であることを評価する
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "同じUUIDの場合、同じハッシュコードが返される")]
    public void GetHashCode_WithSameUuid_ShouldReturnSameHashCode()
    {
        // インスタンスを生成する
        var uuid = Guid.NewGuid();
        var employeeAccount1 = CreateEmployeeAccount(accountUuid: uuid, accountName: "yamada");
        var employeeAccount2 = CreateEmployeeAccount(accountUuid: uuid, accountName: "sato");

        // ハッシュコードを取得する
        var hashCode1 = employeeAccount1.GetHashCode();
        var hashCode2 = employeeAccount2.GetHashCode();

        // ハッシュコードを検証する
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [TestMethod(DisplayName = "ToStringで社員アカウント情報が文字列化される")]
    public void ToString_ShouldContainEmployeeAccountProperties()
    {
        // データを用意する
        var accountUuid = Guid.NewGuid();
        var accountName = "yamada";
        var employee = CreateEmployee(employeeName: "山田太郎");

        // インスタンスを生成する
        var employeeAccount = CreateEmployeeAccount(
            accountUuid: accountUuid,
            accountName: accountName,
            employee: employee);

        // 文字列化する
        var result = employeeAccount.ToString();

        // 文字列に社員アカウント情報が含まれることを検証する
        StringAssert.Contains(result, accountUuid.ToString());
        StringAssert.Contains(result, accountName);
        StringAssert.Contains(result, "山田太郎");
    }

    [TestMethod(DisplayName = "社員がnullの場合でもToStringで社員アカウント情報が文字列化される")]
    public void ToString_WithNullEmployee_ShouldContainEmployeeAccountProperties()
    {
        // データを用意する
        var accountUuid = Guid.NewGuid();
        var accountName = "yamada";
        var accountPass = "password123";

        // 再構築・復元用コンストラクタでインスタンスを生成する
        var employeeAccount = new EmployeeAccount(
            accountUuid,
            accountName,
            accountPass);

        // 文字列化する
        var result = employeeAccount.ToString();

        // 文字列に社員アカウント情報が含まれることを検証する
        StringAssert.Contains(result, accountUuid.ToString());
        StringAssert.Contains(result, accountName);
    }
}