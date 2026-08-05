using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RotoMonster.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RotoMonster.Api
{
    [Route("api/[controller]")]
    public class PostApiController : Controller
    {
        private readonly IRMData playerData;

        public PostApiController(IRMData playerData)
        {
            this.playerData = playerData;
        }

        [Produces("application/json")]
        [HttpGet("search")]
        public async Task<ActionResult> Search()
        {
            try
            {
                string term = HttpContext.Request.Query["term"].ToString();
                var query = playerData.AutoCompletePlayerSearch(term);
                return Ok(query);
            }
            catch
            {
                return BadRequest();
            }
        }

    }
}
