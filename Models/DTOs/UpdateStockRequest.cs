namespace CoffeeShopAPI.Models.DTOs
{
    public class UpdateStockRequest
    {
        public int Stock { get; set; }
    }

    public class UpdateDrinkRequest
    {
        public string? Name { get; set; }
        public decimal? BasePrice { get; set; }
        public int? Stock { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateCakeRequest
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateToppingRequest
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string? ImageUrl { get; set; }
    }
}
