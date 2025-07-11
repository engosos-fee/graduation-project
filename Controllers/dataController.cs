using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_graduation.DTOclass;
using project_graduation.Model;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace project_graduation.Controllers
{
	[Authorize] // Requires authentication to access this controller
    [Route("api/[controller]")]
    [ApiController]
    public class dataController : ControllerBase
    {
        private readonly appDBcontext context;

        public dataController(appDBcontext _context)
        {
            this.context = _context;
        }

        // GET: /api/data/{id}
        // Retrieves user data along with their scan results
        [HttpGet("{userId:int}")]
        public IActionResult GetUserWithScans(int userId)
        {
            // Fetch user with their scan data (navigation property: datas)
            var user = context.Users
                .Include(u => u.datas)
                .FirstOrDefault(u => u.id == userId);

            if (user == null)
                return NotFound($"User with ID {userId} not found");

            // Map user data into a DTO for frontend response
            var userDto = new user_data_dto
            {
                id_user = user.id,
                name_user = user.name,
                scans = user.datas.Select(d => new ScanResultDto
                {
                    scanId = d.Id,
                    scanDate = d.date,
                    scanResult = d.Results
                }).ToList()
            };

            return Ok(userDto);
        }
    }
}
