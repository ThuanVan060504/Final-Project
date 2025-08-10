using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models.User
{
    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        public string Otp { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
