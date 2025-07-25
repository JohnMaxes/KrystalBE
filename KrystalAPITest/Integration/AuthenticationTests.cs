using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using FluentAssertions;
using KrystalAPI.Services;

namespace KrystalAPITest.Integration;

public class AuthenticationTests(JwtService jwt)
{
    [Fact]
    public async Task Login_Should_Return_Token()
    {
        var factory = new WebApplicationFactory<KrystalAPI.Program>();
        var client = factory.CreateClient();

        var loginRequest = new
        {
            username = "baodang",
            password = "Go050398551245!"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadAsStringAsync();
        var validated = jwt.ValidateToken(token);

        validated.Should().NotBeNull();
    }

    [Fact]
    public async Task Signup_Should_Return_Valid_Token()
    {
        var factory = new WebApplicationFactory<KrystalAPI.Program>();
        var client = factory.CreateClient();

        var signupRequest = new
        {
            username = "baodang",
            password = "Go050398551245!"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", signupRequest);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadAsStringAsync();
        var validated = jwt.ValidateToken(token);

        validated.Should().NotBeNull();
    }

    [Fact]
    public async Task Refresh_Token_Should_Work_Properly()
    {
        var factory = new WebApplicationFactory<KrystalAPI.Program>();
        var client = factory.CreateClient();

        var refresh_token_request = new
        {
        };
    }
}