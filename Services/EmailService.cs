using System.Net;
using System.Net.Mail;

namespace project_graduation.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendConfirmationEmail(string toEmail, string token, string userName)
        {
            var fromEmail = _config["EmailSettings:From"];
            var password = _config["EmailSettings:Password"];
            var link = $"http://localhost:5062/api/Auth/confirm-email?email={toEmail}&token={token}";

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
    }
}
