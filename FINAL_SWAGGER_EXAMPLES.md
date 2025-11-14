# ✅ API Testing Guide - Coffee Shop API

## 🔑 Authentication

### Admin Token

```
POST /api/Auth/login
{
  "username": "admin@example.com",
  "password": "Admin@123"
}
```

**Lưu `idToken` để dùng cho Admin endpoints**

### User Token

```
POST /api/Auth/login
{
  "username": "user@example.com",
  "password": "User@123"
}
```

**Lưu `idToken` để dùng cho User endpoints**

### Shipper Token

```
POST /api/shipper/auth/login
{
  "username": "shipper@example.com",
  "password": "Shipper@123"
}
```

**Lưu `token` để dùng cho Shipper endpoints**

---

## 📋 Complete Testing Flow

### PHASE 1: Setup Products (Admin)

#### 1.1. Create Drinks

```
POST /api/Drink
Authorization: Bearer {admin_token}

{
  "name": "Cà phê sữa đá",
  "basePrice": 25000,
  "stock": 100,
  "category": "Coffee",
  "imageUrl": "https://example.com/ca-phe-sua.jpg"
}
```

**Tạo thêm:**

- Trà sữa trân châu (30000đ)
- Cappuccino (35000đ)

#### 1.2. Create Cakes

```
POST /api/Cake
Authorization: Bearer {admin_token}

{
  "name": "Bánh tiramisu",
  "price": 45000,
  "stock": 50,
  "imageUrl": "https://example.com/tiramisu.jpg"
}
```

**Tạo thêm:**

- Bánh cheesecake (50000đ)
- Bánh mousse chocolate (55000đ)

#### 1.3. Create Toppings

```
POST /api/Topping
Authorization: Bearer {admin_token}

{
  "name": "Trân châu đen",
  "price": 5000,
  "stock": 200,
  "imageUrl": "https://example.com/tran-chau.jpg"
}
```

**Tạo thêm:**

- Thạch dừa (5000đ)
- Pudding (7000đ)

**✅ Lưu tất cả IDs để dùng trong order!**

---

### PHASE 2: User Registration & Login

#### 2.1. Register User

```
POST /api/Auth/register

{
  "username": "testuser@example.com",
  "password": "Test@123456",
  "role": "User"
}
```

#### 2.2. Confirm Email

```
POST /api/Auth/confirm

{
  "username": "testuser@example.com",
  "confirmationCode": "123456"
}
```

#### 2.3. Login

```
POST /api/Auth/login

{
  "username": "testuser@example.com",
  "password": "Test@123456"
}
```

**✅ Lưu `idToken`**

---

### PHASE 3: Create Order (User)

```
POST /api/Order
Authorization: Bearer {user_token}

{
  "items": [
    {
      "productId": "DRINK_ID_1",
      "productType": "Drink",
      "quantity": 2,
      "toppingIds": ["TOPPING_ID_1", "TOPPING_ID_2"]
    },
    {
      "productId": "CAKE_ID_1",
      "productType": "Cake",
      "quantity": 1,
      "toppingIds": []
    }
  ],
  "deliveryAddress": "123 Nguyen Hue Street, District 1, HCMC",
  "deliveryPhone": "0901234567",
  "deliveryNote": "Gọi trước 15 phút"
}
```

**Response:**

```json
{
  "message": "Order created successfully",
  "order": {
    "orderId": "114e44d3-be1a-44c7-b6c4-c88b267ec5e4",
    "status": "Pending",
    "totalPrice": 150000,
    "finalPrice": 150000
  },
  "payment": {
    "success": true,
    "payUrl": "https://test-payment.momo.vn/...",
    "qrCodeUrl": "https://test-payment.momo.vn/qrcode/...",
    "deepLink": "momo://...",
    "message": "Tạo payment URL thành công"
  }
}
```

**✅ Lưu `orderId` và `paymentUrl`**

---

### PHASE 4: Payment Testing

#### Option 1: Test Callback Thủ Công (Không cần MoMo app)

```
POST /api/MoMoPayment/test-callback
Authorization: Bearer {user_token}

{
  "orderId": "114e44d3-be1a-44c7-b6c4-c88b267ec5e4",
  "resultCode": 0
}
```

**Response:**

```json
{
  "success": true,
  "message": "✅ Test callback thành công! Order đã chuyển sang Processing",
  "orderId": "114e44d3-be1a-44c7-b6c4-c88b267ec5e4",
  "oldStatus": "Pending",
  "newStatus": "Processing"
}
```

#### Option 2: Thanh Toán Thật (Cần MoMo app)

1. Copy `paymentUrl` từ response
2. Mở trong browser
3. Quét QR bằng app MoMo
4. Thanh toán
5. Tự động redirect về FE

---

### PHASE 5: Admin Confirm Order

```
POST /api/Admin/orders/{orderId}/confirm
Authorization: Bearer {admin_token}
```

**Response:**

