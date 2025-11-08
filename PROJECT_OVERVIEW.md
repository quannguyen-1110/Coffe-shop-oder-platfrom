# ☕ Coffee Shop API - Tổng quan Project

## 🎯 Mục đích

API quản lý cửa hàng cà phê với các chức năng:

- Quản lý sản phẩm (Drinks, Cakes, Toppings)
- Quản lý đơn hàng (Orders)
- Hệ thống loyalty (điểm thưởng, voucher)
- Xác thực người dùng (AWS Cognito)
- Quản lý kho (Stock management)

---

## 📁 Cấu trúc Project

### 🎮 **Controllers/** - API Endpoints

#### **AuthController.cs**

- `POST /api/auth/register` - Đăng ký tài khoản (User/Admin)
- `POST /api/auth/login` - Đăng nhập (trả về JWT token)
- `POST /api/auth/logout` - Đăng xuất
- `POST /api/auth/confirm` - Xác nhận email
- `POST /api/auth/resend` - Gửi lại mã xác nhận
- `POST /api/auth/admin/create-shipper` - Admin tạo tài khoản Shipper
- `GET /api/auth/whoami` - Xem thông tin user hiện tại

**Chức năng:** Quản lý authentication qua AWS Cognito

---

#### **DrinkController.cs**

- `GET /api/drink` - Xem tất cả drinks (public)
- `GET /api/drink/{id}` - Xem chi tiết 1 drink
- `POST /api/drink` - Tạo drink mới (Admin only)
- `PUT /api/drink/{id}` - Cập nhật drink (Admin only)
- `PATCH /api/drink/{id}/stock` - Cập nhật stock nhanh (Admin only)
- `DELETE /api/drink/{id}` - Xóa drink (Admin only)
- `GET /api/drink/low-stock` - Xem drinks sắp hết hàng (Admin only)

**Chức năng:** Quản lý đồ uống

---

#### **CakeController.cs**

- `GET /api/cake` - Xem tất cả cakes (public)
- `GET /api/cake/{id}` - Xem chi tiết 1 cake
- `POST /api/cake` - Tạo cake mới (Admin only)
- `PUT /api/cake/{id}` - Cập nhật cake (Admin only)
- `PATCH /api/cake/{id}/stock` - Cập nhật stock (Admin only)
- `DELETE /api/cake/{id}` - Xóa cake (Admin only)
- `GET /api/cake/low-stock` - Xem cakes sắp hết hàng (Admin only)

**Chức năng:** Quản lý bánh ngọt

---

#### **ToppingController.cs**

- `GET /api/topping` - Xem tất cả toppings (public)
- `GET /api/topping/{id}` - Xem chi tiết 1 topping
- `POST /api/topping` - Tạo topping mới (Admin only)
- `PUT /api/topping/{id}` - Cập nhật topping (Admin only)
- `PATCH /api/topping/{id}/stock` - Cập nhật stock (Admin only)
- `DELETE /api/topping/{id}` - Xóa topping (Admin only)
- `GET /api/topping/low-stock` - Xem toppings sắp hết hàng (Admin only)

**Chức năng:** Quản lý topping (trân châu, thạch, kem cheese...)

---

#### **OrderController.cs**

- `GET /api/order` - Xem tất cả orders (Admin only)
- `GET /api/order/{id}` - Xem chi tiết 1 order (Admin/User)
- `POST /api/order` - Tạo order mới (User)
- `PUT /api/order/{id}/status` - Cập nhật trạng thái order (Admin only)
- `POST /api/order/{id}/apply-voucher` - Áp dụng voucher (User)

**Chức năng:** Quản lý đơn hàng

**Flow:**

1. User tạo order → Status: "Pending"
2. User có thể apply voucher (giảm giá)
3. Admin update status → "Completed"
4. Khi Completed:
   - User nhận điểm thưởng (1 điểm / 10,000đ)
   - Stock tự động giảm

---

#### **InventoryController.cs**

- `GET /api/inventory/overview` - Dashboard tổng quan kho (Admin only)
- `GET /api/inventory/alerts` - Cảnh báo stock thấp/hết (Admin only)

**Chức năng:** Dashboard quản lý kho

**Overview response:**

```json
{
  "drinks": {
    "total": 10,
    "inStock": 8,
    "outOfStock": 2,
    "lowStock": 3,
    "totalValue": 3500000
  },
  "cakes": {...},
  "toppings": {...}
}
```

---

#### **LoyaltyController.cs**

- `GET /api/loyalty/my-vouchers` - Xem vouchers của mình (User)
- `GET /api/loyalty/my-points` - Xem điểm thưởng (User)

**Chức năng:** Quản lý loyalty program

**Cách hoạt động:**

- Mua hàng → nhận điểm (1 điểm / 10,000đ)
- Đủ 100 điểm → tự động nhận voucher giảm 10%
- Voucher có hạn sử dụng 1 tháng

---

#### **CustomerController.cs**

