using System.Net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StrikeArmy.Controllers;
using StrikeArmy.Database;
using StrikeArmy.Services;
using StrikeArmy.StrikeApi;

namespace StrikeArmy;

public static class Program
{
    static JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings)
    {
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        settings.Converters = new JsonConverter[]
        {
            new StringEnumConverter()
        };

        return settings;
    }

    public static void Main(string[] args)
    {
        JsonConvert.DefaultSettings = () => ConfigureJson(new());

        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        var config = builder.Configuration.GetSection("Config").Get<StrikeArmyConfig>();
        services.AddSingleton(config);
        services.AddSingleton(config.Strike);

        services.AddControllers();
        services.AddHttpClient();
        services.AddMemoryCache();

        services.AddTransient<StrikeApi.StrikeApi>();
        services.AddTransient<ProfileCache>();
        services.AddTransient<UserService>();

        services.AddControllers()
            .AddNewtonsoftJson(o => ConfigureJson(o.SerializerSettings));

        ConfigureDb(services, builder.Configuration);

        var app = builder.Build();

        app.UseStaticFiles();
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
    }

    static void ConfigureDb(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StrikeArmyContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("Database")));
    }

    /// <summary>
    /// Dummy method for EF core migrations
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var dummyHost = Host.CreateDefaultBuilder(args);
        dummyHost.ConfigureServices((ctx, svc) => { ConfigureDb(svc, ctx.Configuration); });

        return dummyHost;
    }
}
