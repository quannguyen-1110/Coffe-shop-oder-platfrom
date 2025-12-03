# Quick Tunnel Start Script - NO DOMAIN NEEDED
# Dành cho th?c t?p sinh - 100% MI?N PHÍ

Write-Host "=== Cloudflare Quick Tunnel - KHÔNG C?N DOMAIN ===" -ForegroundColor Cyan
Write-Host ""

$BACKEND_URL = "http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com"
$CLOUDFLARED_PATH = "$env:USERPROFILE\cloudflared.exe"

# Check if cloudflared exists
if (-not (Test-Path $CLOUDFLARED_PATH)) {
    Write-Host "??  Downloading cloudflared..." -ForegroundColor Yellow
    try {
   Invoke-WebRequest -Uri "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe" `
-OutFile $CLOUDFLARED_PATH
  Write-Host "? Downloaded successfully!" -ForegroundColor Green
    }
    catch {
        Write-Host "? Failed to download: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Manual download:" -ForegroundColor Yellow
     Write-Host "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe" -ForegroundColor Gray
        exit 1
    }
}
else {
    Write-Host "? cloudflared.exe found!" -ForegroundColor Green
}

Write-Host ""
Write-Host "?? Starting Quick Tunnel..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Backend: $BACKEND_URL" -ForegroundColor Gray
Write-Host ""
Write-Host "? Please wait for HTTPS URL to appear..." -ForegroundColor Cyan
Write-Host ""
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host ""

# Start tunnel
& $CLOUDFLARED_PATH tunnel --url $BACKEND_URL

# Note: Script will run until Ctrl+C
Write-Host ""
Write-Host "Tunnel stopped." -ForegroundColor Yellow
