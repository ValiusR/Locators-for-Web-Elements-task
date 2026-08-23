using RestSharp;
using Serilog;

namespace Locators_for_Web_Elements.Core;

public class BaseApiClient : IDisposable
{
    private readonly RestClient _restClient;
    private readonly ILogger _logger;

    public BaseApiClient(ApiSettings apiSettings, ILogger logger)
    {
        _logger = logger;
        var options = new RestClientOptions(apiSettings.ApiBaseUrl)
        {
            Timeout = TimeSpan.FromMilliseconds(apiSettings.ApiTimeoutMs)
        };

        _restClient = new RestClient(options);
        _logger.Information("BaseApiClient initialized. BaseUrl: {BaseUrl}, Timeout: {Timeout}ms",
            apiSettings.ApiBaseUrl, apiSettings.ApiTimeoutMs);
    }

    public async Task<RestResponse> SendAsync(RestRequest request, CancellationToken ct = default)
    {
        var url = _restClient.BuildUri(request);
        _logger.Information("Sending {Method} request to {Url}", request.Method, url);
        var response = await _restClient.ExecuteAsync(request, ct);
        _logger.Information("Received status {StatusCode} from {Url}", (int)response.StatusCode, url);
        return response;
    }

    public void Dispose()
    {
        _restClient.Dispose();
        _logger.Information("BaseApiClient disposed.");
    }
}
