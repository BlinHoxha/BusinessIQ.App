using System.Net;
using System.Net.Http.Json;
using Framework.Contracts.AI;
using Framework.Contracts.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace BusinessIQ.IntegrationTests;

public sealed class KnowledgeWorkflowTests
{
    private static WebApplicationFactory<Program> CreateApp() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment("Development").ConfigureLogging(logging => logging.ClearProviders()));

    [Theory]
    [InlineData("business-operations")]
    [InlineData("restaurant-operations")]
    public async Task BusinessManualAnswersWithCitationAndIsolatesOtherOrganizationsAndScopes(string profile)
    {
        await using var app = CreateApp();
        using var client = app.CreateClient();
        var group = Guid.NewGuid();
        var location = Guid.NewGuid();
        var document = new CreateKnowledgeDocumentRequest
        {
            Title = "Closing checklist",
            Content = "The closing checklist requires staff to lock the terrace door and return the keys to the manager.",
            Profile = profile, TenantId = group, ScopeId = location
        };
        var ingestion = await client.PostAsJsonAsync("/api/v1/knowledge/documents", document);
        Assert.Equal(HttpStatusCode.Created, ingestion.StatusCode);
        var saved = await ingestion.Content.ReadFromJsonAsync<CommonResult<KnowledgeDocumentResponse>>();
        var question = new AskKnowledgeRequest
        {
            Question = "What is the closing checklist?", Profile = document.Profile,
            TenantId = group, ScopeId = location
        };
        var answer = await Ask(client, question);
        Assert.True(answer.Grounded);
        Assert.Contains(answer.Citations, citation => citation.DocumentId == saved!.Data!.DocumentId);
        Assert.False((await Ask(client, question with { TenantId = Guid.NewGuid() })).Grounded);
        Assert.False((await Ask(client, question with { ScopeId = Guid.NewGuid() })).Grounded);
        Assert.False((await Ask(client, question with { ScopeId = null })).Grounded);
    }

    [Fact]
    public async Task OrganizationManualIsVisibleInAScopeAndUnknownProfilesAreRejected()
    {
        await using var app = CreateApp();
        using var client = app.CreateClient();
        var group = Guid.NewGuid();
        var document = new CreateKnowledgeDocumentRequest
        {
            Title = "Onboarding",
            Content = "During onboarding every staff member receives a tour from the shift manager.",
            Profile = "staff-onboarding", TenantId = group
        };
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync("/api/v1/knowledge/documents", document)).StatusCode);
        Assert.True((await Ask(client, new AskKnowledgeRequest
        {
            Question = "Who provides onboarding?", Profile = document.Profile,
            TenantId = group, ScopeId = Guid.NewGuid()
        })).Grounded);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/v1/knowledge/documents",
                document with { Profile = "general-document" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/v1/knowledge/documents",
                document with { Content = "" })).StatusCode);
    }

    [Fact]
    public async Task BusinessCatalogOverridesFrameworkDefaultCatalog()
    {
        await using var app = CreateApp();
        using var client = app.CreateClient();
        var response = await client.GetFromJsonAsync<CommonResult<DocumentProfileResponse[]>>(
            "/api/v1/ai/document-profiles");
        Assert.NotNull(response?.Data);
        Assert.Contains(response.Data, profile => profile.Name == "business-operations");
        Assert.Contains(response.Data, profile => profile.Name == "staff-onboarding");
        Assert.Contains(response.Data, profile => profile.Name == "restaurant-operations");
        Assert.Contains(response.Data, profile => profile.Name == "financial-documents" && profile.RequiresHumanReview);
        Assert.Contains(response.Data, profile => profile.Name == "business-planning" && profile.RequiresHumanReview);
        Assert.DoesNotContain(response.Data, profile => profile.Name == "general-document");
    }

    [Theory]
    [InlineData("financial-documents")]
    [InlineData("business-planning")]
    public async Task AdvisoryEvidenceRetainsCitationsAndHumanReviewNotice(string profile)
    {
        await using var app = CreateApp();
        using var client = app.CreateClient();
        var organization = Guid.NewGuid();
        var ingestion = await client.PostAsJsonAsync("/api/v1/knowledge/documents", new CreateKnowledgeDocumentRequest
        {
            Title = "Reviewed quarterly report",
            Content = "The quarterly report records a supplier cost increase. A manager must review the supporting evidence.",
            Profile = profile,
            TenantId = organization
        });
        Assert.Equal(HttpStatusCode.Created, ingestion.StatusCode);
        var saved = await ingestion.Content.ReadFromJsonAsync<CommonResult<KnowledgeDocumentResponse>>();
        var answer = await Ask(client, new AskKnowledgeRequest
        {
            Question = "What does the quarterly report record?",
            Profile = profile,
            TenantId = organization
        });
        Assert.True(answer.Grounded);
        Assert.Contains("Human verification is required", answer.Answer);
        Assert.Contains(answer.Citations, citation => citation.DocumentId == saved!.Data!.DocumentId);
    }

    private static async Task<GroundedAnswerResponse> Ask(HttpClient client, AskKnowledgeRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/v1/ai/answers", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CommonResult<GroundedAnswerResponse>>())!.Data!;
    }
}
