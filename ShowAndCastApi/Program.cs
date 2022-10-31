using Microsoft.EntityFrameworkCore;
using ShowAndCastApi.Models;
using ShowAndCastApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddSimpleConsole(c => c.TimestampFormat = "HH:mm:ss.fff ");
});

builder.Services.AddControllers();

if(builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ShowContext>(opt => opt.UseInMemoryDatabase("ShowAndCast"));
}
else
{
    // tode implement real DB
    builder.Services.AddDbContext<ShowContext>(opt => opt.UseInMemoryDatabase("ShowAndCast"));
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("tvmaze", client =>
{
    client.BaseAddress = new Uri("https://api.tvmaze.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "ShowAndCastApi-AF-Sample");
});
builder.Services.AddScoped<ShowAndCastService>();
builder.Services.AddSingleton<SyncSettings>(builder.Configuration.GetSection("SyncSettings").Get<SyncSettings>());
builder.Services.AddHostedService<SyncBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
