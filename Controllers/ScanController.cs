using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_graduation.DTOclass;
using project_graduation.Model;
using System.Diagnostics;

namespace project_graduation.Controllers
{
    [Route("api/scan")]
    [ApiController]
    public class ScanController : ControllerBase
    {
        private readonly appDBcontext context;

        public ScanController(appDBcontext _context)
        {
            context = _context;
        }

        // 1. Submit URL
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitUrl([FromBody] SubmitScanDto dto)
        {
            var newUrl = new url_input
            {
                Url = dto.Url,
                date = dto.Date,
                userid = dto.UserId,
                Status = "Pending"
            };

            context.url_Input.Add(newUrl);
            await context.SaveChangesAsync();

            return Ok(new
            {
                message = "URL submitted successfully",
                urlId = newUrl.id,
                status = newUrl.Status
            });
        }

        // 2. Run Scan
        [HttpPost("run/{urlId}")]
        public async Task<IActionResult> RunScan(int urlId)
        {
            var urlEntity = await context.url_Input.FindAsync(urlId);
            if (urlEntity == null)
                return NotFound("URL not found");

            string scanOutput = await RunNucleiAsync(urlEntity.Url);

            var result = new data
            {
                Results = scanOutput,
                date = DateTime.Now,
                userid = urlEntity.userid
            };

            context.data.Add(result);
            urlEntity.Status = "Completed";
            await context.SaveChangesAsync();

            return Ok(new
            {
                message = "Scan completed",
                resultId = result.Id,
                urlId = urlEntity.id
            });
        }

        // 3. Get History for User
        [HttpGet("history/{userId}")]
        public IActionResult GetHistory(int userId)
        {
            var urls = context.url_Input
                .Where(u => u.userid == userId)
                .Include(u => u.Users)
                .ToList();

            var dataMap = context.data
                .Where(d => d.userid == userId)
                .ToList();

            var history = urls.Select(u => new ScanDto
            {
                UrlId = u.id,
                Url = u.Url,
                Date = u.date,
                Status = u.Status,
                Result = dataMap.FirstOrDefault(d => d.date.Date == u.date.Date && d.userid == u.userid)?.Results
            }).ToList();

            return Ok(history);
        }

        // Helper to run the scanner script
        private async Task<string> RunNucleiAsync(string url)
        {
            var nucleiPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "nuclei.exe");
            var templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "templates");

            var psi = new ProcessStartInfo
            {
                FileName = nucleiPath,
                Arguments = $"-t \"{templatesPath}\" -u {url}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            string output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return output;
        }
    }
}
