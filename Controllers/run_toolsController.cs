using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using project_graduation.DTOclass;
using project_graduation.Model;
using System.Diagnostics;

namespace project_graduation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class run_toolsController : ControllerBase
    {
        private readonly appDBcontext context;

        public run_toolsController(appDBcontext _context)
        {
            // Injecting the database context
            context = _context;
        }

        // Runs a scan on the provided URL by ID, stores the result in the database
        [HttpPost("scan/{id}")]
        public async Task<IActionResult> ScanAndStoreResult(int id)
        {
            // Retrieve URL entity from the database
            var urlEntity = context.url_Input.FirstOrDefault(u => u.id == id);
            if (urlEntity == null)
                return NotFound("URL not found");

            // Run the scan tool (nuclei)
            string scanOutput = await RunNucleiAsync(urlEntity.Url);

            // Create a new data record with scan results
            var resultEntity = new data
            {
                Results = scanOutput,
                date = DateTime.Now,
                userid = urlEntity.userid
            };

            context.data.Add(resultEntity);
            await context.SaveChangesAsync();

            return Ok(resultEntity);
        }

        // Executes the nuclei scan tool on the specified URL
        private async Task<string> RunNucleiAsync(string url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "scripts/nuclei.exe", // Path to the scan executable
                Arguments = $"-u {url} ",        // Target URL as argument
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start the process and read the output
            var process = Process.Start(psi);
            string output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();
            return output;
        }
    }
}
