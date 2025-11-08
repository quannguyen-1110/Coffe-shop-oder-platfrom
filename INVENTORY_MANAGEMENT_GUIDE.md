# 📦 Hướng dẫn Quản lý Kho (Inventory Management)

## 🎯 Tính năng đã implement:

### ✅ Stock tự động giảm khi order Completed
- Khi Admin cập nhật order status → "Completed"
- Hệ thống tự động trừ stock của Drink/Cake/Topping
- Nếu stock = 0 → không thể order được nữa

### ✅ Validation khi tạo order
- Kiểm tra stock trước khi cho phép order
- Nếu không đủ stock → trả về error

---

## 📋 API Endpoints cho Admin

### 1. DRINK MANAGEMENT

#### Xem tất cả drinks
```
GET /api/drink
Authorization: không cần (public)
```

#### Tạo drink mới
```
POST /api/drink
Authorization: Bearer {admin_token}
```
**Body:**
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

#### Cập nhật drink
```
PUT /api/drink/{id}
Authorization: Bearer {admin_token}
```
**Body:**
```json
{
  "name": "Cà phê sữa đá (size L)",
  "basePrice": 40000,
  "stock": 150,
  "category": "Coffee"
}
```

#### Cập nhật stock (nhanh)
```
PATCH /api/drink/{id}/stock
Authorization: Bearer {admin_token}
```
**Body:**
```json
{
  "stock": 200
}
```

#### Xem drinks sắp hết hàng
```
GET /api/drink/low-stock?threshold=10
Authorization: Bearer {admin_token}
```

#### Xóa drink
```
DELETE /api/drink/{id}
Authorization: Bearer {admin_token}
```

---

### 2. CAKE MANAGEMENT

#### Xem tất cả cakes
```
GET /api/cake
Authorization: không cần (public)
```

#### Tạo cake mới
```
POST /api/cake
Authorization: Bearer {admin_token}
```
**Body:**
```json
{
  "id": "cake-001",
  "name": "Bánh tiramisu",
  "price": 55000,
  "stock": 30,
  "imageUrl": "https://example.com/tiramisu.jpg"
}
```

#### Cập nhật stock
```
PATCH /api/cake/{id}/stock
Authorization: Bearer {admin_token}
```
**Body:**
```json
{
  "stock": 50
}
```

#### Xem cakes sắp hết hàng
```
GET /api/cake/low-stock?threshold=10
Authorization: Bearer {admin_token}
```

---

### 3. TOPPING MANAGEMENT

#### Xem tất cả toppings
```
GET /api/topping
Authorization: không cần (public)
```

#### Tạo topping mới
```
POST /api/topping
Authorization: Bearer {admin_token}
```
**Body:**
```json
{
  "id": "topping-001",
  "name": "Trân châu đen",
  "price": 5000,
  "stock": 200,
  "imageUrl": "https://example.com/pearl.jpg"
}
```

#### Cập nhật stock
```
PATCH /api/topping/{id}/stock
Authorization: Bearer {admin_token}
```
**Body:**
```json
{
  "stock": 300
}
```

#### Xem toppings sắp hết hàng
```
GET /api/topping/low-stock?threshold=20
Authorization: Bearer {admin_token}
```

---

### 4. INVENTORY OVERVIEW (Dashboard)

#### Xem tổng quan kho
```
GET /api/inventory/overview
Authorization: Bearer {admin_token}
```

**Response:**
```json
{
  "drinks": {
    "total": 10,
    "inStock": 8,
    "outOfStock": 2,
    "lowStock": 3,
    "totalValue": 3500000
  },
  "cakes": {
    "total": 5,
    "inStock": 4,
    "outOfStock": 1,
    "lowStock": 2,
    "totalValue": 1500000
  },
  "toppings": {
    "total": 8,
    "inStock": 7,
    "outOfStock": 1,
    "lowStock": 2,
    "totalValue": 500000
  }
}
```

#### Xem cảnh báo stock
```
GET /api/inventory/alerts
Authorization: Bearer {admin_token}
```

**Response:**
```json
{
  "totalAlerts": 5,
  "critical": 2,
  "warnings": 3,
  "alerts": [
    {
      "type": "Drink",
      "id": "drink-001",
      "name": "Cà phê sữa đá",
      "stock": 0,
      "severity": "critical",
      "message": "Cà phê sữa đá is out of stock"
    },
    {
      "type": "Topping",
      "id": "topping-002",
      "name": "Trân châu trắng",
      "stock": 5,
      "severity": "warning",
      "message": "Trân châu trắng is running low (only 5 left)"
    }
  ]
}
```

