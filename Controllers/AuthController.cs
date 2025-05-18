using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using project_graduation.DTOclass;
using project_graduation.Model;
using project_graduation.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        [HttpPost("register")]
        public IActionResult Register(RegisterDto request, [FromServices] EmailService emailService)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.email == request.Email);

            if (existingUser != null)
            {
                if (existingUser.IsEmailConfirmed)
                    return BadRequest("Email already exists.");

                // المستخدم سجل قبل كده بس لسه مفعلش، فلو التوكين انتهى نجدد التوكين ونبعتله تاني
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

            // تسجيل جديد تمام
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

        [HttpPost("login")]
        public IActionResult Login(LoginDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == request.Email);

            if (user == null)
            {
                return Unauthorized("Email not found."); 
            }

            if (!user.IsEmailConfirmed)
            {
                return Unauthorized("Please confirm your email address before logging in.");
            }

            // التحقق من إذا كان الحساب مقفول مؤقتًا
            if (user.LockoutEnd.HasValue)
            {
                if (user.LockoutEnd > DateTime.UtcNow)
                {
                    return Unauthorized("Account is locked. Please try again later.");
                }
                else
                {
                    // الوقت خلص، نعيد التهيئة
                    user.FailedLoginAttempts = 0;
                    user.LockoutEnd = null;
                    _context.SaveChanges();
                }
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.password))
            {
                // زيادة عدد المحاولات الفاشلة
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(20);  // حظر الحساب لمدة
                    _context.SaveChanges();
                    return Unauthorized("Account is locked. Please try again later.");
                }

                _context.SaveChanges();
                return Unauthorized("Invalid credentials.");
            }

            // إذا كانت المحاولات صحيحة، إعادة تعيين عدد المحاولات الفاشلة
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null; // إعادة تعيين وقت القفل
            _context.SaveChanges();

            // توليد التوكين وعودة النتيجة
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

        [HttpPost("resend-confirmation")]
        public IActionResult ResendConfirmation([FromQuery] string email, [FromServices] EmailService emailService)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == email);
            if (user == null)
                return NotFound("User not found.");

            if (user.IsEmailConfirmed)
                return BadRequest("Email already confirmed.");

            // التحقق من وقت الإرسال السابق
            if (user.LastConfirmationEmailSent != null && user.LastConfirmationEmailSent > DateTime.UtcNow.AddMinutes(-1))
            {
                return BadRequest("Please wait at least 1 minute before resending the confirmation email.");
            }

            // تحديث التوكين ووقت انتهاء التوكين
            user.EmailConfirmationToken = Guid.NewGuid().ToString();
            user.TokenExpirationTime = DateTime.UtcNow.AddMinutes(20);

            // تحديث وقت آخر إرسال للتأكيد
            user.LastConfirmationEmailSent = DateTime.UtcNow;

            _context.SaveChanges();

            // إرسال الإيميل
            emailService.SendConfirmationEmail(user.email, user.EmailConfirmationToken, user.name);

            return Ok("Confirmation email sent again.");
        }
    }
}
