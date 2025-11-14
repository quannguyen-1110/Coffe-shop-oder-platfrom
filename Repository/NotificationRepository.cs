using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CoffeeShopAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Repository
{
    public class NotificationRepository
    {
        private readonly DynamoDBContext _context;

        public NotificationRepository(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await _context.SaveAsync(notification);
        }

        public async Task<Notification?> GetNotificationByIdAsync(string notificationId)
        {
            return await _context.LoadAsync<Notification>(notificationId);
        }

        public async Task UpdateNotificationAsync(Notification notification)
        {
            await _context.SaveAsync(notification);
        }

        /// <summary>
        /// Lấy tất cả notifications của user (mới nhất trước)
        /// </summary>
        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit = 50)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("UserId", ScanOperator.Equal, userId)
            };

            var results = await _context.ScanAsync<Notification>(conditions).GetRemainingAsync();
            return results.OrderByDescending(n => n.CreatedAt).Take(limit).ToList();
        }

        /// <summary>
        /// Lấy notifications chưa đọc của user
        /// </summary>
        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("UserId", ScanOperator.Equal, userId),
                new ScanCondition("IsRead", ScanOperator.Equal, false)
            };

            var results = await _context.ScanAsync<Notification>(conditions).GetRemainingAsync();
            return results.OrderByDescending(n => n.CreatedAt).ToList();
        }

        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        public async Task MarkAsReadAsync(string notificationId)
        {
            var notification = await _context.LoadAsync<Notification>(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveAsync(notification);
            }
        }

        /// <summary>
        /// Đánh dấu tất cả notifications của user đã đọc
        /// </summary>
        public async Task MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await GetUnreadNotificationsAsync(userId);
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveAsync(notification);
            }
        }

        /// <summary>
        /// Xóa notification
        /// </summary>
        public async Task DeleteNotificationAsync(string notificationId)
        {
            await _context.DeleteAsync<Notification>(notificationId);
        }

        /// <summary>
        /// Đếm số notifications chưa đọc
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var unread = await GetUnreadNotificationsAsync(userId);
            return unread.Count;
        }
    }
}
