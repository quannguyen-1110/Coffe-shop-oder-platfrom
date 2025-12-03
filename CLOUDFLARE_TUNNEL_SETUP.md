# Cloudflare Tunnel Setup - MI?N PHÍ HTTPS

## ?? L?i ích
- ? SSL/TLS HOÀN TOÀN MI?N PHÍ
- ? Không c?n Load Balancer ($0/tháng)
- ? DDoS protection t? ??ng
- ? CDN mi?n phí
- ? URL c? ??nh không ??i
- ? H? tr? custom domain

## ?? B??c 1: Cài ??t cloudflared

### Windows:
```powershell
# Download
Invoke-WebRequest -Uri "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe" -OutFile "$env:USERPROFILE\cloudflared.exe"

# Thêm vào PATH (PowerShell Admin)
$env:Path += ";$env:USERPROFILE"
[Environment]::SetEnvironmentVariable("Path", $env:Path, [EnvironmentVariableTarget]::User)
```

### macOS:
```bash
brew install cloudflare/cloudflare/cloudflared
```

### Linux:
```bash
wget https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64
chmod +x cloudflared-linux-amd64
sudo mv cloudflared-linux-amd64 /usr/local/bin/cloudflared
```

## ?? B??c 2: Authenticate v?i Cloudflare

```bash
cloudflared tunnel login
```

L?nh này s? m? browser ?? b?n ??ng nh?p Cloudflare và authorize.

## ?? B??c 3: T?o Tunnel

```bash
# T?o tunnel tên "coffeeshop-api"
cloudflared tunnel create coffeeshop-api

# L?u l?i TUNNEL-ID ???c hi?n th? (ví d?: 12345678-1234-1234-1234-123456789012)
```

Cloudflared s? t?o credentials file t?i:
- Windows: `C:\Users\<username>\.cloudflared\<TUNNEL-ID>.json`
- Linux/Mac: `~/.cloudflared/<TUNNEL-ID>.json`

## ?? B??c 4: T?o Configuration File

T?o file `C:\Users\hp\.cloudflared\config.yml` (ho?c `~/.cloudflared/config.yml`):

```yaml
# Thay <TUNNEL-ID> b?ng ID t? b??c 3
tunnel: <TUNNEL-ID>
credentials-file: C:\Users\hp\.cloudflared\<TUNNEL-ID>.json

ingress:
  # Route t?t c? traffic ??n Elastic Beanstalk
  - service: http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com
```

**L?u ý Windows**: Dùng ???ng d?n tuy?t ??i cho `credentials-file`

## ?? B??c 5A: S? d?ng Cloudflare Quick Tunnel (KHÔNG C?N DOMAIN)

N?u b?n KHÔNG có domain riêng:

```bash
cloudflared tunnel --url http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com
```

B?n s? nh?n ???c URL d?ng: `https://random-name.trycloudflare.com`

**Nh??c ?i?m**: URL thay ??i m?i l?n restart

## ?? B??c 5B: S? d?ng v?i Custom Domain (KHUY?N NGH?)

N?u b?n có domain (ví d?: `yourdomain.com`):

### 1. Add domain vào Cloudflare:
- Vào https://dash.cloudflare.com
- Add site ? Nh?p domain c?a b?n
- Follow h??ng d?n thay ??i nameservers

### 2. T?o DNS record:
```bash
cloudflared tunnel route dns coffeeshop-api api.yourdomain.com
```

Ho?c th? công trong Cloudflare Dashboard:
- Type: `CNAME`
- Name: `api`
- Content: `<TUNNEL-ID>.cfargotunnel.com`
- Proxy status: ? Proxied

### 3. Update config.yml:
```yaml
tunnel: <TUNNEL-ID>
credentials-file: C:\Users\hp\.cloudflared\<TUNNEL-ID>.json

ingress:
  - hostname: api.yourdomain.com
    service: http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com
  - service: http_status:404
```

## ?? B??c 6: Run Tunnel

```bash
# Foreground (test)
cloudflared tunnel run coffeeshop-api

# Background (production) - Windows
cloudflared service install
cloudflared service enable

# Background (production) - Linux/Mac
cloudflared service install
sudo systemctl start cloudflared
sudo systemctl enable cloudflared
```

