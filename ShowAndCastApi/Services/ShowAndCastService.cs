using Microsoft.EntityFrameworkCore;
using ShowAndCastApi.DTO;
using ShowAndCastApi.Models;

namespace ShowAndCastApi.Services
{
    public class ShowAndCastService
    {
        private readonly ShowContext context;

        public ShowAndCastService(ShowContext context)
        {
            this.context = context;
        }

        public async Task<List<ShowDto>> GetShows(int page, int pageSize)
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
            return shows;
        }
    }
}
