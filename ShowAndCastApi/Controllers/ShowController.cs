using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowAndCastApi.DTO;
using ShowAndCastApi.Models;

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
        public async Task<ActionResult<IList<ShowDto>>> Get(int page = 0, int pageSize = 20)
        {
            var skipCount = page * pageSize;
            var shows = await this.context.Shows
                .Include(s => s.Casts)
                .ThenInclude(c => c.Person)
                .Skip(skipCount)
                .Take(pageSize)
                .Select(s => new ShowDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Cast = s.Casts.Select(c => new PersonDto
                        {
                            Id = c.Id,
                            Name = c.Person.Name,
                            Birthday = c.Person.Birthday.ToString("yyyy-MM-dd")
                        })
                        .OrderByDescending(c => c.Birthday)
                        .ToList()
                })
                .ToListAsync();

            if (!shows.Any())
            {
                return NotFound();
            }

            return shows;
        }
    }
}
