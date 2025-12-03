# ?? FIX L?I MIXED CONTENT - 2 PHÚT

## L?i hi?n t?i:
```
Mixed Content: The page at 'https://main.d3djm3hylbiyyu.amplifyapp.com' 
was loaded over HTTPS, but requested an insecure XMLHttpRequest endpoint 
'http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com/...'
```

## Nguyên nhân:
- Frontend dùng HTTPS ?
- Backend dùng HTTP ?
- Browser block ? API fails ?

---

## ? GI?I PHÁP NHANH (2 PHÚT)

### B??c 1: M? PowerShell
```powershell
cd C:\Users\hp\Documents\GitHub\Coffe-shop-oder-platfrom
```

### B??c 2: Ch?y script
```powershell
.\quick-tunnel.ps1
```

Script s?:
1. Download cloudflared (n?u ch?a có)
2. Start tunnel
3. Hi?n th? HTTPS URL

**Output:**
```
Your quick Tunnel has been created! Visit it at:
https://random-name-1234.trycloudflare.com
```

### B??c 3: Copy URL ?ó

### B??c 4: Update Amplify
1. Vào https://console.aws.amazon.com/amplify/
2. Ch?n app c?a b?n
3. Environment variables
4. Thêm/update:
   ```
   REACT_APP_API_URL = https://random-name-1234.trycloudflare.com
   ```
5. Save và Redeploy

---

## ? XONG!

M? frontend ? Không còn l?i Mixed Content!

---

## ?? L?U Ý

**PowerShell ph?i gi? ch?y!**
- ??ng t?t PowerShell window
- ??ng nh?n Ctrl+C
- Khi t?t máy ? tunnel ng?ng
- Khi restart ? URL m?i (ph?i update l?i Amplify)

---

## ?? KHI RESTART MÁY

1. M? PowerShell
2. Ch?y: `.\quick-tunnel.ps1`
3. Copy URL m?i
4. Update Amplify
5. Redeploy frontend

---

## ?? MU?N URL C? ??NH?

Xem file: `QUICK_FIX_NO_DOMAIN.md` (ph?n Persistent Tunnel)

Setup thêm 10 phút ? URL không ??i mãi mãi!

---

## ?? CHI PHÍ

| Item | Cost |
|------|------|
| Cloudflared | $0 |
| Quick Tunnel | $0 |
| HTTPS URL | $0 |
| **TOTAL** | **$0** |

---

## ?? L?I?

### "cloudflared.exe not found"
? Script s? t? download

### "Failed to download"
? Download th? công:
https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe

Save vào: `C:\Users\hp\cloudflared.exe`

### Frontend v?n l?i
- Clear cache (Ctrl+Shift+Del)
- Hard refresh (Ctrl+F5)
- Check Amplify env variable ?ã save?
- Check frontend ?ã redeploy xong?

---

**CH?Y NGAY:**
```powershell
.\quick-tunnel.ps1
```

**2 PHÚT LÀ XONG!** ??
