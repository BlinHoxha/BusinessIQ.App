using BusinessIQ.Application.Abstractions;
using Framework.Domain.Exceptions;

namespace BusinessIQ.Api.Security;

internal sealed class CurrentOrganization(IHttpContextAccessor accessor, IHostEnvironment environment)
    : ICurrentOrganization
{
    public Guid OrganizationId
    {
        get
        {
            var context = accessor.HttpContext ?? throw new DomainException("An organization context is required.", 403);
            var selection = context.Request.Headers["X-Organization-Id"].ToString();
            if (environment.IsDevelopment() && context.User.Identity?.IsAuthenticated != true)
                return Parse(selection);

            var organizations = context.User.FindAll("tenant_id")
                .Select(claim => Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty).Distinct().ToArray();
            if (string.IsNullOrEmpty(selection) && organizations.Length == 1) return organizations[0];
            var selected = Parse(selection);
            if (!organizations.Contains(selected))
                throw new DomainException("Organization access is denied.", 403);
            return selected;
        }
    }

    private static Guid Parse(string value) =>
        Guid.TryParse(value, out var id) && id != Guid.Empty
            ? id : throw new DomainException("A valid X-Organization-Id is required.", 403);
}

