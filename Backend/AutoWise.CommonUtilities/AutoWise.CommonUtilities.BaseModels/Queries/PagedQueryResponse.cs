namespace AutoWise.CommonUtilities.Models.Queries;

public class PagedQueryResponse
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPagesCount { get; set; }
    public int TotalItemsCount { get; set; }
}
