using System.ComponentModel.DataAnnotations;

namespace ogrenci_sistemi_restsharp.Models
{
    public class LoginModel
    {
        public class User
        {
            public int Id { get; set; }
            [Required]
            public string Username { get; set; }
            [Required]
            public string Password { get; set; }
            [Required]
            public string Email { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordCheck { get; set; }
        [Required]
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class CaptchaResponse
    {
        public bool Success { get; set; }
        public double Score { get; set; }
    }
}