## ? B??c 7: Test

```bash
# Test v?i curl
curl https://api.yourdomain.com/swagger

# Ho?c m? browser
https://api.yourdomain.com/swagger
```

## ?? B??c 8: Update Frontend Environment Variables

Trong AWS Amplify Console:

```
REACT_APP_API_URL=https://api.yourdomain.com
```

Sau ?ó redeploy frontend.

## ?? Troubleshooting

### Tunnel không k?t n?i:
```bash
# Check status
cloudflared tunnel info coffeeshop-api

# Check logs
cloudflared tunnel --loglevel debug run coffeeshop-api
```

### DNS không resolve:
- ??i 5-10 phút ?? DNS propagate
- Check v?i: `nslookup api.yourdomain.com`

### 403 Forbidden:
- Check CORS settings trong Program.cs
- Verify domain trong Cloudflare SSL/TLS settings = "Full"

## ?? Chi phí

- **Cloudflare Free Plan**: $0/tháng
- **SSL Certificate**: $0 (mi?n phí)
- **Bandwidth**: Unlimited (mi?n phí)
- **DDoS Protection**: Included (mi?n phí)

**T?ng chi phí**: **$0/tháng** ??

## ?? Security Best Practices

### 1. Enable Cloudflare WAF (Web Application Firewall):
- Vào Cloudflare Dashboard
- Security ? WAF
- Enable managed rules (mi?n phí)

### 2. Enable Always Use HTTPS:
- SSL/TLS ? Edge Certificates
- Always Use HTTPS: ON

### 3. Enable Authenticated Origin Pulls:
```yaml
# config.yml
tunnel: <TUNNEL-ID>
credentials-file: C:\Users\hp\.cloudflared\<TUNNEL-ID>.json

# Add origin CA certificate
originRequest:
  caPool: /path/to/origin-ca.pem

ingress:
  - hostname: api.yourdomain.com
    service: http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com
  - service: http_status:404
```

## ?? Monitoring

### View tunnel metrics:
```bash
cloudflared tunnel info coffeeshop-api
```

### View logs:
- Windows: `C:\Users\<username>\.cloudflared\tunnel.log`
- Linux: `/var/log/cloudflared.log`

### Cloudflare Analytics:
- Vào Dashboard ? Analytics
- Xem traffic, bandwidth, threats blocked

## ?? Update Backend

Khi b?n update backend, tunnel t? ??ng forward traffic m?i. KHÔNG C?N restart tunnel.

## ?? Stop/Remove Tunnel

```bash
# Stop service
cloudflared service stop

# Uninstall service
cloudflared service uninstall

# Delete tunnel
cloudflared tunnel delete coffeeshop-api
```

## ?? Bonus: Multiple Environments

B?n có th? t?o nhi?u tunnels cho dev/staging/prod:

```yaml
# config.yml
tunnel: <TUNNEL-ID>
credentials-file: C:\Users\hp\.cloudflared\<TUNNEL-ID>.json

ingress:
  # Production
  - hostname: api.yourdomain.com
    service: http://prod-env.elasticbeanstalk.com
  
  # Staging
  - hostname: api-staging.yourdomain.com
 service: http://staging-env.elasticbeanstalk.com
  
  # Development
  - hostname: api-dev.yourdomain.com
    service: http://localhost:5000
  
  - service: http_status:404
```

## ?? Support

- Cloudflare Docs: https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/
- Community Forum: https://community.cloudflare.com/
- Discord: https://discord.gg/cloudflaredev

---

## ? Checklist

- [ ] Cài ??t cloudflared
- [ ] Authenticate v?i Cloudflare account
- [ ] T?o tunnel
- [ ] T?o config.yml
- [ ] (Optional) Setup custom domain
- [ ] Run tunnel
- [ ] Test HTTPS endpoint
- [ ] Update frontend environment variables
- [ ] Redeploy frontend
- [ ] Test end-to-end t? frontend
- [ ] Setup monitoring
- [ ] Enable WAF và security features

**Th?i gian setup**: ~15-30 phút
**Chi phí**: $0
**K?t qu?**: HTTPS hoàn toàn mi?n phí! ??
