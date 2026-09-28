using System.ComponentModel.DataAnnotations;
using Framework.Contracts.Abstractions;

namespace BusinessIQ.Contracts.Businesses;

public interface IBusinessInput
{
    string Name { get; }
    string Industry { get; }
}

public sealed class CreateBusinessDto : IBusinessInput
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 1)]
    public string Industry { get; set; } = string.Empty;
}

public sealed class UpdateBusinessDto : BaseDto<Guid>, IBusinessInput
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 1)]
    public string Industry { get; set; } = string.Empty;
}

public sealed class BusinessDto : BaseDto<Guid>
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
}

public sealed class BusinessGridDto : BaseDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
}

