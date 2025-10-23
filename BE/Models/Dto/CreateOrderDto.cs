namespace BE.Models.Dto
{
    public class CreateOrderDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<BE.Models.OrderItem> Items { get; set; } = new();
    }
}
