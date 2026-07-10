# API一覧

## 1. 商品一覧取得

- エンドポイント
  - `GET /api/products`

- コントローラー
  - ProductController

- メソッド
  - GetAll()

- レスポンス

```json
[
  {
    "productUuid": "8d3f...",
    "name": "ボールペン",
    "price": 100
  },
  {
    "productUuid": "1c24...",
    "name": "ノート",
    "price": 250
  }
]
```