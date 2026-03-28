using Microsoft.AspNetCore.Identity;

namespace WebCUOI.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }  // Thêm dòng này

        public int Score { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
