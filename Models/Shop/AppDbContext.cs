using Final_Project.Models.Chat;
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
        public DbSet<DotFlashSale> DotFlashSale { get; set; }
        public DbSet<ChiTietFlashSale> ChiTietFlashSale { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<ThuongHieu> ThuongHieus { get; set; }
        public DbSet<SanPhamYeuThich> SanPhamYeuThichs { get; set; }
        public DbSet<Decor> Decors { get; set; }
        public DbSet<DanhMucDecor> DanhMucDecors { get; set; }
        public DbSet<TinTuc> TinTucs { get; set; }
        public DbSet<NhaCungCap> NhaCungCaps { get; set; }
        public DbSet<TinNhan> TinNhans { get; set; }
        public DbSet<NhapKho> NhapKhos { get; set; } // ✅ DbSet cho entity TinTuc không có key
        public DbSet<ChiTietNhapKho> ChiTietNhapKhos { get; set; } // ✅ DbSet cho entity TinTuc không có key
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<TaiKhoanVoucher> TaiKhoanVouchers { get; set; }
        public DbSet<Video> Videos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ⚠️ Luôn giữ dòng này!

            // ✅ Khai báo entity TinTuc không có key (chỉ dùng đọc dữ liệu từ RSS hoặc API)
            modelBuilder.Entity<TinTuc>().HasNoKey();

            // ✅ Các cấu hình decimal cho các bảng có tiền tệ
            modelBuilder.Entity<SanPham>()
                .HasOne(sp => sp.ThuongHieu)
                .WithMany(th => th.SanPhams)
                .HasForeignKey(sp => sp.MaThuongHieu)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ChiTietDonHang>()
                .Property(c => c.DonGia)
                .HasPrecision(18, 2);

            // Cấu hình Precision cho Voucher
            modelBuilder.Entity<Voucher>(e =>
            {
                e.HasIndex(v => v.MaCode).IsUnique(); // Đảm bảo MaCode là duy nhất
                e.Property(v => v.GiaTriGiam).HasPrecision(18, 2);
                e.Property(v => v.GiamGiaToiDa).HasPrecision(18, 2);
                e.Property(v => v.DonHangToiThieu).HasPrecision(18, 2);
            });

            // Cấu hình Unique Key cho TaiKhoanVoucher (mỗi user chỉ lưu 1 voucher 1 lần)
            modelBuilder.Entity<TaiKhoanVoucher>(e =>
            {
                e.HasIndex(tv => new { tv.MaTK, tv.MaVoucherID }).IsUnique();
            });
            // Cấu hình quan hệ DonHang - Voucher
            modelBuilder.Entity<DonHang>(e =>
            {
                e.HasOne(d => d.Voucher)
                 .WithMany(v => v.DonHangs)
                 .HasForeignKey(d => d.MaVoucherID)
                 .OnDelete(DeleteBehavior.SetNull); // Khi xóa voucher, giữ lại đơn hàng (set MaVoucherID = null)
            });

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

            modelBuilder.Entity<SanPham>()
                .HasOne(sp => sp.ThuongHieu)
                .WithMany(th => th.SanPhams)
                .HasForeignKey(sp => sp.MaThuongHieu)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Video>(e =>
            {
                e.HasOne(v => v.SanPham)          // Một Video...
                 .WithMany()                      // ...liên kết với một SanPham (SanPham không cần ICollection<Video>)
                 .HasForeignKey(v => v.MaSP)      // Khóa ngoại là MaSP
                 .OnDelete(DeleteBehavior.Cascade); // Khi xóa SanPham thì xóa luôn Video
            });
        }
    }
}
