# VNPay Callback Test Script
# Sử dụng: .\test-callback.ps1 -OrderId "ORD_xxx"

param(
    [Parameter(Mandatory=$true)]
    [string]$OrderId,
    
    [Parameter(Mandatory=$false)]
    [decimal]$Amount = 100000,
    
    [Parameter(Mandatory=$false)]
    [string]$ResponseCode = "00"
)

$payDate = Get-Date -Format "yyyyMMddHHmmss"
$transactionNo = Get-Random -Minimum 10000000 -Maximum 99999999

$callbackUrl = "http://localhost:5144/api/Payment/vnpay/callback?" +
    "vnp_Amount=$($Amount)00" +
    "&vnp_BankCode=NCB" +
    "&vnp_BankTranNo=VNP01" +
    "&vnp_CardType=ATM" +
    "&vnp_OrderInfo=Thanh%20toan%20don%20hang%20$OrderId" +
    "&vnp_PayDate=$payDate" +
    "&vnp_ResponseCode=$ResponseCode" +
    "&vnp_TmnCode=AEVWGFV2" +
    "&vnp_TransactionNo=$transactionNo" +
    "&vnp_TransactionStatus=$ResponseCode" +
    "&vnp_TxnRef=$OrderId" +
    "&vnp_SecureHash=test123"

Write-Host "=== VNPay Callback Test ===" -ForegroundColor Green
Write-Host "Order ID: $OrderId"
Write-Host "Amount: $Amount VND"
Write-Host "Response Code: $ResponseCode"
Write-Host ""
Write-Host "Calling callback URL..." -ForegroundColor Yellow
Write-Host $callbackUrl
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri $callbackUrl -Method GET -UseBasicParsing
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Status Code: $($response.StatusCode)"
    Write-Host "Response: $($response.Content)"
} catch {
    Write-Host "❌ Error!" -ForegroundColor Red
    Write-Host $_.Exception.Message
}
