var builder = WebApplication.CreateBuilder(args);

// 1. Thêm các service cần thiết
builder.Services.AddDistributedMemoryCache(); // Bắt buộc cho session
builder.Services.AddSession(); // Bật session
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2. Cấu hình middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // <-- Bắt buộc để load ảnh, CSS, JS

app.UseRouting();

app.UseSession();// 💥 Bắt buộc: phải nằm sau UseRouting và trước UseAuthorization

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
