# 🔧 Fix Lỗi 400 Bad Request - "Unable to locate property for key attribute Id"

## 🔍 Điểm sai đã tìm thấy:

### ❌ **Lỗi: Topping model có conflict giữa Table và Nested Object**

**File:** `Models/Topping.cs`

**Vấn đề:**

```csharp
[DynamoDBTable("Toppings")]  // ← Định nghĩa như TABLE độc lập
public class Topping
{
    [DynamoDBHashKey]  // ← Có HashKey
    public string Id { get; set; }
    ...
}
```

**Nhưng:**

```csharp
// OrderItem.cs
public class OrderItem
{
    public List<Topping>? Toppings { get; set; }  // ← Dùng như NESTED object
}
```

**Tại sao lỗi:**

- DynamoDB **KHÔNG THỂ** serialize nested object có `[DynamoDBHashKey]`
- `[DynamoDBHashKey]` chỉ dùng cho table độc lập
- Khi save Order → DynamoDB cố tìm HashKey trong nested Topping → lỗi!

---

## ✅ Giải pháp:

### Tạo 2 class riêng biệt:

**1. `Topping` - Table độc lập (giữ nguyên)**

```csharp
[DynamoDBTable("Toppings")]
public class Topping
{
    [DynamoDBHashKey]  // ✅ OK vì là table độc lập
    public string Id { get; set; }

    [DynamoDBProperty("Name")]
    public string Name { get; set; }

    [DynamoDBProperty("Price")]
    public decimal Price { get; set; }

    [DynamoDBProperty("Stock")]
    public int Stock { get; set; }
}
```

**2. `OrderTopping` - Nested object (MỚI)**

```csharp
// KHÔNG có [DynamoDBTable]
// KHÔNG có [DynamoDBHashKey]
public class OrderTopping
{
    [DynamoDBProperty]  // ✅ Chỉ có DynamoDBProperty
    public string Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; }

    [DynamoDBProperty]
    public decimal Price { get; set; }
}
```

**3. Update OrderItem**

```csharp
public class OrderItem
{
    // Đổi từ List<Topping> → List<OrderTopping>
    public List<OrderTopping>? Toppings { get; set; }
}
```

---

## 📝 Files đã fix:

### ✅ `Models/OrderTopping.cs` (MỚI)

```csharp
using Amazon.DynamoDBv2.DataModel;

namespace CoffeeShopAPI.Models
{
    // Nested object trong OrderItem, KHÔNG phải table độc lập
    public class OrderTopping
    {
        [DynamoDBProperty]
        public string Id { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Name { get; set; } = string.Empty;

        [DynamoDBProperty]
        public decimal Price { get; set; }
    }
}
```

### ✅ `Models/OrderItem.cs` (CẬP NHẬT)

```diff
- public List<Topping>? Toppings { get; set; } = new();
+ public List<OrderTopping>? Toppings { get; set; } = new();
```

### ✅ `Controllers/OrderController.cs` (CẬP NHẬT)

```diff
- Toppings = itemReq.ToppingIds?.Select(id => new Topping { Id = id }).ToList()
+ Toppings = itemReq.ToppingIds?.Select(id => new OrderTopping { Id = id }).ToList()
```

---

## 🎯 Quy tắc DynamoDB Models:

### ✅ **Table độc lập:**

```csharp
[DynamoDBTable("TableName")]
public class MyTable
{
    [DynamoDBHashKey]  // ✅ BẮT BUỘC
    public string Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; }
}
```

### ✅ **Nested object:**

```csharp
// KHÔNG có [DynamoDBTable]
// KHÔNG có [DynamoDBHashKey]
public class MyNestedObject
{
    [DynamoDBProperty]  // ✅ CHỈ có DynamoDBProperty
    public string Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; }
}
```

### ❌ **SAI - Nested object có HashKey:**

```csharp
public class MyNestedObject
{
    [DynamoDBHashKey]  // ❌ LỖI!
    public string Id { get; set; }
}
```

---

## 🚀 Test lại:

### 1. Stop app hiện tại (nếu đang chạy)

Trong terminal, nhấn `Ctrl+C`

### 2. Rebuild

```bash
dotnet build
```

### 3. Run lại

```bash
dotnet run
```

### 4. Test create order

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

**Expected:** ✅ 200 OK

### 5. Test với topping

```json
POST /api/order
{
  "items": [
    {
      "productId": "drink-001",
      "productType": "Drink",
      "quantity": 1,
      "toppingIds": ["topping-001"]
    }
  ]
}
```

**Expected:** ✅ 200 OK (sau khi đã tạo topping trong DynamoDB)

---

## 📊 Tóm tắt:

| Model          | Type   | [DynamoDBTable] | [DynamoDBHashKey] | Dùng cho           |
| -------------- | ------ | --------------- | ----------------- | ------------------ |
| `Topping`      | Table  | ✅ Có           | ✅ Có             | Table độc lập      |
| `OrderTopping` | Nested | ❌ Không        | ❌ Không          | Nested trong Order |
| `Order`        | Table  | ✅ Có           | ✅ Có             | Table độc lập      |
| `OrderItem`    | Nested | ❌ Không        | ❌ Không          | Nested trong Order |

---

## ✅ Checklist

- [x] Tạo `OrderTopping` class mới
- [x] Update `OrderItem.Toppings` từ `List<Topping>` → `List<OrderTopping>`
- [x] Update `OrderController` để dùng `OrderTopping`
- [x] Giữ nguyên `Topping` table độc lập
- [ ] Stop app
- [ ] Rebuild
- [ ] Run lại
- [ ] Test create order

Xong! 🎉
