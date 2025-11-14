namespace CoffeeShopAPI.Models
{
    public class MoMoPaymentResponse
    {
        public bool Success { get; set; }
        public string PayUrl { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public string DeepLink { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class MoMoCallbackResponse
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
