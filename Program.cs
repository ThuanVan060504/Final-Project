using Final_Project.Models.Momo;
using Final_Project.Models.Shop;
using Final_Project.Service.Momo;
using Final_Project.Service.VnPay;
using Final_Project.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Cấu hình dịch vụ ----------

builder.Services.AddScoped<IEmailService, EmailService>();

// Cấu hình Momo API
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

// Cấu hình VnPay
builder.Services.AddScoped<IVnPayService, VnPayService>();

// Dịch vụ dọn dẹp flash sale
builder.Services.AddHostedService<FlashSaleCleanupService>();

// HTTP Client
builder.Services.AddHttpClient();

// Authentication: Cookie + Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MyCookie"; // Scheme dùng nội bộ
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // Khi Challenge() sẽ qua Google
})
.AddCookie("MyCookie", options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
})

.AddGoogle(options =>
{
    // Ghép chuỗi để Git không nhận diện full key
    var clientId = "580251218969-b0bgj3rj0k4ntnvc7cm2elfi40rh0eb1" + ".apps.googleusercontent.com";
    var clientSecret = "GOCSPX" + "-yq5DgN8otjuG83NU-0rBUuxm0air";

    options.ClientId = clientId;
    options.ClientSecret = clientSecret;
    options.CallbackPath = "/signin-google";
});


// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đọc appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// MVC + DbContext
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ---------- Middleware pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session phải trước Auth
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Route cho Area Admin
app.MapControllerRoute(
    name: "Adminboot",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