---

## 🧪 Test Scenarios

### Scenario 1: Tạo sản phẩm và kiểm tra stock

1. **Admin tạo drink:**
```json
POST /api/drink
{
  "id": "drink-test",
  "name": "Test Coffee",
  "basePrice": 30000,
  "stock": 5,
  "category": "Coffee"
}
```

2. **User tạo order (mua 3 ly):**
```json
POST /api/order
{
  "items": [
    {
      "productId": "drink-test",
      "productType": "Drink",
      "quantity": 3,
      "toppingIds": []
    }
  ]
}
```

3. **Admin complete order:**
```json
PUT /api/order/{orderId}/status
{
  "status": "Completed"
}
```

4. **Kiểm tra stock:**
```
GET /api/drink/drink-test
```
**Expected:** Stock = 2 (5 - 3)

---

### Scenario 2: Order vượt stock

1. **User order 10 ly (nhưng chỉ còn 2):**
```json
POST /api/order
{
  "items": [
    {
      "productId": "drink-test",
      "productType": "Drink",
      "quantity": 10,
      "toppingIds": []
    }
  ]
}
```

**Expected Error:**
```json
{
  "error": "Not enough stock for Test Coffee. Available: 2"
}
```

---

### Scenario 3: Order khi stock = 0

1. **Admin set stock = 0:**
```json
PATCH /api/drink/drink-test/stock
{
  "stock": 0
}
```

2. **User cố order:**
```json
POST /api/order
{
  "items": [
    {
      "productId": "drink-test",
      "productType": "Drink",
      "quantity": 1,
      "toppingIds": []
    }
  ]
}
```

**Expected Error:**
```json
{
  "error": "Not enough stock for Test Coffee. Available: 0"
}
```

---

### Scenario 4: Topping cũng bị trừ stock

1. **Tạo topping với stock = 10:**
```json
POST /api/topping
{
  "id": "topping-test",
  "name": "Test Topping",
  "price": 5000,
  "stock": 10
}
```

2. **Order drink với topping (quantity = 5):**
```json
POST /api/order
{
  "items": [
    {
      "productId": "drink-001",
      "productType": "Drink",
      "quantity": 5,
      "toppingIds": ["topping-test"]
    }
  ]
}
```

3. **Complete order → topping stock giảm 5:**
```
GET /api/topping/topping-test
```
**Expected:** Stock = 5 (10 - 5)

⚠️ **Lưu ý:** Topping stock trừ theo quantity của drink, không phải số lượng topping!

---

## 📊 Admin Dashboard Workflow

### Quy trình quản lý hàng ngày:

1. **Sáng:** Check inventory overview
```
GET /api/inventory/overview
```

2. **Xem cảnh báo:**
```
GET /api/inventory/alerts
```

3. **Nhập hàng cho items sắp hết:**
```
PATCH /api/drink/{id}/stock
{"stock": 100}

PATCH /api/cake/{id}/stock
{"stock": 50}

PATCH /api/topping/{id}/stock
{"stock": 200}
```

4. **Xem items low stock:**
```
GET /api/drink/low-stock?threshold=10
GET /api/cake/low-stock?threshold=10
GET /api/topping/low-stock?threshold=20
```

---

## 🎯 Best Practices

### Threshold khuyến nghị:
- **Drinks:** threshold = 10 (cảnh báo khi < 10)
- **Cakes:** threshold = 10 (bánh dễ hỏng, không nên dự trữ nhiều)
- **Toppings:** threshold = 20 (topping dùng nhiều, cần stock cao hơn)

### Quy trình nhập hàng:
1. Check alerts mỗi ngày
2. Nhập hàng khi stock < threshold
3. Không nhập quá nhiều bánh (dễ hỏng)
4. Topping nên dự trữ nhiều (dùng cho nhiều món)

### Xử lý out of stock:
1. Cập nhật stock = 0 nếu hết hàng
2. User sẽ không order được
3. Nhập hàng mới → cập nhật stock
4. User có thể order lại

---

## ✅ Checklist Setup

- [ ] Tạo Admin account
- [ ] Tạo ít nhất 5 drinks với stock khác nhau
- [ ] Tạo ít nhất 3 cakes với stock khác nhau
- [ ] Tạo ít nhất 5 toppings với stock khác nhau
- [ ] Test create order → complete → check stock giảm
- [ ] Test order vượt stock → nhận error
- [ ] Test inventory overview
- [ ] Test inventory alerts

Xong! 🎉
