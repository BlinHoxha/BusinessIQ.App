using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using BuildingBlocks.AI.Core.Abstractions;
using BuildingBlocks.AI.Local;
using BusinessIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessIQ.IntegrationTests;

public sealed class AuthorizationTests
{
    [Theory]
    [InlineData(null, false, false, HttpStatusCode.Unauthorized)]
    [InlineData("staff", true, true, HttpStatusCode.Forbidden)]
    [InlineData("manager", false, true, HttpStatusCode.Forbidden)]
    [InlineData("manager", true, false, HttpStatusCode.Forbidden)]
    [InlineData("manager", true, true, HttpStatusCode.Created)]
    public async Task ProductionIngestionRequiresManagerAndMatchingMembership(
        string? role, bool matchingOrganization, bool matchingScope, HttpStatusCode expected)
    {
        await using var app = CreateProductionApp();
        using var client = app.CreateClient();
        var group = Guid.NewGuid();
        var location = Guid.NewGuid();
        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("Test-Role", role);
            client.DefaultRequestHeaders.Add("Test-Organization", (matchingOrganization ? group : Guid.NewGuid()).ToString());
            client.DefaultRequestHeaders.Add("Test-Scope", (matchingScope ? location : Guid.NewGuid()).ToString());
        }
        var response = await client.PostAsJsonAsync("/api/v1/knowledge/documents", new
        {
            title = "Closing", content = "The closing checklist requires staff to lock the terrace door.",
            profile = "business-operations", tenantId = group, scopeId = location
        });
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData(null, true, HttpStatusCode.Unauthorized)]
    [InlineData("manager", true, HttpStatusCode.Forbidden)]
    [InlineData("owner", false, HttpStatusCode.Forbidden)]
    [InlineData("owner", true, HttpStatusCode.Created)]
    public async Task ProductionBusinessWritesRequireOwnerAndAuthorizedOrganization(
        string? role, bool matchingOrganization, HttpStatusCode expected)
    {
        await using var app = CreateProductionApp();
        using var client = app.CreateClient();
        var organization = Guid.NewGuid();
        client.DefaultRequestHeaders.Add("X-Organization-Id", organization.ToString());
        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("Test-Role", role);
            client.DefaultRequestHeaders.Add("Test-Organization",
                (matchingOrganization ? organization : Guid.NewGuid()).ToString());
        }
        var response = await client.PostAsJsonAsync("/api/v1/businesses", new
        {
            name = "Example Consulting", industry = "Professional services"
        });
        Assert.Equal(expected, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateProductionApp() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            foreach (var setting in new Dictionary<string, string>
            {
                ["AI:Provider"] = "Azure",
                ["AI:Azure:OpenAiEndpoint"] = "https://example.test/",
                ["AI:Azure:ChatDeployment"] = "chat",
                ["AI:Azure:EmbeddingDeployment"] = "embedding",
                ["AI:Azure:SearchEndpoint"] = "https://example.test/",
                ["AI:Azure:SearchIndex"] = "test",
                ["Authentication:Authority"] = "https://example.test/",
                ["Authentication:Audience"] = "businessiq"
            }) builder.UseSetting(setting.Key, setting.Value);
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<BusinessDbContext>>();
                services.RemoveAll<DbContextOptions<BusinessDbContext>>();
                services.RemoveAll<BusinessDbContext>();
                var databaseName = Guid.NewGuid().ToString();
                services.AddDbContext<BusinessDbContext>(options => options.UseInMemoryDatabase(databaseName));
                services.AddSingleton<IKnowledgeIndex, InMemoryKnowledgeStore>();
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultForbidScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
            });
        });

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Test-Role", out var role))
                return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim("tenant_id", Request.Headers["Test-Organization"].ToString()),
                new Claim("scope_id", Request.Headers["Test-Scope"].ToString())
            ], "Test");
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
        }
    }
}

