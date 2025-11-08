# 🚀 Hướng dẫn Setup Hoàn chỉnh - Từ đầu đến cuối

## ❌ Lỗi hiện tại: "Unable to locate property for key attribute Id"

### 🔍 Nguyên nhân:

Lỗi này xảy ra vì bạn đang cố tạo order với `productId: "drink-001"` nhưng:

1. **Drink "drink-001" chưa tồn tại trong DynamoDB**
2. Khi validate order, hệ thống gọi `GetDrinkByIdAsync("drink-001")` → không tìm thấy
3. Hoặc có thể table "Drinks" chưa được tạo

---

## ✅ Giải pháp: Setup theo thứ tự

### Bước 1: Tạo Admin account

```
POST /api/auth/register
```

**Parameters:**

```
username: admin@test.com
password: Admin@123456
role: Admin
```

**Response:**

```json
{
  "message": "User registered successfully!",
  "user": {
    "userId": "abc-123-def",
    "username": "admin@test.com",
    "role": "Admin"
  }
}
```

---

### Bước 2: Login Admin

```
POST /api/auth/login
```

**Parameters:**

```
username: admin@test.com
password: Admin@123456
```

**Response:**

```json
{
  "access_token": "...",
  "id_token": "eyJraWQiOiJ...",  ← COPY CÁI NÀY
  "refresh_token": "..."
}
```

**⚠️ QUAN TRỌNG:** Copy `id_token` (không phải access_token)

---

### Bước 3: Authorize trong Swagger

1. Click nút **"Authorize"** 🔓 ở góc trên bên phải
2. Paste `id_token` vào ô (không cần gõ "Bearer")
3. Click **"Authorize"**
4. Click **"Close"**

---

### Bước 4: Tạo Drinks

```
POST /api/drink
Authorization: Bearer {admin_token}
```

**Body 1:**

```json
{
  "id": "drink-001",
  "name": "Cà phê sữa đá",
  "basePrice": 35000,
  "stock": 100,
  "category": "Coffee",
  "imageUrl": "https://example.com/coffee.jpg"
}
```

**Body 2:**

```json
{
  "id": "drink-002",
  "name": "Trà sữa trân châu",
  "basePrice": 45000,
  "stock": 80,
  "category": "Tea"
}
```

**Body 3:**

```json
{
  "id": "drink-003",
  "name": "Sinh tố bơ",
  "basePrice": 40000,
  "stock": 50,
  "category": "Smoothie"
}
```

**Expected Response:**

```json
{
  "message": "Drink created successfully",
  "drink": {
    "id": "drink-001",
    "name": "Cà phê sữa đá",
    "basePrice": 35000,
    "stock": 100,
    "category": "Coffee"
  }
}
```

---

### Bước 5: Tạo Cakes

```
POST /api/cake
Authorization: Bearer {admin_token}
```

**Body 1:**

```json
{
  "id": "cake-001",
  "name": "Bánh tiramisu",
  "price": 55000,
  "stock": 30,
  "imageUrl": "https://example.com/tiramisu.jpg"
}
```

**Body 2:**

```json
{
  "id": "cake-002",
  "name": "Bánh cheesecake",
  "price": 50000,
  "stock": 25
}
```

---

### Bước 6: Tạo Toppings

```
POST /api/topping
Authorization: Bearer {admin_token}
```

**Body 1:**

```json
{
  "id": "topping-001",
  "name": "Trân châu đen",
  "price": 5000,
  "stock": 200
}
```

**Body 2:**

```json
{
  "id": "topping-002",
  "name": "Thạch dừa",
  "price": 5000,
  "stock": 150
}
```

**Body 3:**

```json
{
  "id": "topping-003",
  "name": "Kem cheese",
  "price": 10000,
  "stock": 100
}
```

---

### Bước 7: Verify data đã tạo

**Xem tất cả drinks:**

```
GET /api/drink
```

**Expected Response:**

```json
[
  {
    "id": "drink-001",
    "name": "Cà phê sữa đá",
    "basePrice": 35000,
    "stock": 100,
    "category": "Coffee"
  },
  {
    "id": "drink-002",
    "name": "Trà sữa trân châu",
    "basePrice": 45000,
    "stock": 80,
    "category": "Tea"
  }
]
```

---

### Bước 8: Tạo User account

```
POST /api/auth/register
```

**Parameters:**

```
username: user@test.com
password: User@123456
role: User
```

---

### Bước 9: Login User

```
POST /api/auth/login
```

**Parameters:**

```
username: user@test.com
password: User@123456
```

**Copy `id_token` và Authorize lại**

---

### Bước 10: Tạo Order (cuối cùng!)

```
POST /api/order
Authorization: Bearer {user_token}
```

**Body - Order đơn giản:**

