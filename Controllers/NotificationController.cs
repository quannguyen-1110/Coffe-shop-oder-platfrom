using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoffeeShopAPI.Repository;
using System.Security.Claims;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Tất cả users đều có thể xem notifications
    public class NotificationController : ControllerBase
    {
        private readonly NotificationRepository _notificationRepo;

        public NotificationController(NotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        /// <summary>
        /// Lấy tất cả notifications của user (mới nhất trước)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int limit = 50)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Invalid token" });

                var notifications = await _notificationRepo.GetUserNotificationsAsync(userId, limit);

                return Ok(new
                {
                    total = notifications.Count,
                    unreadCount = notifications.Count(n => !n.IsRead),
                    notifications = notifications.Select(n => new
                    {
                        n.NotificationId,
                        n.Type,
                        n.Title,
                        n.Message,
                        n.OrderId,
                        n.ShipperId,
                        n.IsRead,
                        n.CreatedAt,
                        n.ReadAt,
                        data = n.Data != null ? System.Text.Json.JsonSerializer.Deserialize<object>(n.Data) : null
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy notifications chưa đọc
        /// </summary>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Invalid token" });

                var notifications = await _notificationRepo.GetUnreadNotificationsAsync(userId);

                return Ok(new
                {
                    count = notifications.Count,
                    notifications = notifications.Select(n => new
                    {
                        n.NotificationId,
                        n.Type,
                        n.Title,
                        n.Message,
                        n.OrderId,
                        n.CreatedAt,
                        data = n.Data != null ? System.Text.Json.JsonSerializer.Deserialize<object>(n.Data) : null
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Đếm số notifications chưa đọc (cho badge)
        /// </summary>
        [HttpGet("unread/count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Invalid token" });

                var count = await _notificationRepo.GetUnreadCountAsync(userId);

                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Invalid token" });

                // Verify notification belongs to user
                var notification = await _notificationRepo.GetNotificationByIdAsync(notificationId);
                if (notification == null)
                    return NotFound(new { error = "Notification not found" });

                if (notification.UserId != userId)
                    return Forbid();

                await _notificationRepo.MarkAsReadAsync(notificationId);

                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu tất cả notifications đã đọc
        /// </summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Invalid token" });

                await _notificationRepo.MarkAllAsReadAsync(userId);

                return Ok(new { message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa notification
        /// </summary>
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(string notificationId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Invalid token" });

                // Verify notification belongs to user
                var notification = await _notificationRepo.GetNotificationByIdAsync(notificationId);
                if (notification == null)
                    return NotFound(new { error = "Notification not found" });

                if (notification.UserId != userId)
                    return Forbid();

                await _notificationRepo.DeleteNotificationAsync(notificationId);

                return Ok(new { message = "Notification deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Helper method to get userId from token (works for both Cognito and Local JWT)
        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("sub")?.Value
                   ?? User.FindFirst("cognito:username")?.Value;
        }
    }
}
