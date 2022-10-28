using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowAndCastApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ShowAndCastApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowController : ControllerBase
    {
        private readonly ShowContext context;

        public ShowController(ShowContext context)
        {
            this.context = context;
        }

        // GET: api/<ShowController>
        [HttpGet]
        public async Task<IEnumerable<Show>> Get()
        {
            return await this.context.Shows.ToListAsync();
        }

        // GET api/<ShowController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
    }
}
