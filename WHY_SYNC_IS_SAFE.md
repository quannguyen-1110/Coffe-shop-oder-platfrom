# ?? Gi?i thích: T?i sao ??NG B? 2 b?ng KHÔNG ?NH H??NG data khác?

## ?? C?U TRÚC 2 B?NG

### **ShipperProfile** - 17 fields (B?ng chi ti?t)
```csharp
[DynamoDBTable("ShipperProfiles")]
public class ShipperProfile
{
  // ? CHUNG v?i User table (4 fields)
 public string FullName { get; set; }
    public string Phone { get; set; }
    public string VehicleType { get; set; }
    public string VehiclePlate { get; set; }
    
    // ? RIÊNG (13 fields - KHÔNG sync)
  public string Email { get; set; }
    public string IdCard { get; set; }
    public string BankAccount { get; set; }        // ? B?o m?t
    public string BankName { get; set; }           // ? B?o m?t
 public decimal TotalEarnings { get; set; }     // ? Stats
  public int TotalDeliveries { get; set; }       // ? Stats
    public double Rating { get; set; }// ? Performance
    public int TotalRatings { get; set; }          // ? Performance
 public bool IsActive { get; set; }             // ?? Khác ngh?a v?i User.IsActive
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
}
```

### **CoffeeShopUsers** - 15 fields (B?ng t?ng h?p)
```csharp
[DynamoDBTable("CoffeeShopUsers")]
public class User
{
  // ?? AUTHENTICATION & AUTHORIZATION (5 fields - KHÔNG ??NG)
    public string UserId { get; set; }
    public string Username { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; }      // Khóa/m? tài kho?n
    public string Role { get; set; }        // User, Admin, Shipper
    
    // ?? LOYALTY (3 fields - CH? dành cho User role)
 public int RewardPoints { get; set; }
    public int VoucherCount { get; set; }
    public List<Voucher>? AvailableVouchers { get; set; }
    
    // ? CHUNG v?i ShipperProfile (4 fields - NH?N SYNC)
    public string? FullName { get; set; }     ? sync t? ShipperProfile
    public string? PhoneNumber { get; set; }  ? sync t? ShipperProfile
    public string? VehicleType { get; set; }  ? sync t? ShipperProfile
    public string? LicensePlate { get; set; } ? sync t? ShipperProfile
    
    // ?? REGISTRATION (3 fields - CH? Admin qu?n lý)
    public string RegistrationStatus { get; set; }  // Pending, Approved, Rejected
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
}
```

---

## ? LOGIC ??NG B? TRONG CODE

### 1. **Shipper Update Profile**

```csharp
[HttpPut("profile")]
[Authorize(Roles = "Shipper")]  // ?? CH? Shipper ???c g?i
public async Task UpdateProfile(UpdateProfileRequest request)
{
    var shipperId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
 // 1. Update ShipperProfile (17 fields)
    var profile = await _profileRepo.GetProfileAsync(shipperId);
    profile.FullName = request.FullName;
    profile.Phone = request.Phone;
    profile.VehicleType = request.VehicleType;
    profile.VehiclePlate = request.VehiclePlate;
    profile.BankAccount = request.BankAccount;  // ? CH? có trong Profile
    profile.BankName = request.BankName;        // ? CH? có trong Profile
    await _profileRepo.CreateOrUpdateProfileAsync(profile);
    
    // 2. Sync CoffeeShopUsers (CH? 4 fields CHUNG)
    var user = await _userRepo.GetUserByIdAsync(shipperId);
    if (user != null && user.Role == "Shipper")  // ?? CHECK ROLE
    {
        user.FullName = request.FullName;        // ? Sync
        user.PhoneNumber = request.Phone; // ? Sync
    user.VehicleType = request.VehicleType;  // ? Sync
        user.LicensePlate = request.VehiclePlate;// ? Sync
        
   // ?? KHÔNG ??ng các field khác:
 // user.RewardPoints - GI? NGUYÊN (dành cho User)
        // user.VoucherCount - GI? NGUYÊN (dành cho User)
        // user.RegistrationStatus - GI? NGUYÊN (dành cho Admin)
        // user.IsActive - GI? NGUYÊN (dành cho Admin)
        // user.PasswordHash - GI? NGUYÊN (authentication)
        
      await _userRepo.UpdateUserAsync(user);
    }
}
```

---

## ?? T?I SAO KHÔNG ?NH H??NG?

### **Scenario 1: User v?i Role = "User"**

