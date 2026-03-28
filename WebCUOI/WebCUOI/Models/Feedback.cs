using Microsoft.AspNetCore.Identity;

namespace WebCUOI.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        public string? UserId { get; set; }      // ✅ Cho phép null
        public IdentityUser? User { get; set; }  // ✅ Cho phép null

        public string Message { get; set; }
        public string? Reply { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RepliedAt { get; set; }
    }
}
