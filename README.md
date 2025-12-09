# ☕ Coffee Shop Order Platform

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)
[![AWS](https://img.shields.io/badge/AWS-Cloud-FF9900)](https://aws.amazon.com/)
[![Elastic Beanstalk](https://img.shields.io/badge/Deployment-Elastic_Beanstalk-FF9900)](https://aws.amazon.com/elasticbeanstalk/)
[![DynamoDB](https://img.shields.io/badge/Database-DynamoDB-4053D6)](https://aws.amazon.com/dynamodb/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Hệ thống quản lý đặt hàng và giao hàng toàn diện cho quán cà phê, được xây dựng với .NET 8.0, tích hợp AWS Services, và hỗ trợ thanh toán điện tử.

---

## 📋 Mục Lục

- [Tổng Quan](#-tổng-quan)
- [Kiến Trúc Hệ Thống](#-kiến-trúc-hệ-thống)
- [Workflow Chi Tiết](#-workflow-chi-tiết)
- [Tính Năng Chính](#-tính-năng-chính)
- [Công Nghệ Sử Dụng](#-công-nghệ-sử-dụng)
- [Cài Đặt & Chạy Local](#-cài-đặt--chạy-local)
- [Cấu Hình](#-cấu-hình)
- [Triển Khai AWS Elastic Beanstalk](#-triển-khai-aws-elastic-beanstalk)
- [API Endpoints](#-api-endpoints)
- [Database Schema](#-database-schema)
- [Xác Thực & Phân Quyền](#-xác-thực--phân-quyền)
- [Tích Hợp Thanh Toán](#-tích-hợp-thanh-toán)
- [Xử Lý Lỗi](#-xử-lý-lỗi)

---

## 🎯 Tổng Quan

**Coffee Shop Order Platform** là hệ thống quản lý đơn hàng và giao hàng toàn diện cho quán cà phê, cho phép:

- 👥 **Khách hàng (Customer)**: Đặt hàng online, thanh toán qua ví điện tử, nhận voucher giảm giá
- 🛵 **Người giao hàng (Shipper)**: Nhận đơn, giao hàng, quản lý thu nhập
- 👨‍💼 **Quản trị viên (Admin)**: Quản lý sản phẩm, xác nhận đơn hàng, duyệt shipper

### Điểm Nổi Bật

- ✅ **Xác thực Hybrid**: AWS Cognito (Khách hàng/Quản trị) + JWT Local (Shipper)
- ✅ **Tính toán khoảng cách thời gian thực**: AWS Location Service với dự phòng thông minh
- ✅ **2 Cổng thanh toán**: VNPay và MoMo
- ✅ **Chương trình tích điểm**: Điểm thưởng và hệ thống voucher
- ✅ **Thông báo Email**: AWS SES cho email tự động
- ✅ **Cơ sở dữ liệu Serverless**: DynamoDB cho khả năng mở rộng cao
- ✅ **Lưu trữ ảnh**: AWS S3 cho hình ảnh sản phẩm
- ✅ **Triển khai đơn giản**: AWS Elastic Beanstalk với tự động mở rộng

---

## 🏗️ Kiến Trúc Hệ Thống

```
┌──────────────────────────────────────────────────────────────┐
│              Frontend (React/Web Application)                │
│           localhost:3000 / AWS Amplify Hosting               │
└──────────────────────┬───────────────────────────────────────┘
                       │ HTTPS/REST API Calls
                       ▼
┌──────────────────────────────────────────────────────────────┐
│           AWS Elastic Beanstalk Environment                  │
│    ┌──────────────────────────────────────────────────┐      │
│    │      Application Load Balancer (ALB)             │      │
│    │         - Health checks                          │      │
│    │         - HTTPS/HTTP traffic routing             │      │
│    └───────────────────┬──────────────────────────────┘      │
│                        │                                     │
│    ┌───────────────────┴──────────────────────────────┐      │
│    │        Auto Scaling Group (EC2 Instances)        │      │
│    │                                                  │      │
│    │  ┌─────────────────────────────────────────┐     │      │
│    │  │   ASP.NET Core 8.0 Web API              │     │      │
│    │  │   Running on .NET 8 Runtime             │     │      │
│    │  │                                         │     │      │
│    │  │  ┌────────────────────────────────┐     │     │      │
│    │  │  │    Controllers Layer           │     │     │      │
│    │  │  │  • AuthController              │     │     │      │
│    │  │  │  • OrderController             │     │     │      │
│    │  │  │  • ShipperController           │     │     │      │
│    │  │  │  • AdminController             │     │     │      │
│    │  │  │  • PaymentController           │     │     │      │
│    │  │  │  • ProductController           │     │     │      │
│    │  │  │  • DrinkController             │     │     │      │
│    │  │  │  • CakeController              │     │     │      │
│    │  │  │  • ToppingController           │     │     │      │
│    │  │  │  • LoyaltyController           │     │     │      │
│    │  │  │  • NotificationController      │     │     │      │
│    │  │  │  • CustomerController          │     │     │      │
│    │  │  │  • DashboardController         │     │     │      │
│    │  │  │  • InventoryController         │     │     │      │
│    │  │  │  • ImageController             │     │     │      │
│    │  │  └────────────────────────────────┘     │     │      │
│    │  │                                         │     │      │
│    │  │  ┌────────────────────────────────┐     │     │      │
│    │  │  │    Services Layer              │     │     │      │
│    │  │  │  • AuthService                 │     │     │      │
│    │  │  │  • ShipperAuthService          │     │     │      │
│    │  │  │  • OrderService                │     │     │      │
│    │  │  │  • OrderItemService            │     │     │      │
│    │  │  │  • ShippingService             │     │     │      │
│    │  │  │  • LoyaltyService              │     │     │      │
│    │  │  │  • VNPayService                │     │     │      │
│    │  │  │  • MoMoService                 │     │     │      │
│    │  │  │  • EmailService                │     │     │      │
│    │  │  │  • NotificationService         │     │     │      │
│    │  │  │  • S3Service                   │     │     │      │
│    │  │  └────────────────────────────────┘     │     │      │
│    │  │                                         │     │      │
│    │  │  ┌────────────────────────────────┐     │     │      │
│    │  │  │    Repository Layer            │     │     │      │
│    │  │  │  • UserRepository              │     │     │      │
│    │  │  │  • OrderRepository             │     │     │      │
│    │  │  │  • ProductRepository           │     │     │      │
│    │  │  │  • DrinkRepository             │     │     │      │
│    │  │  │  • CakeRepository              │     │     │      │
│    │  │  │  • ToppingRepository           │     │     │      │
│    │  │  │  • VoucherRepository           │     │     │      │
│    │  │  │  • NotificationRepository      │     │     │      │
│    │  │  │  • ShipperProfileRepository    │     │     │      │
│    │  │  │  • ShipperDeliveryHistory...   │     │     │      │
│    │  │  └────────────────────────────────┘     │     │      │
│    │  └─────────────────────────────────────────┘     │      │
│    │                                                  │      │
│    └──────────────────────────────────────────────────┘      │
│                                                              │
│    ┌──────────────────────────────────────────────────┐      │
│    │         CloudWatch Logs & Monitoring             │      │
│    │         - Application logs                       │      │
│    │         - Performance metrics                    │      │
│    └──────────────────────────────────────────────────┘      │
└──────────────────────┬───────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────────┐
│                   AWS Services Integration                   │
├──────────────────────────────────────────────────────────────┤
│  • DynamoDB              - NoSQL Database (Tables)           │
│    - CoffeeShopUsers                                         │
│    - CoffeeShopOrders                                        │
│    - CoffeeShopProducts                                      │
│    - CoffeeShopDrinks                                        │
│    - CoffeeShopCakes                                         │
│    - CoffeeShopToppings                                      │
│    - CoffeeShopVouchers                                      │
│    - CoffeeShopNotifications                                 │
│    - ShipperProfiles                                         │
│    - ShipperDeliveryHistory                                  │
│                                                              │
│  • Cognito               - Authentication (Customer/Admin)   │
│    - User pools                                              │
│    - Email verification                                      │
│    - Password management                                     │
│                                                              │
│  • SES                   - Email Notifications               │
│    - Order confirmations                                     │
│    - Delivery updates                                        │
│    - Account verification                                    │
│                                                              │
│  • S3                    - Object Storage                    │
│    - Product images                                          │
│    - Static assets                                           │
│                                                              │
│  • Location Service      - Geocoding & Routes                │
│    - Address to coordinates                                  │
│    - Distance calculation                                    │
│    - Delivery time estimation                                │
│                                                              │
│  • SNS                   - Push Notifications                │
│    - Real-time alerts                                        │
│    - Order status updates                                    │
│                                                              │
│  • IAM                   - Access Management                 │
│    - EC2 instance roles                                      │
│    - Service permissions                                     │
└──────────────────────┬───────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────────┐
│              External Payment Gateway Services               │
├──────────────────────────────────────────────────────────────┤
│  • VNPay                 - ATM/Credit card payments          │
│    - Sandbox for testing                                     │
│    - Callback handling                                       │
│    - IPN (Instant Payment Notification)                      │
│                                                              │
│  • MoMo                  - E-Wallet Payment                  │
│    - QR code payment                                         │
│    - Deep link support                                       │
│    - Server-to-server IPN                                    │
└──────────────────────────────────────────────────────────────┘
```

---

## 🔄 Workflow Chi Tiết Của Hệ Thống

> **💡 Hướng Dẫn Đọc**: Mỗi workflow được chia thành các bước đơn giản, mỗi bước có đánh số và ghi rõ ai làm gì. Đọc theo thứ tự từ trên xuống dưới.

---

### 1️⃣ Đăng Ký & Xác Thực Tài Khoản

#### 📱 A. Customer/Admin Đăng Ký (Dùng AWS Cognito)

**Quy Trình:**

| Bước | Người thực hiện | Hành động | Kết quả |
|------|----------------|-----------|---------|
| 1 | Customer | Điền form đăng ký (email, password, họ tên) | Gửi `POST /api/Auth/register` |
| 2 | Backend | Tạo tài khoản trong AWS Cognito | Email xác thực được gửi đến user |
| 3 | Backend | Lưu thông tin user vào DynamoDB (table `CoffeeShopUsers`) | Tài khoản được tạo nhưng chưa kích hoạt |
| 4 | Customer | Mở email và click link xác thực | Mở form nhập mã xác thực |
| 5 | Customer | Nhập mã xác thực 6 số | Gửi `POST /api/Auth/verify` |
| 6 | Backend | Xác thực mã với AWS Cognito | Tài khoản được kích hoạt ✅ |
| 7 | Customer | Đăng nhập bằng email/password | Gửi `POST /api/Auth/login` |
| 8 | Backend | Xác thực với Cognito, lấy token | Trả về `idToken`, `accessToken` |
| 9 | Frontend | Lưu token vào localStorage/cookie | Dùng token này cho các request tiếp theo |

**✨ Điểm Quan Trọng:**
- ✅ Email **phải được xác thực** mới đăng nhập được
- ✅ Token có **thời hạn** (idToken: 1 giờ)
- ✅ Admin cũng đăng ký theo flow này, chỉ khác ở role

---

#### 🛵 B. Shipper Đăng Ký (Dùng Local JWT)

**Quy Trình:**

| Bước | Người thực hiện | Hành động | Kết quả |
|------|----------------|-----------|---------|
| **GIAI ĐOẠN 1: SHIPPER ĐĂNG KÝ** |
| 1 | Shipper | Điền form đăng ký (username, password, họ tên, email, phone, loại xe, biển số) | Gửi `POST /api/ShipperRegistration/register` |
| 2 | Backend | Mã hóa password bằng BCrypt | Password được hash an toàn |
| 3 | Backend | Lưu vào DynamoDB với `Role: "Shipper"` và `RegistrationStatus: "Pending"` | Tài khoản được tạo nhưng **chưa được duyệt** ⏳ |
| 4 | Frontend | Hiển thị thông báo "Đang chờ Admin phê duyệt" | Shipper phải đợi |
| **GIAI ĐOẠN 2: ADMIN DUYỆT** |
| 5 | Admin | Vào trang quản lý, xem danh sách shipper chờ duyệt | Gửi `GET /api/Admin/shippers/pending` |
| 6 | Backend | Query DynamoDB lấy các shipper có `RegistrationStatus: "Pending"` | Trả về danh sách |
| 7 | Admin | Xem thông tin shipper (tên, email, phone, xe) và quyết định | Click nút "Phê duyệt" hoặc "Từ chối" |
| 8 | Admin | Phê duyệt shipper | Gửi `POST /api/Admin/shippers/:id/approve` |
| 9 | Backend | Cập nhật `RegistrationStatus: "Approved"` | Shipper được phép đăng nhập ✅ |
| 10 | Backend | Gửi email thông báo cho shipper | Email: "Tài khoản đã được duyệt" |
| **GIAI ĐOẠN 3: SHIPPER ĐĂNG NHẬP** |
| 11 | Shipper | Đăng nhập bằng username/password | Gửi `POST /api/ShipperAuth/login` |
| 12 | Backend | Kiểm tra username tồn tại | Tìm user trong DynamoDB |
| 13 | Backend | Verify password bằng BCrypt | So sánh hash |
| 14 | Backend | Kiểm tra `RegistrationStatus == "Approved"` | Đảm bảo đã được duyệt |
| 15 | Backend | Tạo JWT token (custom) | Token có thời hạn 7 ngày |
| 16 | Frontend | Lưu token và thông tin shipper | Shipper có thể nhận đơn hàng 🚚 |

**✨ Điểm Quan Trọng:**
- ⚠️ Shipper **phải được Admin duyệt** mới đăng nhập được
- ✅ Dùng **BCrypt** để hash password (không lưu plaintext)
- ✅ JWT token **riêng biệt** với Cognito (độc lập)

---

### 2️⃣ Đặt Hàng & Thanh Toán

**Quy Trình:**

| Bước | Người thực hiện | Hành động | Kết quả |
|------|----------------|-----------|---------|
| **GIAI ĐOẠN 1: XEM MENU VÀ THÊM VÀO GIỎ HÀNG** |
| 1 | Customer | Vào trang menu, xem danh sách món | Gửi `GET /api/Drink/all`, `GET /api/Cake/all`, `GET /api/Topping/all` |
| 2 | Backend | Truy vấn DynamoDB lấy danh sách sản phẩm | Trả về list products với giá, tồn kho |
| 3 | Customer | Chọn món, chọn topping, nhập số lượng | Thêm vào giỏ hàng (lưu ở frontend) |
| **GIAI ĐOẠN 2: TẠO ĐƠN HÀNG** |
| 4 | Customer | Click "Đặt hàng", điền địa chỉ, phone, chọn voucher (nếu có) | Gửi `POST /api/Order/create` |
| 5 | Backend | Kiểm tra `clientOrderId` để tránh đơn trùng | Nếu trùng → reject |
| 6 | Backend | Validate tất cả items (ID, giá, tồn kho) | Nếu sai → trả lỗi |
| 7 | Backend | Kiểm tra voucher (nếu có): còn hạn? đã dùng chưa? | Tính discount |
| 8 | Backend | Tính tổng tiền: `TotalPrice = Σ(item.price × quantity)` | Trừ discount nếu có |
| 9 | Backend | Tạo order mới với `Status: "Pending"` | Lưu vào DynamoDB table `CoffeeShopOrders` |
| 10 | Backend | Trả về `orderId` và thông tin order | Frontend nhận được orderId |
| **GIAI ĐOẠN 3: THANH TOÁN** |
| 11 | Customer | Chọn phương thức: VNPay hoặc MoMo | Click "Thanh toán" |
| 12 | Frontend | Gửi request tạo payment URL | `POST /api/Payment/vnpay/create` hoặc `POST /api/Payment/momo/create` |
| 13 | Backend | Tạo signature (HMAC-SHA512 cho VNPay, HMAC-SHA256 cho MoMo) | Đảm bảo tính toàn vẹn dữ liệu |
| 14 | Backend | Tạo payment URL với các params: orderId, amount, returnUrl... | Trả về URL |
| 15 | Frontend | Redirect user đến payment gateway | Mở trang VNPay/MoMo |
| 16 | Customer | Nhập thông tin thẻ/tài khoản, xác nhận | Thanh toán trên gateway |
| 17 | Payment Gateway | Xử lý thanh toán (trừ tiền) | Thành công hoặc thất bại |
| **GIAI ĐOẠN 4: XỬ LÝ CALLBACK** |
| 18 | Payment Gateway | Gửi kết quả về backend | `GET /api/Payment/vnpay/callback` hoặc `POST /api/Payment/momo/callback` |
| 19 | Backend | Validate signature từ gateway | Đảm bảo request từ gateway thật |
| 20 | Backend | Kiểm tra `ResponseCode == "00"` (thành công) | Nếu khác 00 → payment failed |
| 21 | Backend | Cập nhật order: `Status: "Processing"`, `PaymentStatus: "Paid"`, `PaidAt: timestamp` | Order đã thanh toán ✅ |
| 22 | Backend | Gửi email xác nhận đơn hàng cho customer | Email với chi tiết order |
| 23 | Backend | Tạo notification cho Admin | Admin thấy có đơn mới |
| 24 | Backend | Redirect user về success page | Frontend hiển thị "Đặt hàng thành công" |

**✨ Điểm Quan Trọng:**
- ✅ `clientOrderId`: Mỗi client tạo ID unique để **tránh đơn trùng** khi user spam click
- ✅ **Validate signature** từ payment gateway để tránh fake callback
- ⚠️ Order chỉ chuyển sang "Processing" khi payment **thành công**
- ✅ Email được gửi **tự động** sau khi thanh toán thành công

---

### 3️⃣ Xử Lý Đơn Hàng (Admin → Shipper → Giao Hàng)

**Quy Trình:**

| Bước | Người thực hiện | Hành động | Kết quả |
|------|----------------|-----------|---------|
| **GIAI ĐOẠN 1: ADMIN XÁC NHẬN ĐƠN HÀNG** |
| 1 | Admin | Vào trang quản lý đơn hàng | Gửi `GET /api/Admin/orders/pending` |
| 2 | Backend | Lấy danh sách đơn có `Status: "Processing"` | Trả về list orders |
| 3 | Admin | Xem chi tiết đơn (items, địa chỉ, customer) | Kiểm tra thông tin |
| 4 | Admin | Click "Xác nhận đơn hàng" | Gửi `POST /api/Admin/orders/:id/confirm` |
| 5 | Backend | Cập nhật: `Status: "Confirmed"`, `ConfirmedAt: timestamp` | Đơn chuyển sang trạng thái "Confirmed" ✅ |
| 6 | Backend | Gửi email cho customer: "Đơn hàng đã được xác nhận" | Customer biết đơn đã được duyệt |
| 7 | Backend | Tạo notification cho tất cả shipper | Shipper thấy có đơn mới để nhận |
| **GIAI ĐOẠN 2: SHIPPER NHẬN ĐơN** |
| 8 | Shipper | Vào app, xem danh sách đơn có sẵn | Gửi `GET /api/Shipper/orders/available` |
| 9 | Backend | Lấy orders có `Status: "Confirmed"` và `ShipperId == null` | Trả về list |
| 10 | Shipper | Chọn đơn, xem địa chỉ giao hàng | Click "Tính phí ship" |
| 11 | Shipper | Gửi request tính phí | `POST /api/Shipper/orders/:id/calculate-fee` |
| 12 | Backend | Dùng **AWS Location Service** geocode địa chỉ | Chuyển địa chỉ text → lat/lng |
| 13 | Backend | Tính khoảng cách từ shop đến địa chỉ customer | Distance (km) |
| 14 | Backend | Tính phí ship: `15,000 VNĐ + (distance × 5,000 VNĐ/km)` | VD: 3km → 15,000 + 15,000 = 30,000 VNĐ |
| 15 | Backend | Ước tính thời gian: `distance / 25 km/h` | VD: 3km → ~7 phút |
| 16 | Backend | Trả về `{distance, shippingFee, estimatedTime}` | Shipper thấy được thông tin |
| 17 | Shipper | Xem phí ship, quyết định nhận | Click "Nhận đơn" |
| 18 | Shipper | Gửi request nhận đơn | `POST /api/Shipper/orders/:id/accept` |
| 19 | Backend | Cập nhật: `Status: "Shipping"`, `ShipperId: shipperId`, `ShippingAt: timestamp`, `ShippingFee: fee` | Đơn đã có shipper ✅ |
| 20 | Backend | Gửi email cho customer: "Đơn hàng đang được giao" + tên shipper + phone | Customer biết shipper là ai |
| **GIAI ĐOẠN 3: GIAO HÀNG** |
| 21 | Shipper | Đến shop lấy hàng, giao đến customer | ... |
| 22 | Shipper | Sau khi giao xong, click "Hoàn thành" | Gửi `POST /api/Shipper/orders/:id/complete` |
| 23 | Backend | Cập nhật: `Status: "Delivered"`, `DeliveredAt: timestamp` | Đơn đã giao ✅ |
| 24 | Backend | Tạo bản ghi lịch sử giao hàng trong table `ShipperDeliveryHistory` | Lưu: orderId, shipperId, distance, fee, deliveredAt |
| 25 | Backend | Cập nhật thu nhập shipper: `TotalEarnings += shippingFee` | Shipper được trả phí ship |
| 26 | Backend | Gửi email cho customer: "Đơn hàng đã được giao" | Nhắc customer xác nhận |
| **GIAI ĐOẠN 4: CUSTOMER XÁC NHẬN** |
| 27 | Customer | Nhận hàng, kiểm tra, click "Xác nhận đã nhận" | Gửi `POST /api/Order/:id/complete` |
| 28 | Backend | Cập nhật: `Status: "Completed"`, `CompletedAt: timestamp` | Đơn hoàn tất ✅✅ |
| 29 | Backend | Tính điểm loyalty: `points = FinalPrice / 10,000` | VD: 250,000 VNĐ → 25 điểm |
| 30 | Backend | Cộng điểm vào tài khoản customer | Lưu vào `LoyaltyPoints` |
| 31 | Backend | Trừ tồn kho sản phẩm (nếu có inventory management) | Update stock |
| 32 | Backend | Trả về kết quả | `{success: true, pointsEarned: 25}` |

**✨ Điểm Quan Trọng:**
- ✅ Flow: **Pending → Processing (paid) → Confirmed (admin) → Shipping (shipper) → Delivered → Completed**
- ✅ **AWS Location Service**: Tính khoảng cách thực tế (có fallback nếu geocoding fail)
- ✅ Shipper chỉ nhận được phí ship khi **hoàn thành giao hàng**
- ✅ Loyalty points chỉ được cộng khi customer **xác nhận nhận hàng**

---

### 4️⃣ Tích Điểm & Sử Dụng Voucher

**Quy Trình:**

| Bước | Người thực hiện | Hành động | Kết quả |
|------|----------------|-----------|---------|
| **GIAI ĐOẠN 1: TÍCH ĐIỂM TỰ ĐỘNG** |
| 1 | (Tự động) | Sau khi order completed | Backend tự động gọi `LoyaltyService.AddPointsAsync()` |
| 2 | Backend | Tính điểm: `points = FinalPrice / 10,000` | VD: 250,000 VNĐ → 25 điểm |
| 3 | Backend | Cộng điểm vào `LoyaltyPoints` của customer trong DynamoDB | Tổng điểm tăng lên |
| **GIAI ĐOẠN 2: ĐỔI ĐIỂM LẤY VOUCHER** |
| 4 | Customer | Vào trang "Điểm thưởng", xem tổng điểm | Gửi `GET /api/Loyalty/points` |
| 5 | Backend | Trả về `{totalPoints: 125}` | Hiển thị số điểm hiện có |
| 6 | Customer | Chọn mức giảm giá: 5% (100 điểm), 10% (100 điểm), hoặc 15% (100 điểm) | Click "Đổi voucher 10%" |
| 7 | Customer | Gửi request đổi điểm | `POST /api/Loyalty/redeem` với `{discountPercent: 10}` |
| 8 | Backend | Kiểm tra `totalPoints >= 100` | Nếu không đủ → trả lỗi |
| 9 | Backend | Tạo voucher code ngẫu nhiên 8 ký tự (VD: "AB12CD34") | Random string |
| 10 | Backend | Tạo voucher trong table `CoffeeShopVouchers`: `Code`, `UserId`, `DiscountPercent`, `ExpiryDate (+30 days)`, `IsUsed: false` | Voucher được tạo ✅ |
| 11 | Backend | Trừ 100 điểm từ tài khoản customer | `LoyaltyPoints -= 100` |
| 12 | Backend | Trả về thông tin voucher | `{voucherCode: "AB12CD34", expiresAt: "2025-01-08"}` |
| **GIAI ĐOẠN 3: SỬ DỤNG VOUCHER KHI ĐẶT HÀNG** |
| 13 | Customer | Tạo đơn hàng mới, nhập voucher code "AB12CD34" | Gửi `POST /api/Order/create` với `voucherCode: "AB12CD34"` |
| 14 | Backend | Tìm voucher trong DynamoDB | Query by `Code = "AB12CD34"` |
| 15 | Backend | Kiểm tra voucher: **tồn tại? chưa dùng? còn hạn? thuộc về user này?** | Validate các điều kiện |
| 16 | Backend | Tính giảm giá: `Discount = TotalPrice × DiscountPercent / 100` | VD: 200,000 × 10% = 20,000 VNĐ |
| 17 | Backend | Tính giá cuối: `FinalPrice = TotalPrice - Discount` | 200,000 - 20,000 = 180,000 VNĐ |
| 18 | Backend | Đánh dấu voucher đã dùng: `IsUsed: true`, `UsedAt: timestamp` | Voucher không thể dùng lại |
| 19 | Backend | Tạo order với `FinalPrice = 180,000`, lưu `VoucherCode` | Order có giảm giá ✅ |

**✨ Điểm Quan Trọng:**
- ✅ **1 điểm = 10,000 VNĐ** (chi 250k → được 25 điểm)
- ✅ **100 điểm đổi 1 voucher** (5%, 10%, hoặc 15% tùy chọn)
- ✅ Voucher có **hạn 30 ngày** kể từ lúc tạo
- ⚠️ Mỗi voucher **chỉ dùng 1 lần** (`IsUsed: true` sau khi dùng)
- ✅ Voucher **chỉ áp dụng cho user tạo ra nó** (không share được)

---

## ✨ Tính Năng Chính

### 🔐 Xác Thực & Phân Quyền

#### Hệ Thống Xác Thực Hybrid
- **AWS Cognito**: Dành cho Customer và Admin
  - Xác minh email tự động
  - Quản lý mật khẩu và đặt lại mật khẩu
  - Cơ chế refresh token
  
- **JWT Local**: Dành riêng cho Shipper
  - Mã hóa mật khẩu BCrypt
  - Tạo JWT token tùy chỉnh
  - Kiểm soát truy cập theo vai trò

#### Vai Trò & Quyền Hạn

| Vai Trò  | Xác Thực       | Chức Năng                                             |
|----------|----------------|-------------------------------------------------------|
| Customer | AWS Cognito    | Đặt hàng, xem lịch sử, đổi voucher                    |
| Admin    | AWS Cognito    | Quản lý sản phẩm, xác nhận đơn, quản lý shipper       |
| Shipper  | Local JWT      | Nhận đơn, giao hàng, theo dõi thu nhập                |

### 📦 Quản Lý Đơn Hàng

#### Luồng Trạng Thái Đơn Hàng
```
Chờ xử lý → Đang xử lý → Đã xác nhận → Đang giao → Đã giao → Hoàn thành
                  ↓
              Đã hủy (có thể hủy ở Chờ xử lý/Đang xử lý/Đã xác nhận)
```

#### Tính Năng
- ✅ Đơn hàng nhiều sản phẩm (Drinks/Cakes) với topping
- ✅ Tự động áp dụng giảm giá voucher
- ✅ Tính phí ship theo khoảng cách thực tế
- ✅ Ngăn chặn đơn hàng trùng lặp với `clientOrderId`
- ✅ Lịch sử đơn hàng chi tiết với thống kê
- ✅ Admin xác nhận → Shipper nhận → Hoàn thành giao hàng

### 💰 Tích Hợp Thanh Toán

#### Phương Thức Thanh Toán Hỗ Trợ

1. **VNPay**
   - Thanh toán ATM/Visa/Mastercard
   - Chế độ Sandbox để test
   - Xác thực chữ ký HMAC-SHA512
   - Hỗ trợ IPN (Instant Payment Notification)
   - Xử lý callback an toàn

2. **MoMo**
   - Thanh toán ví điện tử
   - Thanh toán QR Code
   - Hỗ trợ deep link (ứng dụng mobile)
   - Xử lý callback tự động
   - IPN server-to-server

3. **Tiền mặt** (Dự kiến)
   - Thanh toán khi nhận hàng (COD)

### 🎁 Chương Trình Tích Điểm

#### Hệ Thống Điểm Thưởng
- **Tích điểm**: 1 điểm cho mỗi 10,000 VNĐ chi tiêu
- **Đổi Voucher**: 100 điểm = 1 voucher (giảm 5-15%)
- **Hết hạn Voucher**: 30 ngày kể từ ngày phát hành

#### Tính Năng Voucher
- ✅ Tự động tạo mã voucher ngẫu nhiên (8 ký tự)
- ✅ Kiểm tra trước khi áp dụng đơn hàng
- ✅ Tự động áp dụng khi tạo đơn hàng
- ✅ Theo dõi voucher đã dùng/còn hiệu lực/hết hạn

### 🚚 Hệ Thống Giao Hàng

#### Chiến Lược Tính Khoảng Cách
```
1. AWS Location Service (Ưu tiên)
      ↓ (khi lỗi)
2. Công thức Haversine (Dự phòng thứ 2)
      ↓ (khi lỗi)
3. Ước tính Cố định (Dự phòng cuối cùng)
```

#### Công Thức Phí Ship
```
Distance ≤ 3km:   15,000 VNĐ (base fee)
Distance > 3km:   15,000 + (distance - 3) × 5,000 VNĐ
```

**Ví dụ:**
- 2km → 15,000 VNĐ
- 5km → 15,000 + (2 × 5,000) = 25,000 VNĐ
- 10km → 15,000 + (7 × 5,000) = 50,000 VNĐ

#### Tính Năng Shipper
- ✅ Xem danh sách đơn hàng có sẵn
- ✅ Tính phí ship trước khi nhận đơn
- ✅ Nhận đơn và cập nhật trạng thái
- ✅ Lịch sử giao hàng và thống kê thu nhập
- ✅ Quản lý hồ sơ (thông tin xe, tài khoản ngân hàng)

### 👨‍💼 Khả Năng Quản Trị

#### Quản Lý Shipper
- ✅ Duyệt/từ chối đăng ký shipper
- ✅ Tự động tạo mật khẩu và gửi email
- ✅ Khóa/mở khóa tài khoản shipper
- ✅ Đặt lại mật khẩu shipper
- ✅ Xem thống kê shipper (giao hàng, thu nhập, đánh giá)

#### Quản Lý Đơn Hàng
- ✅ Xem đơn hàng chờ xác nhận
- ✅ Xác nhận đơn hàng (Đang xử lý → Đã xác nhận)
- ✅ Theo dõi trạng thái đơn hàng thời gian thực
- ✅ Quản lý tồn kho và kho hàng

#### Quản Lý Sản Phẩm
- ✅ Các thao tác CRUD cho Đồ uống, Bánh, Topping
- ✅ Quản lý giá và tình trạng sẵn có
- ✅ Tải ảnh sản phẩm lên AWS S3
- ✅ Quản lý danh mục

### 📧 Hệ Thống Thông Báo

#### Thông Báo Email (AWS SES)
- ✅ **Duyệt shipper**: Email kèm username + mật khẩu được tạo
- ✅ **Từ chối shipper**: Email kèm lý do từ chối
- ✅ **Đặt lại mật khẩu**: Email kèm mật khẩu tạm thời mới
- ✅ **Xác nhận đơn hàng**: Email xác nhận gửi cho khách hàng

#### Thông Báo Đẩy (AWS SNS) - Dự kiến
- Cập nhật trạng thái đơn hàng
- Thông báo khuyến mãi
- Theo dõi giao hàng thời gian thực

---

## 🛠️ Công Nghệ Sử Dụng

### Framework Backend
- **Framework**: ASP.NET Core 8.0 (Web API)
- **Ngôn ngữ**: C# 12
- **Kiến trúc**: Repository Pattern + Service Layer
- **Tài liệu API**: Swagger/OpenAPI

### Cơ Sở Dữ Liệu
- **Chính**: Amazon DynamoDB (NoSQL)
  - `CoffeeShopUsers` - Tài khoản người dùng
  - `Orders` - Quản lý đơn hàng
  - `CoffeeShopProducts` - Danh mục sản phẩm
  - `Drinks` - Kho đồ uống
  - `Cakes` - Kho bánh ngọt
  - `Toppings` - Danh mục topping
  - `ShipperProfiles` - Chi tiết shipper
  - `ShipperDeliveryHistory` - Theo dõi giao hàng
  - `Notifications` - Nhật ký thông báo

### Dịch Vụ AWS

| Dịch vụ           | Mục đích                                     |
|-------------------|----------------------------------------------|
| DynamoDB          | Cơ sở dữ liệu NoSQL cho tất cả các thực thể |
| Cognito           | Xác thực người dùng (Khách hàng/Quản trị)    |
| SES               | Dịch vụ Email cho thông báo                  |
| S3                | Lưu trữ ảnh cho hình ảnh sản phẩm            |
| Location Service  | Geocoding & Tính toán khoảng cách/Đường đi   |
| SNS               | Thông báo đẩy (dự kiến)                      |
| Amplify           | Lưu trữ và triển khai Frontend               |

### Tích Hợp Bên Thứ Ba
- **VNPay**: Cổng thanh toán Việt Nam
- **MoMo**: Nhà cung cấp thanh toán ví điện tử
- **BCrypt.Net**: Mã hóa mật khẩu an toàn
- **JWT**: Xác thực JSON Web Token

### Các Gói NuGet Chính

```xml
<PackageReference Include="AWSSDK.DynamoDBv2" Version="4.0.9.4" />
<PackageReference Include="AWSSDK.LocationService" Version="4.0.3.4" />
<PackageReference Include="AWSSDK.S3" Version="4.0.13.1" />
<PackageReference Include="AWSSDK.SimpleEmail" Version="4.0.2.2" />
<PackageReference Include="AWSSDK.SimpleNotificationService" Version="4.0.2.5" />
<PackageReference Include="Amazon.Extensions.CognitoAuthentication" Version="3.1.1" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.14.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.14.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.6" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
```

---

## 🚀 Cài Đặt

### Yêu Cầu Trước

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) hoặc mới hơn
- [Tài khoản AWS](https://aws.amazon.com/) (Đủ điều kiện cho bậc miễn phí)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) hoặc [VS Code](https://code.visualstudio.com/)
- [AWS CLI](https://aws.amazon.com/cli/) (tùy chọn, cho triển khai)

### Bước 1: Sao Chép Kho Mã Nguồn

```bash
git clone https://github.com/quannguyen-1110/Coffe-shop-oder-platfrom.git
cd Coffe-shop-oder-platfrom
```

### Bước 2: Khôi Phục Các Thư Viện Phụ Thuộc

```bash
dotnet restore
```

### Bước 3: Cấu Hình AWS Credentials

**Tùy chọn A: AWS CLI**
```bash
aws configure
```

**Tùy chọn B: Biến Môi Trường**
```bash
# Windows PowerShell
$env:AWS_ACCESS_KEY_ID="your_access_key"
$env:AWS_SECRET_ACCESS_KEY="your_secret_key"
$env:AWS_REGION="ap-southeast-1"

# Linux/Mac
export AWS_ACCESS_KEY_ID=your_access_key
export AWS_SECRET_ACCESS_KEY=your_secret_key
export AWS_REGION=ap-southeast-1
```

**Tùy chọn C: User Secrets (Khuyến nghị cho Phát triển)**
```bash
dotnet user-secrets init
dotnet user-secrets set "AWS:AccessKey" "your_access_key"
dotnet user-secrets set "AWS:SecretKey" "your_secret_key"
dotnet user-secrets set "AWS:Region" "ap-southeast-1"
```

### Bước 4: Thiết Lập Bảng DynamoDB

Các bảng DynamoDB được **tự động tạo** khi chạy ứng dụng lần đầu qua `DynamoDbService.cs`. Dịch vụ sẽ:
- Quét tất cả các model có thuộc tính `[DynamoDBTable]`
- Tạo các bảng còn thiếu với chế độ thanh toán PAY_PER_REQUEST
- Đợi các bảng chuyển sang trạng thái ACTIVE

Không cần tạo bảng thủ công! 🎉

### Bước 5: Thiết Lập AWS Cognito

1. Tạo User Pool trong AWS Cognito Console
2. Tạo App Client (không có client secret)
3. Cấu hình cài đặt đăng ký/đăng nhập:
   - Yêu cầu xác minh email
   - Chính sách mật khẩu (tối thiểu 8 ký tự)
   - Thuộc tính tùy chỉnh: `custom:role` (String)
4. Sao chép `UserPoolId` và `ClientId` vào `appsettings.json`

### Bước 6: Thiết Lập Cổng Thanh Toán

#### VNPay (Sandbox)
1. Đăng ký tài khoản sandbox tại [VNPay Sandbox](https://sandbox.vnpayment.vn/)
2. Lấy `TmnCode` và `HashSecret`
3. Cập nhật `appsettings.json` với thông tin xác thực

#### MoMo (Môi Trường Test)
1. Đăng ký tài khoản test tại [MoMo Developer](https://developers.momo.vn/)
2. Lấy `PartnerCode`, `AccessKey`, `SecretKey`
3. Cập nhật `appsettings.json` với thông tin xác thực

### Bước 7: Cấu Hình Cài Đặt

Chỉnh sửa `appsettings.json` hoặc `appsettings.Development.json`:

```json
{
  "AWS": {
    "Region": "ap-southeast-1"
  },
  "Cognito": {
    "UserPoolId": "ap-southeast-1_XXXXXXXXX",
    "ClientId": "your-client-id-here"
  },
  "Jwt": {
    "LocalKey": "your-secret-key-minimum-32-characters-long",
    "ExpiryMinutes": 60
  },
  "VNPay": {
    "TmnCode": "your-tmn-code",
    "HashSecret": "your-hash-secret",
    "ReturnUrl": "http://localhost:5144/api/Payment/vnpay/callback"
  },
  "MoMo": {
    "PartnerCode": "your-partner-code",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "ReturnUrl": "http://localhost:5144/api/MoMoPayment/callback"
  }
}
```

### Bước 8: Chạy Ứng Dụng

```bash
dotnet run
```

Ứng dụng sẽ chạy tại:
- **HTTP**: http://localhost:5144
- **HTTPS**: https://localhost:7144
- **Swagger UI**: http://localhost:5144/swagger

---

## 📚 API Endpoints

### Xác Thực

| Phương thức | Endpoint                      | Mô tả                              |
|-------------|-------------------------------|------------------------------------|
| POST   | `/Auth/register`                   | Đăng ký Khách hàng/Admin (Cognito) |
| POST   | `/Auth/login`                      | Đăng nhập (Cognito + JWT)          |
| POST   | `/Auth/confirm`                    | Xác nhận email (Cognito)           |
| POST   | `/ShipperRegistration/register`    | Đăng ký Shipper (chờ duyệt)        |
| POST   | `/ShipperAuth/login`               | Đăng nhập Shipper (JWT)            |

#### Được Bảo Vệ (Admin - Cognito Token)

| Phương thức | Endpoint                           | Mô tả                           |
|-------------|------------------------------------|---------------------------------|
| GET    | `/Admin/shippers/pending`          | Lấy danh sách shipper chờ duyệt      |
| POST   | `/Admin/shippers/{id}/approve`     | Duyệt đăng ký shipper                |
| POST   | `/Admin/shippers/{id}/reject`      | Từ chối đăng ký shipper              |
| POST   | `/Admin/shippers/{id}/reset-password`| Đặt lại mật khẩu shipper           |
| POST   | `/Admin/shippers/{id}/lock`        | Khóa tài khoản shipper               |
| POST   | `/Admin/shippers/{id}/unlock`      | Mở khóa tài khoản shipper            |
| GET    | `/Admin/orders/pending`            | Lấy đơn hàng chờ xác nhận            |
| POST   | `/Admin/orders/{orderId}/confirm`  | Xác nhận đơn hàng (Đang xử lý→Đã xác nhận) |

### Endpoint Sản Phẩm

#### Khách Hàng

| Phương thức | Endpoint                      | Vai trò  | Mô tả                                |
|-------------|-------------------------------|----------|--------------------------------------|
| POST   | `/Order`                           | Người dùng | Tạo đơn hàng mới                   |
| GET    | `/Order/my-orders`                 | Người dùng | Xem lịch sử đơn hàng               |
| GET    | `/Order/my-orders/{orderId}`       | Người dùng | Xem chi tiết đơn hàng              |

#### Quản Trị

| Phương thức | Endpoint                      | Vai trò | Mô tả                              |
|-------------|-------------------------------|---------|------------------------------------|
| GET    | `/Admin/orders/pending-confirm`    | Admin | Đơn hàng chờ xác nhận                |
| POST   | `/Admin/orders/{orderId}/confirm`  | Admin | Xác nhận đơn hàng                    |
| GET    | `/Admin/orders`                    | Admin | Tất cả đơn hàng                      |
| PUT    | `/Order/{id}/status`               | Admin | Cập nhật trạng thái đơn hàng         |

#### Shipper

| Phương thức | Endpoint                               | Vai trò | Mô tả                                |
|-------------|----------------------------------------|---------|--------------------------------------|
| GET    | `/Shipper/orders/available`                 | Shipper | Đơn hàng có sẵn để giao              |
| GET    | `/Shipper/orders/{orderId}`                 | Shipper | Chi tiết đơn hàng                    |
| POST   | `/Shipper/orders/{orderId}/calculate-fee`   | Shipper | Tính phí giao hàng                   |
| POST   | `/Shipper/orders/{orderId}/accept`          | Shipper | Nhận đơn hàng                        |
| POST   | `/Shipper/orders/{orderId}/complete`        | Shipper | Hoàn thành giao hàng                 |
| GET    | `/Shipper/orders/history`                   | Shipper | Lịch sử giao hàng                    |

| Phương thức | Endpoint                      | Mô tả                                |
|-------------|-------------------------------|--------------------------------------|
| GET    | `/Drink/all`                       | Lấy tất cả đồ uống                   |
| GET    | `/Drink/{id}`                      | Lấy đồ uống cụ thể                   |
| POST   | `/Drink/add` (Admin)               | Thêm đồ uống mới                     |
| PUT    | `/Drink/{id}` (Admin)              | Cập nhật đồ uống                     |
| DELETE | `/Drink/{id}` (Admin)              | Xóa đồ uống                          |
| GET    | `/Cake/all`                        | Lấy tất cả bánh                      |
| GET    | `/Cake/{id}`                       | Lấy bánh cụ thể                      |
| POST   | `/Cake/add` (Admin)                | Thêm bánh mới                        |
| PUT    | `/Cake/{id}` (Admin)               | Cập nhật bánh                        |
| DELETE | `/Cake/{id}` (Admin)               | Xóa bánh                             |
| GET    | `/Topping/all`                     | Lấy tất cả topping                   |
| POST   | `/Topping/add` (Admin)             | Thêm topping mới                     |
| PUT    | `/Topping/{id}` (Admin)            | Cập nhật topping                     |
| DELETE | `/Topping/{id}` (Admin)            | Xóa topping                          |

### Endpoint Thanh Toán

| Phương thức | Endpoint                      | Mô tả                                |
|-------------|-------------------------------|--------------------------------------|
| POST   | `/Payment/vnpay/create`            | Tạo URL thanh toán VNPay             |
| GET    | `/Payment/vnpay/callback`          | Xử lý callback VNPay                 |
| POST   | `/MoMoPayment/create`              | Tạo yêu cầu thanh toán MoMo          |
| POST   | `/MoMoPayment/callback`            | Xử lý callback MoMo                  |
| POST   | `/MoMoPayment/ipn`                 | Xử lý IPN MoMo                       |

### Endpoint Dashboard (Admin)

| Phương thức | Endpoint                      | Mô tả                                |
|-------------|-------------------------------|--------------------------------------|
| GET    | `/Dashboard/statistics`            | Lấy thống kê tổng quan               |
| GET    | `/Dashboard/revenue`               | Lấy dữ liệu doanh thu                |
| GET    | `/Dashboard/orders/recent`         | Lấy đơn hàng gần đây                 |

---

## 🗄️ Database Schema

### DynamoDB Tables

#### 1. CoffeeShopUsers

```json
{
  "UserId": "string (PK)",
  "Username": "string",
  "Email": "string",
  "FullName": "string",
  "PhoneNumber": "string",
  "Role": "string (User|Admin|Shipper)",
  "RegistrationStatus": "string (Pending|Approved|Rejected)",
  "IsActive": "boolean",
  "LoyaltyPoints": "number",
  "CreatedAt": "datetime",
  "UpdatedAt": "datetime"
}
```

#### 2. CoffeeShopOrders

```json
{
  "OrderId": "string (PK)",
  "UserId": "string",
  "Status": "string (Pending|Processing|Confirmed|Shipping|Delivered|Completed|Cancelled)",
  "Items": [
    {
      "ProductId": "string",
      "ProductType": "string (Drink|Cake)",
      "ProductName": "string",
      "Quantity": "number",
      "UnitPrice": "decimal",
      "TotalPrice": "decimal",
      "Toppings": [
        {
          "ToppingId": "string",
          "ToppingName": "string",
          "Quantity": "number",
          "Price": "decimal"
        }
      ]
    }
  ],
  "TotalPrice": "decimal",
  "FinalPrice": "decimal",
  "AppliedVoucherCode": "string",
  "PaymentMethod": "string (VNPay|MoMo|Cash)",
  "DeliveryAddress": "string",
  "DeliveryPhone": "string",
  "DeliveryNote": "string",
  "ShippingFee": "decimal",
  "DistanceKm": "decimal",
  "ShipperId": "string",
  "ConfirmedBy": "string (adminId)",
  "CreatedAt": "datetime",
  "ConfirmedAt": "datetime",
  "ShippingAt": "datetime",
  "DeliveredAt": "datetime",
  "CompletedAt": "datetime"
}
```

#### 3. CoffeeShopProducts / Drinks / Cakes

```json
{
  "Id": "string (PK)",
  "Name": "string",
  "Price": "decimal",
  "Stock": "number",
  "ImageUrl": "string",
  "Category": "string",
  "Description": "string",
  "IsAvailable": "boolean"
}
```

#### 4. Toppings

```json
{
  "Id": "string (PK)",
  "Name": "string",
  "Price": "decimal",
  "IsAvailable": "boolean"
}
```

#### 5. CoffeeShopVouchers

```json
{
  "VoucherId": "string (PK)",
  "UserId": "string",
  "Code": "string (Unique, 8 chars)",
  "DiscountPercent": "number",
  "IsUsed": "boolean",
  "CreatedAt": "datetime",
  "ExpiresAt": "datetime",
  "UsedAt": "datetime"
}
```

#### 6. ShipperProfiles

```json
{
  "ShipperId": "string (PK)",
  "FullName": "string",
  "Email": "string",
  "Phone": "string",
  "VehicleType": "string (Bike|Motorcycle|Car)",
  "VehiclePlate": "string",
  "BankAccount": "string",
  "BankName": "string",
  "TotalDeliveries": "number",
  "TotalEarnings": "decimal",
  "AverageRating": "decimal",
  "IsAvailable": "boolean",
  "CreatedAt": "datetime"
}
```

#### 7. ShipperDeliveryHistory

```json
{
  "DeliveryId": "string (PK)",
  "ShipperId": "string",
  "OrderId": "string",
  "DistanceKm": "decimal",
  "ShippingFee": "decimal",
  "DeliveredAt": "datetime",
  "CustomerRating": "number",
  "CustomerFeedback": "string"
}
```

#### 8. CoffeeShopNotifications

```json
{
  "NotificationId": "string (PK)",
  "UserId": "string",
  "Type": "string (OrderUpdate|Payment|Loyalty)",
  "Title": "string",
  "Message": "string",
  "IsRead": "boolean",
  "CreatedAt": "datetime"
}
```

---

## 🔐 Xác Thực & Phân Quyền

### Luồng Xác Thực Hybrid

```text
Khách hàng/Quản trị:
1. Đăng ký qua AWS Cognito
2. Yêu cầu xác minh email
3. Đăng nhập → Nhận Cognito ID Token + Access Token
4. Sử dụng ID Token cho các endpoint được bảo vệ
5. Token chứa claims: sub (userId), email, custom:role

Shipper:
1. Đăng ký qua API → Trạng thái: Chờ duyệt
2. Admin duyệt → Trạng thái: Đã duyệt
3. Đăng nhập → Nhận Local JWT Token
4. Token chứa claims: nameid (shipperId), role=Shipper
```

### Thuộc Tính Phân Quyền

```csharp
// Endpoint Khách hàng/Quản trị
[Authorize(Roles = "User")]
[Authorize(Roles = "Admin")]

// Endpoint Shipper
[Authorize(AuthenticationSchemes = "ShipperAuth", Roles = "Shipper")]

// Endpoint công khai
[AllowAnonymous]
```

### Xác Thực Token

**Cognito Token** (Khách hàng/Quản trị):
- Được xác thực với AWS Cognito JWKS endpoint
- Tự động xác thực chữ ký, hết hạn, nhà phát hành, đối tượng
- Claims được trích xuất từ ID token

**Local JWT** (Shipper):
- Được xác thực với khóa đối xứng (`Jwt:LocalKey`)
- Logic xác thực tùy chỉnh trong `ShipperAuthService`
- Claims: `nameid`, `role`, `exp`

---

## 💳 Tích Hợp Thanh Toán

### Tích Hợp VNPay

#### Luồng Thanh Toán

1. Khách hàng khởi tạo thanh toán
2. Backend tạo URL thanh toán với chữ ký
3. Khách hàng được chuyển hướng đến cổng VNPay
4. Khách hàng hoàn tất thanh toán
5. VNPay chuyển hướng đến URL callback với kết quả thanh toán
6. Backend xác thực chữ ký và cập nhật đơn hàng

#### Tạo Chữ Ký (HMAC-SHA512)

```csharp
var rawData = $"vnp_Amount={amount}&vnp_Command=pay&...";
var signature = HMACSHA512(rawData, HashSecret);
var paymentUrl = $"{VNPayUrl}?{rawData}&vnp_SecureHash={signature}";
```

#### Xác Thực Callback

```csharp
var returnSignature = Request.Query["vnp_SecureHash"];
var calculatedSignature = HMACSHA512(responseData, HashSecret);
if (returnSignature != calculatedSignature) 
    return BadRequest("Chữ ký không hợp lệ");
```

### Tích Hợp MoMo

#### Luồng Thanh Toán

1. Backend tạo yêu cầu thanh toán với chữ ký
2. Backend gửi POST đến MoMo API
3. MoMo trả về URL thanh toán (deeplink/weblink)
4. Khách hàng hoàn tất thanh toán trên ứng dụng/web MoMo
5. MoMo gửi callback đến backend
6. Backend xác thực và cập nhật đơn hàng

#### Tạo Chữ Ký (HMAC-SHA256)

```csharp
var rawData = $"accessKey={AccessKey}&amount={amount}&...";
var signature = HMACSHA256(rawData, SecretKey);
```

#### Xử Lý IPN

```csharp
[HttpPost("ipn")]
public async Task<IActionResult> HandleIPN([FromBody] MoMoIPNRequest request)
{
    // Xác thực chữ ký
    // Cập nhật trạng thái đơn hàng
    // Trả về phản hồi thành công cho MoMo
    return Ok(new { resultCode = 0 });
}
```

---

## 🚀 Triển Khai AWS Elastic Beanstalk

### Tổng Quan

AWS Elastic Beanstalk là dịch vụ PaaS (Platform as a Service) giúp triển khai và quản lý ứng dụng web một cách đơn giản mà không cần lo lắng về infrastructure. Elastic Beanstalk sẽ tự động:

- ✅ Tạo Application Load Balancer (ALB)
- ✅ Quản lý Auto Scaling Group với EC2 instances
- ✅ Cấu hình CloudWatch logs và monitoring
- ✅ Cài đặt .NET 8 runtime
- ✅ Quản lý health checks và rolling updates

### Yêu Cầu Trước Khi Triển Khai

1. **AWS CLI đã cài đặt và cấu hình**
   ```bash
   aws configure
   ```

2. **EB CLI (Elastic Beanstalk CLI)**
   ```bash
   pip install awsebcli --upgrade
   ```

3. **IAM Permissions**
   - Quyền tạo Elastic Beanstalk applications
   - Quyền tạo EC2, ALB, Auto Scaling, CloudWatch
   - Quyền truy cập DynamoDB, S3, Cognito, SES, Location Service

### Bước 1: Chuẩn Bị Project

#### 1.1 Tạo file `aws-windows-deployment-manifest.json`

Tạo file này ở thư mục gốc project:

```json
{
  "manifestVersion": 1,
  "deployments": {
    "aspNetCoreWeb": [
      {
        "name": "coffee-shop-api",
        "parameters": {
          "appBundle": ".",
          "iisPath": "/",
          "iisWebSite": "Default Web Site"
        }
      }
    ]
  }
}
```

#### 1.2 Cập nhật `appsettings.json` cho Production

```json
{
  "Environment": "Production",
  "VNPay": {
    "ReturnUrl": "http://your-eb-url.elasticbeanstalk.com/api/Payment/vnpay/callback"
  },
  "MoMo": {
    "ReturnUrl": "http://your-eb-url.elasticbeanstalk.com/api/MoMoPayment/callback",
    "NotifyUrl": "http://your-eb-url.elasticbeanstalk.com/api/MoMoPayment/ipn"
  }
}
```

#### 1.3 Build Project

```bash
dotnet publish -c Release -o ./publish
```

#### 1.4 Tạo Deployment Package

```bash
cd publish
Compress-Archive -Path * -DestinationPath ../coffee-shop-api.zip
cd ..
```

### Bước 2: Tạo Elastic Beanstalk Application

#### 2.1 Khởi tạo EB trong project

```bash
eb init -p "64bit Windows Server 2022 v3.1.0 running IIS 10.0" -r ap-southeast-1 coffee-shop-api
```

Chọn:
- Platform: `64bit Windows Server 2022 running IIS 10.0`
- Region: `ap-southeast-1` (Singapore) hoặc region phù hợp
- Application name: `coffee-shop-api`

#### 2.2 Tạo Environment

```bash
eb create coffee-shop-prod --instance-type t3.small --envvars \
  AWS__Region=ap-southeast-1,\
  Cognito__UserPoolId=your-pool-id,\
  Cognito__ClientId=your-client-id,\
  Jwt__LocalKey=your-secret-key-32-chars,\
  VNPay__TmnCode=your-tmn-code,\
  VNPay__HashSecret=your-hash-secret,\
  MoMo__PartnerCode=your-partner-code,\
  MoMo__SecretKey=your-secret-key
```

Tham số:
- `--instance-type t3.small`: Loại EC2 instance (có thể dùng t3.micro cho bậc miễn phí)
- `--envvars`: Biến môi trường cho ứng dụng
- Tên môi trường: `coffee-shop-prod`

#### 2.3 Cấu hình IAM Instance Profile

Elastic Beanstalk sẽ tạo IAM role tự động, nhưng cần add permissions:

```bash
# Attach policies to instance role
aws iam attach-role-policy \
  --role-name aws-elasticbeanstalk-ec2-role \
  --policy-arn arn:aws:iam::aws:policy/AmazonDynamoDBFullAccess

aws iam attach-role-policy \
  --role-name aws-elasticbeanstalk-ec2-role \
  --policy-arn arn:aws:iam::aws:policy/AmazonS3FullAccess

aws iam attach-role-policy \
  --role-name aws-elasticbeanstalk-ec2-role \
  --policy-arn arn:aws:iam::aws:policy/AmazonSESFullAccess

aws iam attach-role-policy \
  --role-name aws-elasticbeanstalk-ec2-role \
  --policy-arn arn:aws:iam::aws:policy/AmazonLocationFullAccess

aws iam attach-role-policy \
  --role-name aws-elasticbeanstalk-ec2-role \
  --policy-arn arn:aws:iam::aws:policy/AmazonCognitoPowerUser
```

### Bước 3: Deploy Application

#### 3.1 Deploy lần đầu

```bash
eb deploy coffee-shop-prod --staged
```

#### 3.2 Kiểm tra deployment

```bash
# Check environment status
eb status

# View logs
eb logs

# Open application in browser
eb open
```

### Bước 4: Cấu Hình Environment

#### 4.1 Cấu hình Auto Scaling

```bash
eb scale 2 --timeout 5
```

Hoặc qua AWS Console:
1. Vào Elastic Beanstalk → Environment → Configuration
2. Chọn "Capacity"
3. Cấu hình:
   - Min instances: 1
   - Max instances: 4
   - Instance type: t3.small
   - Scaling triggers: CPUUtilization > 70%

#### 4.2 Cấu hình Load Balancer Health Check

Tạo endpoint health check trong project:

```csharp
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Check()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        });
    }
}
```

Cấu hình trong Elastic Beanstalk:
- Health check path: `/api/Health`
- Healthy threshold: 3
- Unhealthy threshold: 5
- Interval: 30 seconds
- Timeout: 5 seconds

#### 4.3 Cấu hình HTTPS (Optional)

1. Tạo SSL certificate trong AWS Certificate Manager
2. Vào Elastic Beanstalk → Environment → Configuration → Load Balancer
3. Add listener:
   - Port: 443
   - Protocol: HTTPS
   - SSL Certificate: Chọn certificate từ ACM
4. Apply changes

### Bước 5: Cấu Hình CloudWatch Logs

```bash
eb logs --cloudwatch-log-source instance
```

Cấu hình trong AWS Console:
1. Vào Elastic Beanstalk → Environment → Configuration
2. Chọn "Software"
3. Enable:
   - CloudWatch Logs: Enabled
   - Log retention: 7 days
   - Log streaming: Enabled

### Bước 6: Update Application

Mỗi khi có thay đổi code:

```bash
# 1. Build lại
dotnet publish -c Release -o ./publish

# 2. Tạo package mới
cd publish
Compress-Archive -Path * -DestinationPath ../coffee-shop-api.zip -Force
cd ..

# 3. Deploy
eb deploy
```

### Bước 7: Monitoring & Troubleshooting

#### 7.1 Xem logs real-time

```bash
eb logs --stream
```

#### 7.2 SSH vào EC2 instance

```bash
eb ssh
```

#### 7.3 Xem metrics trong CloudWatch

```bash
# CPU usage
aws cloudwatch get-metric-statistics \
  --namespace AWS/ElasticBeanstalk \
  --metric-name CPUUtilization \
  --dimensions Name=EnvironmentName,Value=coffee-shop-prod \
  --start-time 2024-01-01T00:00:00Z \
  --end-time 2024-01-02T00:00:00Z \
  --period 3600 \
  --statistics Average
```

### Bước 8: Rollback Version

Nếu deployment bị lỗi:

```bash
# List versions
eb appversion

# Rollback to previous version
eb deploy --version <version-label>
```

### Bước 9: Clean Up

Khi không sử dụng:

```bash
# Terminate environment
eb terminate coffee-shop-prod

# Delete application
eb terminate --all
```

---

## 🔍 Kiến Trúc Elastic Beanstalk Chi Tiết

### Các Thành Phần

```text
┌───────────────────────────────────────────────────────────────┐
│             Bộ Cân Bằng Tải Ứng Dụng (ALB)                    │
│  • Kiểm tra sức khỏe: /api/Health mỗi 30 giây                 │
│  • Phiên dính: Đã bật                                         │
│  • HTTPS Listener (tùy chọn): Cổng 443                        │
│  • HTTP Listener: Cổng 80                                     │
└────────────────────┬──────────────────────────────────────────┘
                     │
                     ▼
┌───────────────────────────────────────────────────────────────┐
│            Nhóm Tự Động Mở Rộng (Auto Scaling)                │
│  ┌─────────────────────────────────────────────────────┐      │
│  │  EC2 Instance 1 (t3.small)                          │      │
│  │  • Windows Server 2022                              │      │
│  │  • IIS 10.0                                         │      │
│  │  • .NET 8 Runtime                                   │      │ 
│  │  • CloudWatch Agent (nhật ký)                       │      │
│  │  • IAM Instance Profile                             │      │
│  └─────────────────────────────────────────────────────┘      │
│  ┌─────────────────────────────────────────────────────┐      │
│  │  EC2 Instance 2 (t3.small)                          │      │
│  │  • Windows Server 2022                              │      │
│  │  • IIS 10.0                                         │      │
│  │  • .NET 8 Runtime                                   │      │
│  │  • CloudWatch Agent (nhật ký)                       │      │
│  │  • IAM Instance Profile                             │      │
│  └─────────────────────────────────────────────────────┘      │
│                                                               │
│  Chính sách mở rộng:                                          │
│  • Tăng quy mô: CPU > 70% trong 5 phút                        │
│  • Giảm quy mô: CPU < 25% trong 10 phút                       │
│  • Tối thiểu: 1, Tối đa: 4 instances                          │
└───────────────────────────────────────────────────────────────┘
```

### Lợi Ích

✅ **Tự Động Mở Rộng**: Tự động tăng/giảm instances theo lưu lượng truy cập  
✅ **Cân Bằng Tải**: Phân phối lưu lượng đều giữa các instances  
✅ **Giám Sát Sức Khỏe**: Tự động khởi động lại instances không khỏe mạnh  
✅ **Triển Khai Liên Tục**: Triển khai không gián đoạn  
✅ **Hạ Tầng Được Quản Lý**: Không cần quản lý máy chủ  
✅ **Rollback Dễ Dàng**: Quay lại phiên bản trước nhanh chóng  

---

## ⚠️ Xử Lý Lỗi & Khắc Phục Sự Cố

### Các Vấn Đề Thường Gặp

#### 1. DynamoDB Bị Từ Chối Truy Cập

**Lỗi**: `An error occurred (AccessDeniedException) when calling the DescribeTable operation`

**Giải pháp**:
```bash
# Check IAM role permissions
aws iam list-attached-role-policies --role-name aws-elasticbeanstalk-ec2-role

# Attach DynamoDB policy
aws iam attach-role-policy \
  --role-name aws-elasticbeanstalk-ec2-role \
  --policy-arn arn:aws:iam::aws:policy/AmazonDynamoDBFullAccess
```

#### 2. Xác Thực Cognito Token Thất Bại

**Lỗi**: `IDX10205: Issuer validation failed`

**Giải pháp**:
- Xác minh `Cognito:UserPoolId` và `Cognito:ClientId` trong biến môi trường
- Kiểm tra `ValidIssuer` trong `Program.cs` khớp với URL Cognito
- Đảm bảo `Authority` được thiết lập đúng

#### 3. Payment Callback Trả Về 404

**Lỗi**: VNPay/MoMo callback trả về 404

**Giải pháp**:
- Cập nhật URL callback trong `appsettings.json` với URL Elastic Beanstalk
- Cập nhật URL callback trong dashboard VNPay/MoMo
- Đảm bảo `[HttpGet]` hoặc `[HttpPost]` khớp với phương thức payment gateway

#### 4. Lỗi CORS

**Lỗi**: `Access to fetch blocked by CORS policy`

**Giải pháp**:
```csharp
// In Program.cs
options.AddPolicy("AllowAll", policy =>
{
    policy.WithOrigins(
        "http://localhost:3000",
        "https://your-frontend-domain.com",
        "https://your-eb-url.elasticbeanstalk.com"
    )
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
});
```

#### 5. Sức Khỏe Môi Trường Suy Giảm

**Nguyên nhân**: Endpoint kiểm tra sức khỏe thất bại

**Giải pháp**:
- Xác minh endpoint kiểm tra sức khỏe trả về 200 OK
- Kiểm tra nhật ký CloudWatch để tìm lỗi
- Kiểm tra endpoint sức khỏe cục bộ: `curl http://your-url/api/Health`

---

## 📝 Giấy Phép

Dự án này được cấp phép theo Giấy phép MIT.

---

## 👥 Người Đóng Góp

- **Quan Nguyen** - [GitHub](https://github.com/quannguyen-1110)

---

## 📧 Hỗ Trợ

Đối với các vấn đề và câu hỏi:
- Tạo issue trên GitHub
- Email: hminhtam15123@gmail.com

---

## 🎉 Lời Cảm Ơn

- AWS SDK cho .NET
- Đội ngũ ASP.NET Core
- VNPay và MoMo cho tài liệu cổng thanh toán
- Đội ngũ AWS Elastic Beanstalk

---

**Chúc Bạn Lập Trình Vui Vẻ! ☕**
