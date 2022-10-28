using ShowAndCastApi.DTO;
using ShowAndCastApi.Models;

namespace ShowAndCastApi.Services
{
    public class SyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IHttpClientFactory clientFactory;

        public SyncBackgroundService(IServiceScopeFactory scopeFactory, IHttpClientFactory clientFactory)
        {
            this.scopeFactory = scopeFactory;
            this.clientFactory = clientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = this.scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ShowContext>();
                    var maxShowId = context.Shows.Max(c => (long?)c.Id) ?? 0;
                    var maxCastShowId = context.Casts.Max(c => (long?)c.ShowId) ?? 0;
                    if (maxCastShowId < 10000)
                    {
                        if (maxCastShowId < maxShowId)
                        {
                            await LoadCastsForShow(maxCastShowId, context);
                        }
                        else
                        {
                            await LoadNextShowsPage(maxShowId, context);
                        }
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception e)
                {
                    // todo
                }
            }
        }

        private async Task LoadCastsForShow(long maxCastShowId, ShowContext context)
        {
            var nextShowId = maxCastShowId + 1;
            var request = new HttpRequestMessage(HttpMethod.Get, $"shows/{nextShowId}/cast");
            var client = this.clientFactory.CreateClient("tvmaze");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var castDtos = await response.Content.ReadAsAsync<IEnumerable<CastDto>>();
                var personsToAdd = castDtos.Select(c => new Person
                {
                    Id = c.Person.Id,
                    Name = c.Person.Name,
                    Birthday = Convert.ToDateTime(c.Person.Birthday),
                }).DistinctBy(p => p.Id);
                var newPersons = personsToAdd.Where(newP => !context.Persons.Any(p => p.Id == newP.Id));
                await context.Persons.AddRangeAsync(newPersons);

                var castsToAdd = castDtos.Select(c => new Cast
                {
                    PersonId = c.Person.Id,
                    ShowId = nextShowId
                }).ToList();
                await context.Casts.AddRangeAsync(castsToAdd);
                await context.SaveChangesAsync();
            }
            else
            {
                // todo
            }
        }

        private async Task LoadNextShowsPage(long maxShowId, ShowContext context)
        {
            var page = (maxShowId + 1) / 250;
            var request = new HttpRequestMessage(HttpMethod.Get, $"shows?page={page}");
            var client = this.clientFactory.CreateClient("tvmaze");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var newShows = await response.Content.ReadAsAsync<IEnumerable<Show>>();
                await context.Shows.AddRangeAsync(newShows);
                await context.SaveChangesAsync();
            }
            else
            {
                // todo
            }
        }
    }
}
