using System.Net;
using System.Text.Json;
using Xunit;
using RestSharp;
using Locators_for_Web_Elements.Business.ApiModels;
using Locators_for_Web_Elements.Core.RequestBuilders;

namespace Locators_for_Web_Elements.Tests.Api;

[Trait("Category", "API")]
public class ApiTests : ApiBaseTest
{
    [Fact]
    public async Task Task1_GetAllUsers_Returns200WithRequiredFields()
    {
        Logger.Information("Task1: Validate list of users received successfully");

        var builder = new UsersRequestBuilder(Logger);
        var response = await SendAsync<List<User>>(builder.Build(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(response.Content!);

        var users = response.Data!;

        Assert.NotEmpty(users);

        Assert.All(users, user =>
        {
            Assert.NotNull(user);
            Assert.False(string.IsNullOrWhiteSpace(user.Name), "User Name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(user.Username), "User Username should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(user.Email), "User Email should not be empty");
            Assert.NotNull(user.Address);
            Assert.False(string.IsNullOrWhiteSpace(user.Phone), "User Phone should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(user.Website), "User Website should not be empty");
            Assert.NotNull(user.Company);
        });
    }

    [Fact]
    public async Task Task2_GetAllUsers_ResponseHeaderContentTypeIsJsonUtf8()
    {
        Logger.Information("Task2: Validate response Content-Type header");

        var builder = new UsersRequestBuilder(Logger);
        var response = await SendAsync(builder.Build(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal("application/json", response.ContentType);
    }

    [Fact]
    public async Task Task3_GetAllUsers_ValidatesArrayStructureAndUniqueness()
    {
        Logger.Information("Task3: Validate response body structure");

        var builder = new UsersRequestBuilder(Logger);
        var response = await SendAsync<List<User>>(builder.Build(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(response.Content!);

        var users = response.Data!;

        Assert.NotEmpty(users);
        Assert.True(users.Count == 10, "Expected exactly 10 users in the response");

        var ids = users.Select(u => u.Id).ToList();
        Assert.True(ids.Distinct().Count() == 10, "All user IDs should be unique");

        Assert.All(users, user =>
        {
            Assert.False(string.IsNullOrWhiteSpace(user.Name), "User Name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(user.Username), "User Username should not be empty");
            Assert.NotNull(user.Company);
            Assert.False(string.IsNullOrWhiteSpace(user.Company.Name), "User Company.Name should not be empty");
        });
    }

    [Fact]
    public async Task Task4_CreateUser_Returns201WithId()
    {
        Logger.Information("Task4: Validate user can be created");

        var newUser = new { name = "Test User", username = "testuser" };

        var builder = new UsersRequestBuilder(Logger)
            .WithMethod(Method.Post)
            .AddJsonBody(newUser);

        var response = await SendAsync(builder.Build(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotEmpty(response.Content!);

        using var doc = JsonDocument.Parse(response.Content!);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("id", out var idProp), "Response should contain 'id' field");
        Assert.True(idProp.ValueKind == JsonValueKind.Number, "'id' should be a number");
        Assert.True(idProp.GetInt32() > 0, "'id' should be greater than 0");
    }

    [Fact]
    public async Task Task5_InvalidEndpoint_Returns404()
    {
        Logger.Information("Task5: Validate 404 for non-existent resource");

        var request = new RestRequest("invalidendpoint", Method.Get);
        var response = await SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
