using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using AutoWise.CommonUtilities.Models.Enums;
using AutoWise.CommonUtilities.Models.Queries;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AutoWise.CommonUtilities.Repository.Abstractions;

public interface IGenericRepository<TEntity> where TEntity : class, IIdBaseEntity
{
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> condition = null);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> condition = null, bool distinct = false);

    Task CreateAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);

    Task<int> ExecuteDeleteByConditionAsync(Expression<Func<TEntity, bool>> condition);

    Task<int> ExecuteDeleteByIdAsync(Guid id);

    Task<TEntity> GetByIdAsync(Guid id,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        bool asNoTracking = true);

    Task<QueryResponse<TEntity>> GetByQueryOptionsAsync(QueryOptions queryOptions,
        Expression<Func<TEntity, bool>> condition,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        bool asNoTracking = true);

    Task<QueryResponse<TResult>> GetByQueryOptionsAsync<TResult>(QueryOptions queryOptions,
        Expression<Func<TEntity, bool>> condition = null,
        Expression<Func<TEntity, TResult>> selectQuery = null,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        bool asNoTracking = true);

    Task<TEntity> GetConditionAsync(Expression<Func<TEntity, bool>> condition,
        RecordPosition recordPosition = RecordPosition.First,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        bool asNoTracking = true);

    Task<IEnumerable<TEntity>> GetManyByConditionAsync(Expression<Func<TEntity, bool>> condition,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        bool asNoTracking = true,
        bool asSplitQuery = false);

    Task<IEnumerable<TResult>> GetManyByConditionAsync<TResult>(Expression<Func<TEntity, bool>> condition,
        Expression<Func<TEntity, TResult>> selectQuery,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        bool asNoTracking = true,
        bool asSplitQuery = false);

    void Update(TEntity entity);
}
