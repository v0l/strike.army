namespace StrikeArmy.Services;

public class AnalyticsMiddleware : IMiddleware
{
    private readonly ILogger<AnalyticsMiddleware> _logger;
    private readonly PlausibleAnalytics _analytics;

    public AnalyticsMiddleware(ILogger<AnalyticsMiddleware> logger, PlausibleAnalytics analytics)
    {
        _logger = logger;
        _analytics = analytics;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await _analytics.TrackPageView(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track page view");
        }

        await next(context);
    }
}
