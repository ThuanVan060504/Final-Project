using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Final_Project.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteBehaviorThuongHieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMuc",
                columns: table => new
                {
                    MaDanhMuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMuc", x => x.MaDanhMuc);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucDecor",
                columns: table => new
                {
                    MaDanhMuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucDecor", x => x.MaDanhMuc);
                });

            migrationBuilder.CreateTable(
                name: "GioHang",
                columns: table => new
                {
                    MaGH = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    MaSP = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHang", x => x.MaGH);
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    MaHD = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.MaHD);
                });

            migrationBuilder.CreateTable(
                name: "NhaCungCap",
                columns: table => new
                {
                    MaNCC = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNCC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaCungCap", x => x.MaNCC);
                });

            migrationBuilder.CreateTable(
                name: "PhanHois",
                columns: table => new
                {
                    MaPhanHoi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    MaSP = table.Column<int>(type: "int", nullable: false),
                    DanhGia = table.Column<int>(type: "int", nullable: false),
                    BinhLuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanHois", x => x.MaPhanHoi);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTK = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SDT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTK);
                });

            migrationBuilder.CreateTable(
                name: "ThuongHieu",
                columns: table => new
                {
                    MaThuongHieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenThuongHieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuongHieu", x => x.MaThuongHieu);
                });

            migrationBuilder.CreateTable(
                name: "TinTuc",
                columns: table => new
                {
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TomTat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nguon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Decor",
                columns: table => new
                {
                    MaDecor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDecor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Link3D = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaDanhMuc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decor", x => x.MaDecor);
                    table.ForeignKey(
                        name: "FK_Decor_DanhMucDecor_MaDanhMuc",
                        column: x => x.MaDanhMuc,
                        principalTable: "DanhMucDecor",
                        principalColumn: "MaDanhMuc",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhapKho",
                columns: table => new
                {
                    MaNhapKho = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayNhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaNCC = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConNo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DaThanhToan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChietKhau = table.Column<float>(type: "real", nullable: false),
                    HinhThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaTK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhapKho", x => x.MaNhapKho);
                    table.ForeignKey(
                        name: "FK_NhapKho_NhaCungCap_MaNCC",
                        column: x => x.MaNCC,
                        principalTable: "NhaCungCap",
                        principalColumn: "MaNCC",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaChiNguoiDung",
                columns: table => new
                {
                    MaDiaChi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNguoiNhan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    DiaChiChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhTP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuanHuyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhuongXa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MacDinh = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaChiNguoiDung", x => x.MaDiaChi);
                    table.ForeignKey(
                        name: "FK_DiaChiNguoiDung_TaiKhoan_MaTK",
                        column: x => x.MaTK,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanPham",
                columns: table => new
                {
                    MaSP = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenSP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GiaGoc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChieuRong = table.Column<int>(type: "int", nullable: true),
                    ChieuCao = table.Column<int>(type: "int", nullable: true),
                    ChieuSau = table.Column<int>(type: "int", nullable: true),
                    MaDanhMuc = table.Column<int>(type: "int", nullable: true),
                    MaThuongHieu = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPham", x => x.MaSP);
                    table.ForeignKey(
                        name: "FK_SanPham_DanhMuc_MaDanhMuc",
                        column: x => x.MaDanhMuc,
                        principalTable: "DanhMuc",
                        principalColumn: "MaDanhMuc");
                    table.ForeignKey(
                        name: "FK_SanPham_ThuongHieu_MaThuongHieu",
                        column: x => x.MaThuongHieu,
                        principalTable: "ThuongHieu",
                        principalColumn: "MaThuongHieu");
                });

            migrationBuilder.CreateTable(
                name: "DonHang",
                columns: table => new
                {
                    MaDonHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    MaDiaChi = table.Column<int>(type: "int", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayGiao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhiVanChuyen = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GiamGia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThaiThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThaiDonHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHang", x => x.MaDonHang);
                    table.ForeignKey(
                        name: "FK_DonHang_DiaChiNguoiDung_MaDiaChi",
                        column: x => x.MaDiaChi,
                        principalTable: "DiaChiNguoiDung",
                        principalColumn: "MaDiaChi",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietNhapKho",
                columns: table => new
                {
                    MaChiTietNhap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhapKho = table.Column<int>(type: "int", nullable: false),
                    MaSP = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietNhapKho", x => x.MaChiTietNhap);
                    table.ForeignKey(
                        name: "FK_ChiTietNhapKho_NhapKho_MaNhapKho",
                        column: x => x.MaNhapKho,
                        principalTable: "NhapKho",
                        principalColumn: "MaNhapKho",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietNhapKho_SanPham_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SanPham",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhGias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Diem = table.Column<int>(type: "int", nullable: false),
                    BinhLuan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    TenNguoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGias_SanPham_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPham",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlashSale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSP = table.Column<int>(type: "int", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashSale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashSale_SanPham_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SanPham",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanPhamYeuThich",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    MaSP = table.Column<int>(type: "int", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamYeuThich", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SanPhamYeuThich_SanPham_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SanPham",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhamYeuThich_TaiKhoan_MaTK",
                        column: x => x.MaTK,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHang",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    MaSP = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHang", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "MaDonHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_SanPham_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SanPham",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_MaDonHang",
                table: "ChiTietDonHang",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_MaSP",
                table: "ChiTietDonHang",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietNhapKho_MaNhapKho",
                table: "ChiTietNhapKho",
                column: "MaNhapKho");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietNhapKho_MaSP",
                table: "ChiTietNhapKho",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_SanPhamId",
                table: "DanhGias",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_Decor_MaDanhMuc",
                table: "Decor",
                column: "MaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_DiaChiNguoiDung_MaTK",
                table: "DiaChiNguoiDung",
                column: "MaTK");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaDiaChi",
                table: "DonHang",
                column: "MaDiaChi");

            migrationBuilder.CreateIndex(
                name: "IX_FlashSale_MaSP",
                table: "FlashSale",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_NhapKho_MaNCC",
                table: "NhapKho",
                column: "MaNCC");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_MaDanhMuc",
                table: "SanPham",
                column: "MaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_MaThuongHieu",
                table: "SanPham",
                column: "MaThuongHieu");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamYeuThich_MaSP",
                table: "SanPhamYeuThich",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamYeuThich_MaTK",
                table: "SanPhamYeuThich",
                column: "MaTK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHang");

            migrationBuilder.DropTable(
                name: "ChiTietNhapKho");

            migrationBuilder.DropTable(
                name: "DanhGias");

            migrationBuilder.DropTable(
                name: "Decor");

            migrationBuilder.DropTable(
                name: "FlashSale");

            migrationBuilder.DropTable(
                name: "GioHang");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "PhanHois");

            migrationBuilder.DropTable(
                name: "SanPhamYeuThich");

            migrationBuilder.DropTable(
                name: "TinTuc");

            migrationBuilder.DropTable(
                name: "DonHang");

            migrationBuilder.DropTable(
                name: "NhapKho");

            migrationBuilder.DropTable(
                name: "DanhMucDecor");

            migrationBuilder.DropTable(
                name: "SanPham");

            migrationBuilder.DropTable(
                name: "DiaChiNguoiDung");

            migrationBuilder.DropTable(
                name: "NhaCungCap");

            migrationBuilder.DropTable(
                name: "DanhMuc");

            migrationBuilder.DropTable(
                name: "ThuongHieu");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
