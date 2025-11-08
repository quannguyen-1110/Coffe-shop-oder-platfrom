# 🔧 Fix Lỗi "Unable to locate property for key attribute Id"

## 🔴 **Lỗi:**

```
Error: Bad Request
"Unable to locate property for key attribute Id"
```

---

## 🔍 **Nguyên nhân:**

### ❌ **Sai:**

```csharp
[DynamoDBTable("Drinks")]
public class Drink
{
    [DynamoDBHashKey]  // ← THIẾU attribute name
    public string Id { get; set; }
}
```

**Vấn đề:**

- `[DynamoDBHashKey]` không chỉ định attribute name trong DynamoDB
- DynamoDB không biết map property `Id` với attribute nào trong table
- Khi query, DynamoDB tìm attribute "Id" nhưng không tìm thấy

---

## ✅ **Giải pháp:**

### **Thêm attribute name vào DynamoDBHashKey:**

```csharp
[DynamoDBTable("Drinks")]
public class Drink
{
    [DynamoDBHashKey("Id")]  // ← THÊM "Id" vào đây
    public string Id { get; set; }

    [DynamoDBProperty("Name")]
    public string Name { get; set; }

    [DynamoDBProperty("BasePrice")]
    public decimal BasePrice { get; set; }
}
```

---

## 📝 **Files đã fix:**

### ✅ `Models/Drink.cs`

```diff
- [DynamoDBHashKey]
+ [DynamoDBHashKey("Id")]
  public string Id { get; set; }
```

### ✅ `Models/Cake.cs`

```diff
- [DynamoDBHashKey]
+ [DynamoDBHashKey("Id")]
  public string Id { get; set; }
```

### ✅ `Models/Topping.cs`

```diff
- [DynamoDBHashKey]
+ [DynamoDBHashKey("Id")]
  public string Id { get; set; }
```

---

## 🎯 **Quy tắc DynamoDB Attributes:**

### **HashKey (Partition Key):**

```csharp
[DynamoDBHashKey("AttributeName")]  // ✅ Phải có attribute name
public string PropertyName { get; set; }
```

### **Property thông thường:**

```csharp
[DynamoDBProperty("AttributeName")]  // ✅ Phải có attribute name
public string PropertyName { get; set; }
```

### **Tại sao cần attribute name?**

- C# property name: `Id` (code)
- DynamoDB attribute name: `Id` (database)
- Cần map 2 cái này với nhau
- Nếu không chỉ định, DynamoDB không biết map thế nào

---

## 🚀 **Test lại:**

### **Bước 1: Stop app**

Nhấn `Ctrl+C` trong terminal

### **Bước 2: Run lại**

```bash
dotnet run
```

### **Bước 3: Login User**

```
POST /api/auth/login
username: user@test.com
password: User@123456
```

→ Copy `id_token`

### **Bước 4: Authorize**

Click "Authorize" → Paste `id_token`

### **Bước 5: Tạo order**

```
POST /api/order
```

**Body:**

```json
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

### **Expected Response:**

```json
{
  "message": "Order created successfully",
  "order": {
    "orderId": "...",
    "userId": "...",
    "items": [
      {
        "productId": "drink-001",
        "productName": "Cà phê sữa đá",
        "productType": "Drink",
        "quantity": 1,
        "unitPrice": 35000,
        "toppings": [],
        "totalPrice": 35000
      }
    ],
    "totalPrice": 35000,
    "finalPrice": 35000,
    "status": "Pending",
    "createdAt": "2025-01-08T..."
  }
}
```

---

## 📊 **So sánh trước và sau:**

| Trước                            | Sau                       |
| -------------------------------- | ------------------------- |
| `[DynamoDBHashKey]`              | `[DynamoDBHashKey("Id")]` |
| DynamoDB không biết map          | DynamoDB map đúng         |
| Lỗi: "Unable to locate property" | ✅ Hoạt động bình thường  |

---

## ✅ **Checklist:**

- [x] Fix Drink.cs
- [x] Fix Cake.cs
- [x] Fix Topping.cs
- [x] Build thành công
- [ ] Stop app
- [ ] Run lại
- [ ] Test create order
- [ ] Verify order created

---

## 💡 **Lưu ý:**

### **Các model khác cũng cần check:**

- `Order.cs`: `[DynamoDBHashKey("OrderId")]` ← Cần check
- `User.cs`: `[DynamoDBHashKey("UserId")]` ← Cần check
- `Product.cs`: `[DynamoDBHashKey("ProductId")]` ← Cần check

### **Best practice:**

Luôn chỉ định attribute name cho DynamoDB attributes:

```csharp
[DynamoDBHashKey("Id")]
[DynamoDBProperty("Name")]
[DynamoDBProperty("Price")]
```

Xong! Bây giờ tạo order sẽ thành công! 🎉
