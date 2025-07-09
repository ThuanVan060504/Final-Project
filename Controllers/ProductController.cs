using Microsoft.AspNetCore.Mvc;
using Final_Project.Models;

namespace Final_Project.Controllers
{
    public class ProductController : Controller
    {
        public static List<Product> Products = new List<Product>
{
    // Gạch - Sàn - Sơn
    new Product { Id = 1, Name = "Gạch lát nền Viglacera 60x60",Category = "Gạch ốp lát", Price = 175000, Description = "Bề mặt nhám nhẹ chống trơn, tông xám hiện đại, nâng tầm không gian sống!", Detail = "Gạch lát nền Viglacera 60x60 là lựa chọn lý tưởng cho các không gian như phòng khách, sảnh lớn nhờ vào độ bền cao, bề mặt chống trơn và họa tiết hiện đại. Dễ dàng vệ sinh, không phai màu, giữ vẻ đẹp bền lâu theo thời gian.", ImageUrl = "/images/product1.jpg" },
    new Product { Id = 2, Name = "Gạch ốp tường Đồng Tâm", Category = "Gạch ốp lát", Price = 195000, Description = "Họa tiết vân mây tinh tế, cho căn nhà thêm phần sang trọng và thanh lịch.", Detail = "Vân mây sang trọng, dễ phối với nội thất hiện đại hoặc cổ điển. Kích thước tiêu chuẩn, chất liệu bền chắc, chống thấm tốt. Thích hợp cho nhà bếp và phòng tắm.", ImageUrl = "/images/product2.jpg" },
    new Product { Id = 3, Name = "Sàn gỗ Malaysia 12mm", Category = "Sàn gỗ", Price = 295000, Description = "Màu sồi vàng nổi bật, siêu chống nước, bền đẹp theo thời gian.", Detail = "Cấu tạo 4 lớp, chịu nước cực tốt, màu gỗ sồi tự nhiên. Dễ lắp đặt, chịu lực tốt, không cong vênh trong môi trường ẩm. Giải pháp lý tưởng cho các không gian sang trọng.", ImageUrl = "/images/product3.jpg" },
    new Product { Id = 4, Name = "Sàn nhựa vinyl vân đá",Category = "Sàn vinyl", Price = 210000, Description = "Lựa chọn tuyệt vời cho phong cách hiện đại, chống trầy xước, chống mối mọt.", Detail = "Sàn nhựa vinyl vân đá mang đến phong cách hiện đại, dễ thi công, không mối mọt, phù hợp với cả nhà ở và văn phòng. Chống nước tuyệt đối, dễ lau chùi.", ImageUrl = "/images/product4.jpg" },
    new Product { Id = 5, Name = "Sơn nội thất Jotun Essence",Category = "Gạch ốp lát", Price = 820000, Description = "Màu trắng tinh khôi, độ phủ cao, an toàn cho sức khỏe.",Detail = "Sơn Jotun Essence là dòng sơn nội thất cao cấp, cho bề mặt mịn màng, bền màu theo thời gian. An toàn cho trẻ nhỏ và người lớn tuổi.", ImageUrl = "/images/product5.jpg" },
    new Product { Id = 6, Name = "Keo dán gạch Weber",Category = "Gạch ốp lát", Price = 320000, Description = "Kết dính mạnh mẽ, chống bong tróc, dễ thi công và bền đẹp lâu dài.",Detail = "Keo Weber chuyên dùng cho ốp lát gạch trong nhà và ngoài trời. Chống thấm, chống nấm mốc, tiết kiệm thời gian thi công.", ImageUrl = "/images/product6.jpg" },

    // Thiết bị vệ sinh
    new Product { Id = 7, Name = "Bồn cầu Inax AC-504",Category = "Thiết bị vệ sinh", Price = 2350000, Description = "Thiết kế gọn gàng, xả nước cực mạnh, tiết kiệm tối ưu.",Detail = "Bồn cầu Inax AC-504 với công nghệ xả xoáy hiện đại, tiết kiệm nước, phù hợp cho các hộ gia đình hiện đại. Dễ lắp đặt và bảo trì.", ImageUrl = "/images/product7.jpg" },
    new Product { Id = 8, Name = "Lavabo treo Caesar",Category = "Thiết bị vệ sinh", Price = 890000, Description = "Sứ cao cấp, chống bám bẩn, dễ vệ sinh và lắp đặt.",Detail = "Lavabo Caesar treo tường giúp tiết kiệm diện tích phòng tắm. Thiết kế tối giản, tinh tế, dễ vệ sinh và lau chùi.", ImageUrl = "/images/product8.jpg" },
    new Product { Id = 9, Name = "Vòi lavabo nóng lạnh Hafele",Category = "Thiết bị vệ sinh", Price = 1290000, Description = "Chất liệu inox 304 bền bỉ, tích hợp nóng lạnh tiện dụng.",Detail = "Vòi Hafele chất lượng cao, không hoen gỉ theo thời gian. Kiểu dáng hiện đại, dễ sử dụng với 2 chế độ nước nóng và lạnh.", ImageUrl = "/images/product9.jpg" },
    new Product { Id = 10, Name = "Phòng tắm kính 90x90", Category = "Thiết bị vệ sinh", Price = 5200000, Description = "Kính cường lực an toàn, không gian tắm riêng tư và hiện đại.",Detail = "Phòng tắm kính với thiết kế cửa mở trượt, kính cường lực an toàn, không bám nước, giúp không gian phòng tắm thêm sang trọng.", ImageUrl = "/images/product10.jpg" },

    // Rèm - Gương - Trang trí
    new Product { Id = 11, Name = "Rèm cửa 2 lớp cao cấp",Category = "Rèm cửa", Price = 980000, Description = "Chất vải bố Hàn Quốc, chống nắng hiệu quả, tăng tính thẩm mỹ.", Detail = "Rèm cửa hai lớp gồm lớp chống nắng và lớp voan nhẹ, vừa chắn sáng vừa trang trí. Thích hợp cho phòng khách hoặc phòng ngủ.", ImageUrl = "/images/product11.jpg" },
    new Product { Id = 12, Name = "Gương tròn decor viền gỗ", Category = "Gương", Price = 650000, Description = "Viền gỗ tự nhiên, đường kính 60cm, điểm nhấn không gian sống.",Detail = "Gương tròn decor viền gỗ không chỉ dùng soi mà còn là món trang trí hiện đại. Phù hợp với phong cách tối giản hoặc cổ điển.", ImageUrl = "/images/product12.jpg" },
    new Product { Id = 13, Name = "Tranh canvas phòng khách",Category = "Gương",  Price = 450000, Description = "Bộ 3 tranh phối màu tinh tế, khung gỗ MDF sang trọng.",Detail = "Tranh canvas được in chất lượng cao, sắc nét, dễ treo và phối với các phong cách nội thất hiện đại. Phù hợp làm quà tặng.", ImageUrl = "/images/product13.jpg" },

    // Thiết bị chiếu sáng - điện
    new Product { Id = 14, Name = "Đèn LED âm trần Rạng Đông",Category = "Thiết bị chiếu sáng", Price = 220000, Description = "Ánh sáng trắng dịu nhẹ, tiết kiệm điện, bền bỉ vượt trội.",Detail = "Đèn LED âm trần giúp tiết kiệm điện năng, tuổi thọ bóng đèn cao, ánh sáng dịu nhẹ không gây mỏi mắt. Phù hợp mọi không gian.", ImageUrl = "/images/product14.jpg" },
    new Product { Id = 15, Name = "Quạt trần Panasonic 5 cánh",Category = "Thiết bị chiếu sáng", Price = 2950000, Description = "Gió mạnh, có remote điều khiển, thiết kế sang trọng.",Detail = "Quạt trần Panasonic 5 cánh vận hành êm ái, tiết kiệm điện, kèm điều khiển từ xa tiện dụng. Thích hợp cho nhà phố hoặc căn hộ.", ImageUrl = "/images/product15.jpg" },
    new Product { Id = 16, Name = "Ổ cắm điện âm tường Schneider",Category = "Thiết bị chiếu sáng", Price = 165000, Description = "Thiết kế chuẩn châu Âu, an toàn tuyệt đối cho gia đình.",Detail = "Ổ cắm âm tường Schneider có thiết kế gọn gàng, sang trọng. Bảo vệ trẻ em và tránh tình trạng chập cháy điện.", ImageUrl = "/images/product16.jpg" },

    // Thiết bị gia dụng
    new Product { Id = 17, Name = "Bếp từ đôi Malloca", Category = "Thiết bị gia dụng", Price = 7390000, Description = "Mặt kính chịu lực, cảm ứng nhạy, tiết kiệm điện hiệu quả.",Detail = "Bếp từ đôi Malloca nấu siêu nhanh, tiết kiệm điện. Bề mặt kính dễ vệ sinh, có chức năng hẹn giờ và khóa an toàn.", ImageUrl = "/images/product17.jpg" },
    new Product { Id = 18, Name = "Máy hút mùi âm Bosch", Category = "Thiết bị gia dụng", Price = 5100000, Description = "Khử mùi siêu nhanh, thiết kế tinh tế, tiết kiệm diện tích.",Detail = "Máy hút mùi Bosch thiết kế âm tủ giúp tiết kiệm không gian. Hệ thống lọc khử mùi bằng than hoạt tính hiệu quả cao.",  ImageUrl = "/images/product18.jpg" },
    new Product { Id = 19, Name = "Lò vi sóng Electrolux 25L", Category = "Thiết bị gia dụng", Price = 2250000, Description = "Tích hợp rã đông và nướng, tiện lợi và nhanh chóng.",Detail = "Lò vi sóng Electrolux dung tích 25L phù hợp cho gia đình 4-5 người. Chức năng nướng, rã đông nhanh chóng và tiện lợi.", ImageUrl = "/images/product19.jpg" },
    new Product { Id = 20, Name = "Máy nước nóng Ferroli", Category = "Thiết bị gia dụng", Price = 2890000, Description = "Giữ nhiệt lâu, chống giật an toàn, dung tích lớn.",Detail = "Máy nước nóng Ferroli trang bị hệ thống chống giật ELCB, vỏ chống nước IPX4, giúp bạn yên tâm sử dụng mỗi ngày.", ImageUrl = "/images/product20.jpg" },

    // Thiết bị công nghệ nhà ở
    new Product { Id = 21, Name = "Khóa cửa điện tử Xiaomi",Category = "Công nghệ thông tin", Price = 3990000, Description = "Bảo mật cao, mở bằng vân tay và mã số tiện lợi.",Detail = "Khóa cửa Xiaomi sử dụng cảm biến vân tay tốc độ cao, kèm mã số và chìa cơ. Kết nối qua app điện thoại rất tiện lợi.", ImageUrl = "/images/product21.jpg" },
    new Product { Id = 22, Name = "Chuông cửa có hình",Category = "Công nghệ thông tin", Price = 2890000, Description = "Camera HD, kết nối điện thoại, giám sát linh hoạt.",Detail = "Chuông cửa có hình giúp bạn quan sát khách đến từ xa, đàm thoại 2 chiều, kết nối wifi và điện thoại cực kỳ tiện lợi.", ImageUrl = "/images/product22.jpg" },
    new Product { Id = 23, Name = "Bộ cảm biến chuyển động",Category = "Công nghệ thông tin", Price = 990000, Description = "Phát hiện thông minh, tăng cường an ninh cho gia đình bạn.",Detail = "Bộ cảm biến phát hiện chuyển động ban đêm, chống trộm hiệu quả. Có thể kết nối hệ thống smart home dễ dàng.", ImageUrl = "/images/product23.jpg" },

    // Nội thất tiện ích
    new Product { Id = 24, Name = "Tủ giày gỗ MDF chống ẩm",Category = "Nội thất tiện ích", Price = 1750000, Description = "Thiết kế 3 tầng gọn nhẹ, chống ẩm và chống trầy.",Detail = "Tủ giày 3 tầng được làm từ gỗ MDF phủ melamine chống ẩm. Thiết kế đơn giản, gọn nhẹ, chứa được từ 10-12 đôi giày.", ImageUrl = "/images/product24.jpg" },
    new Product { Id = 25, Name = "Bàn làm việc gỗ công nghiệp",Category = "Nội thất tiện ích", Price = 1890000, Description = "120x60cm, chân sắt chắc chắn, phù hợp làm việc/học tập.",Detail = "Bàn gỗ công nghiệp có lớp phủ chống trầy, khung sắt sơn tĩnh điện chắc chắn. Thiết kế hiện đại cho học tập/làm việc.", ImageUrl = "/images/product25.jpg" },
    new Product { Id = 26, Name = "Kệ treo nhà tắm inox",Category = "Nội thất tiện ích", Price = 370000, Description = "2 tầng tiện lợi, chống rỉ sét, gắn tường gọn gàng.", Detail = "Kệ inox 2 tầng giúp tối ưu không gian nhà tắm. Dễ lắp đặt, chịu lực tốt, chống gỉ trong môi trường ẩm ướt.",  ImageUrl = "/images/product26.jpg" },

    // Bổ sung khác
    new Product { Id = 27, Name = "Ống nước PPR Bình Minh", Category = "Gạch ốp lát",  Price = 45000, Description = "Dài 4m, chịu lực cao, dễ thi công cho mọi công trình.",Detail = "Ống nước PPR chịu áp lực cao, tuổi thọ trên 50 năm. Sản phẩm chất lượng cao của thương hiệu Bình Minh, dễ dàng kết nối.", ImageUrl = "/images/product27.jpg" },
    new Product { Id = 28, Name = "Xi măng Holcim", Category = "Gạch ốp lát",  Price = 97000, Description = "Chất lượng cao, độ kết dính tốt, cho mọi loại công trình.", Detail = "Xi măng Holcim chất lượng đạt chuẩn quốc tế, phù hợp cho móng nhà, sàn, và các hạng mục chịu lực cao.",  ImageUrl = "/images/product28.jpg" },
    new Product { Id = 29, Name = "Chổi quét trần nhà",Category = "Nội thất tiện ích", Price = 125000, Description = "Tay dài 2m, dễ dàng quét sạch trần nhà và góc khuất.",Detail = "Chổi quét trần tay dài 2m, đầu xoay linh hoạt, giúp làm sạch trần, rèm cửa và các góc cao dễ dàng.", ImageUrl = "/images/product29.jpg" },
    new Product { Id = 30, Name = "Kính chắn bếp cường lực",Category = "Nội thất tiện ích", Price = 1550000, Description = "Kính màu xanh ngọc đẹp mắt, dễ vệ sinh, chịu nhiệt tốt.",Detail = "Kính chắn bếp màu xanh ngọc, sang trọng, chịu nhiệt và chống bám dầu mỡ. Dễ vệ sinh, lắp đặt đơn giản.", ImageUrl = "/images/product30.jpg" }
};


        public IActionResult Index(string? category, string? search, string? sort)
        {
            var products = Products.AsQueryable();

            // Lọc theo danh mục
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category != null && p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name != null && p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Sắp xếp theo giá
            if (!string.IsNullOrEmpty(sort))
            {
                if (sort == "asc")
                    products = products.OrderBy(p => p.Price);
                else if (sort == "desc")
                    products = products.OrderByDescending(p => p.Price);
            }

            return View(products.ToList());
        }

        public IActionResult Details(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }
        public IActionResult Category(string name)
        {
            var filtered = Products.Where(p => p.Category == name).ToList();
            ViewBag.CategoryName = name;
            return View("Category", filtered);
        }

    }
}
