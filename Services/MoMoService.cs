using System.Security.Cryptography;
using System.Text;
using CoffeeShopAPI.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CoffeeShopAPI.Services
{
    public class MoMoService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public MoMoService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<MoMoPaymentResponse> CreatePaymentAsync(string orderId, decimal amount, string orderInfo)
        {
            var endpoint = _configuration["MoMo:Endpoint"];
            var partnerCode = _configuration["MoMo:PartnerCode"];
            var accessKey = _configuration["MoMo:AccessKey"];
            var secretKey = _configuration["MoMo:SecretKey"];
            var returnUrl = _configuration["MoMo:ReturnUrl"];
            var notifyUrl = _configuration["MoMo:NotifyUrl"];

            var requestId = Guid.NewGuid().ToString();
            var requestType = "captureWallet";
            var extraData = ""; // Optional

            // Tạo raw signature
            var rawSignature = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";

            // Tạo signature
            var signature = ComputeHmacSha256(rawSignature, secretKey);

            // Tạo request body
            var requestBody = new
            {
                partnerCode = partnerCode,
                partnerName = "Test",
                storeId = "MomoTestStore",
                requestId = requestId,
                amount = ((long)amount).ToString(),
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                lang = "vi",
                extraData = extraData,
                requestType = requestType,
                signature = signature
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                Console.WriteLine("=== MoMo Payment Request ===");
                Console.WriteLine($"Endpoint: {endpoint}");
                Console.WriteLine($"Request Body: {JsonConvert.SerializeObject(requestBody, Formatting.Indented)}");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Response Body: {responseContent}");
                
                var momoResponse = JsonConvert.DeserializeObject<MoMoApiResponse>(responseContent);

                if (momoResponse != null && momoResponse.ResultCode == 0)
                {
                    return new MoMoPaymentResponse
                    {
                        Success = true,
                        PayUrl = momoResponse.PayUrl,
                        QrCodeUrl = momoResponse.QrCodeUrl,
                        DeepLink = momoResponse.Deeplink,
                        Message = "Tạo payment URL thành công"
                    };
                }
                else
                {
                    return new MoMoPaymentResponse
                    {
                        Success = false,
                        Message = momoResponse?.Message ?? "Lỗi không xác định"
                    };
                }
            }
            catch (Exception ex)
            {
                return new MoMoPaymentResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối MoMo: {ex.Message}"
                };
            }
        }

        public MoMoCallbackResponse ProcessCallback(IQueryCollection queryParams)
        {
            var partnerCode = queryParams["partnerCode"].ToString();
            var orderId = queryParams["orderId"].ToString();
            var requestId = queryParams["requestId"].ToString();
            var amount = queryParams["amount"].ToString();
            var orderInfo = queryParams["orderInfo"].ToString();
            var orderType = queryParams["orderType"].ToString();
            var transId = queryParams["transId"].ToString();
            var resultCode = queryParams["resultCode"].ToString();
            var message = queryParams["message"].ToString();
            var payType = queryParams["payType"].ToString();
            var responseTime = queryParams["responseTime"].ToString();
            var extraData = queryParams["extraData"].ToString();
            var signature = queryParams["signature"].ToString();

            var secretKey = _configuration["MoMo:SecretKey"];

            // Tạo raw signature để verify
            var rawSignature = $"accessKey={_configuration["MoMo:AccessKey"]}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";

            var computedSignature = ComputeHmacSha256(rawSignature, secretKey);

            // Verify signature
            if (computedSignature != signature)
            {
                return new MoMoCallbackResponse
                {
                    Success = false,
                    Message = "Chữ ký không hợp lệ"
                };
            }

            // Check result code
            if (resultCode == "0")
            {
                return new MoMoCallbackResponse
                {
                    Success = true,
                    OrderId = orderId,
                    Amount = decimal.Parse(amount),
                    TransactionId = transId,
                    Message = "Giao dịch thành công"
                };
            }
            else
            {
                return new MoMoCallbackResponse
                {
                    Success = false,
                    OrderId = orderId,
                    Message = GetResultMessage(resultCode)
                };
            }
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private string GetResultMessage(string resultCode)
        {
            return resultCode switch
            {
                "0" => "Giao dịch thành công",
                "9000" => "Giao dịch được khởi tạo, chờ người dùng xác nhận thanh toán",
                "8000" => "Giao dịch đang được xử lý",
                "7000" => "Giao dịch đang chờ thanh toán",
                "1000" => "Giao dịch đã được khởi tạo, chờ người dùng xác nhận thanh toán",
                "11" => "Truy cập bị từ chối",
                "12" => "Phiên bản API không được hỗ trợ cho yêu cầu này",
                "13" => "Xác thực doanh nghiệp thất bại",
                "20" => "Yêu cầu sai định dạng",
                "21" => "Số tiền giao dịch không hợp lệ",
                "40" => "RequestId bị trùng",
                "41" => "OrderId bị trùng",
                "42" => "OrderId không hợp lệ hoặc không được tìm thấy",
                "43" => "Yêu cầu bị từ chối vì xung đột trong quá trình xử lý giao dịch",
                "1001" => "Giao dịch thanh toán thất bại do tài khoản người dùng không đủ tiền",
                "1002" => "Giao dịch bị từ chối do nhà phát hành tài khoản thanh toán",
                "1003" => "Giao dịch bị hủy",
                "1004" => "Giao dịch thất bại do số tiền thanh toán vượt quá hạn mức thanh toán của người dùng",
                "1005" => "Giao dịch thất bại do url hoặc QR code đã hết hạn",
                "1006" => "Giao dịch thất bại do người dùng đã từ chối xác nhận thanh toán",
                "1007" => "Giao dịch bị từ chối vì tài khoản người dùng đang ở trạng thái tạm khóa",
                "2001" => "Giao dịch thất bại do sai thông tin liên kết",
                "3001" => "Liên kết thanh toán không tồn tại hoặc đã hết hạn",
                "3002" => "OrderId không hợp lệ",
                "3003" => "Số tiền thanh toán không hợp lệ",
                "4001" => "Giao dịch bị hủy do quá thời gian thanh toán",
                _ => "Giao dịch thất bại"
            };
        }
    }

    // Response models
    public class MoMoApiResponse
    {
        [JsonProperty("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonProperty("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonProperty("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("responseTime")]
        public long ResponseTime { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty("payUrl")]
        public string PayUrl { get; set; } = string.Empty;

        [JsonProperty("deeplink")]
        public string Deeplink { get; set; } = string.Empty;

        [JsonProperty("qrCodeUrl")]
        public string QrCodeUrl { get; set; } = string.Empty;
    }
}
