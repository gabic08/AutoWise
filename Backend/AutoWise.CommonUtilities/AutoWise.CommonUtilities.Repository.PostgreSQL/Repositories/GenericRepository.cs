using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using AutoWise.CommonUtilities.Models.Enums;
using AutoWise.CommonUtilities.Models.Queries;
using AutoWise.CommonUtilities.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AutoWise.CommonUtilities.Repository.PostgreSQL.Repositories;

public class GenericRepository<TEntity>(DbContext dbContext) : IGenericRepository<TEntity> where TEntity : class, IIdBaseEntity
{
    protected readonly DbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    protected readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> condition = null, CancellationToken cancellationToken = default)
    {
        return condition is null ? await _dbSet.AnyAsync() : await _dbSet.AnyAsync(condition, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> condition = null, CancellationToken cancellationToken = default)
    {
        return condition is null ? await _dbSet.CountAsync(cancellationToken) : await _dbSet.CountAsync(condition, cancellationToken);
    }

    public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities != null && entities.Any())
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }
    }

    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public void Delete(IEnumerable<TEntity> entities)
    {
        if (entities != null && entities.Any())
        {
            _dbSet.RemoveRange(entities);
        }
    }

    public async Task<int> ExecuteDeleteByConditionAsync(Expression<Func<TEntity, bool>> condition, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(condition).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<int> ExecuteDeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<TResult> GetByConditionAsync<TResult>(Expression<Func<TEntity, bool>> condition,
        Expression<Func<TEntity, TResult>> selectQuery = null,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        RecordPosition recordPosition = RecordPosition.First,
        bool asNoTracking = true, CancellationToken cancellationToken = default) where TResult : class
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;

        if (includeQuery != null)
        {
            query = includeQuery.Aggregate(query, (current, include) => include(current));
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        if (sort != null)
        {
            query = sort.SortOrder == SortOrder.ASC ? query.OrderBy(sort.SortByCondition) : query.OrderByDescending(sort.SortByCondition);
        }

        if (selectQuery != null)
        {
            return recordPosition == RecordPosition.First ? await query.Select(selectQuery).FirstOrDefaultAsync(cancellationToken)
                : await query.Select(selectQuery).LastOrDefaultAsync(cancellationToken);
        }
        else
        {
            return recordPosition == RecordPosition.First ? await query.FirstOrDefaultAsync(cancellationToken) as TResult
                : await query.LastOrDefaultAsync(cancellationToken) as TResult;
        }
    }


    public async Task<TEntity> GetByIdAsync(Guid id, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<TResult> GetByIdAsync<TResult>(Guid id,
        Expression<Func<TEntity, TResult>> selectQuery = null,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        bool asNoTracking = true, CancellationToken cancellationToken = default) where TResult : class
    {
        return await GetByConditionAsync(e => e.Id == id, selectQuery, includeQuery, sort, RecordPosition.First, asNoTracking, cancellationToken);
    }


    public async Task<QueryResponse<TResult>> GetByQueryOptionsAsync<TResult>(QueryOptions queryOptions,
        Expression<Func<TEntity, bool>> condition = null,
        Expression<Func<TEntity, TResult>> selectQuery = null,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        bool asNoTracking = true, CancellationToken cancellationToken = default) where TResult : class
    {
        var pagedQueryResponse = (PagedQueryResponse)null;

        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;

        if (includeQuery != null)
        {
            query = includeQuery.Aggregate(query, (current, include) => include(current));
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        if (queryOptions != null)
        {
            // TODO: Implement sorting and pagination logic based on queryOptions
        }

        if (selectQuery != null)
        {
            var projection = query.Select(selectQuery);
            return new QueryResponse<TResult>(await projection.ToListAsync(cancellationToken), pagedQueryResponse);
        }

        return new QueryResponse<TResult>((ICollection<TResult>)await query.ToListAsync(cancellationToken), pagedQueryResponse);
    }


    public async Task<IEnumerable<TResult>> GetManyByConditionAsync<TResult>(Expression<Func<TEntity, bool>> condition,
        Expression<Func<TEntity, TResult>> selectQuery,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        bool asNoTracking = true, CancellationToken cancellationToken = default) where TResult : class
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;

        if (includeQuery != null)
        {
            query = includeQuery.Aggregate(query, (current, include) => include(current));
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        if (sort != null)
        {
            query = sort.SortOrder == SortOrder.ASC ? query.OrderBy(sort.SortByCondition) : query.OrderByDescending(sort.SortByCondition);
        }

        if (selectQuery != null)
        {
            return await query.Select(selectQuery).ToListAsync(cancellationToken);
        }

        return (ICollection<TResult>)await query.ToListAsync(cancellationToken);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void Update(IEnumerable<TEntity> entities)
    {
        if (entities != null && entities.Any())
        {
            _dbSet.UpdateRange(entities);
        }
    }
}
