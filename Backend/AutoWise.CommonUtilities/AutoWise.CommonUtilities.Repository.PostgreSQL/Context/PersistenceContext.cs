using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using AutoWise.CommonUtilities.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Primitives;

namespace AutoWise.CommonUtilities.Repository.PostgreSQL.Context;

public class PersistenceContext<TContext>(IServiceProvider serviceProvider) : IPersistenceContext where TContext : DbContext
{
    protected readonly TContext _dbContext = serviceProvider.GetRequiredService<TContext>();
    protected readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

    public async Task<int> SaveAsync()
    {
        var sessionUserId = GetSessionUserId();

        var entityEntries = _dbContext.ChangeTracker.Entries().Where(e => e.Entity is IIdBaseEntity && e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entityEntries)
        {
            if (entityEntry is IModifiedCreatedAuditBaseEntity)
            {
                SetCreationAndModificationAutidInfo(entityEntry, sessionUserId);
            }
            else if (entityEntry is ICreatedAuditBaseEntity)
            {
                SetCreationAutidInfo(entityEntry, sessionUserId);
            }
        }

        var affectedRows = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return affectedRows;
    }

    internal Guid GetSessionUserId()
    {
        Guid userIdentifier = Guid.Empty;
        if (_httpContextAccessor?.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("UserId", out StringValues authorizationValues);
            if (!StringValues.IsNullOrEmpty(authorizationValues))
            {
                userIdentifier = new Guid(authorizationValues.ToString());
            }
        }

        return userIdentifier;
    }

    private static void SetCreationAndModificationAutidInfo(EntityEntry entityEntry, Guid sessionUserId)
    {
        var entity = (IModifiedCreatedAuditBaseEntity)entityEntry.Entity;

        entity.CreatedOn = DateTime.UtcNow;
        entity.CreatedByUserId = sessionUserId;
        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedByUserId = sessionUserId;
    }

    private static void SetCreationAutidInfo(EntityEntry entityEntry, Guid sessionUserId)
    {
        var entity = (ICreatedAuditBaseEntity)entityEntry.Entity;

        entity.CreatedOn = DateTime.UtcNow;
        entity.CreatedByUserId = sessionUserId;
    }
}
