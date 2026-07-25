namespace AutoWise.UserVehicles.Application.Services.Interfaces;

public interface IUserVehiclesService
{
    Task<Guid> CreateAsync(CreateUserVehicleRequest request, Guid sessionUserId, CancellationToken ct = default);
    Task<QueryResponse<UserVehicleResponse>> GetAllForUserAsync(Guid sessionUserId, GetUserVehiclesRequest request, CancellationToken ct = default);
    Task<UserVehicleResponse> GetByIdAsync(Guid id, Guid sessionUserId, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateUserVehicleRequest request, Guid sessionUserId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid sessionUserId, CancellationToken ct = default);
}
