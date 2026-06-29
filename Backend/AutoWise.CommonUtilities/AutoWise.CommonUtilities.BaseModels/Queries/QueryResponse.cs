namespace AutoWise.CommonUtilities.Models.Queries;

public class QueryResponse<T>(IEnumerable<T> entities, PagedQueryResponse pagedQueryResponse) where T : class
{
    public IEnumerable<T> Entities { get; set; } = entities;
    public PagedQueryResponse PagedQueryResponse { get; set; } = pagedQueryResponse;
}
