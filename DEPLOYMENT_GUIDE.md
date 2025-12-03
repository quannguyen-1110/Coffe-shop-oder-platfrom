# H??ng d?n Deployment và K?t n?i Frontend-Backend

## ?? Thông tin hi?n t?i
- **Backend (Elastic Beanstalk)**: http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/
- **Frontend (Amplify)**: https://main.d3djm3hylbiyyu.amplifyapp.com

## ?? V?N ?? QUAN TR?NG: HTTPS vs HTTP

**C?NH BÁO:** Backend ?ang dùng HTTP, Frontend dùng HTTPS  
? Browser s? block "Mixed Content" ? API calls FAIL!

### ?? GI?I PHÁP KHUY?N NGH?: Cloudflare Tunnel (MI?N PHÍ)

**Xem chi ti?t trong file:** `FREE_HTTPS_SOLUTION.md`

**Quick setup:**
```powershell
# Run auto setup script (PowerShell Admin)
.\setup-cloudflare-tunnel.ps1

# Start tunnel
cloudflared tunnel --url http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com
```

**L?i ích:**
- ? 100% MI?N PHÍ ($0/tháng)
- ? HTTPS t? ??ng
- ? SSL certificate mi?n phí
- ? DDoS protection
- ? Setup trong 15 phút

**Chi ti?t ??y ??:** Xem `CLOUDFLARE_TUNNEL_SETUP.md`

---

## ? Nh?ng gì ?ã ???c c?p nh?t

### 1. CORS Configuration (Program.cs)
- ? ?ã thêm Elastic Beanstalk domain vào CORS policy
- ? ?ã b?t AllowCredentials ?? h? tr? authentication
- ? Cho phép c? localhost (development) và production domains
- ? Thêm middleware x? lý mixed content

### 2. Payment Callbacks (appsettings.json)
- ? VNPay ReturnUrl ?ã ???c c?p nh?t sang Elastic Beanstalk domain
- ? MoMo ReturnUrl và NotifyUrl ?ã ???c c?p nh?t

### 3. Security Headers (Program.cs)
- ? ?ã thêm headers ?? h? tr? HTTPS frontend g?i HTTP backend
- ? Content-Type-Options ?? b?o m?t

---

## ?? Các b??c ti?p theo c?n làm

### B??c 1: Setup HTTPS v?i Cloudflare Tunnel (KHUY?N NGH?)

**? ?ÂY LÀ B??C QUAN TR?NG NH?T**

Xem h??ng d?n chi ti?t trong: `FREE_HTTPS_SOLUTION.md`

Quick start:
```powershell
.\setup-cloudflare-tunnel.ps1
```

### B??c 2: Redeploy Backend lên Elastic Beanstalk

```bash
# T? th? m?c project
dotnet publish -c Release -o ./publish

# Zip file
Compress-Archive -Path ./publish/* -DestinationPath deployment.zip -Force

# Upload lên Elastic Beanstalk qua AWS Console ho?c CLI
```

**Ho?c dùng AWS CLI:**
```bash
eb deploy
```

### B??c 3: C?u hình VNPay và MoMo Dashboard

#### VNPay:
1. ??ng nh?p vào VNPay Merchant Portal
2. C?p nh?t Return URL: 
   ```
   http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/api/Payment/vnpay/callback
   ```

#### MoMo:
1. ??ng nh?p vào MoMo Developer Portal
2. C?p nh?t:
   - Return URL: `http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/api/MoMoPayment/callback`
   - IPN URL: `http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/api/MoMoPayment/ipn`

### B??c 4: Test k?t n?i

#### Test API t? Frontend:
```javascript
// Trong frontend code
const response = await fetch(`${process.env.REACT_APP_API_URL}/api/health`);
console.log(response);
```

#### Test CORS:
```bash
curl -X OPTIONS http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/api/Auth/login \
  -H "Origin: https://main.d3djm3hylbiyyu.amplifyapp.com" \
  -H "Access-Control-Request-Method: POST" \
  -v
```

### B??c 5: Ki?m tra Swagger UI
Truy c?p: http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/swagger

## ?? Nh?ng ?i?m quan tr?ng c?n l?u ý

### 1. HTTPS vs HTTP
- ?? **QUAN TR?NG**: Backend ?ang dùng HTTP, frontend dùng HTTPS
- M?t s? trình duy?t có th? block mixed content (HTTPS ? HTTP)
- **Gi?i pháp**: Nên c?u hình SSL/TLS cho Elastic Beanstalk

