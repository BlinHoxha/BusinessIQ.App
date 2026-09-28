using Framework.Domain.Abstractions;

namespace BusinessIQ.Domain.Businesses;

public sealed class Business : BaseEntity<Guid>
{
    public Business() => Id = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
}

