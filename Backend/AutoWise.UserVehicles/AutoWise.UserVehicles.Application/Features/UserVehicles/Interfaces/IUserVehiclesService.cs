using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;

namespace AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;

public interface IUserVehiclesService
{
    Task<Guid> CreateAsync(CreateUserVehicleRequest request, Guid sessionUserId, CancellationToken ct = default);
    Task<UserVehicleResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateUserVehicleRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
