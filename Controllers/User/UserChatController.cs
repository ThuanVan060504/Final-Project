// Controller
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Final_Project.Controllers
{
    public class UserChatController : Controller
    {
        private readonly string _connectionString;

        public UserChatController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("MaTK");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var messages = new List<MessageModel>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT MaTinNhan, NguoiGuiId, NguoiNhanId, NoiDung, ThoiGianGui 
                    FROM TinNhan
                    WHERE (NguoiGuiId = @UserId AND NguoiNhanId = 3) 
                       OR (NguoiGuiId = 3 AND NguoiNhanId = @UserId)
                    ORDER BY ThoiGianGui ASC", con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new MessageModel
                            {
                                MaTinNhan = (int)reader["MaTinNhan"],
                                NguoiGuiId = (int)reader["NguoiGuiId"],
                                NguoiNhanId = (int)reader["NguoiNhanId"],
                                NoiDung = reader["NoiDung"].ToString(),
                                ThoiGianGui = (DateTime)reader["ThoiGianGui"]
                            });
                        }
                    }
                }
            }

            return View("~/Views/User/TinNhan/Index.cshtml", messages);
        }

        [HttpPost]
        public IActionResult GuiTinNhan(string noiDung)
        {
            var userId = HttpContext.Session.GetInt32("MaTK");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO TinNhan (NguoiGuiId, NguoiNhanId, NoiDung, ThoiGianGui)
                    VALUES (@NguoiGuiId, 3, @NoiDung, GETDATE())", con))
                {
                    cmd.Parameters.AddWithValue("@NguoiGuiId", userId);
                    cmd.Parameters.AddWithValue("@NoiDung", noiDung);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
    }

    public class MessageModel
    {
        public int MaTinNhan { get; set; } // đổi từ Id sang MaTinNhan
        public int NguoiGuiId { get; set; }
        public int NguoiNhanId { get; set; }
        public string NoiDung { get; set; }
        public DateTime ThoiGianGui { get; set; }
    }
}
