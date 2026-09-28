using BusinessIQ.Domain.Businesses;
using Framework.Application.Abstractions.Persistence;

namespace BusinessIQ.Application.Businesses;

public interface IBusinessRepository : IBaseRepository<Business, Guid>;

