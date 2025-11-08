# 🧪 Swagger Test Examples

## 1️⃣ Authentication Flow

### 📝 Register User

```
POST /api/auth/register
```

**Parameters:**

```
username: testuser@example.com
password: Test@123456
role: User
```

### 📝 Register Admin (for testing)

```
POST /api/auth/register
```

**Parameters:**

```
username: admin@example.com
password: Admin@123456
role: Admin
```

### 🔐 Login

```
POST /api/auth/login
```

**Parameters:**

```
username: testuser@example.com
password: Test@123456
```

**Response:** Copy `id_token` và paste vào Swagger Authorize (không cần gõ "Bearer")

---

## 2️⃣ Product Management (cần tạo sản phẩm trước)

### ➕ Create Drink (Admin/Staff only)

```
POST /api/product
Authorization: Bearer {admin_token}
```

**Body:**

```json
{
  "productId": "drink-001",
  "name": "Cà phê sữa đá",
  "price": 35000,
  "description": "Cà phê phin truyền thống pha với sữa đặc",
  "isAvailable": true
}
```

### ➕ Create More Products

```json
{
  "productId": "drink-002",
  "name": "Trà sữa trân châu",
  "price": 45000,
  "description": "Trà sữa Đài Loan với trân châu đen",
  "isAvailable": true
}
```

```json
{
  "productId": "cake-001",
  "name": "Bánh tiramisu",
  "price": 55000,
  "description": "Bánh tiramisu Ý truyền thống",
  "isAvailable": true
}
```

---

## 3️⃣ Tạo Drinks/Cakes/Toppings trong DynamoDB

⚠️ **Lưu ý:** Bạn cần tạo trực qua DynamoDB hoặc tạo API endpoints cho Drink/Cake/Topping

### Drinks (thêm vào table "Drinks")

```json
{
  "Id": "drink-001",
  "Name": "Cà phê sữa đá",
  "BasePrice": 35000,
  "Stock": 100,
  "Category": "Coffee",
  "ImageUrl": "https://example.com/coffee.jpg"
}
```

```json
{
  "Id": "drink-002",
  "Name": "Trà sữa trân châu",
  "BasePrice": 45000,
  "Stock": 80,
  "Category": "Tea",
  "ImageUrl": "https://example.com/milktea.jpg"
}
```

```json
{
  "Id": "drink-003",
  "Name": "Sinh tố bơ",
  "BasePrice": 40000,
  "Stock": 50,
  "Category": "Smoothie"
}
```

### Cakes (thêm vào table "Cakes")

```json
{
  "Id": "cake-001",
  "Name": "Bánh tiramisu",
  "Price": 55000,
  "Stock": 30,
  "ImageUrl": "https://example.com/tiramisu.jpg"
}
```

```json
{
  "Id": "cake-002",
  "Name": "Bánh cheesecake",
  "Price": 50000,
  "Stock": 25
}
```

### Toppings (thêm vào table "Toppings")

```json
{
  "Id": "topping-001",
  "Name": "Trân châu đen",
  "Price": 5000,
  "Stock": 200
}
```

```json
{
  "Id": "topping-002",
  "Name": "Thạch dừa",
  "Price": 5000,
  "Stock": 150
}
```

```json
{
  "Id": "topping-003",
  "Name": "Kem cheese",
  "Price": 10000,
  "Stock": 100
}
```

---

## 4️⃣ Order Flow

### 🛒 Create Order (User only)

```
POST /api/order
Authorization: Bearer {user_token}
```

**Example 1: Order đơn giản (1 drink, không topping)**

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

**Kết quả:** TotalPrice = 35,000 × 2 = 70,000 VNĐ

**Example 2: Order với topping**

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

**Kết quả:** TotalPrice = (45,000 + 5,000 + 10,000) × 1 = 60,000 VNĐ

**Example 3: Order nhiều items**

```json
{
  "items": [
    {
      "productId": "drink-001",
      "productType": "Drink",
      "quantity": 2,
      "toppingIds": ["topping-001"]
    },
    {
      "productId": "cake-001",
      "productType": "Cake",
      "quantity": 1,
      "toppingIds": []
    },
    {
      "productId": "drink-003",
      "productType": "Drink",
      "quantity": 1,
      "toppingIds": ["topping-002", "topping-003"]
    }
  ]
}
```

