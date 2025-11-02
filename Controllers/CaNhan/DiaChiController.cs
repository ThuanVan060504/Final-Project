using Final_Project.Models.Helpers;
using Final_Project.Models.Shop;
using Final_Project.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // <-- THÊM DÒNG NÀY

public class DiaChiController : Controller
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    // Các trường này sẽ đọc giá trị từ appsettings.json
    private readonly string _ghnToken;
    private readonly string _provinceApiUrl;
    private readonly string _districtApiUrl;
    private readonly string _wardApiUrl;

    // Constructor đã được cập nhật
    public DiaChiController(AppDbContext context,
                            IHttpClientFactory httpClientFactory,
                            IConfiguration configuration) // <-- Tiêm IConfiguration
    {
        _context = context;
        _httpClientFactory = httpClientFactory;

        // Đọc cấu hình từ appsettings.json
        _ghnToken = configuration["GhnApi:Token"];
        _provinceApiUrl = configuration["GhnApi:ProvinceApiUrl"];
        _districtApiUrl = configuration["GhnApi:DistrictApiUrl"];
        _wardApiUrl = configuration["GhnApi:WardApiUrl"];
    }

    // --- CÁC ACTION CRUD ĐỊA CHỈ GỐC (GIỮ NGUYÊN) ---

    public IActionResult Index()
    {
        var taiKhoanId = HttpContext.Session.GetInt32("MaTK");
        var diaChiList = _context.DiaChiNguoiDungs
            .Where(dc => dc.MaTK == taiKhoanId)
            .ToList();

        return View(diaChiList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(DiaChiNguoiDung diaChi)
    {
        diaChi.MaTK = (int)HttpContext.Session.GetInt32("MaTK");
        _context.DiaChiNguoiDungs.Add(diaChi);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult SetDefault(int id)
    {
        var diaChi = _context.DiaChiNguoiDungs.FirstOrDefault(d => d.MaDiaChi == id);
        if (diaChi == null) return NotFound();

        var diaChiKhac = _context.DiaChiNguoiDungs
            .Where(d => d.MaTK == diaChi.MaTK && d.MaDiaChi != id).ToList();

        foreach (var d in diaChiKhac)
        {
            d.MacDinh = false;
        }

        diaChi.MacDinh = true;
        _context.SaveChanges();
        return RedirectToAction("Profile", "User", new { fragment = "addressSection" });
    }

    public IActionResult Edit(int id)
    {
        var diaChi = _context.DiaChiNguoiDungs.FirstOrDefault(d => d.MaDiaChi == id);
        if (diaChi == null) return NotFound();
        return View(diaChi);
    }

    [HttpPost]
    public IActionResult Edit(DiaChiNguoiDung model)
    {
        var diaChi = _context.DiaChiNguoiDungs.FirstOrDefault(d => d.MaDiaChi == model.MaDiaChi);
        if (diaChi == null) return NotFound();

        diaChi.DiaChiChiTiet = model.DiaChiChiTiet;
        diaChi.TinhTP = model.TinhTP;
        diaChi.QuanHuyen = model.QuanHuyen;
        diaChi.PhuongXa = model.PhuongXa;
        diaChi.MacDinh = model.MacDinh;
        diaChi.TenNguoiNhan = model.TenNguoiNhan;
        diaChi.SoDienThoai = model.SoDienThoai;

        _context.SaveChanges();
        return RedirectToAction("Profile", "User", new { fragment = "addressSection" });
    }

    // --- CÁC ACTION API (Sử dụng biến cấu hình) ---

    [HttpGet]
    public async Task<IActionResult> GetProvinces()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Token", _ghnToken); // <-- ĐÃ SỬA

        var response = await client.GetAsync(_provinceApiUrl); // <-- ĐÃ SỬA
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
        return BadRequest(new { message = "Lỗi khi gọi API Tỉnh/Thành của GHN." });
    }

    [HttpGet]
    public async Task<IActionResult> GetDistricts(int provinceId)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Token", _ghnToken); // <-- ĐÃ SỬA

        var requestBody = new { province_id = provinceId };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(_districtApiUrl, content); // <-- ĐÃ SỬA
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
        return BadRequest(new { message = "Lỗi khi gọi API Quận/Huyện của GHN." });
    }

    [HttpGet]
    public async Task<IActionResult> GetWards(int districtId)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Token", _ghnToken); // <-- ĐÃ SỬA

        var requestBody = new { district_id = districtId };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(_wardApiUrl, content); // <-- ĐÃ SỬA
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
        return BadRequest(new { message = "Lỗi khi gọi API Phường/Xã của GHN." });
    }
}