using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models.ViewModels
{
    public class SanPhamViewModel
    {
        public string TenSP { get; set; }

        public string MoTa { get; set; }

        public decimal DonGia { get; set; }

        public decimal GiaGoc { get; set; }

        public int SoLuong { get; set; }

        public IFormFile ImageUpload { get; set; }

        public int MaDanhMuc { get; set; }

        public int MaThuongHieu { get; set; }

        public int   ChieuRong { get; set; }

        public int ChieuCao { get; set; }

        public int ChieuSau { get; set; }

        public IEnumerable<SelectListItem> DanhMucList { get; set; }

        public IEnumerable<SelectListItem> ThuongHieuList { get; set; }
    }
}
