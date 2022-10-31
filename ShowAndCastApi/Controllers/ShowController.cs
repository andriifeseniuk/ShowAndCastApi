using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowAndCastApi.DTO;
using ShowAndCastApi.Models;
using ShowAndCastApi.Services;

namespace ShowAndCastApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowController : ControllerBase
    {
        private readonly ShowAndCastService showService;

        public ShowController(ShowAndCastService showService)
        {
            this.showService = showService;
        }

        // GET: api/<ShowController>
        [HttpGet]
        public async Task<ActionResult<IList<ShowDto>>> Get(int page = 0, int pageSize = 20)
        {
            var shows =  await this.showService.GetShows(page, pageSize);
            if (!shows.Any())
            {
                return NotFound();
            }

            return shows;
        }
    }
}
