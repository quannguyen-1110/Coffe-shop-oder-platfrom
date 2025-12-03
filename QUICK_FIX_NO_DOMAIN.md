# ?? GI?I PHÁP NHANH - KHÔNG C?N DOMAIN

## ?? V?n ?? hi?n t?i
Browser block Mixed Content:
- Frontend: `https://main.d3djm3hylbiyyu.amplifyapp.com` (HTTPS)
- Backend: `http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com` (HTTP)
- Result: ? API calls FAIL

## ? Gi?i pháp: Cloudflare Quick Tunnel
- 100% MI?N PHÍ
- KHÔNG C?N DOMAIN
- KHÔNG C?N TÀI KHO?N
- Setup 2 phút

---

## ?? H??NG D?N CHI TI?T

### B??c 1: Download cloudflared (1 l?n duy nh?t)

#### Windows PowerShell:
```powershell
# Download
Invoke-WebRequest -Uri "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe" -OutFile "$env:USERPROFILE\cloudflared.exe"

# Verify
Test-Path "$env:USERPROFILE\cloudflared.exe"
```

#### macOS:
```bash
brew install cloudflare/cloudflare/cloudflared
```

#### Linux:
```bash
wget https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64
chmod +x cloudflared-linux-amd64
sudo mv cloudflared-linux-amd64 /usr/local/bin/cloudflared
```

---

### B??c 2: Start Quick Tunnel

#### Windows:
```powershell
cd $env:USERPROFILE
.\cloudflared.exe tunnel --url http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com
```

#### macOS/Linux:
```bash
cloudflared tunnel --url http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com
```

**Output s? hi?n th?:**
```
2025-01-XX XX:XX:XX INF +--------------------------------------------------------------------------------------------+
2025-01-XX XX:XX:XX INF |  Your quick Tunnel has been created! Visit it at (it may take some time to be reachable):  |
2025-01-XX XX:XX:XX INF |  https://random-name-1234.trycloudflare.com          |
2025-01-XX XX:XX:XX INF +--------------------------------------------------------------------------------------------+
```

**? COPY URL ?Ó!** Ví d?: `https://random-name-1234.trycloudflare.com`

---

### B??c 3: Update Frontend Environment Variables

1. Vào **AWS Amplify Console**: https://console.aws.amazon.com/amplify/
2. Ch?n app c?a b?n: `main.d3djm3hylbiyyu`
3. Sidebar ? **Environment variables**
4. Thêm ho?c update:
   ```
   Key: REACT_APP_API_URL
   Value: https://random-name-1234.trycloudflare.com
   ```
5. **Save**

---

### B??c 4: Redeploy Frontend

V?n trong Amplify Console:
1. Vào tab **Deployments**
2. Click **Redeploy this version**
3. ??i 2-3 phút

**HO?C** git push ?? trigger auto-deploy:
```bash
git commit --allow-empty -m "Update API URL"
git push origin main
```

---

### B??c 5: Test

M? frontend: `https://main.d3djm3hylbiyyu.amplifyapp.com`

**Console s? KHÔNG còn l?i Mixed Content!** ?

Test API call:
- Login
- Fetch products
- T?t c? features ho?t ??ng

---

## ?? KHI RESTART CLOUDFLARED

**L?u ý quan tr?ng:** URL s? thay ??i m?i l?n restart!

### Workflow khi restart:
1. Stop cloudflared (Ctrl+C)
2. Start l?i: `.\cloudflared.exe tunnel --url http://...`
3. **Copy URL m?i**
4. Update l?i Amplify environment variable
5. Redeploy frontend

---

## ?? GI?I PHÁP NÂNG CAO: Persistent Tunnel (V?n mi?n phí)

N?u b?n mu?n URL **KHÔNG thay ??i**:

### B??c 1: T?o tài kho?n Cloudflare (mi?n phí)
- ??ng ký: https://dash.cloudflare.com/sign-up
- Không c?n th? tín d?ng

### B??c 2: Authenticate
```powershell
.\cloudflared.exe tunnel login
```
Browser s? m? ?? b?n authorize.

### B??c 3: T?o persistent tunnel
```powershell
.\cloudflared.exe tunnel create coffeeshop-api
```

Cloudflared s? t?o credentials và **Tunnel ID**.

### B??c 4: T?o config file

T?o file `$env:USERPROFILE\.cloudflared\config.yml`:
```yaml
tunnel: <TUNNEL-ID t? b??c 3>
credentials-file: C:\Users\hp\.cloudflared\<TUNNEL-ID>.json

ingress:
  - service: http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com
```

### B??c 5: Route DNS (dùng subdomain cloudflare mi?n phí)
```powershell
.\cloudflared.exe tunnel route dns coffeeshop-api coffeeshop-api
```

Cloudflare s? t?o subdomain mi?n phí:
```
https://coffeeshop-api.cfargotunnel.com
```

**URL này c? ??nh, không ??i!**

### B??c 6: Run tunnel
```powershell
.\cloudflared.exe tunnel run coffeeshop-api
```

