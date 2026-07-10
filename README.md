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





## UC010 新商品登録

### ベースURL

```
/admin/product
```

---

## 1. 商品名重複チェック

|項目|内容|
|---|---|
|エンドポイント|`GET /admin/product/validate?ProductName={ProductName}`|
|HTTPメソッド|GET|
|コントローラー|RegisterProductController|
|アクションメソッド|ValidateProductName()|

### 概要

登録する商品名が既に存在するかを確認します。

### クエリパラメータ

|項目|型|必須|
|---|---|---|
|ProductName|string|○|

### レスポンス例（200）

```json
{
  "exists": false
}
```

### 重複時（409）

```json
{
  "code": "Product_ALREADY_EXISTS",
  "message": "商品名は既に存在します。"
}
```

### 入力エラー（400）

```json
{
  "code": "INVALID_Product_NAME",
  "message": "商品名は必須です。"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|商品名は使用可能|
|400|商品名未入力|
|409|商品名が既に存在する|
|500|システムエラー|

---

## 2. 新商品登録

|項目|内容|
|---|---|
|エンドポイント|`POST /admin/product/register`|
|HTTPメソッド|POST|
|コントローラー|RegisterProductController|
|アクションメソッド|Register()|

### 概要

新しい商品を登録します。

### リクエスト例

```json
{
  "name": "赤ペン",
  "price": 120,
  "imageUrl": "https://example.com/redpen.png",
  "categoryUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "stock": 10
}
```

※実際の `RegisterViewModel` に合わせて項目を修正してください。

### レスポンス例（201）

```json
{
  "productUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "name": "赤ペン",
  "price": 120,
  "imageUrl": "https://example.com/redpen.png",
  "category": {
    "categoryUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "文房具"
  },
  "stock": {
    "stock": 10
  }
}
```

### 入力エラー（400）

```json
{
  "code": "VALIDATION_ERROR",
  "message": "商品名の入力内容に誤りがあります。"
}
```

### カテゴリ未選択（400）

```json
{
  "code": "CATEGORY_REQUIRED",
  "message": "商品カテゴリを選択してください。"
}
```

### カテゴリ不存在（404）

```json
{
  "code": "CATEGORY_NOT_FOUND",
  "message": "商品カテゴリが存在しません。"
}
```

### 商品重複（400）

```json
{
  "code": "Product_ALREADY_EXISTS",
  "message": "商品名は既に存在します。"
}
```

### ドメインルール違反（400）

```json
{
  "code": "DOMAIN_RULE_VIOLATION",
  "message": "業務ルール違反です。"
}
```

### ステータスコード

|コード|内容|
|---|---|
|201|登録成功|
|400|入力値不正・商品重複・カテゴリ未選択・業務ルール違反|
|404|商品カテゴリが存在しない|
|500|システムエラー|

---

## 3. 商品カテゴリ一覧取得

|項目|内容|
|---|---|
|エンドポイント|`GET /admin/product/categories`|
|HTTPメソッド|GET|
|コントローラー|RegisterProductController|
|アクションメソッド|GetCategories()|

### 概要

商品登録画面で使用する商品カテゴリ一覧を取得します。

### レスポンス例（200）

```json
[
  {
    "categoryUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "文房具"
  },
  {
    "categoryUuid": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
    "name": "食品"
  }
]
```

### ステータスコード

|コード|内容|
|---|---|
|200|取得成功|
|500|システムエラー|





## UC011 商品検索（カテゴリ）

### ベースURL

```
/admin/product/category
```

---

## 1. 商品カテゴリ検索

|項目|内容|
|---|---|
|エンドポイント|`GET /admin/product/category?productCategoryId={productCategoryId}`|
|HTTPメソッド|GET|
|コントローラー|SearchProductByCategory|
|アクションメソッド|Search()|

### 概要

指定した商品カテゴリIDに属する商品一覧を取得します。

### クエリパラメータ

|項目|型|必須|
|---|---|---|
|productCategoryId|int|○|

### レスポンス例（200）

```json
[
  {
    "productUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "赤ペン",
    "price": 120,
    "imageUrl": "https://example.com/redpen.png",
    "category": {
      "categoryUuid": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "name": "文房具"
    },
    "stock": {
      "stock": 10
    },
    "sales": 0
  },
  {
    "productUuid": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
    "name": "青ペン",
    "price": 120,
    "imageUrl": "https://example.com/bluepen.png",
    "category": {
      "categoryUuid": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "name": "文房具"
    },
    "stock": {
      "stock": 8
    },
    "sales": 2
  }
]
```

### ステータスコード

|コード|内容|
|---|---|
|200|検索成功|
|500|システムエラー|





## UC012 商品修正

### ベースURL

```
/api/admin/products
```

---

## 1. 商品修正

|項目|内容|
|---|---|
|エンドポイント|`PUT /api/admin/products/{productUuid}`|
|HTTPメソッド|PUT|
|コントローラー|UpdateProductController|
|アクションメソッド|UpdateAsync()|

### 概要

指定した商品UUIDの商品情報を修正します。

### パスパラメータ

|項目|型|必須|
|---|---|---|
|productUuid|string(UUID)|○|

### リクエスト例

```json
{
  "name": "赤ペン",
  "price": 120,
  "stockQuantity": 10,
  "categoryUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

※実際の `UpdateProductViewModel` の項目に合わせて修正してください。

### レスポンス例（200）

```json
{
  "success": true,
  "data": {
    "productUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "赤ペン",
    "price": 120,
    "stockQuantity": 10,
    "categoryUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

※`ProductUpdateCompleteResult` の内容に合わせて修正してください。

### リクエスト未入力（400）

```json
{
  "success": false,
  "code": "VALIDATION_ERROR",
  "message": "商品修正情報を入力してください。",
  "target": "request"
}
```

### 入力値エラー（400）

```json
{
  "success": false,
  "code": "VALIDATION_ERROR",
  "message": "入力値が不正です。"
}
```

※メッセージは `DomainException` の内容によって変わります。

### 商品が存在しない場合（404）

```json
{
  "success": false,
  "code": "NOT_FOUND",
  "message": "商品が存在しません。",
  "target": "productUuid"
}
```

※メッセージは `NotFoundException` の内容によって変わります。

### サーバーエラー（500）

```json
{
  "success": false,
  "code": "INTERNAL_ERROR",
  "message": "商品修正中にエラーが発生しました。"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|商品修正成功|
|400|入力値不正、またはリクエスト未入力|
|401|未認証（JWTトークン未設定・無効）|
|404|指定した商品が存在しない|
|500|システムエラー|





## UC013 商品削除

### ベースURL

```
/api/admin/products
```

---

## 1. 商品削除

|項目|内容|
|---|---|
|エンドポイント|`DELETE /api/admin/products/{productUuid}`|
|HTTPメソッド|DELETE|
|コントローラー|DeleteProductController|
|アクションメソッド|DeleteAsync()|

### 概要

指定した商品UUIDの商品を論理削除します。

### パスパラメータ

|項目|型|必須|
|---|---|---|
|productUuid|string(UUID)|○|

### レスポンス例（200）

```json
{
  "success": true,
  "data": {
    "productUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "message": "商品を削除しました。"
  },
  "errors": []
}
```

※`ProductDeleteCompleteResult` の内容に合わせて修正してください。

### 入力値エラー（400）

```json
{
  "success": false,
  "data": null,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "商品UUIDが不正です。",
      "field": "productUuid"
    }
  ]
}
```

※メッセージは `DomainException` の内容によって変わります。

### 商品が存在しない場合（404）

```json
{
  "success": false,
  "data": null,
  "errors": [
    {
      "code": "NOT_FOUND",
      "message": "商品が存在しません。",
      "field": "productUuid"
    }
  ]
}
```

※メッセージは `NotFoundException` の内容によって変わります。

### サーバーエラー（500）

```json
{
  "success": false,
  "data": null,
  "errors": [
    {
      "code": "INTERNAL_ERROR",
      "message": "商品削除中にエラーが発生しました。",
      "field": null
    }
  ]
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|商品削除成功|
|400|入力値不正|
|404|指定した商品が存在しない|
|500|システムエラー|





## UC014 商品カテゴリ登録

### ベースURL

```
/admin/category
```

---

## 1. 商品カテゴリ名重複チェック

|項目|内容|
|---|---|
|エンドポイント|`GET /admin/category/validate?categoryName={categoryName}`|
|HTTPメソッド|GET|
|コントローラー|RegisterCategoryController|
|アクションメソッド|ValidateCategoryName()|

### 概要

登録する商品カテゴリ名が既に存在するかを確認します。

### クエリパラメータ

|項目|型|必須|
|---|---|---|
|categoryName|string|○|

### レスポンス例（200）

```json
{
  "exists": false,
  "message": "使用できるカテゴリ名です"
}
```

### 重複時（409）

```json
{
  "code": "CATEGORY_ALREADY_EXISTS",
  "exists": true,
  "message": "商品カテゴリ名は既に存在します。"
}
```

※`message` は `ExistsException` の内容によって変わります。

### 入力エラー（400）

```json
{
  "code": "VALIDATION_ERROR",
  "message": "カテゴリ名を入力してください。"
}
```

※`message` は `DomainException` の内容によって変わります。

### システムエラー（500）

```json
{
  "code": "SYSTEM_ERROR",
  "message": "システムエラーが発生しました。管理者に連絡してください"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|カテゴリ名は使用可能|
|400|入力値不正|
|409|カテゴリ名が既に存在する|
|500|システムエラー|

---

## 2. 商品カテゴリ登録

|項目|内容|
|---|---|
|エンドポイント|`POST /admin/category/register`|
|HTTPメソッド|POST|
|コントローラー|RegisterCategoryController|
|アクションメソッド|Register()|

### 概要

新しい商品カテゴリを登録します。

### リクエスト例

```json
{
  "categoryName": "文房具"
}
```

※実際の `RegisterCategoryViewModel` の項目に合わせて修正してください。

### レスポンス例（201）

```json
{
  "message": "商品カテゴリ「文房具」を登録しました",
  "category": {
    "categoryUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "文房具"
  }
}
```

### 入力エラー（400）

```json
{
  "code": "VALIDATION_ERROR",
  "message": "カテゴリ名の入力内容に誤りがあります。"
}
```

または

```json
{
  "code": "DOMAIN_RULE_VIOLATION",
  "message": "カテゴリ名は50文字以内で入力してください。"
}
```

※`message` は `DomainException` の内容によって変わります。

### カテゴリ重複（409）

```json
{
  "code": "CATEGORY_ALREADY_EXISTS",
  "message": "商品カテゴリ名は既に存在します。"
}
```

### 内部エラー（500）

```json
{
  "code": "INTERNAL_ERROR",
  "message": "システムエラーが発生しました。管理者に連絡してください"
}
```

### システムエラー（500）

```json
{
  "code": "SYSTEM_ERROR",
  "message": "システムエラーが発生しました。管理者に連絡してください"
}
```

### ステータスコード

|コード|内容|
|---|---|
|201|登録成功|
|400|入力値不正・業務ルール違反|
|409|商品カテゴリ名が既に存在する|
|500|システムエラー|





## UC015 購入履歴検索

### ベースURL

```
/admin/order/search
```

---

## 1. 購入履歴検索画面初期表示

|項目|内容|
|---|---|
|エンドポイント|`GET /admin/order/search`|
|HTTPメソッド|GET|
|コントローラー|SearchOrdersController|
|アクションメソッド|Get()|

### 概要

購入履歴検索画面の初期表示情報を取得します。  
初期表示では登録されている注文履歴をすべて取得します。

### レスポンス例（200）

```json
{
  "message": "",
  "orderList": [
    {
      "orderUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "orderDate": "2026-07-10",
      "customerAccountName": "yamada",
      "totalPrice": 2500
    }
  ]
}
```

※`SearchOrdersResultViewModel` の内容に合わせて修正してください。

注文履歴が存在しない場合

```json
{
  "message": "注文が登録されていません",
  "orderList": []
}
```

### システムエラー（500）

```json
{
  "code": "ORDER_DATA_FETCH_ERROR",
  "message": "注文情報の取得に失敗しました"
}
```

または

```json
{
  "code": "SYSTEM_ERROR",
  "message": "注文情報の取得に失敗しました"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|取得成功|
|500|システムエラー|

---

## 2. 購入履歴検索

|項目|内容|
|---|---|
|エンドポイント|`GET /admin/order/search/result?orderDate={orderDate}&customerAccountName={customerAccountName}`|
|HTTPメソッド|GET|
|コントローラー|SearchOrdersController|
|アクションメソッド|Search()|

### 概要

購入日または顧客アカウント名を条件として購入履歴を検索します。

### クエリパラメータ

|項目|型|必須|
|---|---|---|
|orderDate|string|－|
|customerAccountName|string|－|

※どちらか一方、または両方を指定して検索できます。

### レスポンス例（200）

```json
{
  "message": "",
  "orderList": [
    {
      "orderUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "orderDate": "2026-07-10",
      "customerAccountName": "yamada",
      "totalPrice": 2500
    }
  ]
}
```

※`SearchOrdersResultViewModel` の内容に合わせて修正してください。

### 入力エラー（400）

```json
{
  "code": "VALIDATION_ERROR",
  "message": "入力値が不正です。"
}
```

※`DomainException` の内容によってメッセージは変わります。

### システムエラー（500）

```json
{
  "code": "ORDER_DATA_FETCH_ERROR",
  "message": "注文情報の取得に失敗しました"
}
```

または

```json
{
  "code": "SYSTEM_ERROR",
  "message": "注文情報の取得に失敗しました"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|検索成功|
|400|入力値不正|
|500|システムエラー|





## UC016 注文ステータス更新

### ベースURL

```
/admin/order/status/update
```

---

## 1. 注文ステータス更新入力画面表示

|項目|内容|
|---|---|
|エンドポイント|`GET /admin/order/status/update/{orderId}`|
|HTTPメソッド|GET|
|コントローラー|UpdateOrderStatusController|
|アクションメソッド|GetInput()|

### 概要

指定した注文の情報と更新可能な注文ステータス一覧を取得し、注文ステータス更新入力画面を表示します。

### パスパラメータ

|項目|型|必須|
|---|---|---|
|orderId|Guid|○|

### レスポンス例（200）

```json
{
  "orderId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "customerName": "山田 太郎",
  "currentStatus": "受付",
  "orderStatuses": [
    {
      "statusId": 1,
      "statusName": "受付"
    },
    {
      "statusId": 2,
      "statusName": "発送済"
    }
  ]
}
```

※`UpdateOrderStatusInputViewModel` に合わせて修正してください。

### 注文が存在しない場合（404）

```json
{
  "code": "ORDER_NOT_FOUND",
  "message": "指定された注文は存在しません",
  "redirectUrl": "/admin/order/search"
}
```

### システムエラー（500）

```json
{
  "code": "ORDER_STATUS_INPUT_ERROR",
  "message": "注文情報の取得に失敗しました"
}
```

または

```json
{
  "code": "SYSTEM_ERROR",
  "message": "システムエラーが発生しました。管理者に連絡してください"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|取得成功|
|400|入力値不正|
|404|注文が存在しない|
|500|システムエラー|

---

## 2. 注文ステータス更新確認

|項目|内容|
|---|---|
|エンドポイント|`POST /admin/order/status/update/confirm`|
|HTTPメソッド|POST|
|コントローラー|UpdateOrderStatusController|
|アクションメソッド|Confirm()|

### 概要

入力された注文ステータスを確認し、確認画面に表示する情報を取得します。

### リクエスト例

```json
{
  "orderId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "newStatusId": 2
}
```

### レスポンス例（200）

```json
{
  "orderId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "customerName": "山田 太郎",
  "currentStatus": "受付",
  "newStatus": "発送済"
}
```

※`UpdateOrderStatusConfirmViewModel` に合わせて修正してください。

### 入力エラー（400）

```json
{
  "code": "VALIDATION_ERROR",
  "message": "入力内容が不正です"
}
```

### 注文が存在しない場合（404）

```json
{
  "code": "ORDER_NOT_FOUND",
  "message": "指定された注文は存在しません",
  "redirectUrl": "/admin/order/search"
}
```

### システムエラー（500）

```json
{
  "code": "ORDER_STATUS_CONFIRM_ERROR",
  "message": "システムエラーが発生しました。管理者に連絡してください"
}
```

または

```json
{
  "code": "SYSTEM_ERROR",
  "message": "システムエラーが発生しました。管理者に連絡してください"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|確認成功|
|400|入力値不正|
|404|注文が存在しない|
|500|システムエラー|

---

## 3. 注文ステータス更新

|項目|内容|
|---|---|
|エンドポイント|`POST /admin/order/status/update/complete`|
|HTTPメソッド|POST|
|コントローラー|UpdateOrderStatusController|
|アクションメソッド|Complete()|

### 概要

注文ステータスを更新し、更新完了画面の情報を取得します。

### リクエスト例

```json
{
  "orderId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "newStatusId": 2
}
```

### レスポンス例（200）

```json
{
  "message": "注文ステータスを更新しました",
  "orderId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "newStatus": "発送済"
}
```

※`UpdateOrderStatusCompleteViewModel` に合わせて修正してください。

### 入力エラー（400）

```json
{
  "code": "VALIDATION_ERROR",
  "message": "入力内容が不正です"
}
```

### 注文が存在しない場合（404）

```json
{
  "code": "ORDER_NOT_FOUND",
  "message": "指定された注文は存在しません",
  "redirectUrl": "/admin/order/search"
}
```

### システムエラー（500）

```json
{
  "code": "ORDER_STATUS_UPDATE_ERROR",
  "message": "システムエラーが発生しました。管理者に連絡してください"
}
```

または

```json
{
  "code": "SYSTEM_ERROR",
  "message": "システムエラーが発生しました。管理者に連絡してください"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|更新成功|
|400|入力値不正|
|404|注文が存在しない|
|500|システムエラー|





## UC017 担当者ログイン

### ベースURL

```
/api/admin/auth
```

---

## 1. 担当者ログイン

|項目|内容|
|---|---|
|エンドポイント|`POST /api/admin/auth/login`|
|HTTPメソッド|POST|
|コントローラー|AdminAuthController|
|アクションメソッド|LoginAsync()|

### 概要

担当者アカウントでログイン認証を行います。  
認証に成功した場合は担当者認証用Cookieを発行します。

### リクエスト例

```json
{
  "accountName": "admin01",
  "password": "Password123!"
}
```

※実際の `AdminLoginViewModel` の項目に合わせて修正してください。

### レスポンス例（200）

```json
{
  "success": true,
  "data": {
    "accountUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "accountName": "admin01",
    "employeeName": "山田 太郎"
  },
  "errors": []
}
```

※`AdminLoginResult` の内容に合わせて修正してください。

### 入力エラー（400）

```json
{
  "success": false,
  "data": null,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "ログイン情報を入力してください。",
      "field": "request"
    }
  ]
}
```

または

```json
{
  "success": false,
  "data": null,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "アカウント名は必須です。",
      "field": null
    }
  ]
}
```

※`DomainException` の内容によってメッセージは変わります。

### 認証失敗（401）

```json
{
  "success": false,
  "data": null,
  "errors": [
    {
      "code": "AUTHENTICATION_FAILED",
      "message": "アカウント名またはパスワードが正しくありません。",
      "field": null
    }
  ]
}
```

※`UnauthorizedAccessException` の内容によってメッセージは変わります。

### システムエラー（500）

```json
{
  "success": false,
  "data": null,
  "errors": [
    {
      "code": "INTERNAL_ERROR",
      "message": "担当者ログイン中にエラーが発生しました。",
      "field": null
    }
  ]
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|ログイン成功（認証Cookie発行）|
|400|入力値不正|
|401|認証失敗（アカウント名またはパスワード誤り）|
|500|システムエラー|












## UC019 商品キーワード検索

### ベースURL

```
/api/products/keyword
```

---

## 1. 商品キーワード検索

|項目|内容|
|---|---|
|エンドポイント|`GET /api/products/keyword?keyword={keyword}`|
|HTTPメソッド|GET|
|コントローラー|SearchProductByKeywordController|
|アクションメソッド|Search()|

### 概要

入力されたキーワードを含む商品を検索します。

### クエリパラメータ

|項目|型|必須|
|---|---|---|
|keyword|string|○|

### レスポンス例（200）

```json
[
  {
    "productUuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "赤ペン",
    "price": 120,
    "imageUrl": "https://example.com/redpen.png",
    "category": {
      "categoryUuid": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "name": "文房具"
    },
    "stock": {
      "stock": 10
    },
    "sales": 0
  },
  {
    "productUuid": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
    "name": "青ペン",
    "price": 120,
    "imageUrl": "https://example.com/bluepen.png",
    "category": {
      "categoryUuid": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "name": "文房具"
    },
    "stock": {
      "stock": 8
    },
    "sales": 2
  }
]
```

※実際の `Product` クラスのプロパティに合わせて修正してください。

### 入力エラー（400）

```json
{
  "code": "INVALID_KEYWORD",
  "message": "検索キーワードを入力してください。"
}
```

### ステータスコード

|コード|内容|
|---|---|
|200|検索成功|
|400|検索キーワード未入力|
|500|システムエラー|