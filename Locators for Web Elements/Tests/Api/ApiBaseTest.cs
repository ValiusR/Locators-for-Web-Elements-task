using System.Net;
using RestSharp;
using Serilog;
using Xunit;
using Locators_for_Web_Elements.Core;
using Locators_for_Web_Elements.Core.RequestBuilders;
using Locators_for_Web_Elements.Tests;

[assembly: AssemblyFixture(typeof(TestEnvironmentFixture))]

namespace Locators_for_Web_Elements.Tests.Api;

public abstract class ApiBaseTest : IAsyncLifetime
{
    protected BaseApiClient BaseApiClient { get; private set; } = null!;
    protected ILogger Logger { get; }
    protected TestSettings Settings { get; }

    protected ApiBaseTest()
    {
        var fixture = TestEnvironmentFixture.Instance
            ?? throw new InvalidOperationException(
                $"{nameof(TestEnvironmentFixture)} was not initialized. " +
                "Check that the [assembly: AssemblyFixture(...)] attribute is present.");

        Settings = fixture.Settings;
        Logger = Log.ForContext(GetType());
        Logger.Information("Starting API test class: {TestClass}", GetType().Name);
    }

    public ValueTask InitializeAsync()
    {
        var apiSettings = new ApiSettings
        {
            ApiBaseUrl = Settings.ApiBaseUrl,
            ApiTimeoutMs = Settings.ApiTimeoutMs
        };

        BaseApiClient = new BaseApiClient(apiSettings, Logger);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Logger.Information("Disposing BaseApiClient for test class: {TestClass}", GetType().Name);
        BaseApiClient.Dispose();
        return ValueTask.CompletedTask;
    }

    protected async Task<RestResponse> SendAsync(RestRequest request, CancellationToken ct = default)
    {
        var response = await BaseApiClient.SendAsync(request, ct);
        return response;
    }

    protected async Task<RestResponse<T>> SendAsync<T>(RestRequest request, CancellationToken ct = default)
    {
        var response = await BaseApiClient.SendAsync<T>(request, ct);
        return response;
    }
}
