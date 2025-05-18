using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using project_graduation.DTOclass;
using project_graduation.Model;

namespace project_graduation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class urlController : ControllerBase
    {
        private readonly appDBcontext context;
        public urlController(appDBcontext _context)
        {
            this.context = _context;
        }
        [HttpPost]
        public IActionResult PostUrlOnly([FromBody] data_from_front dto)
        {
            if (ModelState.IsValid)
            {
                var newUrl = new url_input
                {
                    Url = dto.Url,
                    date = dto.Date,
                    userid = dto.UserId
                };

                context.url_Input.Add(newUrl);
                context.SaveChanges();


                return Ok();//new
                //{
                //    id = newUrl.id,
                //    url = newUrl.Url,
                //    date = newUrl.date,
                //    userId = newUrl.userid
                //});
            }

            return BadRequest(ModelState);
        }


    }
}

