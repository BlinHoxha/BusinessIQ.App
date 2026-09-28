using System.Net;
using System.Net.Http.Json;
using AutoMapper;
using BusinessIQ.Contracts.Businesses;
using BusinessIQ.Infrastructure.Persistence;
using Framework.Contracts.Common;
using Framework.Contracts.Pagination;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BusinessIQ.IntegrationTests;

public sealed class BusinessCrudTests
{
    private static WebApplicationFactory<Program> CreateApp() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment("Development").ConfigureLogging(logging => logging.ClearProviders()));

    [Fact]
    public async Task InheritedCrudMapsProjectsIsolatesAndArchives()
    {
        await using var app = CreateApp();
        using var owner = app.CreateClient();
        using var other = app.CreateClient();
        var organizationId = Guid.NewGuid();
        owner.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
        other.DefaultRequestHeaders.Add("X-Organization-Id", Guid.NewGuid().ToString());

        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();

        var created = await owner.PostAsJsonAsync("/api/v1/businesses", new
        {
            name = "  North Construction  ", industry = "Construction",
            organizationId = Guid.NewGuid(), isArchived = true, id = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<CommonResult<EntityIdResponse<Guid>>>())!.Data!.Id;
        var details = await owner.GetFromJsonAsync<CommonResult<BusinessDto>>("/api/v1/businesses/" + id);
        Assert.Equal("North Construction", details!.Data!.Name);
        Assert.Equal(organizationId, details.Data.OrganizationId);
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync("/api/v1/businesses/" + id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PutAsJsonAsync("/api/v1/businesses",
            new UpdateBusinessDto { Id = id, Name = "Hijacked", Industry = "Retail" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync("/api/v1/businesses/" + id)).StatusCode);

        var updated = await owner.PutAsJsonAsync("/api/v1/businesses",
            new UpdateBusinessDto { Id = id, Name = "North Builders", Industry = "Construction" });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var page = await owner.GetFromJsonAsync<CommonResult<PageResponse<BusinessGridDto>>>(
            "/api/v1/businesses/filtered-search?PageNumber=1&PageSize=1&SortBy=Name");
        Assert.Equal(1, page!.Data!.TotalCount);
        Assert.Equal("North Builders", Assert.Single(page.Data.Items).Name);
        var otherPage = await other.GetFromJsonAsync<CommonResult<PageResponse<BusinessGridDto>>>(
            "/api/v1/businesses/filtered-search");
        Assert.Equal(0, otherPage!.Data!.TotalCount);

        Assert.Equal(HttpStatusCode.OK, (await owner.DeleteAsync("/api/v1/businesses/" + id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await owner.GetAsync("/api/v1/businesses/" + id)).StatusCode);
        // Verify DELETE used the shared method while preserving the row as archived.
        using var verificationScope = app.Services.CreateScope();
        var database = verificationScope.ServiceProvider.GetRequiredService<BusinessDbContext>();
        var archived = await database.Businesses.IgnoreQueryFilters().SingleAsync(x => x.Id == id);
        Assert.True(archived.IsArchived);
        Assert.Equal(organizationId, archived.OrganizationId);
    }

    [Fact]
    public async Task MissingOrganizationAndInvalidInputAreRejected()
    {
        await using var app = CreateApp();
        using var client = app.CreateClient();
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.GetAsync("/api/v1/businesses/filtered-search")).StatusCode);
        client.DefaultRequestHeaders.Add("X-Organization-Id", Guid.NewGuid().ToString());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/businesses",
            new CreateBusinessDto { Name = " ", Industry = "Retail" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/v1/businesses/filtered-search?PageSize=0")).StatusCode);
    }
}

