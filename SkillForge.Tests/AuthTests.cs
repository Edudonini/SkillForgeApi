using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using SkillForge.Tests.Utils;

namespace SkillForge.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string Pass = "Admin123!@#";

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Gera_Token_Refresh()
    {
        var resp = await _client.PostAsJsonAsync("/login",
                    new { UserName = "admin", Password = Pass });

        var content = await resp.Content.ReadAsStringAsync();
        Console.WriteLine(">>> RESPONSE BODY:");
        Console.WriteLine(content);

        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Login falhou. Status: {(int)resp.StatusCode}. Body:\n{error}");
        }

        var body = JsonDocument.Parse(content).RootElement;
        body.TryGetProperty("access", out _).Should().BeTrue();
        body.TryGetProperty("refresh", out _).Should().BeTrue();
    }
}
