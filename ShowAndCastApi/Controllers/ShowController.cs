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
        public async Task<IEnumerable<Show>> Get(int page = 0, int pageSize = 20)
        {
            var skipCount = page > 0 ? (page - 1) * pageSize : 0;
            return await this.context.Shows
                .Include(s => s.Casts.OrderByDescending(c => c.Person.Birthday))
                .ThenInclude(c => c.Person)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
