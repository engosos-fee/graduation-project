﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_graduation.DTOclass;
using project_graduation.Model;
using System.Diagnostics;
using System.Text.Json;

namespace project_graduation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class dataController : ControllerBase
    {
        private readonly appDBcontext context;
        public dataController(appDBcontext _context)
        {
            this.context = _context;
        }

        public appDBcontext Context { get; }

        [HttpGet("{id:int}")]
        public IActionResult result(int id)
        {
            users out_from_db = context.Users.Include(e => e.datas) .FirstOrDefault(e => e.id == id);
            var outtofront = new user_data_dto()
            {
                id_user = out_from_db.id,
                name_user = out_from_db.name,
                result = out_from_db.datas.Select(e => e.Results).ToList()
            };
            
            return Ok(outtofront);
        }
    }
}