#### Cách thêm HTTPS cho Elastic Beanstalk:
1. T?o certificate trong AWS Certificate Manager (ACM)
2. Thêm Load Balancer listener cho HTTPS (port 443)
3. C?p nh?t l?i URLs trong appsettings.json

### 2. AWS Cognito Configuration
C?n thêm callback URLs trong Cognito User Pool:
1. Vào AWS Cognito Console
2. Ch?n User Pool: `ap-southeast-1_na1YdboYh`
3. App Integration ? App client: `7lrsfiqi9o26g5esb7d6gfivld`
4. Thêm Callback URLs:
   ```
   https://main.d3djm3hylbiyyu.amplifyapp.com/auth/callback
   https://main.d3djm3hylbiyyu.amplifyapp.com/
   ```
5. Thêm Sign out URLs:
   ```
   https://main.d3djm3hylbiyyu.amplifyapp.com/
   ```

### 3. Security Headers
Xem xét thêm security headers trong Program.cs:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

### 4. Environment-specific Configuration
Nên t?o `appsettings.Production.json` riêng:

```json
{
  "Environment": "Production",
  "VNPay": {
    "ReturnUrl": "http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/api/Payment/vnpay/callback"
  },
  "MoMo": {
    "ReturnUrl": "http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/api/MoMoPayment/callback",
    "NotifyUrl": "http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/api/MoMoPayment/ipn"
  }
}
```

## ?? Troubleshooting

### L?i CORS
**Tri?u ch?ng**: Console log "CORS policy: No 'Access-Control-Allow-Origin' header"

**Gi?i pháp**:
1. Ki?m tra logs trong Elastic Beanstalk
2. Verify CORS policy trong Program.cs
3. Test v?i Postman/curl tr??c

### L?i Authentication
**Tri?u ch?ng**: 401 Unauthorized

**Gi?i pháp**:
1. Ki?m tra JWT token có ?úng format
2. Verify Cognito User Pool settings
3. Check token expiry time

### L?i Payment Callback
**Tri?u ch?ng**: Payment thành công nh?ng không redirect v?

**Gi?i pháp**:
1. Verify callback URLs trong VNPay/MoMo dashboard
2. Check logs trong API
3. Test v?i ngrok ?? debug locally

## ?? Monitoring

### CloudWatch Logs
- Elastic Beanstalk t? ??ng g?i logs sang CloudWatch
- Vào AWS Console ? CloudWatch ? Log groups
- Tìm log group: `/aws/elasticbeanstalk/fixenv-env/`

### Application Insights
Xem xét thêm Application Insights cho monitoring t?t h?n:
```bash
dotnet add package AWS.XRay.Recorder.Handlers.AspNetCore
```

## ?? Performance Tips

1. **Enable Response Compression**:
```csharp
builder.Services.AddResponseCompression();
```

2. **Add Health Checks**:
```csharp
builder.Services.AddHealthChecks();
app.MapHealthChecks("/health");
```

3. **Configure Logging**:
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
    "AWS": "Information"
  }
}
```

## ?? Checklist tr??c khi Go Live

- [ ] SSL/TLS ???c c?u hình cho Elastic Beanstalk
- [ ] Cognito callback URLs ?ã ???c c?p nh?t
- [ ] VNPay và MoMo callback URLs ?ã ???c c?u hình
- [ ] Frontend environment variables ?ã ???c set
- [ ] CORS policy ?ã ???c test
- [ ] Health check endpoint ho?t ??ng
- [ ] CloudWatch logs ?ang ???c ghi
- [ ] Backup database ?ã ???c thi?t l?p
- [ ] Monitoring và alerting ?ã ???c c?u hình

## ?? Useful Links

- **Swagger UI**: http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/swagger
- **Frontend**: https://main.d3djm3hylbiyyu.amplifyapp.com
- **AWS Console**: https://console.aws.amazon.com/
- **Elastic Beanstalk**: https://ap-southeast-1.console.aws.amazon.com/elasticbeanstalk/

## ?? Next Steps - Nâng cao

1. **Setup Custom Domain**:
   - Mua domain (ví d?: coffeeshop.com)
   - C?u hình Route 53
   - Point ??n Elastic Beanstalk và Amplify

2. **Enable HTTPS**:
   - Request SSL certificate t? ACM
   - Configure Load Balancer
   - Update all URLs to HTTPS

3. **Setup CI/CD**:
   - GitHub Actions cho auto-deploy
   - Automated testing
   - Blue-green deployment

4. **Add CDN**:
   - CloudFront cho static assets
   - Caching strategy
   - Geographic distribution

---

**L?u ý**: Document này ???c t?o t? ??ng d?a trên c?u hình hi?n t?i c?a project.
