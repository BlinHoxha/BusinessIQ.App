using AutoMapper;
using BusinessIQ.Contracts.Businesses;
using BusinessIQ.Domain.Businesses;
using Framework.Application.Services;
using Framework.Contracts.Abstractions;
using Microsoft.Extensions.Logging;

namespace BusinessIQ.Application.Businesses;

public sealed class BusinessService(IBusinessRepository repository, ILogger<BusinessService> logger, IMapper mapper)
    : BaseService<Business, Guid>(repository, logger, mapper), IBusinessService
{
    public override Task<Guid> Add<TMap>(TMap dto, CancellationToken cancellationToken = default)
    {
        Validate(dto);
        return base.Add(dto, cancellationToken);
    }

    public override Task<Guid> Update<TMap>(TMap dto, CancellationToken cancellationToken = default)
    {
        Validate(dto);
        if (dto.Id == Guid.Empty) throw new ArgumentException("Business ID is required.");
        return base.Update(dto, cancellationToken);
    }

    public override Task AddRange<TMap>(IEnumerable<TMap> dtos, CancellationToken cancellationToken = default)
    {
        var items = dtos.ToArray();
        foreach (var dto in items) Validate(dto);
        return base.AddRange(items, cancellationToken);
    }

    // Generic detached bulk updates cannot preserve ownership and archive state.
    // Keep this unsupported until a transactional, authorized bulk use case exists.
    public override Task UpdateRange<TMap>(IEnumerable<TMap> dtos, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Bulk business updates are not supported.");

    private static void Validate(object dto)
    {
        if (dto is not IBusinessInput input ||
            string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 200 ||
            string.IsNullOrWhiteSpace(input.Industry) || input.Industry.Trim().Length > 100)
            throw new ArgumentException("Business name (1-200 characters) and industry (1-100 characters) are required.");
    }
}