### B??c 7: Update Amplify
```
REACT_APP_API_URL=https://coffeeshop-api.cfargotunnel.com
```

---

## ??? Ch?y cloudflared d??i background (Windows)

### Option 1: Install as Windows Service
```powershell
# Run PowerShell as Administrator
.\cloudflared.exe service install
.\cloudflared.exe service start

# Check status
Get-Service cloudflared
```

### Option 2: Dùng Task Scheduler
1. M? Task Scheduler
2. Create Task
3. Trigger: At startup
4. Action: `C:\Users\hp\cloudflared.exe tunnel run coffeeshop-api`
5. Settings: Run whether user is logged on or not

### Option 3: Dùng screen session (Linux/macOS)
```bash
screen -S cloudflared
cloudflared tunnel run coffeeshop-api
# Press Ctrl+A, then D to detach
```

---

## ?? SO SÁNH CÁC PH??NG ÁN

| Ph??ng án | Setup | URL c? ??nh | C?n tài kho?n | Khuy?n ngh? |
|-----------|-------|-------------|---------------|-------------|
| **Quick Tunnel** | 2 phút | ? | ? | ? Testing |
| **Persistent Tunnel** | 10 phút | ? | ? (free) | ? BEST |
| **With custom domain** | 15 phút | ? | ? + domain | Production |

---

## ?? TROUBLESHOOTING

### L?i: "cloudflared not found"
```powershell
# Thêm vào PATH
$env:Path += ";$env:USERPROFILE"
```

### L?i: "tunnel disconnected"
- Check internet connection
- Restart cloudflared
- Check Elastic Beanstalk ?ang ch?y

### L?i: Frontend v?n g?i HTTP
- Clear browser cache (Ctrl+Shift+Delete)
- Hard refresh (Ctrl+F5)
- Check Amplify environment variables ?ã save ch?a
- Verify frontend ?ã redeploy xong

### Console log: "ERR_NETWORK"
- Verify cloudflared ?ang ch?y
- Check URL có ?úng không
- Test v?i curl: `curl https://your-tunnel-url.com/swagger`

---

## ?? MONITORING

### Check tunnel status:
```powershell
# Logs s? hi?n th? trong terminal
# Press Ctrl+C ?? stop
```

### Test HTTPS endpoint:
```powershell
# PowerShell
Invoke-WebRequest -Uri "https://your-tunnel-url.com/swagger" -UseBasicParsing

# Or browser
https://your-tunnel-url.com/swagger
```

---

## ?? VIDEO H??NG D?N

YouTube tutorial (English):
- Cloudflare Tunnel Quick Start: https://www.youtube.com/watch?v=ZvIdFs3M5ic
- Self-hosted tunnel: https://www.youtube.com/watch?v=ey4u7OUAF3c

---

## ?? CHECKLIST

### Quick Tunnel (2 phút):
- [ ] Download cloudflared.exe
- [ ] Ch?y: `cloudflared tunnel --url http://...`
- [ ] Copy HTTPS URL
- [ ] Update Amplify environment variable
- [ ] Redeploy frontend
- [ ] Test t? frontend

### Persistent Tunnel (10 phút):
- [ ] T?o tài kho?n Cloudflare
- [ ] Run: `cloudflared tunnel login`
- [ ] Run: `cloudflared tunnel create coffeeshop-api`
- [ ] T?o config.yml
- [ ] Run: `cloudflared tunnel route dns coffeeshop-api coffeeshop-api`
- [ ] Run: `cloudflared tunnel run coffeeshop-api`
- [ ] Copy cfargotunnel.com URL
- [ ] Update Amplify environment variable
- [ ] Redeploy frontend
- [ ] (Optional) Install as Windows Service

---

## ?? K?T QU?

Sau khi setup:
- ? Frontend HTTPS ho?t ??ng
- ? Backend accessible qua HTTPS
- ? Không còn Mixed Content errors
- ? T?t c? API calls work
- ? Payment callbacks work
- ? Authentication work
- ? Chi phí: $0

---

## ?? CHI PHÍ

| Item | Cost |
|------|------|
| Cloudflare account | $0 |
| Quick Tunnel | $0 |
| Persistent Tunnel | $0 |
| cfargotunnel.com subdomain | $0 |
| SSL certificate | $0 (t? ??ng) |
| Bandwidth | $0 (unlimited) |
| **TOTAL** | **$0/month** |

---

## ?? LINKS H?U ÍCH

- Cloudflare Tunnel Docs: https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/
- Quick Tunnel Guide: https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/run-tunnel/trycloudflare/
- Community Forum: https://community.cloudflare.com/
- Discord: https://discord.gg/cloudflaredev

---

**L?U Ý:** Gi?i pháp này phù h?p cho:
- ? Th?c t?p sinh / h?c sinh
- ? Demo / POC
- ? Development / Staging
- ? Personal projects
- ?? Production (nên dùng custom domain)

**B?T ??U NGAY!** Ch? m?t 2 phút! ??