```json
// TR??C KHI shipper update
CoffeeShopUsers (UserId: "user-001", Role: "User"):
{
  "rewardPoints": 100,
  "voucherCount": 5,
  "availableVouchers": [...],
  "fullName": "Nguy?n V?n A",
  "phoneNumber": "0901234567",
  "vehicleType": null,        ? KHÔNG dùng
  "licensePlate": null   ? KHÔNG dùng
}

// SAU KHI shipper update profile
// ?? KHÔNG ?NH H??NG vì:
// 1. Endpoint yêu c?u [Authorize(Roles = "Shipper")]
// 2. User không th? g?i endpoint này
// 3. Code check: if (user.Role == "Shipper")

CoffeeShopUsers (UserId: "user-001", Role: "User"):
{
  "rewardPoints": 100,        ? GI? NGUYÊN ?
  "voucherCount": 5,          ? GI? NGUYÊN ?
  "availableVouchers": [...], ? GI? NGUYÊN ?
  "fullName": "Nguy?n V?n A", ? GI? NGUYÊN ?
  "phoneNumber": "0901234567",? GI? NGUYÊN ?
  "vehicleType": null,        ? GI? NGUYÊN ?
  "licensePlate": null        ? GI? NGUYÊN ?
}
```

**? K?T LU?N:** Data User HOÀN TOÀN KHÔNG B? ??NG

---

### **Scenario 2: Admin v?i Role = "Admin"**

```json
// Admin KHÔNG TH? g?i /api/shipper/profile
// Vì endpoint yêu c?u [Authorize(Roles = "Shipper")]

CoffeeShopUsers (UserId: "admin-001", Role: "Admin"):
{
  "userId": "admin-001",
  "role": "Admin",
  "fullName": "Admin Nguy?n",
  "vehicleType": null,   ? KHÔNG B? ??NG
  "licensePlate": null ? KHÔNG B? ??NG
}
```

**? K?T LU?N:** Admin data AN TOÀN tuy?t ??i

---

### **Scenario 3: Shipper update ? Ch? ?nh h??ng data SHIPPER**

```json
// TR??C
CoffeeShopUsers (UserId: "shipper-001", Role: "Shipper"):
{
  "fullName": "Shipper A",
  "phoneNumber": "0909999999",
  "vehicleType": "XE MÁY",
"licensePlate": "59A-12345",
  "registrationStatus": "Approved", ? KHÔNG ??NG
  "isActive": true,                 ? KHÔNG ??NG
  "rewardPoints": 0, ? KHÔNG ??NG (không dùng cho Shipper)
  "voucherCount": 0        ? KHÔNG ??NG (không dùng cho Shipper)
}

ShipperProfile (ShipperId: "shipper-001"):
{
  "fullName": "Shipper A",
  "phone": "0909999999",
  "vehicleType": "XE MÁY",
  "vehiclePlate": "59A-12345",
  "bankAccount": "1234567",
  "totalEarnings": 2500000,
  "totalDeliveries": 125,
  "rating": 4.8
}

// SAU KHI update profile
// PUT /api/shipper/profile
// { "fullName": "Shipper A M?I", "phone": "0911111111" }

CoffeeShopUsers (UserId: "shipper-001", Role: "Shipper"):
{
  "fullName": "Shipper A M?I",    ? SYNC ?
  "phoneNumber": "0911111111",    ? SYNC ?
  "vehicleType": "XE MÁY", ? GI? NGUYÊN
  "licensePlate": "59A-12345",? GI? NGUYÊN
  "registrationStatus": "Approved", ? GI? NGUYÊN ? QUAN TR?NG
  "isActive": true,     ? GI? NGUYÊN ? QUAN TR?NG
  "rewardPoints": 0,     ? GI? NGUYÊN ?
  "voucherCount": 0                 ? GI? NGUYÊN ?
}

ShipperProfile (ShipperId: "shipper-001"):
{
"fullName": "Shipper A M?I",      ? UPDATE ?
  "phone": "0911111111",      ? UPDATE ?
  "vehicleType": "XE MÁY",
  "vehiclePlate": "59A-12345",
  "bankAccount": "1234567",     ? GI? NGUYÊN (KHÔNG sync)
  "totalEarnings": 2500000,      ? GI? NGUYÊN (KHÔNG sync)
  "totalDeliveries": 125,      ? GI? NGUYÊN (KHÔNG sync)
  "rating": 4.8      ? GI? NGUYÊN (KHÔNG sync)
}
```

**? K?T LU?N:**
- CH? 4 fields ???c sync
- Các field QUAN TR?NG (RegistrationStatus, IsActive) KHÔNG B? ??NG
- Stats trong ShipperProfile (TotalEarnings, Rating) KHÔNG B? ??NG

---

## ??? B?O V? 3 L?P

### **L?p 1: Authorization**
```csharp
[Authorize(Roles = "Shipper")]  // CH? Shipper ???c g?i
public async Task UpdateProfile()
```
? User và Admin **KHÔNG TH?** g?i endpoint này

