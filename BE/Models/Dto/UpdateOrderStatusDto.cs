namespace BE.Models.Dto
{
    public class UpdateOrderStatusDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
