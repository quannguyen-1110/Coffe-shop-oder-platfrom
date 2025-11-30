using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoffeeShopAPI.Repository;
using CoffeeShopAPI.Models;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
  [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly DrinkRepository _drinkRepo;
        private readonly CakeRepository _cakeRepo;
        private readonly ToppingRepository _toppingRepo;
        private readonly OrderRepository _orderRepo;
   private readonly UserRepository _userRepo;

        public DashboardController(
     DrinkRepository drinkRepo,
        CakeRepository cakeRepo,
            ToppingRepository toppingRepo,
            OrderRepository orderRepo,
       UserRepository userRepo)
        {
            _drinkRepo = drinkRepo;
_cakeRepo = cakeRepo;
        _toppingRepo = toppingRepo;
     _orderRepo = orderRepo;
            _userRepo = userRepo;
        }

    /// <summary>
        /// 📊 Admin Dashboard Overview - Tổng quan toàn diện
   /// </summary>
        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
     var drinks = await _drinkRepo.GetAllDrinksAsync();
            var cakes = await _cakeRepo.GetAllCakesAsync();
    var toppings = await _toppingRepo.GetAllToppingsAsync();
       var orders = await _orderRepo.GetAllOrdersAsync();
            var users = await _userRepo.GetAllUsersAsync();

      var today = DateTime.UtcNow.Date;
            var thisWeek = today.AddDays(-7);
var thisMonth = today.AddDays(-30);

          // 📈 Key Metrics Summary
          var todayOrders = orders.Where(o => o.CreatedAt.Date == today).ToList();
            var todayRevenue = todayOrders.Where(o => o.Status == "Completed").Sum(o => o.FinalPrice);
     var todayPendingOrders = todayOrders.Count(o => o.Status is "Pending" or "Processing");
            
  var thisWeekRevenue = orders.Where(o => o.CreatedAt >= thisWeek && o.Status == "Completed").Sum(o => o.FinalPrice);
     var lastWeekRevenue = orders.Where(o => o.CreatedAt >= thisWeek.AddDays(-7) && o.CreatedAt < thisWeek && o.Status == "Completed").Sum(o => o.FinalPrice);
            var revenueGrowth = lastWeekRevenue > 0 ? ((thisWeekRevenue - lastWeekRevenue) / lastWeekRevenue) * 100 : 0;

            // 📦 Inventory Summary
    var totalProducts = drinks.Count + cakes.Count + toppings.Count;
var lowStockCount = drinks.Count(d => d.Stock > 0 && d.Stock < 10) + 
           cakes.Count(c => c.Stock > 0 && c.Stock < 10) + 
     toppings.Count(t => t.Stock > 0 && t.Stock < 20);
       var outOfStockCount = drinks.Count(d => d.Stock == 0) + cakes.Count(c => c.Stock == 0) + toppings.Count(t => t.Stock == 0);

            var summary = new
        {
       // Metrics chính hiển thị ở top cards
   todayRevenue,
    todayOrders = todayOrders.Count,
          totalCustomers = users.Count(u => u.Role == "User"),
    pendingOrders = orders.Count(o => o.Status is "Pending" or "Processing" or "Confirmed"),
 
     // Growth indicators
      revenueGrowth = Math.Round(revenueGrowth, 1),
    ordersGrowth = CalculateOrdersGrowth(orders, today),
         customersGrowth = CalculateCustomersGrowth(users, today),
         
           // Inventory alerts
       lowStockCount,
     outOfStockCount,
        totalProducts,
   inventoryHealthScore = CalculateInventoryHealth(totalProducts, lowStockCount, outOfStockCount)
        };

    // 📊 Detailed Statistics
          var orderStats = GetOrderStatistics(orders, today, thisWeek, thisMonth);
      var userStats = GetUserStatistics(users, today, thisWeek, thisMonth);
       var inventoryStats = GetInventoryStatistics(drinks, cakes, toppings);
    var revenueChart = GetRevenueChartData(orders, 7); // Last 7 days
         var topProducts = GetTopSellingProducts(orders.Where(o => o.CreatedAt >= thisMonth && o.Status == "Completed").ToList());

            // 🔔 Recent Activities
            var recentActivities = GetRecentActivities(orders, users);

  // 🚨 Alerts & Notifications
      var alerts = GetSystemAlerts(orders, drinks, cakes, toppings, users);

       return Ok(new
  {
        summary,
          statistics = new
                {
                    orders = orderStats,
      users = userStats,
    inventory = inventoryStats
           },
 charts = new
        {
       revenueChart,
         topProducts,
    orderStatusDistribution = GetOrderStatusDistribution(orders),
    monthlyRevenueTrend = GetRevenueChartData(orders, 30)
             },
                recentActivities,
      alerts,
        timestamp = DateTime.UtcNow,
        refreshInterval = 300 // seconds
            });
        }

      /// <summary>
     /// 📊 Real-time Metrics - Lightweight endpoint for real-time updates
   /// </summary>
        [HttpGet("realtime")]
        public async Task<IActionResult> GetRealtimeMetrics()
        {
      var orders = await _orderRepo.GetAllOrdersAsync();
            var today = DateTime.UtcNow.Date;

        var todayOrders = orders.Where(o => o.CreatedAt.Date == today).ToList();
            var pendingOrders = orders.Count(o => o.Status is "Pending" or "Processing" or "Confirmed");
            var activeShippers = orders.Count(o => o.Status == "Shipping");

        return Ok(new
            {
      todayRevenue = todayOrders.Where(o => o.Status == "Completed").Sum(o => o.FinalPrice),
           todayOrders = todayOrders.Count,
 pendingOrders,
        activeShippers,
           lastUpdated = DateTime.UtcNow
            });
        }

        /// <summary>
        /// 📈 Performance Analytics - Chi tiết về hiệu suất kinh doanh
   /// </summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetPerformanceAnalytics([FromQuery] int days = 30)
        {
            var orders = await _orderRepo.GetAllOrdersAsync();
     var users = await _userRepo.GetAllUsersAsync();
       var startDate = DateTime.UtcNow.Date.AddDays(-days);

     var periodOrders = orders.Where(o => o.CreatedAt >= startDate).ToList();
            var completedOrders = periodOrders.Where(o => o.Status == "Completed").ToList();

    var analytics = new
            {
         period = $"Last {days} days",
           revenue = new
                {
  total = completedOrders.Sum(o => o.FinalPrice),
   average = completedOrders.Count > 0 ? completedOrders.Average(o => o.FinalPrice) : 0,
    daily = GetDailyRevenue(completedOrders, days),
         growth = CalculateRevenueGrowth(orders, startDate, days)
             },
            orders = new
                {
        total = periodOrders.Count,
          completed = completedOrders.Count,
cancelled = periodOrders.Count(o => o.Status == "Cancelled"),
  averageOrderValue = completedOrders.Count > 0 ? completedOrders.Average(o => o.FinalPrice) : 0,
       completionRate = periodOrders.Count > 0 ? (double)completedOrders.Count / periodOrders.Count * 100 : 0
    },
   customers = new
     {
           new_customers = users.Count(u => u.Role == "User" && u.CreatedAt >= startDate),
      returning_customers = GetReturningCustomers(completedOrders),
 retention_rate = CalculateCustomerRetentionRate(users, completedOrders, startDate)
 }
     };

  return Ok(analytics);
  }

   #region Helper Methods

        private static double CalculateOrdersGrowth(List<Order> orders, DateTime today)
        {
      var todayOrders = orders.Count(o => o.CreatedAt.Date == today);
      var yesterdayOrders = orders.Count(o => o.CreatedAt.Date == today.AddDays(-1));
    return yesterdayOrders > 0 ? ((double)(todayOrders - yesterdayOrders) / yesterdayOrders) * 100 : 0;
 }

    private static double CalculateCustomersGrowth(List<User> users, DateTime today)
 {
       var thisWeek = today.AddDays(-7);
  var lastWeek = today.AddDays(-14);
            
       var thisWeekCustomers = users.Count(u => u.Role == "User" && u.CreatedAt >= thisWeek);
            var lastWeekCustomers = users.Count(u => u.Role == "User" && u.CreatedAt >= lastWeek && u.CreatedAt < thisWeek);

            return lastWeekCustomers > 0 ? ((double)(thisWeekCustomers - lastWeekCustomers) / lastWeekCustomers) * 100 : 0;
   }

        private static double CalculateInventoryHealth(int totalProducts, int lowStock, int outOfStock)
 {
 if (totalProducts == 0) return 0;
 var healthyProducts = totalProducts - lowStock - outOfStock;
    return (double)healthyProducts / totalProducts * 100;
        }

  private static object GetOrderStatistics(List<Order> orders, DateTime today, DateTime thisWeek, DateTime thisMonth)
        {
return new
     {
today = new
     {
     total = orders.Count(o => o.CreatedAt.Date == today),
   revenue = orders.Where(o => o.CreatedAt.Date == today && o.Status == "Completed").Sum(o => o.FinalPrice),
          pending = orders.Count(o => o.CreatedAt.Date == today && o.Status == "Pending"),
          completed = orders.Count(o => o.CreatedAt.Date == today && o.Status == "Completed")
},
                thisWeek = new
      {
       total = orders.Count(o => o.CreatedAt >= thisWeek),
       revenue = orders.Where(o => o.CreatedAt >= thisWeek && o.Status == "Completed").Sum(o => o.FinalPrice),
           averageOrderValue = orders.Where(o => o.CreatedAt >= thisWeek && o.Status == "Completed")
   .DefaultIfEmpty().Average(o => o?.FinalPrice ?? 0)
      },
         thisMonth = new
        {
    total = orders.Count(o => o.CreatedAt >= thisMonth),
           revenue = orders.Where(o => o.CreatedAt >= thisMonth && o.Status == "Completed").Sum(o => o.FinalPrice)
     },
         status = new
            {
    pending = orders.Count(o => o.Status == "Pending"),
                    processing = orders.Count(o => o.Status == "Processing"),
           confirmed = orders.Count(o => o.Status == "Confirmed"),
   shipping = orders.Count(o => o.Status == "Shipping"),
         delivered = orders.Count(o => o.Status == "Delivered"),
          completed = orders.Count(o => o.Status == "Completed"),
    cancelled = orders.Count(o => o.Status == "Cancelled")
      }
       };
        }

        private static object GetUserStatistics(List<User> users, DateTime today, DateTime thisWeek, DateTime thisMonth)
        {
    return new
          {
       total = users.Count,
         active = users.Count(u => u.IsActive),
                customers = users.Count(u => u.Role == "User"),
    shippers = users.Count(u => u.Role == "Shipper"),
                admins = users.Count(u => u.Role == "Admin"),
          newToday = users.Count(u => u.CreatedAt.Date == today),
   newThisWeek = users.Count(u => u.CreatedAt >= thisWeek),
        newThisMonth = users.Count(u => u.CreatedAt >= thisMonth),
pendingShippers = users.Count(u => u.Role == "Shipper" && u.RegistrationStatus == "Pending"),
             approvedShippers = users.Count(u => u.Role == "Shipper" && u.RegistrationStatus == "Approved")
  };
        }

    private static object GetInventoryStatistics(List<Drink> drinks, List<Cake> cakes, List<Topping> toppings)
        {
       return new
            {
       drinks = new
                {
           total = drinks.Count,
        inStock = drinks.Count(d => d.Stock > 0),
   lowStock = drinks.Count(d => d.Stock > 0 && d.Stock < 10),
      outOfStock = drinks.Count(d => d.Stock == 0),
        totalValue = drinks.Sum(d => d.BasePrice * d.Stock)
         },
       cakes = new
       {
  total = cakes.Count,
         inStock = cakes.Count(c => c.Stock > 0),
          lowStock = cakes.Count(c => c.Stock > 0 && c.Stock < 10),
   outOfStock = cakes.Count(c => c.Stock == 0),
    totalValue = cakes.Sum(c => c.Price * c.Stock)
   },
   toppings = new
   {
     total = toppings.Count,
  inStock = toppings.Count(t => t.Stock > 0),
           lowStock = toppings.Count(t => t.Stock > 0 && t.Stock < 20),
     outOfStock = toppings.Count(t => t.Stock == 0),
  totalValue = toppings.Sum(t => t.Price * t.Stock)
     }
  };
        }

      private static List<object> GetRevenueChartData(List<Order> orders, int days)
        {
            var chartData = new List<object>();
      var today = DateTime.UtcNow.Date;

            for (int i = days - 1; i >= 0; i--)
            {
   var date = today.AddDays(-i);
       var dayOrders = orders.Where(o => o.CreatedAt.Date == date).ToList();
                var dayRevenue = dayOrders.Where(o => o.Status == "Completed").Sum(o => o.FinalPrice);

          chartData.Add(new
      {
    date = date.ToString("yyyy-MM-dd"),
  dayName = date.ToString("ddd"),
   revenue = dayRevenue,
      orders = dayOrders.Count,
   completed = dayOrders.Count(o => o.Status == "Completed")
     });
        }

        return chartData;
      }

     private static List<object> GetTopSellingProducts(List<Order> completedOrders)
  {
            var productSales = new Dictionary<string, dynamic>();

            foreach (var order in completedOrders)
            {
     if (order.Items != null)
             {
             foreach (var item in order.Items)
{
           var key = $"{item.ProductType}_{item.ProductId}";
       if (!productSales.ContainsKey(key))
                 {
     productSales[key] = new
           {
   productId = item.ProductId,
        productName = item.ProductName,
   productType = item.ProductType,
  totalQuantity = item.Quantity,
        totalRevenue = item.TotalPrice,
   orderCount = 1
          };
         }
                else
                  {
         var existing = productSales[key];
 productSales[key] = new
       {
           productId = existing.productId,
    productName = existing.productName,
      productType = existing.productType,
             totalQuantity = existing.totalQuantity + item.Quantity,
   totalRevenue = existing.totalRevenue + item.TotalPrice,
         orderCount = existing.orderCount + 1
        };
             }
          }
   }
         }

        return productSales.Values
         .OrderByDescending(p => p.totalQuantity)
                .Take(10)
  .Cast<object>()
          .ToList();
        }

        private static List<object> GetRecentActivities(List<Order> orders, List<User> users)
        {
    var activities = new List<object>();

    // Recent orders (last 10)
   var recentOrders = orders.OrderByDescending(o => o.CreatedAt).Take(8);
            foreach (var order in recentOrders)
            {
 activities.Add(new
{
id = order.OrderId,
           type = "order",
               action = GetOrderActionText(order.Status),
  description = $"Đơn hàng #{order.OrderId[..8]} - ₫{order.FinalPrice:N0}",
 amount = order.FinalPrice,
      timestamp = order.CreatedAt,
   status = order.Status,
        icon = GetOrderIcon(order.Status),
            priority = GetActivityPriority(order.Status)
   });
       }

            // Recent user registrations
            var recentUsers = users.Where(u => u.Role == "User")
   .OrderByDescending(u => u.CreatedAt).Take(5);
            foreach (var user in recentUsers)
      {
 activities.Add(new
   {
             id = user.UserId,
        type = "user",
        action = "registered",
                  description = $"Khách hàng mới: {user.Username}",
   amount = (decimal?)null,
      timestamp = user.CreatedAt,
         status = "completed",
         icon = "👤",
          priority = "low"
             });
            }

            // Recent shipper activities
       var recentShippers = users.Where(u => u.Role == "Shipper" && u.RegistrationStatus == "Approved")
       .OrderByDescending(u => u.ApprovedAt).Take(3);
         foreach (var shipper in recentShippers)
     {
                activities.Add(new
      {
     id = shipper.UserId,
 type = "shipper",
       action = "approved",
   description = $"Shipper mới: {shipper.FullName ?? shipper.Username}",
            amount = (decimal?)null,
         timestamp = shipper.ApprovedAt ?? shipper.CreatedAt,
        status = "completed",
    icon = "🛵",
      priority = "medium"
                });
     }

            return activities.OrderByDescending(a => ((dynamic)a).timestamp).Take(15).ToList();
        }

   private static List<object> GetSystemAlerts(List<Order> orders, List<Drink> drinks, List<Cake> cakes, List<Topping> toppings, List<User> users)
   {
            var alerts = new List<object>();

            // Inventory alerts
            var lowStockDrinks = drinks.Where(d => d.Stock > 0 && d.Stock < 10).ToList();
      var outOfStockDrinks = drinks.Where(d => d.Stock == 0).ToList();
            var lowStockCakes = cakes.Where(c => c.Stock > 0 && c.Stock < 10).ToList();
            var outOfStockCakes = cakes.Where(c => c.Stock == 0).ToList();

         if (outOfStockDrinks.Any() || outOfStockCakes.Any())
            {
    alerts.Add(new
      {
           type = "inventory",
   severity = "critical",
  title = "Out of Stock Alert",
      message = $"{outOfStockDrinks.Count + outOfStockCakes.Count} items are out of stock",
  action = "Restock immediately",
             timestamp = DateTime.UtcNow,
icon = "⚠️"
  });
            }

  if (lowStockDrinks.Any() || lowStockCakes.Any())
    {
       alerts.Add(new
      {
        type = "inventory",
      severity = "warning",
    title = "Low Stock Alert",
 message = $"{lowStockDrinks.Count + lowStockCakes.Count} items are running low",
         action = "Consider restocking",
     timestamp = DateTime.UtcNow,
   icon = "📦"
  });
   }

   // Order processing alerts
var pendingOrders = orders.Count(o => o.Status == "Processing");
  if (pendingOrders > 5)
        {
       alerts.Add(new
          {
        type = "orders",
          severity = "high",
        title = "High Pending Orders",
message = $"{pendingOrders} orders waiting for confirmation",
                action = "Review and confirm orders",
   timestamp = DateTime.UtcNow,
   icon = "📋"
     });
     }

       // Shipper registration alerts
 var pendingShippers = users.Count(u => u.Role == "Shipper" && u.RegistrationStatus == "Pending");
 if (pendingShippers > 0)
            {
        alerts.Add(new
                {
         type = "shippers",
          severity = "medium",
          title = "Pending Shipper Applications",
         message = $"{pendingShippers} shipper applications need review",
          action = "Review applications",
          timestamp = DateTime.UtcNow,
     icon = "🛵"
             });
     }

  return alerts.Take(10).ToList();
        }

   private static object GetOrderStatusDistribution(List<Order> orders)
        {
  var total = orders.Count;
        if (total == 0) return new { };

       return new
     {
             pending = new { count = orders.Count(o => o.Status == "Pending"), percentage = Math.Round((double)orders.Count(o => o.Status == "Pending") / total * 100, 1) },
           processing = new { count = orders.Count(o => o.Status == "Processing"), percentage = Math.Round((double)orders.Count(o => o.Status == "Processing") / total * 100, 1) },
      confirmed = new { count = orders.Count(o => o.Status == "Confirmed"), percentage = Math.Round((double)orders.Count(o => o.Status == "Confirmed") / total * 100, 1) },
     shipping = new { count = orders.Count(o => o.Status == "Shipping"), percentage = Math.Round((double)orders.Count(o => o.Status == "Shipping") / total * 100, 1) },
       completed = new { count = orders.Count(o => o.Status == "Completed"), percentage = Math.Round((double)orders.Count(o => o.Status == "Completed") / total * 100, 1) }
    };
 }

        private static string GetOrderActionText(string status) => status switch
        {
        "Pending" => "created",
    "Processing" => "payment confirmed",
     "Confirmed" => "confirmed by admin",
            "Shipping" => "out for delivery",
      "Delivered" => "delivered",
            "Completed" => "completed",
     "Cancelled" => "cancelled",
     _ => "updated"
 };

   private static string GetOrderIcon(string status) => status switch
        {
            "Pending" => "⏳",
     "Processing" => "💳",
          "Confirmed" => "✅",
      "Shipping" => "🚚",
            "Delivered" => "📦",
            "Completed" => "🎉",
         "Cancelled" => "❌",
            _ => "📋"
 };

        private static string GetActivityPriority(string status) => status switch
        {
       "Pending" or "Processing" => "high",
 "Confirmed" or "Shipping" => "medium",
          _ => "low"
        };

        private static List<object> GetDailyRevenue(List<Order> completedOrders, int days)
        {
            var today = DateTime.UtcNow.Date;
            var dailyRevenue = new List<object>();

            for (int i = days - 1; i >= 0; i--)
 {
                var date = today.AddDays(-i);
     var dayRevenue = completedOrders.Where(o => o.CreatedAt.Date == date).Sum(o => o.FinalPrice);
      dailyRevenue.Add(new
        {
        date = date.ToString("yyyy-MM-dd"),
    revenue = dayRevenue
     });
}

            return dailyRevenue;
    }

        private static double CalculateRevenueGrowth(List<Order> orders, DateTime startDate, int days)
        {
            var currentPeriod = orders.Where(o => o.CreatedAt >= startDate && o.Status == "Completed").Sum(o => o.FinalPrice);
       var previousPeriodStart = startDate.AddDays(-days);
        var previousPeriod = orders.Where(o => o.CreatedAt >= previousPeriodStart && o.CreatedAt < startDate && o.Status == "Completed").Sum(o => o.FinalPrice);

    return previousPeriod > 0 ? (double)((currentPeriod - previousPeriod) / previousPeriod) * 100 : 0;
        }

        private static int GetReturningCustomers(List<Order> completedOrders)
        {
 var customerOrderCounts = completedOrders.GroupBy(o => o.UserId).Where(g => g.Count() > 1);
            return customerOrderCounts.Count();
        }

      private static double CalculateCustomerRetentionRate(List<User> users, List<Order> completedOrders, DateTime startDate)
        {
            var activeCustomers = completedOrders.Select(o => o.UserId).Distinct().ToList();
     var totalCustomers = users.Count(u => u.Role == "User" && u.CreatedAt < startDate);
       
     return totalCustomers > 0 ? (double)activeCustomers.Count / totalCustomers * 100 : 0;
        }

        #endregion
    }
}