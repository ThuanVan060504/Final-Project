using Final_Project.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Final_Project.Models.Shop
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<DiaChiNguoiDung> DiaChiNguoiDungs { get; set; }
        public DbSet<PhanHoi> PhanHois { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // nhớ giữ dòng này

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
        }
    }

}
