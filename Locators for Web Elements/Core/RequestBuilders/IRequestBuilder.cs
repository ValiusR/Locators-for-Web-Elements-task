using RestSharp;

namespace Locators_for_Web_Elements.Core.RequestBuilders;

public interface IRequestBuilder
{
    IRequestBuilder WithMethod(Method method);
    IRequestBuilder AddHeader(string key, string value);
    IRequestBuilder AddQueryParameter(string key, string value);
    IRequestBuilder AddJsonBody(object body);
    RestRequest Build();
}
