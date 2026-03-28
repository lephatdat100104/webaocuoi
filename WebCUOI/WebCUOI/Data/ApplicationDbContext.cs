using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WebCUOI.Models; // Đảm bảo đã import namespace của models
using Microsoft.AspNetCore.Identity; // Cho IdentityUser

namespace WebCUOI.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        internal object ChiTietDonHang;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AoCuoi> AoCuoi { get; set; }
        public DbSet<DichVuCuoiHoi> DichVuCuoiHoi { get; set; }
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Luôn gọi base.OnModelCreating đầu tiên khi dùng IdentityDbContext
            base.OnModelCreating(modelBuilder);

            // Cấu hình mối quan hệ giữa ChiTietDonHang và DonHang
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany(dh => dh.ChiTietDonHangs)
                .HasForeignKey(ct => ct.DonHangID)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ giữa ChiTietDonHang và AoCuoi
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.AoCuoi)
                .WithMany()
                .HasForeignKey(ct => ct.AoCuoiID)
                .IsRequired(false);

            // Cấu hình mối quan hệ giữa ChiTietDonHang và DichVuCuoiHoi
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DichVuCuoiHoi)
                .WithMany()
                .HasForeignKey(ct => ct.DichVuCuoiHoiID)
                .IsRequired(false);

            // Cấu hình độ chính xác cho thuộc tính TongTien của DonHang
            // Ví dụ: DECIMAL(18, 2) nghĩa là tổng 18 chữ số, 2 chữ số sau dấu thập phân.
            // Em có thể điều chỉnh 18 và 2 tùy theo độ lớn và độ chính xác của tiền tệ mà em cần.
            modelBuilder.Entity<DonHang>()
                .Property(dh => dh.TongTien)
                .HasPrecision(18, 2);
        }
    }
}