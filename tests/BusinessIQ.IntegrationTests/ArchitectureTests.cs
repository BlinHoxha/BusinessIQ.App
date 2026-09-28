using BusinessIQ.Application.Businesses;
using BusinessIQ.Contracts.Businesses;
using BusinessIQ.Domain.Businesses;
using BusinessIQ.Infrastructure.Persistence;
using Framework.Application.Services;
using Framework.Infrastructure.Persistence;
using Framework.Infrastructure.Persistence.Repositories;
using Framework.Web.Controllers;

namespace BusinessIQ.IntegrationTests;

public sealed class ArchitectureTests
{
    [Fact]
    public void InnerLayersDoNotDependOnInfrastructureOrWeb()
    {
        foreach (var assembly in new[] { typeof(Business).Assembly, typeof(BusinessDto).Assembly, typeof(BusinessService).Assembly })
        {
            var references = assembly.GetReferencedAssemblies().Select(x => x.Name!).ToArray();
            Assert.DoesNotContain(references, x => x.EndsWith(".Infrastructure") || x.EndsWith(".Api") ||
                x.StartsWith("Microsoft.EntityFrameworkCore") || x.StartsWith("Microsoft.AspNetCore"));
        }
        Assert.DoesNotContain(typeof(Business).Assembly.GetReferencedAssemblies(),
            x => x.Name == "BusinessIQ.Application" || x.Name == "BusinessIQ.Contracts");
    }

    [Fact]
    public void BusinessSliceUsesSharedImplementationsWithoutReferencingReferenceHost()
    {
        Assert.Equal(typeof(FrameworkDbContext), typeof(BusinessDbContext).BaseType);
        Assert.Equal(typeof(BaseService<Business, Guid>), typeof(BusinessService).BaseType);
        Assert.Equal(typeof(BaseRepository<Business, Guid, BusinessDbContext>), typeof(BusinessRepository).BaseType);
        Assert.Equal(typeof(BaseControllerCRUD<,,,,,,>),
            typeof(BusinessIQ.Api.Controllers.BusinessesController).BaseType!.GetGenericTypeDefinition());
        Assert.DoesNotContain(typeof(Program).Assembly.GetReferencedAssemblies(), x => x.Name == "Framework.Api");
    }
}
