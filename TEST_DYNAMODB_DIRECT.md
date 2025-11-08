# 🧪 Test DynamoDB Direct - Debug Guide

## Vấn đề hiện tại:

Vẫn lỗi "Unable to locate property for key attribute Id" sau khi đã fix code.

## Nguyên nhân có thể:

1. ❌ App chưa restart với code mới → **ĐÃ FIX** (app đã stop)
2. ❌ DynamoDB attribute name không khớp với code
3. ❌ Có conflict giữa nhiều models

## 🔍 Debug Steps:

### Bước 1: Run app mới

```bash
dotnet run
```

### Bước 2: Test GET drink trước (để verify mapping)

```
GET /api/drink/drink-001
```

**Nếu GET thành công** → Drink mapping OK
**Nếu GET lỗi** → Drink mapping SAI

### Bước 3: Nếu GET thành công, test create order

```
POST /api/order
{
  "items": [{
    "productId": "drink-001",
    "productType": "Drink",
    "quantity": 1,
    "toppingIds": []
  }]
}
```

### Bước 4: Nếu vẫn lỗi, check log

Xem console output khi run `dotnet run` để tìm error message chi tiết

---

## 🎯 Alternative: Test đơn giản hơn

Thay vì debug, hãy tạo lại drinks qua API để đảm bảo format đúng 100%:

### 1. Xóa tất cả drinks trong DynamoDB Console

- Vào DynamoDB Console
- Chọn table "Drinks"
- Xóa hết items

### 2. Tạo lại qua API

```
POST /api/drink
{
  "id": "test-001",
  "name": "Test Coffee",
  "basePrice": 30000,
  "stock": 10,
  "category": "Coffee"
}
```

### 3. Test create order với drink mới

```
POST /api/order
{
  "items": [{
    "productId": "test-001",
    "productType": "Drink",
    "quantity": 1,
    "toppingIds": []
  }]
}
```

---

## 📝 Checklist:

- [ ] Stop app cũ (đã xong)
- [ ] Clean build (đã xong)
- [ ] Run app mới: `dotnet run`
- [ ] Test GET /api/drink/drink-001
- [ ] Nếu GET OK → Test POST /api/order
- [ ] Nếu vẫn lỗi → Xóa drinks cũ, tạo mới qua API

---

## 🚨 Nếu vẫn lỗi sau tất cả:

Có thể là vấn đề với DynamoDB Context caching. Thử:

1. Restart DynamoDB Local (nếu dùng local)
2. Hoặc xóa toàn bộ table "Drinks" và để app tự tạo lại
3. Hoặc check AWS credentials

Hãy run `dotnet run` và test lại!
