namespace BusinessIQ.Application.Abstractions;

// Resolved by the host from authorized membership, never from write DTOs.
public interface ICurrentOrganization
{
    Guid OrganizationId { get; }
}