**Kết quả:**

- Item 1: (35,000 + 5,000) × 2 = 80,000
- Item 2: 55,000 × 1 = 55,000
- Item 3: (40,000 + 5,000 + 10,000) × 1 = 55,000
- **Total: 190,000 VNĐ**

### 📋 Get All Orders

```
GET /api/order
Authorization: Bearer {admin_token}
```

### 🔍 Get Order by ID

```
GET /api/order/{orderId}
Authorization: Bearer {user_token}
```

**Example:**

```
GET /api/order/abc-123-def-456
```

---

## 5️⃣ Voucher & Loyalty

### 🎟️ Apply Voucher to Order

```
POST /api/order/{orderId}/apply-voucher
Authorization: Bearer {user_token}
```

**Body:**

```json
{
  "voucherCode": "abc12345"
}
```

⚠️ **Lưu ý:** User cần có voucher trong `AvailableVouchers` trước. Voucher tự động tạo khi đủ 100 điểm.

### 🎁 Get My Vouchers

```
GET /api/loyalty/my-vouchers
Authorization: Bearer {user_token}
```

### ⭐ Get My Points

```
GET /api/loyalty/my-points
Authorization: Bearer {user_token}
```

---

## 6️⃣ Update Order Status (Admin)

### ✅ Complete Order (tự động cộng điểm + trừ stock)

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

**Các status khác:**

```json
{"status": "Pending"}
{"status": "Processing"}
{"status": "Shipping"}
{"status": "Cancelled"}
```

---

## 7️⃣ Customer Management (Admin only)

### 👥 Get All Customers

```
GET /api/customer
Authorization: Bearer {admin_token}
```

### 👤 Get Customer by ID

```
GET /api/customer/{userId}
Authorization: Bearer {admin_token}
```

### 🔒 Lock/Unlock Customer

```
PUT /api/customer/{userId}/status
Authorization: Bearer {admin_token}
```

**Body:**

```json
{
  "isActive": false
}
```

---

## 8️⃣ Admin - Shipper Management

### 🚚 Create Shipper Account

```
POST /api/auth/admin/create-shipper
Authorization: Bearer {admin_token}
```

**Body:**

```json
{
  "username": "shipper01@example.com",
  "password": "Shipper@123"
}
```

### 📦 Get All Shippers

```
GET /api/admin/shippers
Authorization: Bearer {admin_token}
```

### 🔐 Lock/Unlock Shipper

```
PUT /api/admin/shipper/{userId}/lock
Authorization: Bearer {admin_token}
```

**Body:**

```json
{
  "unlock": false
}
```

---

## 🧪 Test Scenarios

### Scenario 1: User mua hàng và nhận điểm

1. Login as User → lấy token
2. Create Order với TotalPrice = 100,000 VNĐ
3. Admin update status → "Completed"
4. User nhận 10 điểm (100,000 / 10,000)
5. Check points: `GET /api/loyalty/my-points`

### Scenario 2: User đổi điểm lấy voucher

1. User cần có ≥ 100 điểm
2. Hệ thống tự động tạo voucher giảm 10%
3. Check vouchers: `GET /api/loyalty/my-vouchers`

### Scenario 3: User dùng voucher

1. Create Order → lấy orderId
2. Apply voucher: `POST /api/order/{orderId}/apply-voucher`
3. FinalPrice = TotalPrice × 0.9 (giảm 10%)
4. Complete order → cộng điểm dựa trên FinalPrice

### Scenario 4: Kiểm tra stock

1. Create Order với quantity = 200 (vượt stock)
2. Hệ thống trả về error: "Not enough stock"
3. Create Order với quantity hợp lệ
4. Complete order → stock tự động trừ

---

## 🔑 Quick Token Setup

1. Register Admin:

   - username: `admin@test.com`
   - password: `Admin@123456`
   - role: `Admin`

2. Register User:

   - username: `user@test.com`
   - password: `User@123456`
   - role: `User`

3. Login và copy `id_token`

4. Click "Authorize" ở Swagger UI, paste token (không cần "Bearer")

5. Bắt đầu test! 🚀
