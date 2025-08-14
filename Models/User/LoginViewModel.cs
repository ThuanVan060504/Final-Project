using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?]).{6,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ in hoa, 1 số, 1 ký tự đặc biệt và dài tối thiểu 6 ký tự")]
        public string MatKhau { get; set; }
    }
}
