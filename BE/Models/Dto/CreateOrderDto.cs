namespace BE.Models.Dto
{
    public class CreateOrderDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
    }
}