---

### **L?p 2: Role Check**
```csharp
var user = await _userRepo.GetUserByIdAsync(shipperId);
if (user != null && user.Role == "Shipper")  // KI?M TRA ROLE
{
  // Ch? sync n?u Role = "Shipper"
}
```
? N?u somehow token b? hack, v?n check Role tr??c khi sync

---

### **L?p 3: Field Selection**
```csharp
// CH? update 4 fields
user.FullName = request.FullName;
user.PhoneNumber = request.Phone;
user.VehicleType = request.VehicleType;
user.LicensePlate = request.VehiclePlate;

// KHÔNG update:
// user.RewardPoints
// user.VoucherCount
// user.RegistrationStatus
// user.IsActive
// user.PasswordHash
```
? Dù có bug, v?n KHÔNG ??NG các field quan tr?ng

---

## ?? SO SÁNH CÁC FIELD

| Field | ShipperProfile | CoffeeShopUsers | Sync? | Lý do |
|-------|----------------|-----------------|-------|-------|
| **FullName** | ? | ? | ? | Chung, c?n hi?n th? |
| **Phone** | ? | ? (PhoneNumber) | ? | Chung, c?n hi?n th? |
| **VehicleType** | ? | ? | ? | Chung, c?n hi?n th? |
| **VehiclePlate** | ? | ? (LicensePlate) | ? | Chung, c?n hi?n th? |
| **BankAccount** | ? | ? | ? | B?o m?t, ch? Shipper c?n |
| **BankName** | ? | ? | ? | B?o m?t, ch? Shipper c?n |
| **TotalEarnings** | ? | ? | ? | Stats, ch? ShipperProfile |
| **TotalDeliveries** | ? | ? | ? | Stats, ch? ShipperProfile |
| **Rating** | ? | ? | ? | Performance, ch? ShipperProfile |
| **IdCard** | ? | ? | ? | KYC, ch? ShipperProfile |
| **Email** | ? | ? | ? | Authentication, không sync |
| **RewardPoints** | ? | ? | ? | Ch? User role |
| **VoucherCount** | ? | ? | ? | Ch? User role |
| **RegistrationStatus** | ? | ? | ? | Ch? Admin qu?n lý |
| **IsActive** | ? | ? | ? | Khác ngh?a, không sync |
| **PasswordHash** | ? | ? | ? | Authentication, không sync |

---

## ?? K?T LU?N

### ? AN TOÀN TUY?T ??I:

1. **User data (Role = "User"):**
   - RewardPoints, VoucherCount ? KHÔNG B? ??NG
   - Authorization ? KHÔNG TH? G?I endpoint

2. **Admin data (Role = "Admin"):**
 - RegistrationStatus, ApprovedBy ? KHÔNG B? ??NG
   - Authorization ? KHÔNG TH? G?I endpoint

3. **Shipper data (Role = "Shipper"):**
   - CH? 4 fields ???c sync (FullName, Phone, VehicleType, Plate)
   - Stats fields (TotalEarnings, Rating) ? KHÔNG sync
   - Banking fields ? KHÔNG sync
   - Authentication fields ? KHÔNG ??NG

---

## ?? T?I SAO THI?T K? NH? V?Y?

### **ShipperProfile** = "Source of Truth" cho Shipper data
- Chi ti?t, ??y ??
- Banking, Stats, Performance
- Update th??ng xuyên

### **CoffeeShopUsers** = "Index Table" cho authentication & authorization
- Lightweight
- Ch? c?n basic info ?? hi?n th?
- Sync 4 fields ?? Admin không ph?i query 2 b?ng m?i l?n view

---

## ?? BEST PRACTICES

? **DO:**
- Sync CH? các field CHUNG và C?N THI?T
- Check Role tr??c khi sync
- Gi? ShipperProfile là source of truth cho stats

? **DON'T:**
- Sync toàn b? fields
- Sync fields authentication (Password, Email)
- Sync fields authorization (Role, RegistrationStatus)
- Sync stats fields (TotalEarnings, Rating)

---

## ?? TÓM T?T

**CÂU TR? L?I:**
> ??ng b? 2 b?ng **HOÀN TOÀN KHÔNG ?NH H??NG** data User và Admin vì:
> 1. **Authorization**: Ch? Shipper g?i ???c endpoint
> 2. **Role Check**: Code ki?m tra Role tr??c khi sync
> 3. **Field Selection**: Ch? sync 4 fields CHUNG
> 4. **Không sync**: RewardPoints, VoucherCount, RegistrationStatus, IsActive, PasswordHash
>
> ? User và Admin data **AN TOÀN TUY?T ??I** ?
