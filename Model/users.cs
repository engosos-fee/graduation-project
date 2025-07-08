using System.ComponentModel.DataAnnotations; // Used for key attributes
using System.ComponentModel.DataAnnotations.Schema;

namespace project_graduation.Model
{
    // Represents a user in the system
    public class users
    {
        // Primary key
        public int id { get; set; }

        // User's full name
        public string name { get; set; }

        // User's email address
        public string email { get; set; }

        // Hashed password
        public string password { get; set; }

        // User role (e.g., 'u' for user, 'a' for admin)
        public char role { get; set; }

        // Indicates whether the email is confirmed
        public bool IsEmailConfirmed { get; set; } = false;

        // Token sent for email confirmation
        public string? EmailConfirmationToken { get; set; }

        // Expiration time of the email confirmation token
        public DateTime? TokenExpirationTime { get; set; }

        // Number of failed login attempts
        public int FailedLoginAttempts { get; set; } = 0;

        // Lockout end time (if the user is temporarily locked out)
        public DateTime? LockoutEnd { get; set; }

        // Timestamp of the last confirmation email sent
        public DateTime? LastConfirmationEmailSent { get; set; }

        // List of scan result data linked to the user
        public virtual List<data> datas { get; set; }

        // List of URLs submitted by the user
        public virtual List<url_input> Url_Inputs { get; set; }

        // Token used for password reset
        public string? PasswordResetToken { get; set; }

        // Expiration time for the password reset token
        public DateTime? PasswordResetTokenExpiration { get; set; }
    }
}
