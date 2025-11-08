# 🚀 Quick Setup DynamoDB Data

## Cách thêm data vào DynamoDB

### Option 1: Qua AWS Console
1. Vào AWS Console → DynamoDB → Tables
2. Chọn table (Drinks/Cakes/Toppings)
3. Click "Explore table items" → "Create item"
4. Paste JSON dưới đây

### Option 2: Tạo API endpoints (khuyến nghị)

Tạo file `Controllers/DrinkController.cs`:

```csharp
using CoffeeShopAPI.Models;
using CoffeeShopAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrinkController : ControllerBase
    {
        private readonly DrinkRepository _drinkRepo;

        public DrinkController(DrinkRepository drinkRepo)
        {
            _drinkRepo = drinkRepo;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDrink([FromBody] Drink drink)
        {
            await _drinkRepo.AddDrinkAsync(drink);
            return Ok(new { message = "Drink created", drink });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllDrinks()
        {
            var drinks = await _drinkRepo.GetAllDrinksAsync();
            return Ok(drinks);
        }
    }
}
```

Thêm methods vào `Repository/DrinkRepository.cs`:

```csharp
public async Task AddDrinkAsync(Drink drink)
{
    await _context.SaveAsync(drink);
}

public async Task<List<Drink>> GetAllDrinksAsync()
{
    return await _context.ScanAsync<Drink>(new List<ScanCondition>()).GetRemainingAsync();
}
```

Làm tương tự cho Cake và Topping.

---

## 📦 Sample Data để Insert

### Drinks
```json
{
  "Id": "drink-001",
  "Name": "Cà phê sữa đá",
  "BasePrice": 35000,
  "Stock": 100,
  "Category": "Coffee"
}
```

```json
{
  "Id": "drink-002",
  "Name": "Trà sữa trân châu",
  "BasePrice": 45000,
  "Stock": 80,
  "Category": "Tea"
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

```json
{
  "Id": "drink-004",
  "Name": "Cà phê đen",
  "BasePrice": 30000,
  "Stock": 120,
  "Category": "Coffee"
}
```

```json
{
  "Id": "drink-005",
  "Name": "Trà đào cam sả",
  "BasePrice": 42000,
  "Stock": 60,
  "Category": "Tea"
}
```

### Cakes
```json
{
  "Id": "cake-001",
  "Name": "Bánh tiramisu",
  "Price": 55000,
  "Stock": 30
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

```json
{
  "Id": "cake-003",
  "Name": "Bánh red velvet",
  "Price": 52000,
  "Stock": 20
}
```

```json
{
  "Id": "cake-004",
  "Name": "Bánh mousse chocolate",
  "Price": 58000,
  "Stock": 15
}
```

### Toppings
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

```json
{
  "Id": "topping-004",
  "Name": "Trân châu trắng",
  "Price": 5000,
  "Stock": 180
}
```

```json
{
  "Id": "topping-005",
  "Name": "Pudding",
  "Price": 8000,
  "Stock": 120
}
```

---

## 🎯 Test Order với data này

### Order 1: Simple
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
**Expected:** 35,000 × 2 = **70,000 VNĐ**

### Order 2: With Toppings
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
**Expected:** (45,000 + 5,000 + 10,000) × 1 = **60,000 VNĐ**

### Order 3: Mixed (Drink + Cake)
```json
{
  "items": [
    {
      "productId": "drink-004",
      "productType": "Drink",
      "quantity": 2,
      "toppingIds": ["topping-002"]
    },
    {
      "productId": "cake-001",
      "productType": "Cake",
      "quantity": 1,
      "toppingIds": []
    }
  ]
}
```
**Expected:** 
- Drink: (30,000 + 5,000) × 2 = 70,000
- Cake: 55,000 × 1 = 55,000
- **Total: 125,000 VNĐ**

### Order 4: Large Order (để test loyalty points)
```json
{
  "items": [
    {
      "productId": "drink-001",
      "productType": "Drink",
      "quantity": 5,
      "toppingIds": ["topping-001", "topping-003"]
    },
    {
      "productId": "cake-002",
      "productType": "Cake",
      "quantity": 3,
      "toppingIds": []
    },
    {
      "productId": "drink-005",
      "productType": "Drink",
      "quantity": 2,
      "toppingIds": ["topping-005"]
    }
  ]
}
```
**Expected:**
- Item 1: (35,000 + 5,000 + 10,000) × 5 = 250,000
- Item 2: 50,000 × 3 = 150,000
- Item 3: (42,000 + 8,000) × 2 = 100,000
- **Total: 500,000 VNĐ**
- **Points earned: 50 điểm** (500,000 / 10,000)

---

## ✅ Checklist Setup

- [ ] Tạo Admin account
- [ ] Tạo User account
- [ ] Insert Drinks vào DynamoDB
- [ ] Insert Cakes vào DynamoDB
- [ ] Insert Toppings vào DynamoDB
- [ ] Test create order
- [ ] Test apply voucher
- [ ] Test complete order (check stock giảm)
- [ ] Test loyalty points

Xong! 🎉
