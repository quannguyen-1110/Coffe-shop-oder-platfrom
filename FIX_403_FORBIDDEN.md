# 🔧 Fix Lỗi 403 Forbidden khi Create Order

## 🔍 Các điểm sai đã tìm thấy:

### ❌ **Lỗi 1: Conflict Authorization Attributes**

**File:** `Controllers/OrderController.cs`
**Dòng 15:**

```csharp
[Authorize(Roles = "Admin,User")]  // ← Class level
public class OrderController : ControllerBase
```

**Dòng 62:**

```csharp
[Authorize(Roles = "User")]  // ← Method level
public async Task<IActionResult> CreateOrder(...)
```

**Vấn đề:**

- Có 2 `[Authorize]` attributes chồng lên nhau
- ASP.NET Core sẽ yêu cầu thỏa mãn CẢ HAI điều kiện
- Gây confusion và có thể reject request

**✅ Đã fix:** Xóa `[Authorize]` ở class level, chỉ giữ ở method level

---

### ❌ **Lỗi 2: Claim Mapping không đúng**

**Dòng 67-68:**

```csharp
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
             ?? User.FindFirstValue("sub");
```

**Vấn đề:**

- Cognito JWT token có claim name khác với ASP.NET Core mặc định
- `ClaimTypes.NameIdentifier` có thể không map với Cognito claims
- Thứ tự tìm kiếm không tối ưu (nên tìm "sub" trước)

**✅ Đã fix:**

```csharp
var userId = User.FindFirstValue("sub")  // Cognito sub claim (ưu tiên)
             ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
             ?? User.FindFirstValue("cognito:username")
             ?? User.Identity?.Name;
```

---

### ❌ **Lỗi 3: Role "User" có thể không tồn tại**

**Dòng 62:**

```csharp
[Authorize(Roles = "User")]
```

**Vấn đề:**

- Khi register, bạn có thể đã dùng role khác (Customer, customer, user)
- Cognito case-sensitive với custom attributes
- Nếu role không khớp → 403 Forbidden

**✅ Đã fix:** Đổi thành `[Authorize]` (không yêu cầu role cụ thể)

---

### ❌ **Lỗi 4: Không có error message rõ ràng**

**Vấn đề:** Khi không lấy được userId, chỉ trả về "Cannot identify user" → khó debug

**✅ Đã fix:** Thêm debug info:

```csharp
return Unauthorized(new
{
    error = "Cannot identify user from token",
    availableClaims = claims,  // Show tất cả claims trong token
    hint = "Make sure you're using the ID token, not access token"
});
```

---

## 🎯 Cách test lại:

### Bước 1: Kiểm tra token đang dùng

Trong Swagger, khi login thành công, bạn nhận được:

```json
{
  "access_token": "...",
  "id_token": "...",      ← DÙNG CÁI NÀY
  "refresh_token": "..."
}
```

⚠️ **QUAN TRỌNG:** Phải dùng **`id_token`**, KHÔNG phải `access_token`!

### Bước 2: Authorize trong Swagger

1. Click nút "Authorize" 🔓
2. Paste **id_token** (không cần gõ "Bearer")
3. Click "Authorize"

### Bước 3: Test create order

```json
POST /api/order
{
  "items": [
    {
      "productId": "drink-001",
      "productType": "Drink",
      "quantity": 1,
      "toppingIds": []
    }
  ]
}
```

### Bước 4: Nếu vẫn lỗi 403

Response sẽ show tất cả claims:

```json
{
  "error": "Cannot identify user from token",
  "availableClaims": [
    { "type": "sub", "value": "abc-123-def" },
    { "type": "cognito:username", "value": "user@test.com" },
    { "type": "custom:role", "value": "User" }
  ],
  "hint": "Make sure you're using the ID token, not access token"
}
```

Gửi cho tôi phần `availableClaims` để tôi fix tiếp!

---

## 🔧 Các fix đã áp dụng:

### 1. OrderController.cs

```diff
- [Authorize(Roles = "Admin,User")]  // Xóa class-level authorize
  public class OrderController : ControllerBase

- [Authorize(Roles = "User")]  // Đổi thành chỉ cần authenticated
+ [Authorize]
  public async Task<IActionResult> CreateOrder(...)

- var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
-              ?? User.FindFirstValue("sub");
+ var userId = User.FindFirstValue("sub")  // Cognito sub claim
+              ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
+              ?? User.FindFirstValue("cognito:username")
+              ?? User.Identity?.Name;

+ // Debug: show all claims nếu không tìm thấy userId
+ var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
+ return Unauthorized(new { error = "...", availableClaims = claims });
```

### 2. Phân quyền rõ ràng hơn

```csharp
[HttpGet]
[Authorize(Roles = "Admin")]  // Chỉ Admin xem tất cả orders
public async Task<IActionResult> GetAllOrders()

[HttpGet("{id}")]
[Authorize(Roles = "Admin,User")]  // Admin hoặc User xem order cụ thể
public async Task<IActionResult> GetOrderById(string id)

[HttpPost]
[Authorize]  // Bất kỳ ai đã login đều tạo order được
public async Task<IActionResult> CreateOrder(...)

[HttpPut("{id}/status")]
[Authorize(Roles = "Admin")]  // Chỉ Admin update status
public async Task<IActionResult> UpdateOrderStatus(...)
```

---

## ✅ Checklist

- [x] Xóa conflict authorization attributes
- [x] Fix claim mapping cho Cognito
- [x] Đổi từ role-based sang authenticated-based cho create order
- [x] Thêm debug info khi không tìm thấy userId
- [x] Phân quyền rõ ràng cho từng endpoint

---

## 🚀 Test ngay:

1. **Rebuild project:**

```bash
dotnet build
```

2. **Run:**

```bash
dotnet run
```

3. **Login và lấy id_token**

4. **Authorize với id_token**

5. **Test create order**

Nếu vẫn lỗi, gửi cho tôi response với `availableClaims`! 🔍