- `GET /api/customer` - Xem tất cả customers (Admin only)
- `GET /api/customer/{id}` - Xem chi tiết customer (Admin only)
- `PUT /api/customer/{userId}/status` - Khóa/mở khóa customer (Admin only)
- `DELETE /api/customer/{id}` - Xóa customer (soft delete) (Admin only)

**Chức năng:** Admin quản lý khách hàng

---

#### **AdminController.cs**

- `GET /api/admin/shippers` - Xem tất cả shippers (Admin only)
- `PUT /api/admin/shipper/{userId}/lock` - Khóa/mở khóa shipper (Admin only)
- `POST /api/admin/shipper/{userId}/reset-password` - Reset password shipper (Admin only)

**Chức năng:** Admin quản lý shipper

---

#### **ProductController.cs**

- `GET /api/product` - Xem tất cả products (public)
- `GET /api/product/{id}` - Xem chi tiết product
- `POST /api/product` - Tạo product (Admin/Staff)
- `PUT /api/product/{id}` - Cập nhật product (Admin/Staff)
- `DELETE /api/product/{id}` - Xóa product (Admin/Staff)

**Chức năng:** Quản lý sản phẩm chung (legacy, có thể không dùng)

---

#### **OrderItemController.cs**

- `POST /api/orderitem/validate` - Validate item trước khi thêm vào order (User)

**Chức năng:** Helper endpoint để validate order items

---

### 🗄️ **Models/** - Data Models

#### **User.cs**

```csharp
- UserId (HashKey)
- Username
- Role (User/Admin/Shipper)
- RewardPoints (điểm thưởng)
- VoucherCount
- AvailableVouchers (list)
- IsActive
- CreatedAt
```

**Table:** CoffeeShopUsers

---

#### **Drink.cs**

```csharp
- Id (HashKey)
- Name
- BasePrice
- Stock
- Category (Coffee/Tea/Smoothie)
- ImageUrl
```

**Table:** Drinks

---

#### **Cake.cs**

```csharp
- Id (HashKey)
- Name
- Price
- Stock
- ImageUrl
```

**Table:** Cakes

---

#### **Topping.cs**

```csharp
- Id (HashKey)
- Name
- Price
- Stock
- ImageUrl
```

**Table:** Toppings

---

#### **Order.cs**

```csharp
- OrderId (HashKey)
- UserId
- Items (List<OrderItem>)
- TotalPrice
- FinalPrice (sau khi giảm giá)
- AppliedVoucherCode
- Status (Pending/Processing/Completed/Cancelled)
- CreatedAt
- CompletedAt
```

**Table:** Orders

---

#### **OrderItem.cs** (Nested object)

```csharp
- ProductId
- ProductName
- ProductType (Drink/Cake)
- Quantity
- UnitPrice
- Toppings (List<OrderTopping>)
- TotalPrice
```

---

#### **OrderTopping.cs** (Nested object)

```csharp
- ToppingId
- Name
- Price
```

---

#### **Voucher.cs** (Nested object trong User)

```csharp
- Code
- DiscountValue (0.1 = 10%)
- RequiredPoints (100)
- ExpirationDate
- IsUsed
- IsActive
```

---

### 🔧 **Services/** - Business Logic

#### **AuthService.cs**

- `RegisterAsync()` - Đăng ký user vào Cognito
- `LoginAsync()` - Đăng nhập, lấy JWT token
- `GlobalSignOutAsync()` - Đăng xuất
- `ConfirmSignUpAsync()` - Xác nhận email
- `CreateShipperAsync()` - Admin tạo shipper
- `AdminDisableUserAsync()` - Khóa user
- `AdminEnableUserAsync()` - Mở khóa user
- `AdminResetUserPasswordAsync()` - Reset password

**Chức năng:** Tích hợp AWS Cognito

---

#### **OrderService.cs**

- `CreateOrderAsync()` - Tạo order mới
  - Validate items
  - Tính TotalPrice tự động
  - Check stock
- `ApplyVoucherAsync()` - Áp dụng voucher giảm giá
- `UpdateStatusAsync()` - Cập nhật status
  - Khi Completed → cộng điểm + trừ stock
- `GetOrderAsync()` - Lấy order theo ID
- `GetUserOrdersAsync()` - Lấy orders của user

**Chức năng:** Xử lý logic đơn hàng

---

#### **OrderItemService.cs**

- `ValidateAndCalculateItemAsync()` - Validate item
  - Check product tồn tại
  - Check stock đủ
  - Tính giá tự động (product + toppings)
- `UpdateStockAfterOrderAsync()` - Trừ stock sau khi order completed

**Chức năng:** Xử lý logic order items

---

#### **LoyaltyService.cs**

- `AddPointsAsync()` - Cộng điểm thưởng
  - 1 điểm / 10,000đ
  - Đủ 100 điểm → tạo voucher tự động
- `ApplyVoucherAsync()` - Áp dụng voucher
  - Check voucher hợp lệ
  - Check chưa hết hạn
  - Tính giá sau giảm
