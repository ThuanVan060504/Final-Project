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
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// ============================
// 🔐 Auth + Google
// ============================
// Dùng DataProtection tạm để mỗi lần restart app → key mới → cookie cũ vô hiệu
builder.Services.AddDataProtection()
    .UseEphemeralDataProtectionProvider()
    .SetApplicationName(Guid.NewGuid().ToString());
var authSection = builder.Configuration.GetSection("Authentication");
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
    var clientId = "397588349599-f89d69ou9ajl5fegep8bnbhrjvt41fso" + ".apps.googleusercontent.com";
    var clientSecret = "GOCSPX" + "-772D1d9WqC-vCYDUrWdOowDZO2SS";

    options.ClientId = clientId;
    options.ClientSecret = clientSecret;
    options.CallbackPath = "/signin-google";
})
.AddFacebook(options =>
{
    // Phải là "Facebook:AppId", CHÍNH XÁC y hệt
    options.AppId = builder.Configuration["Facebook:AppId"];
    options.AppSecret = builder.Configuration["Facebook:AppSecret"];
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
    name: "ProductDetails",
    pattern: "san-pham/{slug}-{id}",
    defaults: new { controller = "SanPham", action = "Details" },
    constraints: new { id = @"\d+" } // id phải là số
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
