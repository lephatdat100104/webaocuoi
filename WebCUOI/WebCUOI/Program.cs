using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // <<< Cần dòng này
using WebCUOI.Models;
using WebCUOI.Data;


; // <<< Cần dòng này, thay WebCUOI bằng tên dự án của bạn

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//
builder.Services.AddDistributedMemoryCache(); // Cần thiết cho Session State
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian Session hết hạn
    options.Cookie.HttpOnly = true; // Cookie chỉ truy cập được qua HTTP
    options.Cookie.IsEssential = true; // Quan trọng cho GDPR
});
// Thêm dòng này để đăng ký ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity Services
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession(); // Kích hoạt Session Middleware
app.UseRouting();
// Quan trọng: Phải có UseAuthentication và UseAuthorization
app.UseAuthentication(); // Thêm dòng này (phải trước UseAuthorization)
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();