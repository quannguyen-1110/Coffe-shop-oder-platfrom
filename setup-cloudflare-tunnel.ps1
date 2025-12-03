# Cloudflare Tunnel Auto Setup Script
# Run this script in PowerShell as Administrator

Write-Host "=== Cloudflare Tunnel Setup Script ===" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "??  Please run this script as Administrator" -ForegroundColor Red
 exit 1
}

# Variables
$BACKEND_URL = "http://fixenv-env.eba-vgperhwx.ap-southeast-1.elasticbeanstalk.com"
$TUNNEL_NAME = "coffeeshop-api"
$CLOUDFLARED_PATH = "$env:USERPROFILE\cloudflared.exe"

Write-Host "Step 1: Downloading cloudflared..." -ForegroundColor Yellow
if (-not (Test-Path $CLOUDFLARED_PATH)) {
    try {
    Invoke-WebRequest -Uri "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe" `
        -OutFile $CLOUDFLARED_PATH
        Write-Host "? Downloaded cloudflared" -ForegroundColor Green
  }
    catch {
        Write-Host "? Failed to download cloudflared: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "? cloudflared already exists" -ForegroundColor Green
}

# Add to PATH
Write-Host "`nStep 2: Adding to PATH..." -ForegroundColor Yellow
$userPath = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::User)
if ($userPath -notlike "*$env:USERPROFILE*") {
    [Environment]::SetEnvironmentVariable("Path", "$userPath;$env:USERPROFILE", [EnvironmentVariableTarget]::User)
    $env:Path = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::Machine) + ";" + [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::User)
    Write-Host "? Added to PATH" -ForegroundColor Green
}
else {
    Write-Host "? Already in PATH" -ForegroundColor Green
}

Write-Host "`nStep 3: Authenticating with Cloudflare..." -ForegroundColor Yellow
Write-Host "A browser window will open. Please login to Cloudflare and authorize."
Start-Sleep -Seconds 2
& $CLOUDFLARED_PATH tunnel login

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Authentication failed" -ForegroundColor Red
    exit 1
}
Write-Host "? Authenticated successfully" -ForegroundColor Green

Write-Host "`nStep 4: Creating tunnel '$TUNNEL_NAME'..." -ForegroundColor Yellow
$tunnelOutput = & $CLOUDFLARED_PATH tunnel create $TUNNEL_NAME 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Tunnel created successfully" -ForegroundColor Green
    
    # Extract Tunnel ID from output
    $tunnelId = ($tunnelOutput | Select-String -Pattern "([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})").Matches.Value
    
 if ($tunnelId) {
 Write-Host "?? Tunnel ID: $tunnelId" -ForegroundColor Cyan
    }
    else {
        Write-Host "??  Could not extract Tunnel ID. Check output manually." -ForegroundColor Yellow
    }
}
elseif ($tunnelOutput -like "*already exists*") {
    Write-Host "? Tunnel already exists" -ForegroundColor Green
    
    # Get existing tunnel info
    $tunnelList = & $CLOUDFLARED_PATH tunnel list 2>&1
    $tunnelId = ($tunnelList | Select-String -Pattern "$TUNNEL_NAME\s+([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})").Matches.Groups[1].Value
    
    if ($tunnelId) {
        Write-Host "?? Tunnel ID: $tunnelId" -ForegroundColor Cyan
    }
}
else {
    Write-Host "? Failed to create tunnel: $tunnelOutput" -ForegroundColor Red
exit 1
}

Write-Host "`nStep 5: Creating configuration file..." -ForegroundColor Yellow
$configPath = "$env:USERPROFILE\.cloudflared"
$configFile = "$configPath\config.yml"
$credentialsFile = "$configPath\$tunnelId.json"

if (-not (Test-Path $configPath)) {
    New-Item -ItemType Directory -Path $configPath -Force | Out-Null
}

# Check if credentials file exists
if (-not (Test-Path $credentialsFile)) {
    Write-Host "??  Credentials file not found at: $credentialsFile" -ForegroundColor Yellow
    Write-Host "   Please check ~/.cloudflared/ directory for the correct credentials file." -ForegroundColor Yellow
}

$configContent = @"
tunnel: $tunnelId
credentials-file: $credentialsFile

ingress:
  - service: $BACKEND_URL
"@

Set-Content -Path $configFile -Value $configContent -Encoding UTF8
Write-Host "? Configuration file created at: $configFile" -ForegroundColor Green

Write-Host "`n=== Setup Complete! ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Option A - Quick Tunnel (No domain needed):" -ForegroundColor White
Write-Host "  cloudflared tunnel --url $BACKEND_URL" -ForegroundColor Gray
Write-Host "  This will give you a temporary https://xxx.trycloudflare.com URL" -ForegroundColor Gray
Write-Host ""
Write-Host "Option B - With Custom Domain (Recommended):" -ForegroundColor White
Write-Host "  1. Add your domain to Cloudflare: https://dash.cloudflare.com" -ForegroundColor Gray
Write-Host "  2. Create DNS record:" -ForegroundColor Gray
Write-Host "     cloudflared tunnel route dns $TUNNEL_NAME api.yourdomain.com" -ForegroundColor Gray
Write-Host "  3. Run tunnel:" -ForegroundColor Gray
Write-Host "     cloudflared tunnel run $TUNNEL_NAME" -ForegroundColor Gray
Write-Host ""
Write-Host "Option C - Install as Windows Service:" -ForegroundColor White
Write-Host "cloudflared service install" -ForegroundColor Gray
Write-Host "  cloudflared service start" -ForegroundColor Gray
Write-Host ""
Write-Host "?? Full guide: CLOUDFLARE_TUNNEL_SETUP.md" -ForegroundColor Cyan
Write-Host ""

# Prompt to start Quick Tunnel
$startQuickTunnel = Read-Host "Do you want to start Quick Tunnel now? (y/n)"
if ($startQuickTunnel -eq 'y') {
    Write-Host "`nStarting Quick Tunnel... Press Ctrl+C to stop" -ForegroundColor Yellow
    Write-Host "Your HTTPS URL will appear below:" -ForegroundColor Cyan
  & $CLOUDFLARED_PATH tunnel --url $BACKEND_URL
}
else {
    Write-Host "`nSetup complete! Run the commands above when ready." -ForegroundColor Green
}
