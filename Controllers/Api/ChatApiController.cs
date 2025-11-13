using Final_Project.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

[Route("api/chat")]
[ApiController]
public class ChatApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public ChatApiController(AppDbContext context)
    {
        _context = context;
    }

    // ===============================================
    // HÀM HỖ TRỢ 1: PHÂN TÍCH NGƯỠNG GIÁ MAX
    // ===============================================
    // Trong file ChatApiController.cs, thay thế hàm TryParseMaxPrice hiện tại:

    // Trong file ChatApiController.cs, thay thế hàm TryParseMaxPrice hiện tại:

    private decimal? TryParseMaxPrice(string query, out string cleanedQuery)
    {
        cleanedQuery = query;
        if (string.IsNullOrWhiteSpace(query)) return null;

        var lowerQuery = query.ToLower();
        decimal? maxPrice = null;

        // 💡 REGEX CỰC KỲ LINH HOẠT: Sử dụng \s* thay cho \s+ và loại bỏ \b
        var priceMatch = Regex.Match(
            lowerQuery,
            // Dưới/Max (0 hoặc nhiều khoảng trắng) (Số) (0 hoặc nhiều khoảng trắng) (Đơn vị)
            @"(dưới|max|bé hơn)\s*(\d+)\s*(tr|triệu|k|nghìn)",
            RegexOptions.IgnoreCase
        );

        if (priceMatch.Success)
        {
            if (decimal.TryParse(priceMatch.Groups[2].Value, out decimal number))
            {
                var unit = priceMatch.Groups[3].Value;
                if (unit.Contains("tr")) maxPrice = number * 1000000;
                else if (unit.Contains("k")) maxPrice = number * 1000;
            }

            cleanedQuery = Regex.Replace(
                query,
                priceMatch.Value,
                " ",
                RegexOptions.IgnoreCase
            ).Trim();
        }

        return maxPrice;
    }

    // ===============================================
    // HÀM HỖ TRỢ 2: PHÂN TÍCH NGƯỠNG KÍCH THƯỚC MIN
    // ===============================================
    private (int? minDim, string dimField, string cleanedQuery) TryParseDimension(string query)
    {
        var lowerQuery = query.ToLower();
        string dimField = null;
        int? minDim = null;
        string cleanedQuery = query;

        // Regex đã được làm linh hoạt hơn
        var dimMatch = Regex.Match(
            lowerQuery,
            @"\b(dài|rộng|cao)\b\s*hơn\s+(\d+)\s*(cm|centimet|mét|m)\b",
            RegexOptions.IgnoreCase
        );

        if (dimMatch.Success)
        {
            if (int.TryParse(dimMatch.Groups[2].Value, out int number))
            {
                var unit = dimMatch.Groups[3].Value;

                if (unit.Contains("mét") || unit.Contains("m"))
                {
                    minDim = number * 100; // Meters to CM
                }
                else if (unit.Contains("cm"))
                {
                    minDim = number; // CM remains CM
                }

                var type = dimMatch.Groups[1].Value;
                if (type.Contains("dài")) dimField = nameof(SanPham.ChieuRong);
                else if (type.Contains("rộng")) dimField = nameof(SanPham.ChieuRong);
                else if (type.Contains("cao")) dimField = nameof(SanPham.ChieuCao);
            }

            cleanedQuery = Regex.Replace(
                query,
                dimMatch.Value,
                " ",
                RegexOptions.IgnoreCase
            ).Trim();
        }

        return (minDim, dimField, cleanedQuery);
    }

    // ===============================================
    // PHƯƠNG THỨC CHÍNH: SEARCH PRODUCTS
    // ===============================================
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(new List<SanPham>());
        }

        // --- PHASE 1 & 2: PARSING ---
        var (minDim, dimField, dimCleanedQuery) = TryParseDimension(query);
        string keywordQuery;
        decimal? maxPrice = TryParseMaxPrice(dimCleanedQuery, out keywordQuery);

        // --- PHASE 3: TOKENIZATION & STOP WORD FILTERING ---
        var lowerKeywordQuery = keywordQuery.ToLower().Trim();
        var rawKeywords = lowerKeywordQuery.Split(
            new[] { ' ' },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        // 💡 CẬP NHẬT LỚN DANH SÁCH STOP WORDS
        var stopWords = new HashSet<string>
        {
            "tôi", "cần", "muốn", "một", "cái", "chi phí", "hay", "và", "những", "loại", "sản phẩm",
            "với", "rất", "quá", "nên", "gì", "nên", 
            // Thêm các từ ngữ cảnh để Intent Rule được kích hoạt
            "phòng", "tối", "sáng", "lưng", "đau", "mệt"
        };

        var meaningfulKeywords = rawKeywords
            .Where(k => !stopWords.Contains(k))
            .ToList();

        var efQuery = _context.SanPhams.AsQueryable();

        // --- PHASE 4: INTENT RULES (Xử lý ngữ cảnh) ---

        // RULE 1: PHÒNG TỐI (Bây giờ sẽ được kích hoạt vì "phòng" và "tối" là stop words)
        if (meaningfulKeywords.Count == 0 && (lowerKeywordQuery.Contains("tối") || lowerKeywordQuery.Contains("sáng")))
        {
            meaningfulKeywords.Add("đèn");
            meaningfulKeywords.Add("chiếu sáng");
        }

        // RULE 2: ĐAU LƯNG
        else if (meaningfulKeywords.Count == 0 && lowerKeywordQuery.Contains("lưng"))
        {
            meaningfulKeywords.Add("ghế");
            meaningfulKeywords.Add("công thái học");
            meaningfulKeywords.Add("massage");
        }

        // ---------------------------------------------
        // 5. ÁP DỤNG CÁC FILTER VÀO TRUY VẤN
        // ---------------------------------------------

        // A. FILTER GIÁ
        if (maxPrice.HasValue)
        {
            efQuery = efQuery.Where(p => p.DonGia <= maxPrice.Value);
        }

        // B. FILTER KÍCH THƯỚC (Min Dimension)
        if (minDim.HasValue && dimField != null)
        {
            if (dimField == nameof(SanPham.ChieuRong))
                efQuery = efQuery.Where(p => p.ChieuRong.HasValue && p.ChieuRong.Value >= minDim.Value);
            else if (dimField == nameof(SanPham.ChieuCao))
                efQuery = efQuery.Where(p => p.ChieuCao.HasValue && p.ChieuCao.Value >= minDim.Value);
        }

        // C. FILTER TỪ KHÓA (Sử dụng OR logic cho tất cả từ khóa có ý nghĩa)
        if (meaningfulKeywords.Count > 0)
        {
            efQuery = efQuery.Where(p => meaningfulKeywords.Any(keyword =>
                p.TenSP.ToLower().Contains(keyword) ||
                (p.MoTa != null && p.MoTa.ToLower().Contains(keyword))
            ));
        }
        else if (!maxPrice.HasValue && !minDim.HasValue)
        {
            return Ok(new List<SanPham>());
        }

        // 6. Thực thi và trả về kết quả
        var products = await efQuery
            .Select(p => new
            {
                p.MaSP,
                p.TenSP,
                p.DonGia,
                p.ImageURL,
            })
            .Take(5)
            .ToListAsync();

        return Ok(products);
    }
}