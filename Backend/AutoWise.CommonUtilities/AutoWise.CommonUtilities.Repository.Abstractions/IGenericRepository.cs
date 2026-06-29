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
    Task CreateAsync(IEnumerable<TEntity> entities);
    void Delete(TEntity entity);
    void Delete(IEnumerable<TEntity> entities);
    Task<int> ExecuteDeleteByConditionAsync(Expression<Func<TEntity, bool>> condition);
    Task<int> ExecuteDeleteByIdAsync(Guid id);


    Task<TResult> GetByConditionAsync<TResult>(Expression<Func<TEntity, bool>> condition,
        Expression<Func<TEntity, TResult>> selectQuery = null,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        RecordPosition recordPosition = RecordPosition.First,
        bool asNoTracking = true) where TResult : class;

    Task<TEntity> GetByIdAsync(Guid id, bool asNoTracking = true);

    Task<TResult> GetByIdAsync<TResult>(Guid id,
        Expression<Func<TEntity, TResult>> selectQuery = null,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        bool asNoTracking = true) where TResult : class;


    Task<QueryResponse<TResult>> GetByQueryOptionsAsync<TResult>(QueryOptions queryOptions,
        Expression<Func<TEntity, bool>> condition = null,
        Expression<Func<TEntity, TResult>> selectQuery = null,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        bool asNoTracking = true) where TResult : class;

    Task<IEnumerable<TResult>> GetManyByConditionAsync<TResult>(Expression<Func<TEntity, bool>> condition,
        Expression<Func<TEntity, TResult>> selectQuery,
        List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> includeQuery = null,
        SortEntityParameters<TEntity> sort = null,
        bool asNoTracking = true) where TResult : class;

    void Update(TEntity entity);
    void Update(IEnumerable<TEntity> entities);
}
