using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AutoWise.CommonUtilities.Persistence.PostgreSQL.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditInfo(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyAuditInfo(DbContext context)
    {
        if (context == null)
        {
            return;
        }

        var sessionUserId = GetSessionUserId();
        var now = DateTime.UtcNow;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IIdBaseEntity &&
                (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.Entity is IModifiedCreatedAuditBaseEntity)
            {
                SetCreationAndModificationAuditInfo(entry, sessionUserId, now);
            }
            else if (entry.Entity is ICreatedAuditBaseEntity)
            {
                SetCreationAuditInfo(entry, sessionUserId, now);
            }
        }
    }

    private static Guid GetSessionUserId()
    {
        var userIdentifier = new Guid("54cf3f84-ef0b-47e7-9480-a6e5d0be9052");
        return userIdentifier;
    }

    private static void SetCreationAndModificationAuditInfo(EntityEntry entityEntry, Guid sessionUserId, DateTime now)
    {
        var entity = (IModifiedCreatedAuditBaseEntity)entityEntry.Entity;

        if (entityEntry.State == EntityState.Added)
        {
            entity.CreatedOn = now;
            entity.CreatedByUserId = sessionUserId;
        }

        entity.LastModifiedOn = now;
        entity.LastModifiedByUserId = sessionUserId;
    }

    private static void SetCreationAuditInfo(EntityEntry entityEntry, Guid sessionUserId, DateTime now)
    {
        var entity = (ICreatedAuditBaseEntity)entityEntry.Entity;

        entity.CreatedOn = now;
        entity.CreatedByUserId = sessionUserId;
    }
}
