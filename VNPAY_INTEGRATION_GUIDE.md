# 💳 Hướng dẫn tích hợp VNPay

## ✅ Đã tích hợp xong!

### 📦 **Files đã tạo:**

1. `Services/VNPayService.cs` - Service xử lý VNPay
2. `Controllers/PaymentController.cs` - API endpoints thanh toán
3. `Models/VNPayModels.cs` - Models cho VNPay
4. Cập nhật `appsettings.json` - Config VNPay
5. Cập nhật `Program.cs` - Register VNPayService

---

## 🔧 **Bước 1: Cấu hình VNPay Sandbox**

### **1.1. Đăng ký tài khoản VNPay Sandbox:**

- Truy cập: https://sandbox.vnpayment.vn/devreg/
- Đăng ký tài khoản merchant test
- Sau khi đăng ký, bạn sẽ nhận được:
  - **TmnCode**: Mã website (ví dụ: `DEMOSHOP`)
  - **HashSecret**: Secret key để mã hóa

### **1.2. Cập nhật appsettings.json:**

```json
{
  "VNPay": {
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "TmnCode": "DEMOSHOP", // ← Thay bằng TmnCode của bạn
    "HashSecret": "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456", // ← Thay bằng HashSecret của bạn
    "ReturnUrl": "http://localhost:5144/api/payment/vnpay/callback"
  }
}
```

---

## 🚀 **Bước 2: Test Payment Flow**

### **2.1. Tạo Order:**

```
POST /api/order
Authorization: Bearer {user_token}
```

**Body:**

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

**Response:**

```json
{
  "message": "Order created successfully",
  "order": {
    "orderId": "abc-123-def", // ← Copy orderId này
    "totalPrice": 70000,
    "finalPrice": 70000,
    "status": "Pending"
  }
}
```

---

### **2.2. Tạo Payment URL:**

```
POST /api/payment/vnpay/create
Authorization: Bearer {user_token}
```

**Body:**

```json
{
  "orderId": "abc-123-def",
  "returnUrl": "http://localhost:5144/api/payment/vnpay/callback"
}
```

**Response:**

```json
{
  "success": true,
  "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=7000000&vnp_Command=pay&...",
  "message": "Tạo URL thanh toán thành công"
}
```

---

### **2.3. Thanh toán:**

1. Copy `paymentUrl` từ response
2. Mở URL trong browser
3. Trang VNPay sandbox sẽ hiện ra
4. Nhập thông tin test:

**Thẻ test VNPay Sandbox:**

```
Ngân hàng: NCB
Số thẻ: 9704198526191432198
Tên chủ thẻ: NGUYEN VAN A
Ngày phát hành: 07/15
Mật khẩu OTP: 123456
```

5. Xác nhận thanh toán
6. VNPay sẽ redirect về `returnUrl`

---

### **2.4. Kiểm tra trạng thái:**

```
GET /api/payment/status/{orderId}
Authorization: Bearer {user_token}
```

**Response:**

```json
{
  "orderId": "abc-123-def",
  "status": "Processing", // Đã thanh toán
  "totalPrice": 70000,
  "finalPrice": 70000,
  "isPaid": true,
  "message": "Đã thanh toán, đang xử lý"
}
```

---

## 📋 **API Endpoints**

### **1. POST /api/payment/vnpay/create**

Tạo URL thanh toán VNPay

**Request:**

```json
{
  "orderId": "order-id",
  "returnUrl": "http://localhost:5144/api/payment/vnpay/callback"
}
```

**Response:**

```json
{
  "success": true,
  "paymentUrl": "https://sandbox.vnpayment.vn/...",
  "message": "Tạo URL thanh toán thành công"
}
```

---

### **2. GET /api/payment/vnpay/callback**

Callback từ VNPay sau khi thanh toán (tự động)

**Query params:** VNPay tự động gửi

**Response:** Redirect về trang success/failed

---

### **3. GET /api/payment/vnpay/ipn**

IPN (Instant Payment Notification) từ VNPay

**Response:**

```json
{
  "RspCode": "00",
  "Message": "Confirm Success"
}
```

---

### **4. GET /api/payment/status/{orderId}**

Kiểm tra trạng thái thanh toán

**Response:**

```json
{
  "orderId": "abc-123",
  "status": "Processing",
  "isPaid": true,
  "message": "Đã thanh toán, đang xử lý"
}
```

---

## 🔄 **Payment Flow**

