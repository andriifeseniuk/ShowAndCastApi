using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShowAndCastApi.DTO;
using ShowAndCastApi.Models;

namespace ShowAndCastApi.Services
{
    public class SyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IHttpClientFactory clientFactory;
        private readonly SyncSettings settings;
        private readonly ILogger logger;

        private int throttlingInterval;
        private DateTime throttlingChangedTime;

        public SyncBackgroundService(
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory clientFactory,
            SyncSettings settings,
            ILogger<SyncBackgroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.clientFactory = clientFactory;
            this.settings = settings;
            this.logger = logger;

            this.throttlingInterval = this.settings.MinThrottlingInterval;
            this.throttlingChangedTime = DateTime.MaxValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(this.throttlingInterval, stoppingToken);

                    using var scope = this.scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ShowContext>();
                    if (!await context.ShowSyncs.AnyAsync())
                    {
                        await context.ShowSyncs.AddAsync(new ShowSync {IsSyncCompleted = false, LastLoadedShowId = -1});
                        await context.SaveChangesAsync();
                    }

                    var sync = await context.ShowSyncs.FirstAsync();
                    if (sync.IsSyncCompleted)
                    {
                        await Task.Delay(this.settings.SyncCompletedInterval, stoppingToken);
                        sync.IsSyncCompleted = false;
                        await context.SaveChangesAsync();
                        continue;
                    }

                    if (!context.Shows.Any())
                    {
                        await this.LoadShowsPage(context, 0);
                        continue;
                    }

                    if (!context.Casts.Any())
                    {
                        var minShowId = await context.Shows.MinAsync(s => s.Id);
                        await this.LoadCastsForShow(context, minShowId);
                        continue;
                    }

                    var showsToLoad = context.Shows.Where(s => s.Id > sync.LastLoadedShowId);
                    if (await showsToLoad.AnyAsync())
                    {
                        var showIdToLoad = await showsToLoad.MinAsync(s => s.Id);
                        await LoadCastsForShow(context, showIdToLoad);
                    }
                    else
                    {
                        var maxShowId = await context.Shows.MaxAsync(s => s.Id);
                        var nextPage = maxShowId / this.settings.ShowsPageSize + 1;
                        await LoadShowsPage(context, nextPage);
                    }

                    if (DateTime.UtcNow - this.throttlingChangedTime > TimeSpan.FromMilliseconds(this.settings.ThrottlingRecalculateInterval))
                    {
                        this.DecreaseThrottlingInterval();
                    }
                }
                catch (Exception e)
                {
                    this.logger.LogError("Error occurred in SyncBackgroundService", e);
                }
            }
        }

        private async Task LoadCastsForShow(ShowContext context, long showId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"shows/{showId}/cast");
            var client = this.clientFactory.CreateClient("tvmaze");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var castDtos = JsonConvert.DeserializeObject<IEnumerable<CastDto>>(await response.Content.ReadAsStringAsync());
                var personsToAdd = castDtos.Select(c => new Person
                    {
                        Id = c.Person.Id,
                        Name = c.Person.Name,
                        Birthday = Convert.ToDateTime(c.Person.Birthday),
                    })
                    .GroupBy(p => p.Id)
                    .Select(g => g.First())
                    .ToList();
                var newPersons = personsToAdd.Where(newP => !context.Persons.Any(p => p.Id == newP.Id));
                await context.Persons.AddRangeAsync(newPersons);

                var castsToAdd = castDtos.Select(c => new Cast
                {
                    PersonId = c.Person.Id,
                    ShowId = showId
                }).ToList();
                await context.Casts.AddRangeAsync(castsToAdd);
                var sync = await context.ShowSyncs.FirstAsync();
                sync.LastLoadedShowId = showId;
                await context.SaveChangesAsync();
                this.logger.LogInformation("Loaded {0} Casts for Show {1}", castsToAdd.Count, showId);
            }
            else
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.TooManyRequests:
                        this.IncreaseThrottlingInterval();
                        this.logger.LogWarning("Too Many Requests when loading casts for show {0}", showId);
                        break;
                    default:
                        this.logger.LogWarning("{0} when loading loading casts for show {1}", response.StatusCode, showId);
                        break;
                }
            }
        }

        private async Task LoadShowsPage(ShowContext context, long page)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"shows?page={page}");
            var client = this.clientFactory.CreateClient("tvmaze");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var loadedShows = JsonConvert.DeserializeObject<IEnumerable<Show>>(await response.Content.ReadAsStringAsync());
                var newShows = loadedShows.ToList();
                await context.Shows.AddRangeAsync(newShows);
                await context.SaveChangesAsync();
                this.logger.LogInformation("Loaded {0} Shows from page {1}", newShows.Count, page);
            }
            else
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        var sync = await context.ShowSyncs.FirstAsync();
                        sync.IsSyncCompleted = true;
                        await context.SaveChangesAsync();
                        this.logger.LogInformation("Sync completed");
                        break;
                    case HttpStatusCode.TooManyRequests:
                        this.IncreaseThrottlingInterval();
                        this.logger.LogWarning("Too Many Requests when loading shows page {0}", page);
                        break;
                    default:
                        this.logger.LogWarning("{0} when loading shows page {1}", response.StatusCode, page);
                        break;
                }
            }
        }

        private void DecreaseThrottlingInterval()
        {
            var decreasedInterval = this.throttlingInterval / 2;
            this.throttlingInterval = decreasedInterval > this.settings.MaxThrottlingInterval 
                ? this.settings.MaxThrottlingInterval
                : decreasedInterval < this.settings.MinThrottlingInterval 
                    ? this.settings.MinThrottlingInterval
                    : decreasedInterval;
        }

        private void IncreaseThrottlingInterval()
        {
            var decreasedInterval = this.throttlingInterval * 2;
            this.throttlingInterval = decreasedInterval > this.settings.MaxThrottlingInterval 
                ? this.settings.MaxThrottlingInterval
                : decreasedInterval < this.settings.MinThrottlingInterval 
                    ? this.settings.MinThrottlingInterval
                    : decreasedInterval;
        }
    }
}