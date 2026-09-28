using BusinessIQ.Application.Abstractions;
using BusinessIQ.Domain.Businesses;
using Framework.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusinessIQ.Infrastructure.Persistence;

public sealed class BusinessDbContext(DbContextOptions<BusinessDbContext> options, ICurrentOrganization organization)
    : FrameworkDbContext(options)
{
    public DbSet<Business> Businesses => Set<Business>();
    private Guid OrganizationId => organization.OrganizationId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var business = modelBuilder.Entity<Business>();
        business.HasKey(x => x.Id);
        business.Property(x => x.Name).HasMaxLength(200).IsRequired();
        business.Property(x => x.Industry).HasMaxLength(100).IsRequired();
        business.HasIndex(x => new { x.OrganizationId, x.IsArchived });
        business.HasQueryFilter(x => x.OrganizationId == OrganizationId && !x.IsArchived);
    }

    public override int SaveChanges() => throw new NotSupportedException("Use SaveChangesAsync for authorized writes.");
    public override int SaveChanges(bool acceptAllChangesOnSuccess) => throw new NotSupportedException("Use SaveChangesAsync for authorized writes.");
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(true, cancellationToken);

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var organizationId = OrganizationId;
        foreach (var entry in ChangeTracker.Entries<Business>().ToArray())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.OrganizationId = organizationId;
                entry.Entity.IsArchived = false;
            }
            else if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                // Also check persisted ownership for detached entities; DTOs cannot transfer tenants.
                var ownsRecord = await Businesses.IgnoreQueryFilters().AsNoTracking().AnyAsync(
                    x => x.Id == entry.Entity.Id && x.OrganizationId == organizationId && !x.IsArchived,
                    cancellationToken);
                if (!ownsRecord || entry.Entity.OrganizationId != organizationId)
                    throw new KeyNotFoundException("Business was not found.");
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsArchived = true;
                }
            }
        }
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
