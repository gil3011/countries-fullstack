using System.ComponentModel.DataAnnotations;

namespace Server.DTO
{
    public class UserLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LanguageRequest
    {
        public string Language { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
