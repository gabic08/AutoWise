namespace AutoWise.UserVehicles.Application.Dtos;

public record GetUserVehiclesRequest(int Page = 1, int PageSize = 20, string Search = null, string SortBy = null, bool SortDescending = false);
