using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using project_graduation.DTOclass;
using project_graduation.Model;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace project_graduation.Controllers
{
	[Authorize] // Require authentication for all actions in this controller
    [Route("api/[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly appDBcontext context;

        public ToolsController(appDBcontext _context)
        {
            context = _context;
        }

        // Unified API: Save URL, run nuclei, save result
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitUrlAndScan([FromBody] data_from_front dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Save URL to url_input table
            var newUrl = new url_input
            {
                Url = dto.Url,
                date = dto.Date,
                userid = dto.UserId
            };

            context.url_Input.Add(newUrl);
            await context.SaveChangesAsync();

            // 2. Run nuclei.exe on the given URL
            string scanOutput = await RunNucleiAsync(newUrl.Url);

            // 3. Save scan result to data table
            var resultEntity = new data
            {
                Results = scanOutput,
                date = DateTime.Now,
                userid = newUrl.userid
            };

            context.data.Add(resultEntity);
            await context.SaveChangesAsync();

            // 4. Return result to the frontend
            return Ok(new
            {
                message = "URL saved and scanned successfully",
                urlId = newUrl.id,
                resultId = resultEntity.Id,
                result = resultEntity.Results
            });
        }

        // Executes nuclei.exe and returns its output
        private async Task<string> RunNucleiAsync(string url)
        {
            // Path to nuclei.exe
            var nucleiPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "nuclei.exe");

            // Path to templates directory
            var templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "templates");

            var psi = new ProcessStartInfo
            {
                FileName = nucleiPath,
                Arguments = $"-t \"{templatesPath}\" -u {url}", // Run with template directory and URL
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
