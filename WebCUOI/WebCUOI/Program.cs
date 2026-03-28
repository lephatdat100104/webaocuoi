using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Cần dòng này
using WebCUOI.Models;
using WebCUOI.Data;
using WebCUOI.Services; // Thêm dòng này để sử dụng EmailSender của em
using Microsoft.AspNetCore.Identity.UI.Services; // Thêm dòng này để sử dụng IEmailSender tiêu chuẩn
using WebCUOI.VNPAY;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache(); // Cần thiết cho Session State
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian Session hết hạn
    options.Cookie.HttpOnly = true; // Cookie chỉ truy cập được qua HTTP
    options.Cookie.IsEssential = true; // Quan trọng cho GDPR
});

// 🟢 Đăng ký ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🟢 Cấu hình Identity Services
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 🟢 Dịch vụ Email
builder.Services.AddTransient<WebCUOI.Services.IEmailSender, EmailSender>();
builder.Services.AddRazorPages();

// 🟢 Dịch vụ VNPAY
builder.Services.AddScoped<WebCUOI.Services.IVnPayService, WebCUOI.Services.VnPayService>();
builder.Services.Configure<VnpayConfig>(builder.Configuration.GetSection("VnpayConfig"));
builder.Services.AddSingleton<VnpayLibrary>();
builder.Services.AddScoped<NotificationService>();


// 🟢 Cấu hình CORS (phải đặt TRƯỚC builder.Build())
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build(); // Gọi builder.Build() MỘT LẦN DUY NHẤT ở đây

// Configure the HTTP request pipeline.
// === CHỈ SỬA ĐOẠN NÀY THÔI!!! ===
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();        // ĐẨY LÊN TRÊN CÙNG – ẢNH wwwroot HIỆN NGAY!!!
app.UseSession();
app.UseRouting();            // ĐỂ SAU UseStaticFiles
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// 🟢 Seed dữ liệu (Roles + Admin)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Áp dụng migration
        context.Database.Migrate();

        // Tạo roles nếu chưa có
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Tạo tài khoản Admin nếu chưa có
        var adminUser = await userManager.FindByEmailAsync("anhhoang25102004@gmail.com");
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = "anhhoang25102004@gmail.com",
                Email = "anhhoang25102004@gmail.com",
                EmailConfirmed = true
            };
            var createAdminResult = await userManager.CreateAsync(adminUser, "Tai123!");
            if (createAdminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("✅ Admin user created and assigned 'Admin' role.");
            }
            else
            {
                var errors = string.Join(", ", createAdminResult.Errors.Select(e => e.Description));
                services.GetRequiredService<ILogger<Program>>()
                    .LogError($"Failed to create admin user: {errors}");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database with roles and admin user.");
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

// 🟢 Map routes
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
