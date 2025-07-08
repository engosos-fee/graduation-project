using System.ComponentModel.DataAnnotations;

namespace project_graduation.DTOclass
{
    // DTO used for user login requests
    public class LoginDto
    {
        // User email (required)
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        // User password (required)
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
