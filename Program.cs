using StrikeArmy;
using StrikeArmy.StrikeApi;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var config = builder.Configuration.GetSection("Config").Get<StrikeArmyConfig>();
services.AddSingleton(config);
services.AddSingleton(config.Strike);

services.AddControllers();
services.AddHttpClient();
services.AddMemoryCache();

services.AddTransient<StrikeApi>();

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(ep =>
{
    ep.MapControllers();
    ep.MapFallbackToFile("index.html");
});

app.Run();
