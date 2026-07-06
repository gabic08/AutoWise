using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using AutoWise.CommonUtilities.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Primitives;

namespace AutoWise.CommonUtilities.Repository.PostgreSQL.Context;

public class AuditableDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor)
    : DbContext(options), IAuditableDbContext
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    private void ApplyAuditInfo()
    {
        var sessionUserId = GetSessionUserId();

        var entityEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is IIdBaseEntity &&
                (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entityEntries)
        {
            if (entityEntry.Entity is IModifiedCreatedAuditBaseEntity)
            {
                SetCreationAndModificationAuditInfo(entityEntry, sessionUserId);
            }
            else if (entityEntry.Entity is ICreatedAuditBaseEntity)
            {
                SetCreationAuditInfo(entityEntry, sessionUserId);
            }
        }
    }

    private Guid GetSessionUserId()
    {
        var userIdentifier = Guid.Empty;

        if (httpContextAccessor?.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("UserId", out StringValues authorizationValues);
            if (!StringValues.IsNullOrEmpty(authorizationValues))
            {
                userIdentifier = new Guid(authorizationValues.ToString());
            }
        }

        return userIdentifier;
    }

    private static void SetCreationAndModificationAuditInfo(EntityEntry entityEntry, Guid sessionUserId)
    {
        var entity = (IModifiedCreatedAuditBaseEntity)entityEntry.Entity;
        var now = DateTime.UtcNow;

        if (entityEntry.State == EntityState.Added)
        {
            entity.CreatedOn = now;
            entity.CreatedByUserId = sessionUserId;
        }

        entity.LastModifiedOn = now;
        entity.LastModifiedByUserId = sessionUserId;
    }

    private static void SetCreationAuditInfo(EntityEntry entityEntry, Guid sessionUserId)
    {
        var entity = (ICreatedAuditBaseEntity)entityEntry.Entity;

        entity.CreatedOn = DateTime.UtcNow;
        entity.CreatedByUserId = sessionUserId;
    }
}
