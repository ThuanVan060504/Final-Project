using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Final_Project.Models.Shop;

namespace Final_Project.Services
{
    public class FlashSaleCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromHours(6);

        public FlashSaleCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = DateTime.Now;

                    // ❌ Xoá các Flash Sale đã hết hạn + phục hồi giá gốc
                    var expiredFlashSales = db.FlashSales.Where(f => f.ThoiGianKetThuc <= now).ToList();
                    foreach (var fs in expiredFlashSales)
                    {
                        var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == fs.MaSP);
                        if (sanPham != null && sanPham.GiaGoc != null)
                        {
                            sanPham.DonGia = sanPham.GiaGoc.Value;
                            sanPham.GiaGoc = null; 
                        }
                    }

                    db.FlashSales.RemoveRange(expiredFlashSales);
                    await db.SaveChangesAsync();

                    Console.WriteLine($"[FlashSaleCleanup] Xoá {expiredFlashSales.Count} FlashSale và phục hồi giá gốc.");

                    // ✅ Tạo mới 8 sản phẩm Flash Sale nếu không còn cái nào
                    if (!db.FlashSales.Any(f => f.ThoiGianKetThuc > now))
                    {
                        var products = db.SanPhams
                            .Where(p => p.GiaGoc == null) 
                            .OrderBy(r => Guid.NewGuid())
                            .Take(10) 
                            .ToList();

                        foreach (var p in products)
                        {
                            int discountPercent = random.Next(20, 61); 
                            if (p.GiaGoc == null) 
                                p.GiaGoc = p.DonGia;

                            p.DonGia = p.DonGia * (1 - discountPercent / 100.0M); 

                            db.FlashSales.Add(new FlashSale
                            {
                                MaSP = p.MaSP,
                                ThoiGianBatDau = now,
                                ThoiGianKetThuc = now.AddHours(6),
                                DiscountPercent = discountPercent
                            });
                        }

                        await db.SaveChangesAsync();
                        Console.WriteLine("[FlashSaleCleanup] Tạo mới 8 sản phẩm Flash Sale.");
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
