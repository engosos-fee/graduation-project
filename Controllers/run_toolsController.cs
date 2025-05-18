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
            context = _context;
        }


        [HttpPost("scan/{id}")]
        public async Task<IActionResult> ScanAndStoreResult(int id)
        {

            var urlEntity = context.url_Input.FirstOrDefault(u => u.id == id);
            if (urlEntity == null)
                return NotFound("URL not found");


            string scanOutput = await RunNucleiAsync(urlEntity.Url);

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



        ///////////////////////////////// runtools ///////////////////////////////////////////////
        private async Task<string> RunNucleiAsync(string url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "scripts/nuclei.exe", // nMAE OF file script
                Arguments = $"-u {url} ", //-json
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
