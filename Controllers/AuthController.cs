using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using project_graduation.DTOclass;
using project_graduation.Model;
using project_graduation.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace project_graduation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly appDBcontext _context;
        private readonly IConfiguration _config;

        public AuthController(appDBcontext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Handles user registration and sends confirmation email
        [HttpPost("register")]
        public IActionResult Register(RegisterDto request, [FromServices] EmailService emailService)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.email == request.Email);

            if (existingUser != null)
            {
                if (existingUser.IsEmailConfirmed)
                    return BadRequest("Email already exists.");

                // User registered before but didn't confirm email
                if (existingUser.TokenExpirationTime < DateTime.UtcNow)
                {
                    existingUser.EmailConfirmationToken = Guid.NewGuid().ToString();
                    existingUser.TokenExpirationTime = DateTime.UtcNow.AddMinutes(20);
                    _context.SaveChanges();

                    emailService.SendConfirmationEmail(existingUser.email, existingUser.EmailConfirmationToken, existingUser.name);
                    return Ok("A new confirmation email has been sent.");
                }

                return BadRequest("Please check your email to confirm your account. Token still valid.");
            }

            // New user registration
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var token = Guid.NewGuid().ToString();

            var newUser = new users
            {
                name = request.Name,
                email = request.Email,
                password = hashedPassword,
                role = 'u',
                IsEmailConfirmed = false,
                EmailConfirmationToken = token,
                TokenExpirationTime = DateTime.UtcNow.AddMinutes(20)
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            emailService.SendConfirmationEmail(newUser.email, token, newUser.name);

            return Ok("Registration successful. Please check your email to confirm your account.");
        }

        // Handles user login and returns JWT token
        [HttpPost("login")]
        public IActionResult Login(LoginDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == request.Email);

            if (user == null)
                return Unauthorized("Email not found.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.password))
                return Unauthorized("Invalid credentials.");

            if (!user.IsEmailConfirmed)
                return Unauthorized("Please confirm your email address before logging in.");

            // Check if the account is temporarily locked
            if (user.LockoutEnd.HasValue)
            {
                if (user.LockoutEnd > DateTime.UtcNow)
                    return Unauthorized("Account is locked. Please try again later.");
                else
                {
                    // Lockout expired, reset attempts
                    user.FailedLoginAttempts = 0;
                    user.LockoutEnd = null;
                    _context.SaveChanges();
                }
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.password))
            {
                // Wrong password → increment attempts
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(20);  // Lock account
                    _context.SaveChanges();
                    return Unauthorized("Account is locked. Please try again later.");
                }

                _context.SaveChanges();
                return Unauthorized("Invalid credentials.");
            }

            // Reset failed attempts after successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            _context.SaveChanges();

            // Generate JWT token
            var claims = new[]
            {
                new Claim("id", user.id.ToString()),
                new Claim("email", user.email),
                new Claim("role", user.role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                userId = user.id,
                role = user.role,
            });
        }

        // Confirms user email using a token
        [HttpGet("confirm-email")]
        public IActionResult ConfirmEmail(string email, string token)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == email);

            if (user == null || user.EmailConfirmationToken != token)
                return BadRequest("Invalid token or email.");

            if (user.TokenExpirationTime < DateTime.UtcNow)
                return BadRequest("Token has expired.");

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.TokenExpirationTime = null;

            _context.SaveChanges();

            return Ok("Email confirmed successfully.");
        }

        // Resend confirmation email if not already confirmed
        [HttpPost("resend-confirmation")]
        public IActionResult ResendConfirmation([FromQuery] string email, [FromServices] EmailService emailService)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == email);
            if (user == null)
                return NotFound("User not found.");

            if (user.IsEmailConfirmed)
                return BadRequest("Email already confirmed.");

            // Prevent frequent resends
            if (user.LastConfirmationEmailSent != null && user.LastConfirmationEmailSent > DateTime.UtcNow.AddMinutes(-1))
                return BadRequest("Please wait at least 1 minute before resending the confirmation email.");

            // Generate new token
            user.EmailConfirmationToken = Guid.NewGuid().ToString();
            user.TokenExpirationTime = DateTime.UtcNow.AddMinutes(20);
            user.LastConfirmationEmailSent = DateTime.UtcNow;

            _context.SaveChanges();

            emailService.SendConfirmationEmail(user.email, user.EmailConfirmationToken, user.name);

            return Ok("Confirmation email sent again.");
        }

        // Send password reset email to user
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromQuery] string email, [FromServices] EmailService emailService)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == email);
            if (user == null || !user.IsEmailConfirmed)
                return BadRequest("Invalid email or email not confirmed.");

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpiration = DateTime.UtcNow.AddMinutes(20);

            _context.SaveChanges();

            emailService.SendResetPasswordEmail(user.email, user.PasswordResetToken, user.name);

            return Ok("Password reset link sent.");
        }

        // Validate password complexity
        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
            bool hasMinLength = password.Length >= 8;

            return hasUpper && hasLower && hasDigit && hasSpecial && hasMinLength;
        }

        // Reset user password using valid token
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.PasswordResetToken == dto.Token);

            if (user == null || user.PasswordResetTokenExpiration < DateTime.UtcNow)
                return BadRequest("Invalid or expired token.");

            if (!IsPasswordStrong(dto.NewPassword))
                return BadRequest("Password must be at least 8 characters and include uppercase, lowercase, number, and special character.");

            user.password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiration = null;

            _context.SaveChanges();

            return Ok("Password has been reset successfully.");
        }

        // Authenticated users can update profile info (name, email, password)
        [Authorize]
        [HttpPut("update-profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not found.");

            var user = _context.Users.FirstOrDefault(u => u.id == int.Parse(userId));
            if (user == null) return NotFound("User not found.");

            bool isUpdated = false;
            List<string> updatedFields = new List<string>();

            // Update name
            if (!string.IsNullOrEmpty(dto.NewName) && dto.NewName != user.name)
            {
                user.name = dto.NewName;
                updatedFields.Add("Name");
                isUpdated = true;
            }

            // Update email
            if (!string.IsNullOrEmpty(dto.NewEmail) && dto.NewEmail != user.email)
            {
                var existing = _context.Users.FirstOrDefault(u => u.email == dto.NewEmail);
                if (existing != null)
                    return BadRequest("This email is already in use.");

                user.email = dto.NewEmail;
                user.IsEmailConfirmed = false;
                user.EmailConfirmationToken = Guid.NewGuid().ToString();
                user.TokenExpirationTime = DateTime.UtcNow.AddMinutes(20);
                updatedFields.Add("Email");
                isUpdated = true;

                var emailService = HttpContext.RequestServices.GetService<EmailService>();
                emailService?.SendConfirmationEmail(user.email, user.EmailConfirmationToken, user.name);
            }

            // Update password
            if (!string.IsNullOrEmpty(dto.CurrentPassword) && !string.IsNullOrEmpty(dto.NewPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.password))
                    return BadRequest("Current password is incorrect.");

                if (!IsPasswordStrong(dto.NewPassword))
                    return BadRequest("Password must be at least 8 characters and include uppercase, lowercase, number, and special character.");

                user.password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                updatedFields.Add("Password");
                isUpdated = true;
            }

            if (!isUpdated)
                return BadRequest("No changes were made.");

            _context.SaveChanges();

            string message = "Updated: " + string.Join(", ", updatedFields);
            return Ok(message);
        }
    }
}
