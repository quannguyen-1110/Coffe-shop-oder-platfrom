namespace CoffeeShopAPI.Models
{
    public class CreateOrderRequest
    {
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductType { get; set; } = "Drink"; // Drink | Cake
        public int Quantity { get; set; }
        public List<string>? ToppingIds { get; set; }
    }
}
