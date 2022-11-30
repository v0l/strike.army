using Microsoft.AspNetCore.Authentication.Cookies;
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
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.Converters = new JsonConverter[]
        {
            new StringEnumConverter()
        };

        return settings;
    }

    public static async Task Main(string[] args)
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

        services.AddStrikeApi();
        services.AddTransient<UserService>();

        services.AddControllers()
            .AddNewtonsoftJson(o => ConfigureJson(o.SerializerSettings));

        ConfigureDb(services, builder.Configuration);
        services.AddHealthChecks();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(o => { o.LoginPath = "/auth"; });

        services.AddAuthorization();

        if (config.Plausible != null)
        {
            services.AddTransient<PlausibleAnalytics>();
            services.AddTransient<AnalyticsMiddleware>();
        }

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StrikeArmyContext>();
            await db.Database.MigrateAsync();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHealthChecks("/healthz");

        if (config.Plausible != null)
        {
            app.UseMiddleware<AnalyticsMiddleware>();
        }

        app.UseEndpoints(ep =>
        {
            ep.Map("/.well-known/lnurlp/{username}", ctx =>
            {
                if (ctx.Request.RouteValues.TryGetValue("username", out var username))
                {
                    ctx.Response.Redirect($"/{PayController.PathBase}/{username as string}", true);
                }

                return Task.CompletedTask;
            });

            ep.MapControllers();
            ep.MapFallbackToFile("index.html");
        });

        await app.RunAsync();
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
