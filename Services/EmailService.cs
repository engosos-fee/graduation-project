using System.Net;
using System.Net.Mail;

namespace project_graduation.Services
{
    // Service responsible for sending emails (confirmation and password reset)
    public class EmailService
    {
        private readonly IConfiguration _config;

        // Inject configuration to read email settings
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // Sends a confirmation email with a verification link
        public void SendConfirmationEmail(string toEmail, string token, string userName)
        {
            var fromEmail = _config["EmailSettings:From"];
            var password = _config["EmailSettings:Password"];
            //local host --> http://localhost:5062/api/Auth/confirm-email?email={toEmail}&token={token}
            var link = $"https://graduation-project-wcad.onrender.com/api/Auth/confirm-email?email={toEmail}&token={token}";

            var subject = "Confirm your email";
            var body = $"<p>Hi {userName},</p><p>Please click the link below to confirm your email (valid for 20 minutes):</p><a href='{link}'>Confirm Email</a>";

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var message = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            smtp.Send(message);
        }

        // Sends a password reset email with a secure reset link
        public void SendResetPasswordEmail(string toEmail, string token, string userName)
        {
            var fromEmail = _config["EmailSettings:From"];
            var password = _config["EmailSettings:Password"];
            // local --> http://localhost:5062/reset-password.html?email={toEmail}&token={token}
            var link = $"https://secure-scan-jade.vercel.app/reset-password?email={toEmail}&token={token}";

            var subject = "Password Reset Request";
            var body = $"<p>Hi {userName},</p><p>Please click the link below to reset your password (valid for 20 minutes):</p><a href='{link}'>Reset Password</a>";

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var message = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            smtp.Send(message);
        }
    }
}
