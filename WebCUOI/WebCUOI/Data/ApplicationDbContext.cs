using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // <<< Thêm dòng này
using WebCUOI.Models;
using Microsoft.AspNetCore.Identity;

namespace WebCUOI.Data
{
    // Thay DbContext bằng IdentityDbContext<IdentityUser>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> // Hoặc IdentityDbContext nếu bạn không muốn tùy chỉnh User
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AoCuoi> AoCuoi { get; set; }
        public DbSet<DichVuCuoiHoi> DichVuCuoiHoi { get; set; }
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHang { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Các cấu hình model của bạn ở đây
            // Lưu ý: Nếu bạn dùng IdentityDbContext, bạn phải gọi base.OnModelCreating(modelBuilder); đầu tiên
            // để các cấu hình cho IdentityDbContext được áp dụng trước.

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany()
                .HasForeignKey(ct => ct.DonHangID);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.AoCuoi)
                .WithMany()
                .HasForeignKey(ct => ct.AoCuoiID)
                .IsRequired(false);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DichVuCuoiHoi)
                .WithMany()
                .HasForeignKey(ct => ct.DichVuCuoiHoiID)
                .IsRequired(false);
        }
    }
}