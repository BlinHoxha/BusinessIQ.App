using BuildingBlocks.AI.Core.Abstractions;
using BusinessIQ.Application.Businesses;
using BusinessIQ.Application.Mapping;
using Framework.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BusinessIQ.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessApplication(this IServiceCollection services)
    {
        services.AddApplication();
        services.Replace(ServiceDescriptor.Singleton<IDocumentProfileCatalog, BusinessDocumentProfiles>());
        services.AddAutoMapper(typeof(BusinessMappingProfile).Assembly);
        services.AddScoped<IBusinessService, BusinessService>();
        return services;
    }
}

