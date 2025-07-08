using System.ComponentModel.DataAnnotations;

namespace project_graduation.DTOclass
{
    // DTO used for user registration
    public class RegisterDto
    {
        // User full name (required)
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        // User email address (required and must be valid format)
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        // User password (required, with strength validation)
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "Password must contain upper, lower, digit, and special character")]
        public string Password { get; set; }
    }
}
