using RestSharp;
using Serilog;

namespace Locators_for_Web_Elements.Core.RequestBuilders;

public sealed class UsersRequestBuilder : IRequestBuilder
{
    private RestRequest _request;
    private readonly ILogger _logger;

    public UsersRequestBuilder(ILogger logger)
    {
        _logger = logger;
        _request = new RestRequest("users", Method.Get);
    }

    public IRequestBuilder WithMethod(Method method)
    {
        _request.Method = method;
        _logger.Debug("Method changed to: {Method}", method);
        return this;
    }

    public IRequestBuilder AddHeader(string key, string value)
    {
        _request.AddHeader(key, value);
        _logger.Debug("Added header: {Key}={Value}", key, value);
        return this;
    }

    public IRequestBuilder AddQueryParameter(string key, string value)
    {
        _request.AddQueryParameter(key, value);
        _logger.Debug("Added query param: {Key}={Value}", key, value);
        return this;
    }

    public IRequestBuilder AddJsonBody(object body)
    {
        _request.AddJsonBody(body);
        _logger.Debug("Added JSON body: {Body}", body);
        return this;
    }

    public RestRequest Build()
    {
        _logger.Information("Built request: {Method} {Resource}", _request.Method, _request.Resource);
        return _request;
    }
}
