using AutoMapper;
using BusinessIQ.Application.Businesses;
using BusinessIQ.Domain.Businesses;
using Framework.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace BusinessIQ.Infrastructure.Persistence;

public sealed class BusinessRepository(BusinessDbContext context, ILogger<BusinessRepository> logger, IMapper mapper)
    : BaseRepository<Business, Guid, BusinessDbContext>(context, logger, mapper), IBusinessRepository;

