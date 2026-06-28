using AutoWise.CommonUtilities.Models.Enums;
using System.Linq.Expressions;

namespace AutoWise.CommonUtilities.Models.Queries;

public class SortEntityParameters<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, object>> SortByCondition { get; set; }
    public SortOrder SortOrder { get; set; } = SortOrder.ASC;
}
