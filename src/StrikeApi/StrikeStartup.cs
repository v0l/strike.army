namespace StrikeArmy.StrikeApi;

public static class StrikeStartup
{
    public static void AddStrikeApi(this IServiceCollection services)
    {
        services.AddTransient<StrikeApi>();
        services.AddTransient<ProfileCache>();
        services.AddTransient<StrikeAuthService>();
    }
}
