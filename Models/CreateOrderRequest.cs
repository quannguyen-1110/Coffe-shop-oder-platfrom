namespace CoffeeShopAPI.Models
{
    public class CreateOrderRequest
    {
        public List<OrderItemRequest> Items { get; set; } = new();
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? DeliveryPhone { get; set; }
        public string? DeliveryNote { get; set; }
        public string? ClientOrderId { get; set; } // FE t? generate ð? tránh duplicate
        public string PaymentMethod { get; set; } = "MoMo"; // MoMo | VNPay | Cash
    }

    public class OrderItemRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductType { get; set; } = "Drink"; // Drink | Cake
        public int Quantity { get; set; }
        public List<string>? ToppingIds { get; set; }
    }
}
