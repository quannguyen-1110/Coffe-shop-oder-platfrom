# ☕ Coffee Shop Order Platform - API Backend

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)
[![AWS](https://img.shields.io/badge/AWS-Cloud-FF9900)](https://aws.amazon.com/)
[![DynamoDB](https://img.shields.io/badge/Database-DynamoDB-4053D6)](https://aws.amazon.com/dynamodb/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A comprehensive order and delivery management system for coffee shops, built with .NET 8.0, integrated with AWS Services, and supporting electronic payments.

---

## 📋 Table of Contents

- [Overview](#-overview)
- [System Architecture](#-system-architecture)
- [Key Features](#-key-features)
- [Technology Stack](#-technology-stack)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [API Endpoints](#-api-endpoints)
- [Workflow](#-workflow)
- [Database Schema](#-database-schema)
- [Authentication & Authorization](#-authentication--authorization)
- [Payment Integration](#-payment-integration)
- [Deployment](#-deployment)
- [Troubleshooting](#-troubleshooting)

---

## 🎯 Overview

**Coffee Shop Order Platform** is a comprehensive order management and delivery system for coffee shops that enables:

- 👥 **Customers** to order online, pay via e-wallets, and receive vouchers
- 🛵 **Shippers** to accept orders, deliver products, and manage earnings
- 👨‍💼 **Admins** to manage products, orders, and approve shippers

### Key Highlights

- ✅ **Hybrid Authentication**: AWS Cognito (Customer/Admin) + Local JWT (Shipper)
- ✅ **Real-time Distance Calculation**: AWS Location Service with intelligent fallback
- ✅ **Dual Payment Gateway**: VNPay and MoMo integration
- ✅ **Loyalty Program**: Reward points and voucher system
- ✅ **Email Notifications**: AWS SES for automated notifications
- ✅ **Serverless Database**: DynamoDB for high scalability
- ✅ **Image Upload**: AWS S3 for product images

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                Frontend (React/Web)                          │
│         localhost:3000 / AWS Amplify Deploy                  │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTPS/REST API
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              ASP.NET Core 8.0 Web API                        │
│                   (Program.cs)                               │
├─────────────────────────────────────────────────────────────┤
│  Controllers Layer                                           │
│  • AuthController           • OrderController                │
│  • ShipperAuthController    • AdminController                │
│  • PaymentController        • ProductController              │
│  • DrinkController          • CakeController                 │
│  • ToppingController        • LoyaltyController              │
│  • NotificationController   • CustomerController             │
│  • ShipperController        • DashboardController            │
│  • InventoryController      • ImageController                │
├─────────────────────────────────────────────────────────────┤
│  Services Layer                                              │
│  • AuthService              • ShipperAuthService             │
│  • OrderService             • OrderItemService               │
│  • ShippingService          • LoyaltyService                 │
│  • VNPayService             • MoMoService                    │
│  • EmailService             • NotificationService            │
│  • S3Service                                                 │
├─────────────────────────────────────────────────────────────┤
│  Repository Layer                                            │
│  • UserRepository           • OrderRepository                │
│  • ProductRepository        • DrinkRepository                │
│  • CakeRepository           • ToppingRepository              │
│  • VoucherRepository        • NotificationRepository         │
│  • ShipperProfileRepository                                  │
│  • ShipperDeliveryHistoryRepository                          │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    AWS Services                              │
├─────────────────────────────────────────────────────────────┤
│  • DynamoDB           - NoSQL Database                       │
│  • Cognito            - Authentication (Customer/Admin)         │
│  • SES                - Email Notifications                  │
│  • S3                 - Image storage                        │
│  • Location Service   - Geocoding & Distance Calculation     │
│  • SNS                - Push Notifications                   │
│  • Amplify            - Frontend Hosting                     │
└─────────────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              External Services                               │
├─────────────────────────────────────────────────────────────┤
│  • VNPay              - Payment Gateway (ATM/Credit cards)   │
│  • MoMo               - E-Wallet Payment                     │
└─────────────────────────────────────────────────────────────┘
```

---

## ✨ Key Features

### 🔐 Authentication & Authorization

#### Hybrid Auth System
- **AWS Cognito**: For Customer and Admin users
  - Email verification workflow
  - Password management and reset
  - Token refresh mechanism
  
- **Local JWT**: Dedicated for Shipper users
  - BCrypt password hashing
  - Custom JWT token generation
  - Role-based access control

#### User Roles & Permissions

| Role     | Authentication | Capabilities                                         |
|----------|----------------|------------------------------------------------------|
| Customer | AWS Cognito    | Order placement, history view, voucher redemption    |
| Admin    | AWS Cognito    | Product management, order approval, shipper mgmt     |
| Shipper  | Local JWT      | Order acceptance, delivery, earnings tracking        |

### 📦 Order Management

#### Order Status Flow
```
Pending → Processing → Confirmed → Shipping → Delivered → Completed
                  ↓
              Cancelled (cancellable at Pending/Processing/Confirmed)
```

#### Features
- ✅ Multi-item orders (Drinks/Cakes) with toppings
- ✅ Automatic voucher discount application
- ✅ Real distance-based shipping fee calculation
- ✅ Duplicate order prevention with `clientOrderId`
- ✅ Detailed order history with statistics
- ✅ Admin confirmation → Shipper assignment → Delivery completion

### 💰 Payment Integration

#### Supported Payment Methods

1. **VNPay**
   - ATM/Visa/Mastercard payment
   - Sandbox mode for testing
   - HMAC-SHA512 signature validation
   - IPN (Instant Payment Notification) support
   - Secure callback handling

2. **MoMo**
   - E-wallet payment
   - QR Code payment
   - Deep link support (mobile app)
   - Automatic callback handling
   - Server-to-server IPN

3. **Cash** (Planned)
   - Cash on delivery

### 🎁 Loyalty & Rewards Program

#### Reward Points System
- **Earn Points**: 1 point per 10,000 VNĐ spent
- **Redeem Vouchers**: 100 points = 1 voucher (5-15% discount)
- **Voucher Expiry**: 30 days from issue date

#### Voucher Features
- ✅ Auto-generated random voucher codes (8 characters)
- ✅ Pre-validation before order application
- ✅ Automatic application during order creation
- ✅ Track used/active/expired vouchers

### 🚚 Shipping & Delivery System

#### Distance Calculation Strategy
```
1. AWS Location Service (Primary)
      ↓ (on error)
2. Haversine Formula (Secondary fallback)
      ↓ (on error)
3. Fixed Estimation (Final fallback)
```

#### Shipping Fee Formula
```
Distance ≤ 3km:   15,000 VNĐ (base fee)
Distance > 3km:   15,000 + (distance - 3) × 5,000 VNĐ
```

**Examples:**
- 2km → 15,000 VNĐ
- 5km → 15,000 + (2 × 5,000) = 25,000 VNĐ
- 10km → 15,000 + (7 × 5,000) = 50,000 VNĐ

#### Shipper Features
- ✅ View available orders
- ✅ Calculate shipping fee before acceptance
- ✅ Accept and update order status
- ✅ Delivery history and earnings statistics
- ✅ Profile management (vehicle info, bank account)

### 👨‍💼 Admin Panel Capabilities

#### Shipper Management
- ✅ Approve/reject shipper registrations
- ✅ Auto-generate passwords and send emails
- ✅ Lock/unlock shipper accounts
- ✅ Reset shipper passwords
- ✅ View shipper statistics (deliveries, earnings, ratings)

#### Order Management
- ✅ View pending confirmation orders
- ✅ Confirm orders (Processing → Confirmed)
- ✅ Real-time order status tracking
- ✅ Inventory and stock management

#### Product Management
- ✅ CRUD operations for Drinks, Cakes, Toppings
- ✅ Price and availability management
- ✅ Product image upload to AWS S3
- ✅ Category management

### 📧 Notification System

#### Email Notifications (AWS SES)
- ✅ **Shipper approval**: Email with username + generated password
- ✅ **Shipper rejection**: Email with rejection reason
- ✅ **Password reset**: Email with new temporary password
- ✅ **Order confirmation**: Email confirmation to customers

#### Push Notifications (AWS SNS) - Planned
- Order status updates
- Promotional notifications
- Real-time delivery tracking

---

## 🛠️ Technology Stack

### Backend Framework
- **Framework**: ASP.NET Core 8.0 (Web API)
- **Language**: C# 12
- **Architecture**: Repository Pattern + Service Layer
- **API Documentation**: Swagger/OpenAPI

### Database
- **Primary**: Amazon DynamoDB (NoSQL)
  - `CoffeeShopUsers` - User accounts
  - `Orders` - Order management
  - `CoffeeShopProducts` - Product catalog
  - `Drinks` - Beverages inventory
  - `Cakes` - Desserts inventory
  - `Toppings` - Add-ons catalog
  - `ShipperProfiles` - Shipper details
  - `ShipperDeliveryHistory` - Delivery tracking
  - `Notifications` - Notification logs

### AWS Services

| Service           | Purpose                                      |
|-------------------|----------------------------------------------|
| DynamoDB          | NoSQL Database for all entities              |
| Cognito           | User Authentication (Customer/Admin)         |
| SES               | Email Service for notifications              |
| S3                | Image storage for product photos             |
| Location Service  | Geocoding & Distance/Route calculation       |
| SNS               | Push Notifications (planned)                 |
| Amplify           | Frontend hosting and deployment              |

### Third-party Integrations
- **VNPay**: Vietnam payment gateway
- **MoMo**: E-wallet payment provider
- **BCrypt.Net**: Secure password hashing
- **JWT**: JSON Web Token authentication

### Key NuGet Packages

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

## 🚀 Installation

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [AWS Account](https://aws.amazon.com/) (Free Tier eligible)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [AWS CLI](https://aws.amazon.com/cli/) (optional, for deployment)

### Step 1: Clone Repository

```bash
git clone https://github.com/quannguyen-1110/Coffe-shop-oder-platfrom.git
cd Coffe-shop-oder-platfrom
```

### Step 2: Restore Dependencies

```bash
dotnet restore
```

### Step 3: Configure AWS Credentials

**Option A: AWS CLI**
```bash
aws configure
```

**Option B: Environment Variables**
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

**Option C: User Secrets (Recommended for Development)**
```bash
dotnet user-secrets init
dotnet user-secrets set "AWS:AccessKey" "your_access_key"
dotnet user-secrets set "AWS:SecretKey" "your_secret_key"
dotnet user-secrets set "AWS:Region" "ap-southeast-1"
```

### Step 4: Setup DynamoDB Tables

DynamoDB tables are **automatically created** on first application run via `DynamoDbService.cs`. The service:
- Scans all models with `[DynamoDBTable]` attribute
- Creates missing tables with PAY_PER_REQUEST billing
- Waits for tables to become ACTIVE

No manual table creation required! 🎉

### Step 5: Setup AWS Cognito

1. Create User Pool in AWS Cognito Console
2. Create App Client (without client secret)
3. Configure sign-up/sign-in settings:
   - Email verification required
   - Password policy (min 8 characters)
   - Custom attribute: `custom:role` (String)
4. Copy `UserPoolId` and `ClientId` to `appsettings.json`

### Step 6: Setup Payment Gateways

#### VNPay (Sandbox)
1. Register sandbox account at [VNPay Sandbox](https://sandbox.vnpayment.vn/)
2. Obtain `TmnCode` and `HashSecret`
3. Update `appsettings.json` with credentials

#### MoMo (Test Environment)
1. Register test account at [MoMo Developer](https://developers.momo.vn/)
2. Obtain `PartnerCode`, `AccessKey`, `SecretKey`
3. Update `appsettings.json` with credentials

### Step 7: Configure Settings

Edit `appsettings.json` or `appsettings.Development.json`:

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

### Step 8: Run Application

```bash
dotnet run
```

Application will start at:
- **HTTP**: http://localhost:5144
- **HTTPS**: https://localhost:7144
- **Swagger UI**: http://localhost:5144/swagger

---

## ⚙️ Configuration

### Environment Variables

For production, use environment variables instead of `appsettings.json`:

```bash
# AWS Configuration
AWS__Region=ap-southeast-1
Cognito__UserPoolId=your-pool-id
Cognito__ClientId=your-client-id

# JWT Configuration
Jwt__LocalKey=your-secret-key-32-chars-min

# Payment Gateway Configuration
VNPay__TmnCode=your-tmn-code
VNPay__HashSecret=your-hash-secret
MoMo__PartnerCode=your-partner-code
MoMo__SecretKey=your-secret-key
```

### CORS Configuration

Update CORS policy in `Program.cs` to allow your frontend domains:

```csharp
options.AddPolicy("AllowAll", policy =>
{
    policy.WithOrigins(
        "http://localhost:3000",
        "https://your-production-domain.com"
    )
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
});
```

---

## 📚 API Endpoints

### Base URL
```
Development: http://localhost:5144/api
Production:  https://your-domain.com/api
```

### Authentication Endpoints

#### Public

| Method | Endpoint                           | Description                          |
|--------|------------------------------------|--------------------------------------|
| POST   | `/Auth/register`                   | Register Customer/Admin (Cognito)    |
| POST   | `/Auth/login`                      | Login (Hybrid: Cognito + Local JWT)  |
| POST   | `/Auth/confirm`                    | Confirm email (Cognito)              |
| POST   | `/Auth/resend`                     | Resend confirmation code             |
| POST   | `/ShipperRegistration/register`    | Register as Shipper (pending approval)|

#### Protected

| Method | Endpoint                           | Role    | Description                          |
|--------|------------------------------------|---------|--------------------------------------|
| POST   | `/Auth/logout`                     | All     | Logout (Cognito global sign-out)     |
| POST   | `/Auth/change-password`            | Shipper | Change password (Shipper only)       |
| GET    | `/Auth/whoami`                     | All     | Get current user info                |

### Order Endpoints

#### Customer

| Method | Endpoint                           | Role     | Description                          |
|--------|------------------------------------|----------|--------------------------------------|
| POST   | `/Order`                           | User     | Create new order                     |
| GET    | `/Order/my-orders`                 | User     | View order history                   |
| GET    | `/Order/my-orders/{orderId}`       | User     | View order details                   |

#### Admin

| Method | Endpoint                           | Role  | Description                          |
|--------|------------------------------------|-------|--------------------------------------|
| GET    | `/Admin/orders/pending-confirm`    | Admin | Orders awaiting confirmation         |
| POST   | `/Admin/orders/{orderId}/confirm`  | Admin | Confirm order                        |
| GET    | `/Admin/orders`                    | Admin | All orders                           |
| PUT    | `/Order/{id}/status`               | Admin | Update order status                  |

#### Shipper

| Method | Endpoint                                    | Role    | Description                          |
|--------|---------------------------------------------|---------|--------------------------------------|
| GET    | `/Shipper/orders/available`                 | Shipper | Available orders for delivery        |
| GET    | `/Shipper/orders/{orderId}`                 | Shipper | Order details                        |
| POST   | `/Shipper/orders/{orderId}/calculate-fee`   | Shipper | Calculate shipping fee               |
| POST   | `/Shipper/orders/{orderId}/accept`          | Shipper | Accept order                         |
| POST   | `/Shipper/orders/{orderId}/complete`        | Shipper | Complete delivery                    |
| GET    | `/Shipper/orders/history`                   | Shipper | Delivery history                     |
| GET    | `/Shipper/statistics`                       | Shipper | Shipper statistics                   |

### Product Endpoints

| Method | Endpoint                           | Role        | Description                          |
|--------|------------------------------------|-------------|--------------------------------------|
| GET    | `/Product`                         | Public      | List all products                    |
| GET    | `/Product/{id}`                    | Public      | Get product details                  |
| POST   | `/Product`                         | Admin       | Create product                       |
| PUT    | `/Product/{id}`                    | Admin       | Update product                       |
| DELETE | `/Product/{id}`                    | Admin       | Delete product                       |

### Drinks, Cakes, Toppings

Similar CRUD endpoints:
- `/Drink/*` - Beverages management
- `/Cake/*` - Desserts management
- `/Topping/*` - Add-ons management

### Loyalty Endpoints

| Method | Endpoint                           | Role | Description                          |
|--------|------------------------------------|------|--------------------------------------|
| GET    | `/Loyalty/my-points`               | User | View reward points                   |
| GET    | `/Loyalty/my-vouchers`             | User | View vouchers                        |
| POST   | `/Loyalty/claim-voucher`           | User | Redeem voucher (100 points)          |
| POST   | `/Loyalty/validate-voucher`        | User | Validate voucher before use          |

### Payment Endpoints

| Method | Endpoint                           | Role   | Description                          |
|--------|------------------------------------|--------|--------------------------------------|
| POST   | `/Payment/vnpay/create`            | User   | Create VNPay payment                 |
| GET    | `/Payment/vnpay/callback`          | Public | VNPay callback handler               |
| GET    | `/Payment/vnpay/ipn`               | Public | VNPay IPN handler                    |
| POST   | `/MoMoPayment/create`              | User   | Create MoMo payment                  |
| GET    | `/MoMoPayment/callback`            | Public | MoMo callback handler                |
| POST   | `/MoMoPayment/ipn`                 | Public | MoMo IPN handler                     |

### Admin Shipper Management

| Method | Endpoint                                  | Role  | Description                          |
|--------|-------------------------------------------|-------|--------------------------------------|
| GET    | `/Admin/shippers/pending`                 | Admin | Pending shipper registrations        |
| GET    | `/Admin/shippers`                         | Admin | Approved shippers                    |
| POST   | `/Admin/shipper/{userId}/approve`         | Admin | Approve shipper                      |
| POST   | `/Admin/shipper/{userId}/reject`          | Admin | Reject shipper                       |
| PUT    | `/Admin/shipper/{userId}/lock`            | Admin | Lock/unlock shipper account          |
| POST   | `/Admin/shipper/{userId}/reset-password`  | Admin | Reset shipper password               |

---

## 🔄 Workflow

### Customer Order Flow

```
1. Customer browses products (Drinks/Cakes/Toppings)
2. Customer adds items to cart with selected toppings
3. Customer applies voucher code (optional)
4. System validates voucher and calculates final price
5. Customer selects payment method (VNPay/MoMo)
6. System creates order with status "Pending"
7. Customer redirects to payment gateway
8. Customer completes payment
9. Payment gateway sends callback/IPN
10. System updates order status to "Processing"
11. Customer receives confirmation email
```

### Admin Confirmation Flow

```
1. Admin views pending orders (status: Processing)
2. Admin verifies order details and payment
3. Admin clicks "Confirm Order"
4. System updates order status to "Confirmed"
5. Order becomes available for shippers
6. Customer receives confirmation notification
```

### Shipper Delivery Flow

```
1. Shipper views available orders (status: Confirmed)
2. Shipper calculates shipping fee based on distance
3. Shipper accepts order
4. System updates order status to "Shipping"
5. System assigns ShipperId to order
6. Shipper delivers order to customer
7. Shipper marks order as "Delivered"
8. System updates ShipperProfile (earnings, delivery count)
9. System creates DeliveryHistory record
10. Order status becomes "Completed"
11. Customer earns reward points
```

---

## 🗄️ Database Schema

### CoffeeShopUsers Table

| Field                | Type     | Description                              |
|----------------------|----------|------------------------------------------|
| UserId (PK)          | String   | Cognito sub or GUID                      |
| Username             | String   | Email or username                        |
| Role                 | String   | User/Admin/Shipper                       |
| PasswordHash         | String?  | BCrypt hash (Shipper only)               |
| IsActive             | Boolean  | Account status                           |
| RegistrationStatus   | String   | Pending/Approved/Rejected                |
| RewardPoints         | Integer  | Loyalty points                           |
| VoucherCount         | Integer  | Available voucher count                  |
| AvailableVouchers    | List     | Voucher array                            |
| FullName             | String?  | Full name                                |
| Email                | String?  | Email address                            |
| PhoneNumber          | String?  | Phone number                             |
| VehicleType          | String?  | Vehicle type (Shipper)                   |
| LicensePlate         | String?  | License plate (Shipper)                  |
| ApprovedAt           | DateTime?| Approval timestamp                       |
| ApprovedBy           | String?  | Admin UserId who approved                |
| CreatedAt            | DateTime | Account creation timestamp               |

### Orders Table

| Field                | Type     | Description                              |
|----------------------|----------|------------------------------------------|
| OrderId (PK)         | String   | GUID                                     |
| UserId               | String   | Customer UserId                          |
| Items                | List     | OrderItem array                          |
| TotalPrice           | Decimal  | Total before discount                    |
| FinalPrice           | Decimal  | Total after discount                     |
| AppliedVoucherCode   | String   | Voucher code used                        |
| Status               | String   | Order status                             |
| PaymentMethod        | String   | MoMo/VNPay/Cash                          |
| ShipperId            | String?  | Shipper UserId                           |
| DeliveryAddress      | String   | Delivery address                         |
| DeliveryPhone        | String?  | Delivery phone                           |
| DeliveryNote         | String?  | Delivery notes                           |
| ShippingFee          | Decimal  | Shipping fee                             |
| DistanceKm           | Decimal  | Distance in km                           |
| ClientOrderId        | String?  | FE generated ID (anti-duplicate)         |
| CreatedAt            | DateTime | Order creation                           |
| ConfirmedAt          | DateTime?| Admin confirmation time                  |
| ConfirmedBy          | String?  | Admin UserId                             |
| ShippingAt           | DateTime?| Shipper acceptance time                  |
| DeliveredAt          | DateTime?| Delivery completion time                 |
| CompletedAt          | DateTime?| Order completion time                    |

### OrderItem (Nested in Orders)

| Field          | Type     | Description                              |
|----------------|----------|------------------------------------------|
| ProductId      | String   | Product ID                               |
| ProductType    | String   | Drink/Cake                               |
| ProductName    | String   | Product name                             |
| Quantity       | Integer  | Quantity                                 |
| UnitPrice      | Decimal  | Unit price                               |
| TotalPrice     | Decimal  | Line total                               |
| Toppings       | List     | OrderTopping array                       |

### Drinks Table

| Field          | Type     | Description                              |
|----------------|----------|------------------------------------------|
| Id (PK)        | String   | GUID                                     |
| Name           | String   | Drink name                               |
| BasePrice      | Decimal  | Base price                               |
| Stock          | Integer  | Stock quantity                           |
| Category       | String   | Category                                 |
| ImageUrl       | String?  | S3 image URL                             |

### Cakes Table

| Field          | Type     | Description                              |
|----------------|----------|------------------------------------------|
| Id (PK)        | String   | GUID                                     |
| Name           | String   | Cake name                                |
| Price          | Decimal  | Price                                    |
| Stock          | Integer  | Stock quantity                           |
| ImageUrl       | String?  | S3 image URL                             |

### Toppings Table

| Field          | Type     | Description                              |
|----------------|----------|------------------------------------------|
| Id (PK)        | String   | GUID                                     |
| Name           | String   | Topping name                             |
| Price          | Decimal  | Additional price                         |
| IsAvailable    | Boolean  | Availability status                      |

---

## 🔐 Authentication & Authorization

### Hybrid Authentication Implementation

The system uses **two authentication schemes**:

```csharp
// AWS Cognito JWT for Customer/Admin
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}",
        ValidAudience = clientId,
        RoleClaimType = "custom:role",
        NameClaimType = "cognito:username"
    };
})

// Local JWT for Shipper
.AddJwtBearer("ShipperAuth", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(shipperJwtKey)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
});
```

### Authorization Examples

```csharp
// Cognito authenticated users only
[Authorize(Roles = "User,Admin")]
public class CustomerController : ControllerBase { }

// Local JWT authenticated shippers only
[Authorize(AuthenticationSchemes = "ShipperAuth", Roles = "Shipper")]
public class ShipperController : ControllerBase { }

// Allow both authentication schemes
[Authorize]
public IActionResult CommonEndpoint() { }
```

### Password Security

- **Cognito Users**: AWS managed (min 8 chars, complexity requirements)
- **Shipper Users**: BCrypt with cost factor 12

```csharp
// Hashing
var hash = BCrypt.Net.BCrypt.HashPassword(password);

// Verification
bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
```

---

## 💳 Payment Integration

### VNPay Implementation

#### Flow
1. Create payment URL with order info + signature
2. Redirect customer to VNPay
3. Customer completes payment
4. VNPay redirects to callback URL
5. Validate signature
6. Update order status
7. VNPay sends IPN (redundancy)

#### Security
- HMAC-SHA512 signature
- Timestamp validation
- Replay attack prevention

### MoMo Implementation

#### Flow
1. Create payment request with HMAC-SHA256 signature
2. Receive `payUrl`, `qrCodeUrl`, `deeplink`
3. Customer scans QR or clicks deeplink
4. Customer confirms in MoMo app
5. MoMo redirects to callback URL
6. MoMo sends IPN to notify URL
7. Update order status (idempotent)

---

## 🚀 Deployment

### Deploy to AWS Elastic Beanstalk

```bash
# Install EB CLI
pip install awsebcli

# Initialize
eb init -p "64bit Amazon Linux 2023 v2.5.0 running .NET 8" coffee-shop-api

# Create environment
eb create coffee-shop-prod --instance-type t3.small

# Deploy
dotnet publish -c Release
eb deploy
```

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CoffeeShopAPI.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CoffeeShopAPI.dll"]
```

```bash
# Build
docker build -t coffee-shop-api .

# Run
docker run -d -p 5144:80 --name coffee-api coffee-shop-api
```

---

## 🐛 Troubleshooting

### Common Issues

#### DynamoDB Access Denied
**Error**: `User: arn:aws:iam::xxx is not authorized to perform: dynamodb:CreateTable`
**Solution**: Attach `AmazonDynamoDBFullAccess` policy to your IAM user

#### Cognito Token Expired
**Error**: `IDX10223: Lifetime validation failed. The token is expired.`
**Solution**: Re-login to obtain a new token

#### VNPay Signature Mismatch
**Error**: `Invalid signature`
**Solution**: Verify `HashSecret` matches your VNPay sandbox account

#### CORS Error
**Error**: `No 'Access-Control-Allow-Origin' header is present`
**Solution**: Add your frontend URL to CORS policy in `Program.cs`

---

## 📝 License

This project is licensed under the MIT License.

---

## 🗺️ Project Status & Roadmap

### ✅ Completed Features
- [x] Hybrid authentication system
- [x] Order management (full lifecycle)
- [x] Payment integration (VNPay, MoMo)
- [x] Loyalty & voucher system
- [x] Shipper registration & approval
- [x] Distance-based shipping calculation
- [x] Email notifications
- [x] Product management (Drinks, Cakes, Toppings)
- [x] Admin dashboard capabilities

### 🚧 In Progress
- [ ] Real-time order tracking
- [ ] Push notifications (AWS SNS)
- [ ] Advanced analytics dashboard

### 📋 Planned
- [ ] Order rating & review system
- [ ] Mobile apps (iOS/Android)
- [ ] AI-powered product recommendations
- [ ] Multi-store support
- [ ] Chatbot integration

