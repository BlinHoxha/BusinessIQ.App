using BuildingBlocks.AI.Core.Abstractions;
using BuildingBlocks.AI.Core.Models;

namespace BusinessIQ.Application;

public sealed class BusinessDocumentProfiles : IDocumentProfileCatalog
{
    private static readonly DocumentProfile[] Profiles =
    [
        new("business-operations", "Cross-industry business procedures, policies and operating manuals.",
            ["ingest", "question-answering"], ["*"], false),
        new("staff-onboarding", "Employee onboarding and training material across businesses and industries.",
            ["ingest", "question-answering"], ["*"], false),
        new("financial-documents", "Financial reports and supporting evidence for explanation; authoritative calculations belong to deterministic services.",
            ["ingest", "question-answering"], ["*"], true),
        new("business-planning", "Business plans, assumptions, objectives and advisory context requiring human review.",
            ["ingest", "question-answering"], ["*"], true),
        // Preserve existing document profile IDs as an optional industry profile.
        new("restaurant-operations", "Restaurant-specific procedures within the optional hospitality industry context.",
            ["ingest", "question-answering"], ["*"], false)
    ];

    public IReadOnlyCollection<DocumentProfile> GetAll() => Profiles.ToArray();

    public DocumentProfile GetRequired(string name) =>
        Profiles.FirstOrDefault(profile => string.Equals(profile.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Unknown business document profile '{name}'.", nameof(name));
}
