using System.ComponentModel.DataAnnotations;

namespace project_graduation.DTOclass
{
    // DTO used for user registration
    public class RegisterDto
    {
        // User's full name (Arabic or English letters only, 2 to 25 characters)
        [Required(ErrorMessage = "Name is required")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]{2,25}$", ErrorMessage = "Name can only contain letters (Arabic or English) and spaces")]
        public string Name { get; set; }

        // User email address (must be valid format)
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        // User password (required, strong format: upper, lower, digit, special char, min 8 chars)
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "Password must contain upper, lower, digit, and special character")]
        public string Password { get; set; }
    }
}