```json
{
  "message": "Order confirmed successfully",
  "order": {
    "orderId": "114e44d3-be1a-44c7-b6c4-c88b267ec5e4",
    "status": "Confirmed",
    "confirmedAt": "2025-01-14T12:00:00Z",
    "confirmedBy": "admin-id"
  }
}
```

**✅ Notification 1 created!**

---

### PHASE 6: Shipper Registration

#### 6.1. Register Shipper

```
POST /api/ShipperRegistration/register

{
  "fullName": "Nguyễn Văn Shipper",
  "email": "shipper@example.com"
}
```

**✅ Lưu `userId`**

#### 6.2. Admin Approve Shipper

```
POST /api/Admin/shipper/{userId}/approve
Authorization: Bearer {admin_token}
```

**✅ Check email → Nhận password tạm thời**

#### 6.3. Shipper Login

```
POST /api/shipper/auth/login

{
  "username": "shipper@example.com",
  "password": "Abc123!@"
}
```

**✅ Lưu `token`**

---

### PHASE 7: Shipper Operations

#### 7.1. Get Available Orders

```
GET /api/Shipper/orders/available
Authorization: Bearer {shipper_token}
```

#### 7.2. Calculate Shipping Fee

```
POST /api/Shipper/orders/{orderId}/calculate-fee
Authorization: Bearer {shipper_token}
```

**Response:**

```json
{
  "orderId": "114e44d3-be1a-44c7-b6c4-c88b267ec5e4",
  "deliveryAddress": "123 Nguyen Hue Street...",
  "distanceKm": 5.2,
  "shippingFee": 25000,
  "estimatedTime": 30
}
```

#### 7.3. Accept Order

```
POST /api/Shipper/orders/{orderId}/accept
Authorization: Bearer {shipper_token}
```

**✅ Notification 2 created!**
**✅ ShipperDeliveryHistory record created!**

#### 7.4. Complete Delivery

```
POST /api/Shipper/orders/{orderId}/complete
Authorization: Bearer {shipper_token}
```

**✅ Notification 3 created!**
**✅ ShipperProfile updated (totalEarnings, totalDeliveries)!**
**✅ ShipperDeliveryHistory updated!**

#### 7.5. Get Statistics

```
GET /api/Shipper/statistics
Authorization: Bearer {shipper_token}
```

**Response:**

```json
{
  "totalOrders": 10,
  "completedOrders": 8,
  "shippingOrders": 2,
  "totalEarnings": 250000,
  "todayOrders": 3
}
```

#### 7.6. Get Delivery History

```
GET /api/Shipper/history
Authorization: Bearer {shipper_token}
```

#### 7.7. Get/Update Profile

```
GET /api/Shipper/profile
PUT /api/Shipper/profile
Authorization: Bearer {shipper_token}

Body (PUT):
{
  "fullName": "Nguyễn Văn A",
  "phone": "0901234567",
  "vehicleType": "Motorbike",
  "vehiclePlate": "59A-12345",
  "bankAccount": "1234567890",
  "bankName": "Vietcombank"
}
```

---

### PHASE 8: Notifications (User)

#### 8.1. Get Unread Count

```
GET /api/Notification/unread/count
Authorization: Bearer {user_token}
```

#### 8.2. Get All Notifications

```
GET /api/Notification
Authorization: Bearer {user_token}
```

#### 8.3. Mark as Read

```
PUT /api/Notification/{notificationId}/read
Authorization: Bearer {user_token}
```

#### 8.4. Mark All as Read

```
PUT /api/Notification/read-all
Authorization: Bearer {user_token}
```

---

### PHASE 9: Complete Order (Admin)

```
PUT /api/Order/{orderId}/status
Authorization: Bearer {admin_token}

{
  "status": "Completed"
}
```

**Kết quả:**

- ✅ User nhận loyalty points
- ✅ Stock tự động giảm
- ✅ Order hoàn tất

---

## 🎯 Quick Reference

### Product Schemas

```
Drink:   name, basePrice, stock, category, imageUrl
Cake:    name, price, stock, imageUrl
Topping: name, price, stock, imageUrl
```

### Order Status Flow

```
Pending → Processing → Confirmed → Shipping → Delivered → Completed
```

### Authentication

```
User/Admin: Cognito idToken
Shipper:    Local JWT token
```

### Payment

```
Development: Test callback endpoint
Production:  Real MoMo payment
```

---

## 🐛 Common Errors

- **401 Unauthorized**: Token hết hạn hoặc sai
- **403 Forbidden**: Sai role (dùng Admin token cho User endpoint)
- **404 Not Found**: Sai ID hoặc resource không tồn tại
- **400 Bad Request**: Thiếu field hoặc validation error

---

## 🚀 Production Checklist

- [ ] Đổi Frontend URL trong appsettings.json
- [ ] Đổi MoMo credentials thành production
- [ ] Setup AWS services (Cognito, DynamoDB, SNS, SES)
- [ ] Deploy Backend lên server
- [ ] Test payment flow với domain thật
- [ ] Monitor logs và errors

---

## ✅ Testing Complete!

Bạn đã test thành công toàn bộ flow! 🎉
