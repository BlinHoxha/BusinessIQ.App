using BusinessIQ.Domain.Businesses;
using Framework.Application.Abstractions.Services;

namespace BusinessIQ.Application.Businesses;

public interface IBusinessService : IBaseService<Business, Guid>;

