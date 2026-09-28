using BusinessIQ.Application.Businesses;
using BusinessIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessIQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessInfrastructure(
        this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var provider = configuration["Database:Provider"] ?? (isDevelopment ? "InMemory" : "PostgreSql");
        if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase) && !isDevelopment)
            throw new InvalidOperationException("InMemory business persistence is only available in Development.");
        // One isolated database per host, shared across request scopes.
        var databaseName = "businessiq-" + Guid.NewGuid();
        services.AddDbContext<BusinessDbContext>(options =>
        {
            if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
                options.UseInMemoryDatabase(databaseName);
            else
            {
                var connection = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required for business persistence.");
                if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
                    options.UseNpgsql(connection);
                else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                    options.UseSqlServer(connection);
                else throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
            }
        });
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        return services;
    }
}

