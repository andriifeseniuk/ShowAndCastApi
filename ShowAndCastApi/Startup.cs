using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShowAndCastApi.Models;
using ShowAndCastApi.Services;

namespace ShowAndCastApi
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.HostingEnvironment = env;
        }

        public IWebHostEnvironment HostingEnvironment { get; set; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpClient("tvmaze", client =>
            {
                client.BaseAddress = new Uri("https://api.tvmaze.com/");
                client.DefaultRequestHeaders.Add("User-Agent", "ShowAndCastApi-AF-Sample");
            });
            services.AddSwaggerGen();
            services.AddSingleton<SyncSettings>(this.Configuration.GetSection("SyncSettings").Get<SyncSettings>());
            services.AddHostedService<SyncBackgroundService>();
            if (this.HostingEnvironment.IsDevelopment())
            {
                services.AddDbContext<ShowContext>(opt => opt.UseInMemoryDatabase("ShowAndCast"));
            }
            else
            {
                //todo implement real DB
                services.AddDbContext<ShowContext>(opt => opt.UseInMemoryDatabase("ShowAndCast"));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapSwagger();
            });
        }
    }
}
