# API一覧

# API一覧

## UC009 担当者アカウント登録

### ベースURL

```
/admin/account
```

---

## 1. 入力画面初期表示情報取得

|項目|内容|
|---|---|
|エンドポイント|`GET /admin/account/form`|
|HTTPメソッド|GET|
|コントローラー|RegisterEmployeeAccountController|
|アクションメソッド|GetForm()|

### 概要

担当者アカウント登録画面の初期表示情報を取得します。
未登録の社員一覧を返します。

### レスポンス例（200）

```json
{
  "title": "担当者アカウント登録(入力)",
  "employees": [
    {
      "employeeUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "employeeName": "山田 太郎"
    }
  ]
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|取得成功|
|404|未登録社員なし|
|500|システムエラー|

---

## 2. アカウント名重複チェック

|項目|内容|
|---|---|
|エンドポイント|`GET /admin/account/validate?accountName={accountName}`|
|HTTPメソッド|GET|
|コントローラー|RegisterEmployeeAccountController|
|アクションメソッド|ValidateAccountName()|

### クエリパラメータ

|項目|型|必須|
|---|---|---|
|accountName|string|○|

### レスポンス例（200）

```json
{
  "exists": false,
  "message": "使用できるアカウント名です"
}
```

### 重複時（409）

```json
{
  "code": "ACCOUNT_NAME_ALREADY_EXISTS",
  "exists": true,
  "message": "アカウント名は既に存在します"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|使用可能|
|400|入力値不正|
|409|重複あり|
|500|システムエラー|

---

## 3. 入力内容確認

|項目|内容|
|---|---|
|エンドポイント|`POST /admin/account/confirm`|
|HTTPメソッド|POST|
|コントローラー|RegisterEmployeeAccountController|
|アクションメソッド|Confirm()|

### リクエスト例

```json
{
  "employeeUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "accountName": "yamada",
  "password": "Password123!"
}
```

※実際のViewModelに合わせて項目を追加してください。

### レスポンス

入力内容確認用ViewModelを返却します。

### ステータスコード

|コード|内容|
|---|---|
|200|確認成功|
|400|入力値不正|
|404|社員が存在しない|
|500|システムエラー|

---

## 4. 担当者アカウント登録

|項目|内容|
|---|---|
|エンドポイント|`POST /admin/account/register`|
|HTTPメソッド|POST|
|コントローラー|RegisterEmployeeAccountController|
|アクションメソッド|Register()|

### リクエスト例

```json
{
  "employeeUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "accountName": "yamada",
  "password": "Password123!"
}
```

### レスポンス例（201）

```json
{
  "accountUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "employeeName": "山田 太郎",
  "accountName": "yamada"
}
```

### ステータスコード

|コード|内容|
|---|---|
|201|登録成功|
|400|入力値不正|
|404|社員が存在しない|
|409|アカウント重複|
|500|システムエラー|