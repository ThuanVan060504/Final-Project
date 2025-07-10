using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Final_Project.Models.Shop;

namespace Final_Project.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private const string CART_KEY = "cart";

        public CartController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.SanPhams.FirstOrDefault(p => p.MaSP == id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.MaSP,
                    ProductName = product.TenSP,
                    Price = product.DonGia,
                    ImageUrl = product.ImageURL,
                    Quantity = 1
                });
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }


        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        private List<CartItem> GetCart()
        {
            var session = HttpContext.Session.GetString(CART_KEY);
            return session == null
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(session) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(cart));
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, string actionType)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null)
            {
                if (actionType == "increase")
                {
                    item.Quantity++;
                }
                else if (actionType == "decrease")
                {
                    item.Quantity--;
                    if (item.Quantity <= 0)
                    {
                        cart.Remove(item);
                    }
                }
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Message"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }
            return View(cart); // Dẫn đến view để chọn phương thức thanh toán
        }

        [HttpPost]
        public IActionResult CompleteCheckout(string method)
        {
            // Sau khi thanh toán thành công, xóa session giỏ hàng
            HttpContext.Session.Remove(CART_KEY);
            TempData["Success"] = $"Cảm ơn bạn đã mua hàng! Phương thức: {method}";
            return RedirectToAction("ThankYou");
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
