using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

public class StrikeApi
{
    private readonly ILogger<StrikeApi> _logger;
    private readonly HttpClient _client;

    public StrikeApi(StrikeApiSettings settings, ILogger<StrikeApi> logger)
    {
        _logger = logger;
        _client = new HttpClient
        {
            BaseAddress = settings.Uri ?? new Uri("https://api.strike.me/")
        };

        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.ApiKey}");
    }

    public Task<Invoice?> GenerateInvoice(CreateInvoiceRequest invoiceRequest)
    {
        var path = !string.IsNullOrEmpty(invoiceRequest.Handle)
            ? $"/v1/invoices/handle/{invoiceRequest.Handle}"
            : "/v1/invoices";

        return SendRequest<Invoice>(HttpMethod.Post, path, invoiceRequest);
    }

    public Task<Profile?> GetProfile(string handle)
    {
        return SendRequest<Profile>(HttpMethod.Get, $"/v1/accounts/handle/{handle}/profile");
    }

    public Task<Profile?> GetProfile(Guid id)
    {
        return SendRequest<Profile>(HttpMethod.Get, $"/v1/accounts/{id}/profile");
    }

    public Task<Invoice?> GetInvoice(Guid id)
    {
        return SendRequest<Invoice>(HttpMethod.Get, $"/v1/invoices/{id}");
    }

    public Task<InvoiceQuote?> GetInvoiceQuote(Guid id, string descriptionHash)
    {
        return SendRequest<InvoiceQuote>(HttpMethod.Post, $"/v1/invoices/{id}/quote?descriptionHash={descriptionHash}");
    }

    public Task<IEnumerable<WebhookSubscription>?> GetWebhookSubscriptions()
    {
        return SendRequest<IEnumerable<WebhookSubscription>>(HttpMethod.Get, "/v1/subscriptions");
    }

    public Task<WebhookSubscription?> CreateWebhook(NewWebhook hook)
    {
        return SendRequest<WebhookSubscription>(HttpMethod.Post, "/v1/subscriptions", hook);
    }

    public Task DeleteWebhook(Guid id)
    {
        return SendRequest<object>(HttpMethod.Delete, $"/v1/subscriptions/{id}");
    }

    private async Task<TReturn?> SendRequest<TReturn>(HttpMethod method, string path, object? bodyObj = default)
        where TReturn : class
    {
        var request = new HttpRequestMessage(method, path);
        if (bodyObj != default)
        {
            var reqJson = JsonConvert.SerializeObject(bodyObj);
            request.Content = new StringContent(reqJson, Encoding.UTF8, "application/json");
        }

        var rsp = await _client.SendAsync(request);
        var okResponse = method.Method switch
        {
            "POST" => HttpStatusCode.Created,
            _ => HttpStatusCode.OK
        };

        var json = await rsp.Content.ReadAsStringAsync();
        _logger.LogInformation(json);
        return rsp.StatusCode == okResponse ? JsonConvert.DeserializeObject<TReturn>(json) : default;
    }
}