- `GetVouchersAsync()` - Lấy danh sách vouchers

**Chức năng:** Xử lý loyalty program

---

### 💾 **Repository/** - Data Access Layer

#### **DrinkRepository.cs**

- `GetDrinkByIdAsync()`
- `GetAllDrinksAsync()`
- `AddDrinkAsync()`
- `UpdateDrinkAsync()`
- `DeleteDrinkAsync()`

#### **CakeRepository.cs**

- Tương tự Drink

#### **ToppingRepository.cs**

- Tương tự Drink

#### **OrderRepository.cs**

- `GetOrderByIdAsync()`
- `GetAllOrdersAsync()`
- `GetOrdersByUserAsync()`
- `AddOrderAsync()`
- `UpdateOrderAsync()`

#### **UserRepository.cs**

- `GetUserByIdAsync()`
- `GetUserByUsernameAsync()`
- `GetUsersByRoleAsync()`
- `AddUserAsync()`
- `UpdateUserAsync()`
- `UpdateUserStatusAsync()`

**Chức năng:** Tương tác với DynamoDB

---

### 🗃️ **Data/** - Database Setup

#### **DynamoDbService.cs**

- Tự động tạo tables khi app start
- Kiểm tra tables đã tồn tại
- Đợi tables ACTIVE trước khi dùng

**Tables được tạo:**

- CoffeeShopUsers
- Drinks
- Cakes
- Toppings
- Orders
- CoffeeShopProducts

---

## 🔐 Authentication & Authorization

### **Roles:**

- **User**: Khách hàng thông thường
  - Tạo order
  - Xem vouchers
  - Xem điểm thưởng
- **Admin**: Quản trị viên
  - Quản lý sản phẩm (CRUD)
  - Quản lý orders (update status)
  - Quản lý customers
  - Quản lý shippers
  - Xem inventory dashboard
- **Shipper**: Nhân viên giao hàng
  - (Chưa có endpoints cụ thể)

### **JWT Token:**

- Sử dụng AWS Cognito
- Token có claim `custom:role`
- Expire sau 1 giờ

---

## 🔄 Business Flow

### **Flow 1: User mua hàng**

1. User register → Cognito + DynamoDB
2. User login → nhận id_token
3. User tạo order:
   - Chọn drinks/cakes
   - Chọn toppings (optional)
   - System validate stock
   - System tính giá tự động
4. User có thể apply voucher (nếu có)
5. Admin update status → "Completed"
6. System:
   - Cộng điểm cho user
   - Trừ stock
   - Nếu đủ 100 điểm → tạo voucher mới

### **Flow 2: Admin quản lý kho**

1. Admin login
2. Xem inventory overview
3. Xem alerts (items sắp hết)
4. Cập nhật stock cho items cần nhập hàng
5. Tạo sản phẩm mới (nếu cần)

### **Flow 3: Loyalty Program**

1. User mua hàng 100,000đ → nhận 10 điểm
2. Mua 10 lần → đủ 100 điểm
3. System tự động tạo voucher giảm 10%
4. User dùng voucher cho order tiếp theo
5. FinalPrice = TotalPrice × 0.9

---

## 📊 Database Schema

### **DynamoDB Tables:**

```
CoffeeShopUsers
├─ UserId (PK)
├─ Username
├─ Role
├─ RewardPoints
└─ AvailableVouchers[]

Drinks
├─ Id (PK)
├─ Name
├─ BasePrice
├─ Stock
└─ Category

Cakes
├─ Id (PK)
├─ Name
├─ Price
└─ Stock

Toppings
├─ Id (PK)
├─ Name
├─ Price
└─ Stock

Orders
├─ OrderId (PK)
├─ UserId
├─ Items[]
│  ├─ ProductId
│  ├─ Quantity
│  └─ Toppings[]
├─ TotalPrice
├─ FinalPrice
└─ Status
```

---

## 🎯 Key Features

### ✅ **Đã implement:**

1. Authentication (AWS Cognito)
2. CRUD sản phẩm (Drinks, Cakes, Toppings)
3. Order management
4. Stock management (auto decrease)
5. Loyalty program (points + vouchers)
6. Inventory dashboard
7. Role-based authorization

### 🚧 **Có thể mở rộng:**

1. Payment integration
2. Shipper assignment
3. Order tracking
4. Customer reviews
5. Promotion campaigns
6. Analytics dashboard
7. Email notifications

---

## 🚀 Tech Stack

- **Backend:** ASP.NET Core 9.0
- **Database:** AWS DynamoDB
- **Authentication:** AWS Cognito
- **API Documentation:** Swagger/OpenAPI
- **Architecture:** Repository Pattern + Service Layer

---

## 📝 Notes

- Stock tự động giảm khi order Completed
- Voucher tự động tạo khi đủ 100 điểm
- Giá được tính ở server-side (không tin client)
- Tất cả prices đều validate >= 0
- Tất cả quantities đều validate > 0

Xong! Đây là tổng quan đầy đủ về project của bạn! 🎉
