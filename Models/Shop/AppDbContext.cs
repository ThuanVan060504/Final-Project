
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Final_Project.Models.Shop
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ✅ DbSet cho các bảng thật sự trong database
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<DiaChiNguoiDung> DiaChiNguoiDungs { get; set; }
        public DbSet<PhanHoi> PhanHois { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<FlashSale> FlashSales { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
     
        public DbSet<GioHang> GioHangs { get; set; }

        public DbSet<ThuongHieu> ThuongHieus { get; set; }
        public DbSet<SanPhamYeuThich> SanPhamYeuThichs { get; set; }

        public DbSet<Decor> Decors { get; set; }
        public DbSet<DanhMucDecor> DanhMucDecors { get; set; }
        public object DonHang { get; internal set; }
        public object ChiTietDonHang { get; internal set; }
        public object SanPham { get; internal set; }
        public DbSet<NhaCungCap> NhaCungCaps { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ⚠️ Luôn giữ dòng này!

            // ✅ Khai báo entity TinTuc không có key (chỉ dùng đọc dữ liệu từ RSS hoặc API)
            modelBuilder.Entity<TinTuc>().HasNoKey();

            // ✅ Các cấu hình decimal cho các bảng có tiền tệ
            modelBuilder.Entity<SanPham>()
                .Property(p => p.DonGia)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ChiTietDonHang>()
                .Property(c => c.DonGia)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DonHang>()
                .Property(d => d.GiamGia)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DonHang>()
                .Property(d => d.PhiVanChuyen)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DonHang>()
                .Property(d => d.TongTien)
                .HasPrecision(18, 2);

            modelBuilder.Entity<HoaDon>()
                .Property(h => h.TongTien)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.SanPham)
                .WithMany(sp => sp.DanhGias)
                .HasForeignKey(d => d.SanPhamId);
            modelBuilder.Entity<SanPhamYeuThich>().ToTable("SanPhamYeuThich");
        }
    }
}
