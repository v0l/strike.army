using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StrikeArmy;
using StrikeArmy.Controllers;
using StrikeArmy.StrikeApi;

JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings)
{
    settings.NullValueHandling = NullValueHandling.Ignore;
    settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    settings.Converters = new JsonConverter[]
    {
        new StringEnumConverter()
    };

    return settings;
}

JsonConvert.DefaultSettings = () => ConfigureJson(new());

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var config = builder.Configuration.GetSection("Config").Get<StrikeArmyConfig>();
services.AddSingleton(config);
services.AddSingleton(config.Strike);

services.AddControllers();
services.AddHttpClient();
services.AddMemoryCache();

services.AddTransient<StrikeApi>();

services.AddControllers()
    .AddNewtonsoftJson(o => ConfigureJson(o.SerializerSettings));

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(ep =>
{
    ep.Map("/.well-known/lnurlp/{username}", ctx =>
    {
        if (ctx.Request.RouteValues.TryGetValue("username", out var username))
        {
            ctx.Response.Redirect($"/{PayController.PathBase}/{username as string}", true);
        }
        else
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        return Task.CompletedTask;
    });

    ep.MapControllers();
    ep.MapFallbackToFile("index.html");
});

app.Run();
