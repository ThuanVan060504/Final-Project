using Final_Project.Models.Shop;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm các service cần thiết
builder.Services.AddDistributedMemoryCache(); // Bắt buộc cho session
builder.Services.AddAuthentication("MyCookie")
    .AddCookie("MyCookie", options =>
    {
        options.LoginPath = "/Dangnhap"; // nếu chưa login sẽ chuyển tới đây
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ⏰ Session tồn tại 30 phút
    options.Cookie.HttpOnly = true; // 🔐 An toàn
    options.Cookie.IsEssential = true; // ✅ Bắt buộc cho hoạt động
});

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. Cấu hình middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ⛅️ Load ảnh, CSS, JS

app.UseRouting();
app.UseSession(); // 💥 Phải có dòng này trước UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

// Cấu hình route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
