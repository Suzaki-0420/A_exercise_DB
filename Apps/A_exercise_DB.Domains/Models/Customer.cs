using A_exercise_DB.Domains.Exceptions;
namespace A_exercise_DB.Domains.Models;
/// <summary>
/// 顧客を表すドメインオブジェクト
/// </summary>
public class Customer
{
    /// <summary>
    /// 顧客識別ID(UUID)
    /// </summary>
    public Guid? CustomerUuid { get; private set; }
    /// <summary>
    /// 顧客名
    /// </summary>
    public string? Name { get; private set; } = string.Empty;
    /// <summary>
    /// 住所1
    /// </summary>
    public string? Address1 { get; private set; } = string.Empty;
    /// <summary>
    /// 住所2
    /// </summary>
    public string? Address2 { get; private set; } = string.Empty;
    /// <summary>
    /// 電話番号
    /// </summary>
    public string? PhoneNumber { get; private set; } = string.Empty;
    /// <summary>
    /// メールアドレス
    /// </summary>
    public string? MailAddress { get; private set; } = string.Empty;
    /// <summary>
    /// ユーザー名
    /// </summary>
    public string? Username { get; private set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    public string? Password { get; private set; } = string.Empty;
    /// <summary>
    /// 登録日時
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 顧客名の最大長
    /// </summary>
    private const int MaxLengthName = 20;
    /// <summary>
    /// 住所の最大長
    /// </summary>
    private const int MaxLengthAddress = 100;
    /// <summary>
    /// 電話番号の最大長
    /// </summary>
    private const int MaxLengthPhone = 20;
    /// <summary>
    /// メールアドレスの最大長
    /// </summary>
    private const int MaxLengthMail = 200;
    /// <summary>
    /// アカウント名の最大長
    /// </summary>
    private const int MaxLengthUserName = 30;
    /// <summary>
    /// パスワードの最大長
    /// </summary>
    private const int MaxLengthPass = 255;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public Customer(Guid? customerUuid, string? customerName, string? address1, string? address2, string? phoneNumber, string? mailAddress, string? userName, string? passWord, DateTime createdAt)
    {
        ValidateCustomerUuid(customerUuid);
        CustomerUuid = customerUuid;
        ValidateName(customerName);
        Name = customerName;
        ValidateAddress1(address1);
        Address1 = address1;
        ValidateAddress2(address2);
        Address2 = address2;
        ValidatePhoneNumber(phoneNumber);
        PhoneNumber = phoneNumber;
        ValidateMail(mailAddress);
        MailAddress = mailAddress;
        ValidateUserName(userName);
        Username = userName;
        ValidatePass(passWord);
        Password = passWord;
        ValidateCreatedAt(createdAt);
        CreatedAt = createdAt;
    }

    /// <summary>
    /// ID未定の顧客を作成する場合のコンストラクタ
    /// </summary>
    public Customer(string customerName, string address1, string address2, string phoneNumber, string mailAddress, string userName, string passWord, DateTime createdAt)
        : this(null, customerName, address1, address2, phoneNumber, mailAddress, userName, passWord, createdAt) { }

    /// <summary>
    /// 顧客識別IDの検証
    /// </summary>
    private void ValidateCustomerUuid(Guid? customerUuid)
    {
        if (customerUuid == Guid.Empty)
            throw new DomainException("顧客識別IDが不正です");
    }

    /// <summary>
    /// 顧客名の検証
    /// </summary>
    private void ValidateName(string customerName)
    {
        if (customerName.Length > MaxLengthName)
            throw new DomainException($"顧客名は{MaxLengthName}文字以内で入力してください");
    }

    /// <summary>
    /// 住所1の検証
    /// </summary>
    private void ValidateAddress1(string address1)
    {
        if (address1.Length > MaxLengthAddress)
            throw new DomainException($"住所1は{MaxLengthAddress}文字以内で入力してください");
    }
    /// <summary>
    /// 住所2の検証
    /// </summary>
    private void ValidateAddress2(string Address2)
    {
        if (Address2.Length > MaxLengthAddress)
            throw new DomainException($"住所2は{MaxLengthAddress}文字以内で入力してください");
    }
    /// <summary>
    /// 電話番号の検証
    /// </summary>
    private void ValidatePhoneNumber(string phoneNumber)
    {
        if (phoneNumber.Length > MaxLengthPhone)
            throw new DomainException($"電話番号は{MaxLengthPhone}文字以内で入力してください");
    }
    /// <summary>
    /// メールアドレスの検証
    /// </summary>
    private void ValidateMail(string mailAddress)
    {
        if (mailAddress.Length > MaxLengthMail)
            throw new DomainException($"メールアドレスは{MaxLengthMail}文字以内で入力してください");
    }
    /// <summary>
    /// ユーザー名の検証
    /// </summary>
    private void ValidateUserName(string userName)
    {
        if (userName.Length > MaxLengthUserName)
            throw new DomainException($"ユーザー名は{MaxLengthUserName}文字以内で入力してください");
    }
    /// <summary>
    /// パスワードの検証
    /// </summary>
    private void ValidatePass(string passWord)
    {
        if (passWord.Length > MaxLengthPass)
            throw new DomainException($"パスワードは{MaxLengthPass}文字以内で入力してください");
    }
    /// <summary>
    /// 登録日時の検証
    /// </summary>
    private void ValidateCreatedAt(DateTime createdAt)
    {
        if (createdAt == default)
            throw new DomainException("登録日時が不正です");

        if (createdAt > DateTime.Now)
            throw new DomainException("登録日時に未来日は指定できません");
    }
    /// <summary>
    /// 等価性（IDによる比較）
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not Customer other) return false;
        return CustomerUuid == other.CustomerUuid;
    }

    public override int GetHashCode() => CustomerUuid?.GetHashCode() ?? 0;

    public override string ToString()
        => $"{CustomerUuid?.ToString() ?? "未登録"}: {Name},{Address1}{Address2},{PhoneNumber},{MailAddress},{CreatedAt}";
}