```json
{
  "items": [
    {
      "productId": "drink-001",
      "productType": "Drink",
      "quantity": 2,
      "toppingIds": []
    }
  ]
}
```

**Expected Response:**

```json
{
  "message": "Order created successfully",
  "order": {
    "orderId": "xyz-789-abc",
    "userId": "user-id",
    "items": [
      {
        "productId": "drink-001",
        "productName": "Cà phê sữa đá",
        "productType": "Drink",
        "quantity": 2,
        "unitPrice": 35000,
        "toppings": [],
        "totalPrice": 70000
      }
    ],
    "totalPrice": 70000,
    "finalPrice": 70000,
    "status": "Pending",
    "createdAt": "2025-01-08T10:30:00Z"
  }
}
```

---

### Bước 11: Order với topping

```json
{
  "items": [
    {
      "productId": "drink-002",
      "productType": "Drink",
      "quantity": 1,
      "toppingIds": ["topping-001", "topping-003"]
    }
  ]
}
```

**Expected:**

- Drink: 45,000
- Topping 1: 5,000
- Topping 2: 10,000
- **Total: 60,000 VNĐ**

---

### Bước 12: Admin complete order

**Login lại với Admin token**

```
PUT /api/order/{orderId}/status
Authorization: Bearer {admin_token}
```

**Body:**

```json
{
  "status": "Completed"
}
```

**Kết quả:**

- ✅ Order status → "Completed"
- ✅ User nhận điểm: 70,000 / 10,000 = 7 điểm
- ✅ Stock tự động giảm: drink-001 stock: 100 → 98

---

## 🔍 Troubleshooting

### Lỗi: "Unable to locate property for key attribute Id"

**Nguyên nhân:**

- Drink/Cake/Topping chưa tồn tại trong DynamoDB
- Bạn đang dùng productId không tồn tại

**Giải pháp:**

1. Tạo drinks/cakes/toppings trước (Bước 4-6)
2. Verify bằng GET /api/drink
3. Dùng đúng ID khi tạo order

---

### Lỗi: "Not enough stock"

**Nguyên nhân:**

- Stock không đủ cho quantity

**Giải pháp:**

```
PATCH /api/drink/{id}/stock
{
  "stock": 100
}
```

---

### Lỗi: "Cannot identify user from token"

**Nguyên nhân:**

- Dùng access_token thay vì id_token
- Token hết hạn

**Giải pháp:**

1. Login lại
2. Copy **id_token** (không phải access_token)
3. Authorize lại

---

### Lỗi: 403 Forbidden

**Nguyên nhân:**

- Không có quyền (role không đúng)
- Chưa authorize

**Giải pháp:**

1. Đảm bảo đã Authorize với token đúng
2. Admin endpoints cần Admin token
3. User endpoints cần User token

---

## ✅ Checklist đầy đủ

- [ ] Register Admin
- [ ] Login Admin → copy id_token
- [ ] Authorize với Admin token
- [ ] Tạo ít nhất 3 drinks
- [ ] Tạo ít nhất 2 cakes
- [ ] Tạo ít nhất 3 toppings
- [ ] Verify data: GET /api/drink
- [ ] Register User
- [ ] Login User → copy id_token
- [ ] Authorize với User token
- [ ] Tạo order đơn giản (không topping)
- [ ] Tạo order có topping
- [ ] Login Admin lại
- [ ] Complete order
- [ ] Verify stock đã giảm
- [ ] Verify user nhận điểm

---

## 🎯 Quick Test Script

```bash
# 1. Register Admin
POST /api/auth/register
username=admin@test.com, password=Admin@123456, role=Admin

# 2. Login Admin
POST /api/auth/login
username=admin@test.com, password=Admin@123456
→ Copy id_token

# 3. Authorize
Click "Authorize" → Paste id_token

# 4. Create Drink
POST /api/drink
{"id":"drink-001","name":"Cà phê","basePrice":35000,"stock":100,"category":"Coffee"}

# 5. Register User
POST /api/auth/register
username=user@test.com, password=User@123456, role=User

# 6. Login User
POST /api/auth/login
username=user@test.com, password=User@123456
→ Copy id_token

# 7. Authorize lại
Click "Authorize" → Paste user id_token

# 8. Create Order
POST /api/order
{"items":[{"productId":"drink-001","productType":"Drink","quantity":1,"toppingIds":[]}]}

# 9. Success! ✅
```

---

## 📝 Notes

- **id_token** có thời hạn 1 giờ, sau đó cần login lại
- **Stock** tự động giảm khi order Completed
- **Points** tự động cộng: 1 điểm / 10,000 VNĐ
- **Voucher** tự động tạo khi đủ 100 điểm (giảm 10%)

Xong! 🎉
