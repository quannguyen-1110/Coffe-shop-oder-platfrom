using Amazon.LocationService;
using Amazon.LocationService.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Services
{
    public class ShippingService
    {
        private readonly IConfiguration _configuration;
        private readonly string _shopAddress;
        private readonly decimal _basePrice;
        private readonly decimal _baseDistance;
        private readonly decimal _pricePerKm;

        public ShippingService(IConfiguration configuration)
        {
            _configuration = configuration;
            _shopAddress = configuration["Shipping:ShopAddress"] ?? "10.771479,106.704170"; // Default: Bitexco Tower, District 1, HCMC
            _basePrice = decimal.Parse(configuration["Shipping:BasePrice"] ?? "15000");
            _baseDistance = decimal.Parse(configuration["Shipping:BaseDistance"] ?? "3");
            _pricePerKm = decimal.Parse(configuration["Shipping:PricePerKm"] ?? "5000");
        }

        /// <summary>
        /// Tính khoảng cách từ shop đến địa chỉ giao hàng (km)
        /// Sử dụng AWS Location Service
        /// </summary>
        public async Task<decimal> CalculateDistanceAsync(string deliveryAddress)
        {
            try
            {
                var useAWSLocation = _configuration.GetValue<bool>("Shipping:UseAWSLocation", false);
                
                if (!useAWSLocation)
                {
                    Console.WriteLine("📐 AWS Location disabled, using fallback");
                    return CalculateFallbackDistance(deliveryAddress);
                }

                var placeIndexName = _configuration["AWS:Location:PlaceIndexName"];
                var routeCalculatorName = _configuration["AWS:Location:RouteCalculatorName"];
                
                if (string.IsNullOrEmpty(placeIndexName) || string.IsNullOrEmpty(routeCalculatorName))
                {
                    Console.WriteLine("⚠️ AWS Location not configured, using fallback");
                    return CalculateFallbackDistance(deliveryAddress);
                }

                Console.WriteLine($"🌐 Using AWS Location Service...");
                Console.WriteLine($"   Shop: {_shopAddress}");
                Console.WriteLine($"   Delivery: {deliveryAddress}");

                var client = new AmazonLocationServiceClient();

                // Geocode shop address
                var shopCoords = ParseCoordinates(_shopAddress);
                Console.WriteLine($"   Shop coords: [{shopCoords[0]}, {shopCoords[1]}]");

                // Geocode delivery address
                var deliveryCoords = await GeocodeAddressAsync(client, placeIndexName, deliveryAddress);
                Console.WriteLine($"   Delivery coords: [{deliveryCoords[0]}, {deliveryCoords[1]}]");

                // Calculate route distance
                var distance = await CalculateRouteDistanceAsync(client, routeCalculatorName, shopCoords, deliveryCoords);
                Console.WriteLine($"✅ AWS Location: Distance = {distance:F2} km");
                
                // Validation: If distance is suspiciously small, use Haversine as fallback
                if (distance < 0.1m)
                {
                    Console.WriteLine($"⚠️ Distance too small ({distance:F4} km), likely geocoding error");
                    Console.WriteLine($"🔄 Recalculating with Haversine formula...");
                    
                    // Convert coords back to lat/lng for Haversine
                    var haversineDistance = CalculateHaversineDistance(
                        shopCoords[1], shopCoords[0],      // shop: lat, lng
                        deliveryCoords[1], deliveryCoords[0] // delivery: lat, lng
                    );
                    
                    Console.WriteLine($"📐 Haversine: Distance = {haversineDistance:F2} km");
                    
                    // If Haversine also shows small distance, geocoding might be correct
                    if (haversineDistance < 0.5m)
                    {
                        Console.WriteLine($"✅ Confirmed: Delivery is very close to shop");
                        return distance;
                    }
                    
                    // Otherwise, geocoding failed, use fallback
                    Console.WriteLine($"❌ Geocoding failed, using fallback distance");
                    return CalculateFallbackDistance(deliveryAddress);
                }
                
                return distance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AWS Location Error: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                Console.WriteLine("🔄 Using fallback calculation...");
                return CalculateFallbackDistance(deliveryAddress);
            }
        }

        /// <summary>
        /// Tính phí ship dựa trên khoảng cách
        /// </summary>
        public decimal CalculateShippingFee(decimal distanceKm)
        {
            if (distanceKm <= _baseDistance)
                return _basePrice;

            var extraKm = distanceKm - _baseDistance;
            return _basePrice + (extraKm * _pricePerKm);
        }

        /// <summary>
        /// Tính phí ship cho đơn hàng (bao gồm cả tính khoảng cách)
        /// </summary>
        public async Task<ShippingCalculation> CalculateShippingAsync(string deliveryAddress)
        {
            var distance = await CalculateDistanceAsync(deliveryAddress);
            var fee = CalculateShippingFee(distance);

            return new ShippingCalculation
            {
                DistanceKm = distance,
                ShippingFee = fee,
                EstimatedTime = EstimateDeliveryTime(distance)
            };
        }

        // ========== PRIVATE HELPERS ==========

        private async Task<List<double>> GeocodeAddressAsync(AmazonLocationServiceClient client, string placeIndexName, string address)
        {
            var request = new SearchPlaceIndexForTextRequest
            {
                IndexName = placeIndexName,
                Text = address,
                MaxResults = 1
            };

            var response = await client.SearchPlaceIndexForTextAsync(request);

            if (response.Results.Count == 0)
                throw new Exception($"Cannot geocode address: {address}");

            var point = response.Results[0].Place.Geometry.Point;
            // AWS Location returns [lng, lat], keep it as is
            return new List<double> { point[0], point[1] }; // [lng, lat]
        }

        private async Task<decimal> CalculateRouteDistanceAsync(
            AmazonLocationServiceClient client,
            string routeCalculatorName,
            List<double> origin,
            List<double> destination)
        {
            var request = new CalculateRouteRequest
            {
                CalculatorName = routeCalculatorName,
                DeparturePosition = origin, // Already [lng, lat]
                DestinationPosition = destination // Already [lng, lat]
            };

            var response = await client.CalculateRouteAsync(request);
            var distanceMeters = response.Summary.Distance;
            return (decimal)(distanceMeters / 1000); // Convert to km
        }

        private List<double> ParseCoordinates(string coords)
        {
            var parts = coords.Split(',');
            if (parts.Length != 2)
                throw new Exception("Invalid coordinates format. Expected: lat,lng");

            var lat = double.Parse(parts[0].Trim());
            var lng = double.Parse(parts[1].Trim());
            
            // Convert from "lat,lng" format to [lng, lat] for AWS Location
            return new List<double> { lng, lat };
        }

        /// <summary>
        /// Tính khoảng cách Haversine (đường chim bay)
        /// </summary>
        private decimal CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;

            return (decimal)distance;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Fallback: Tính khoảng cách giả lập dựa trên độ dài địa chỉ
        /// (Chỉ dùng khi chưa setup AWS Location Service)
        /// </summary>
        private decimal CalculateFallbackDistance(string address)
        {
            // Simple heuristic: longer address = farther distance
            var length = address.Length;
            if (length < 30) return 2m;
            if (length < 50) return 5m;
            if (length < 70) return 8m;
            return 10m;
        }

        /// <summary>
        /// Ước tính thời gian giao hàng (phút)
        /// </summary>
        private int EstimateDeliveryTime(decimal distanceKm)
        {
            // Giả sử tốc độ trung bình 20km/h trong thành phố
            const decimal avgSpeedKmh = 20m;
            var hours = distanceKm / avgSpeedKmh;
            var minutes = (int)(hours * 60);

            // Thêm 10 phút chuẩn bị
            return minutes + 10;
        }

        public class ShippingCalculation
        {
            public decimal DistanceKm { get; set; }
            public decimal ShippingFee { get; set; }
            public int EstimatedTime { get; set; } // minutes
        }
    }
}
