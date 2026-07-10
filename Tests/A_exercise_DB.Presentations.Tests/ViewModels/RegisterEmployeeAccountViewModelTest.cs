using System.ComponentModel.DataAnnotations;
using A_exercise_DB.Presentations.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A_exercise_DB.Presentations.Tests.ViewModels;

/// <summary>
/// RegisterEmployeeAccountViewModelの単体テスト
/// </summary>
[TestClass]
public class RegisterEmployeeAccountViewModelTest
{
    /// <summary>
    /// すべての入力値が正常な場合、検証エラーが発生しないこと
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_すべての入力値が正常な場合は検証エラーが発生しない")]
    public void RegisterEmployeeAccountViewModel_WhenAllValuesAreValid_HasNoValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "user01",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// 社員UUIDがnullの場合、検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_社員UUIDがnullの場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenEmployeeUuidIsNull_HasValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = null,
            AccountName = "user01",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "社員名を選択してください",
            nameof(RegisterEmployeeAccountViewModel.EmployeeUuid));
    }

    /// <summary>
    /// アカウント名が未入力の場合、検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_アカウント名が未入力の場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenAccountNameIsEmpty_HasValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = string.Empty,
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "アカウント名を入力してください",
            nameof(RegisterEmployeeAccountViewModel.AccountName));
    }

    /// <summary>
    /// アカウント名が5文字未満の場合、検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_アカウント名が5文字未満の場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenAccountNameIsTooShort_HasValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "abcd",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "アカウント名は5～20文字で入力してください",
            nameof(RegisterEmployeeAccountViewModel.AccountName));
    }

    /// <summary>
    /// アカウント名が20文字の場合、検証エラーが発生しないこと
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_アカウント名が20文字の場合は検証エラーが発生しない")]
    public void RegisterEmployeeAccountViewModel_WhenAccountNameLengthIsTwenty_HasNoValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "abcdefghijklmnopqrst",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// アカウント名が20文字を超える場合、検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_アカウント名が20文字を超える場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenAccountNameIsTooLong_HasValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "abcdefghijklmnopqrstu",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "アカウント名は5～20文字で入力してください",
            nameof(RegisterEmployeeAccountViewModel.AccountName));
    }

    /// <summary>
    /// アカウント名に半角英数字以外が含まれる場合、検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_アカウント名に半角英数字以外が含まれる場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenAccountNameContainsInvalidCharacter_HasValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "user-01",
            Password = "pass01"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "アカウント名は半角英数字で入力し、同じ文字のみの登録はできません",
            nameof(RegisterEmployeeAccountViewModel.AccountName));
    }

    /// <summary>
    /// パスワードが未入力の場合、検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_パスワードが未入力の場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenPasswordIsEmpty_HasValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "user01",
            Password = string.Empty
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "パスワードを入力してください",
            nameof(RegisterEmployeeAccountViewModel.Password));
    }

    /// <summary>
    /// パスワードが5文字未満の場合、検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_パスワードが5文字未満の場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenPasswordIsTooShort_HasValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "user01",
            Password = "abcd"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "パスワードは5～20文字で入力してください",
            nameof(RegisterEmployeeAccountViewModel.Password));
    }

    /// <summary>
    /// パスワードが20文字の場合、検証エラーが発生しないこと
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_パスワードが20文字の場合は検証エラーが発生しない")]
    public void RegisterEmployeeAccountViewModel_WhenPasswordLengthIsTwenty_HasNoValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "user01",
            Password = "abcdefghijklmnopqrst"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        Assert.IsEmpty(results);
    }

    /// <summary>
    /// パスワードが20文字を超える場合、検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_パスワードが20文字を超える場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenPasswordIsTooLong_HasValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "user01",
            Password = "abcdefghijklmnopqrstu"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "パスワードは5～20文字で入力してください",
            nameof(RegisterEmployeeAccountViewModel.Password));
    }

    /// <summary>
    /// パスワードに半角英数字以外が含まれる場合、検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_パスワードに半角英数字以外が含まれる場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenPasswordContainsInvalidCharacter_HasValidationError()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "user01",
            Password = "pass-01"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "パスワードは半角英数字で入力し、同じ文字のみの登録はできません",
            nameof(RegisterEmployeeAccountViewModel.Password));
    }

    /// <summary>
    /// デフォルト値の場合、各プロパティの検証エラーが発生すること
    /// </summary>
    [TestMethod(DisplayName = "RegisterEmployeeAccountViewModel_デフォルト値の場合は検証エラーが発生する")]
    public void RegisterEmployeeAccountViewModel_WhenDefaultValue_HasValidationErrors()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel();

        // Act
        var results = ValidateModel(model);

        // Assert
        AssertValidationError(
            results,
            "社員名を選択してください",
            nameof(RegisterEmployeeAccountViewModel.EmployeeUuid));

        AssertValidationError(
            results,
            "アカウント名を入力してください",
            nameof(RegisterEmployeeAccountViewModel.AccountName));

        AssertValidationError(
            results,
            "パスワードを入力してください",
            nameof(RegisterEmployeeAccountViewModel.Password));
    }

    /// <summary>
    /// ViewModelの検証を実行する
    /// </summary>
    private static List<ValidationResult> ValidateModel(RegisterEmployeeAccountViewModel model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(
            model,
            context,
            results,
            true);

        return results;
    }

    /// <summary>
    /// 指定した検証エラーが存在することを確認する
    /// </summary>
    private static void AssertValidationError(
        List<ValidationResult> results,
        string errorMessage,
        string memberName)
    {
        Assert.IsTrue(
            results.Any(x =>
                x.ErrorMessage == errorMessage
                && x.MemberNames.Contains(memberName)),
            $"期待した検証エラーが見つかりません。ErrorMessage: {errorMessage}, MemberName: {memberName}");
    }

    [TestMethod]
    public void AccountName_WhenSameCharacterOnly_ShouldBeInvalid()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "AAAAA",
            Password = "abcde1"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(
            model,
            context,
            results,
            true);

        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r =>
            r.ErrorMessage == "アカウント名は半角英数字で入力し、同じ文字のみの登録はできません"));
    }

    [TestMethod]
    public void Password_WhenSameCharacterOnly_ShouldBeInvalid()
    {
        // Arrange
        var model = new RegisterEmployeeAccountViewModel
        {
            EmployeeUuid = Guid.NewGuid(),
            AccountName = "user01",
            Password = "11111"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(
            model,
            context,
            results,
            true);

        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r =>
            r.ErrorMessage == "パスワードは半角英数字で入力し、同じ文字のみの登録はできません"));
    }
}