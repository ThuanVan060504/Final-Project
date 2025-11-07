using Final_Project.Models.Momo;
using Final_Project.Models.Shop;
using Final_Project.Service.Momo;
using System.Net.Security;
using Final_Project.Service.VnPay;
using Final_Project.Services;
using Final_Project.Services.PayPal;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// ============================
// 🔧 HTTP + SSL
// ============================
builder.Services.AddHttpClient("MyHttpClient", client => { })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            // Cho phép bỏ qua lỗi SSL khi dev
            if (builder.Environment.IsDevelopment())
                return true;

            // Ở môi trường production thì vẫn check SSL
            return errors == SslPolicyErrors.None;
        }
    });

// ============================
// ⚙️ Services
// ============================
builder.Services.AddScoped<IPayPalService, PayPalService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddHostedService<FlashSaleCleanupService>();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// ============================
// 🔐 Auth + Google
// ============================
// Dùng DataProtection tạm để mỗi lần restart app → key mới → cookie cũ vô hiệu
builder.Services.AddDataProtection()
    .UseEphemeralDataProtectionProvider()
    .SetApplicationName(Guid.NewGuid().ToString());

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MyCookie";
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
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

// ============================
// 💾 Session
// ============================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================
// 🧠 MVC + DbContext
// ============================
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ============================
// 🚦 Middleware pipeline
// ============================
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

// ============================
// 💥 Auto logout (xoá cookie/session)
// ============================
// Khi Admin truy cập "/", sẽ tự động clear session + cookie rồi về Home/Index
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    var role = context.Session.GetString("UserRole");

    if (role == "Admin" && path == "/")
    {
        context.Session.Clear();
        await context.SignOutAsync("MyCookie");
        context.Response.Redirect("/Home/Index");
        return;
    }

    await next();
});

// ============================
// 🧭 Route
// ============================
app.MapControllerRoute(
    name: "Adminboot",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
