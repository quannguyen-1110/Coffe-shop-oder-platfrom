namespace CoffeeShopAPI.Models
{
    public class VNPayPaymentRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
    }

    public class VNPayPaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class VNPayCallbackResponse
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string PayDate { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
