namespace AutoWise.CommonUtilities.Models.Queries;

public class QueryResponse<T>
{
    public IEnumerable<T> Entities { get; set; }
    public PagedQueryResponse PagedQueryResponse { get; set; }
}
