using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace project_graduation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordCheckController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();

        [HttpPost]
        public async Task<IActionResult> CheckPassword([FromBody] PasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required.");

            // Hash password using SHA1
            string hash = ComputeSha1Hash(request.Password).ToUpper();
            string prefix = hash.Substring(0, 5);
            string suffix = hash.Substring(5);

            // Call HaveIBeenPwned API
            var response = await httpClient.GetAsync($"https://api.pwnedpasswords.com/range/{prefix}");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error contacting breach API.");

            var content = await response.Content.ReadAsStringAsync();
            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts[0].Trim().Equals(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    int breachCount = int.Parse(parts[1].Trim());
                    string message = $"⚠️ This password has been found in {breachCount} data breaches. Please choose a stronger, unique password.";

                    return Ok(new { breached = true, count = breachCount, message });
                }
            }

            return Ok(new
            {
                breached = false,
                message = "✅ This password has not been found in known data breaches. Good job!"
            });
        }

        private string ComputeSha1Hash(string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha1.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }

    public class PasswordRequest
    {
        public string Password { get; set; }
    }
}