```
1. User tạo Order
   ↓
   Status: "Pending"

2. User tạo Payment URL
   ↓
   Nhận paymentUrl

3. User mở paymentUrl
   ↓
   Trang VNPay hiện ra

4. User nhập thông tin thẻ
   ↓
   VNPay xử lý thanh toán

5. VNPay redirect về callback
   ↓
   System cập nhật Order status → "Processing"

6. Admin complete order
   ↓
   Status: "Completed"
   User nhận điểm + Stock giảm
```

---

## 🎯 **Order Status Flow**

| Status         | Ý nghĩa                   | Ai có thể thay đổi          |
| -------------- | ------------------------- | --------------------------- |
| **Pending**    | Chờ thanh toán            | System (khi tạo order)      |
| **Processing** | Đã thanh toán, đang xử lý | System (sau khi thanh toán) |
| **Completed**  | Hoàn thành                | Admin                       |
| **Cancelled**  | Đã hủy                    | Admin                       |

---

## 🧪 **Test Cases**

### **Test 1: Thanh toán thành công**

1. Tạo order → orderId
2. Tạo payment URL
3. Thanh toán với thẻ test
4. Check status → "Processing" ✅

### **Test 2: User hủy thanh toán**

1. Tạo order → orderId
2. Tạo payment URL
3. Click "Hủy giao dịch" trên VNPay
4. Check status → "Pending" (không đổi)

### **Test 3: Thanh toán với voucher**

1. Tạo order → orderId
2. Apply voucher → FinalPrice giảm
3. Tạo payment URL (amount = FinalPrice)
4. Thanh toán
5. Check status → "Processing" ✅

---

## 🔐 **Security**

### **Đã implement:**

- ✅ HMAC SHA512 signature validation
- ✅ Kiểm tra chữ ký từ VNPay
- ✅ Validate order status trước khi thanh toán
- ✅ Chỉ cập nhật order khi signature hợp lệ

### **Best practices:**

- Không lưu thông tin thẻ
- Validate tất cả params từ VNPay
- Log tất cả transactions
- Kiểm tra amount khớp với order

---

## 📝 **VNPay Response Codes**

| Code | Ý nghĩa                                 |
| ---- | --------------------------------------- |
| 00   | Giao dịch thành công                    |
| 07   | Trừ tiền thành công (nghi ngờ gian lận) |
| 09   | Thẻ chưa đăng ký Internet Banking       |
| 10   | Xác thực sai quá 3 lần                  |
| 11   | Hết hạn chờ thanh toán                  |
| 12   | Thẻ bị khóa                             |
| 13   | Sai OTP                                 |
| 24   | Khách hàng hủy giao dịch                |
| 51   | Tài khoản không đủ tiền                 |
| 65   | Vượt hạn mức giao dịch                  |
| 75   | Ngân hàng bảo trì                       |

---

## 🎨 **Customize Return URL**

### **Option 1: Redirect về frontend:**

```json
{
  "orderId": "abc-123",
  "returnUrl": "https://your-frontend.com/payment-result"
}
```

### **Option 2: Xử lý trong backend:**

Giữ nguyên returnUrl mặc định, VNPay sẽ callback về API

---

## 🚨 **Troubleshooting**

### **Lỗi: "Invalid signature"**

- Check HashSecret trong appsettings.json
- Đảm bảo HashSecret khớp với VNPay merchant

### **Lỗi: "Order not found"**

- Check orderId có đúng không
- Check order đã tồn tại trong database

### **Lỗi: "Order must be in Pending status"**

- Order đã thanh toán rồi
- Không thể thanh toán lại

### **VNPay không redirect về callback:**

- Check returnUrl có đúng không
- Check firewall/network
- Check VNPay sandbox có hoạt động không

---

## 📚 **Tài liệu tham khảo**

- VNPay Sandbox: https://sandbox.vnpayment.vn/
- VNPay API Docs: https://sandbox.vnpayment.vn/apis/docs/
- Test Cards: https://sandbox.vnpayment.vn/apis/vnpay-demo/

---

## ✅ **Checklist**

- [ ] Đăng ký VNPay Sandbox
- [ ] Cập nhật TmnCode và HashSecret
- [ ] Test tạo order
- [ ] Test tạo payment URL
- [ ] Test thanh toán với thẻ test
- [ ] Test callback
- [ ] Test check payment status
- [ ] Test với voucher
- [ ] Test user hủy thanh toán

Xong! 🎉
