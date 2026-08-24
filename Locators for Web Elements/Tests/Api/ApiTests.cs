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
        var response = await SendAsync(builder.Build());

        Assert.Equal((int)HttpStatusCode.OK, (int)response.StatusCode);
        Assert.NotNull(response.Content);
        Assert.NotEmpty(response.Content);

        var users = JsonSerializer.Deserialize<List<User>>(response.Content!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(users);
        Assert.NotEmpty(users);

        foreach (var user in users)
        {
            Assert.NotNull(user);
            Assert.False(string.IsNullOrWhiteSpace(user.Name), "User Name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(user.Username), "User Username should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(user.Email), "User Email should not be empty");
            Assert.NotNull(user.Address);
            Assert.False(string.IsNullOrWhiteSpace(user.Phone), "User Phone should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(user.Website), "User Website should not be empty");
            Assert.NotNull(user.Company);
        }

        Logger.Information("Task1 passed. Users count: {Count}, all required fields validated.", users.Count);
    }

    [Fact]
    public async Task Task2_GetAllUsers_ResponseHeaderContentTypeIsJsonUtf8()
    {
        Logger.Information("Task2: Validate response Content-Type header");

        var builder = new UsersRequestBuilder(Logger);
        var response = await SendAsync(builder.Build());

        Assert.Equal((int)HttpStatusCode.OK, (int)response.StatusCode);

        Assert.False(string.IsNullOrEmpty(response.ContentType),
            "Content-Type header is missing from response");

        Assert.Equal("application/json", response.ContentType);

        Logger.Information("Task2 passed. Content-Type: {ContentType}", response.ContentType);
    }

    [Fact]
    public async Task Task3_GetAllUsers_ValidatesArrayStructureAndUniqueness()
    {
        Logger.Information("Task3: Validate response body structure");

        var builder = new UsersRequestBuilder(Logger);
        var response = await SendAsync(builder.Build());

        Assert.Equal((int)HttpStatusCode.OK, (int)response.StatusCode);
        Assert.NotNull(response.Content);

        var users = JsonSerializer.Deserialize<List<User>>(response.Content!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(users);
        Assert.True(users.Count == 10, "Expected exactly 10 users in the response");

        var ids = users.Select(u => u.Id).ToList();
        Assert.True(ids.Distinct().Count() == 10, "All user IDs should be unique");

        foreach (var user in users)
        {
            Assert.False(string.IsNullOrWhiteSpace(user.Name), "User Name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(user.Username), "User Username should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(user.Company.Name), "User Company.Name should not be empty");
        }

        Logger.Information("Task3 passed. {Count} users, all unique IDs, all non-empty Name/Username/Company.Name.", users.Count);
    }

    [Fact]
    public async Task Task4_CreateUser_Returns201WithId()
    {
        Logger.Information("Task4: Validate user can be created");

        var newUser = new { name = "Test User", username = "testuser" };

        var builder = new UsersRequestBuilder(Logger)
            .WithMethod(Method.Post)
            .AddJsonBody(newUser);

        var response = await SendAsync(builder.Build());

        Assert.Equal((int)HttpStatusCode.Created, (int)response.StatusCode);
        Assert.NotNull(response.Content);
        Assert.NotEmpty(response.Content);

        using var doc = JsonDocument.Parse(response.Content!);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("id", out var idProp), "Response should contain 'id' field");
        Assert.True(idProp.ValueKind == JsonValueKind.Number, "'id' should be a number");
        Assert.True(idProp.GetInt32() > 0, "'id' should be greater than 0");

        Logger.Information("Task4 passed. Created user with ID: {Id}", idProp.GetInt32());
    }

    [Fact]
    public async Task Task5_InvalidEndpoint_Returns404()
    {
        Logger.Information("Task5: Validate 404 for non-existent resource");

        var request = new RestRequest("invalidendpoint", Method.Get);
        var response = await SendAsync(request);

        Assert.Equal((int)HttpStatusCode.NotFound, (int)response.StatusCode);

        Logger.Information("Task5 passed. Received 404 for invalid endpoint.");
    }
}
