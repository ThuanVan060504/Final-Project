using System.ComponentModel.DataAnnotations;

namespace Final_Project.Models.User
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string MatKhau { get; set; }
    }
}
