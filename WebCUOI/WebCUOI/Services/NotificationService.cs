using WebCUOI.Data;
using WebCUOI.Models;

namespace WebCUOI.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Tạo thông báo mới
        public async Task CreateAsync(string userId, string content)
        {
            var notify = new Notification
            {
                UserId = userId,
                Content = content
            };
            _context.Notifications.Add(notify);
            await _context.SaveChangesAsync();
        }

        // ✅ Lấy danh sách chưa đọc
        public List<Notification> GetUnread(string userId)
        {
            return _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }
    }
}
