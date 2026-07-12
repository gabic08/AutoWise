using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;

namespace AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;

public interface IUserVehicleEventService
{
    Task<Guid> AddEventAsync(Guid vehicleId, CreateUserVehicleEventRequest request, CancellationToken ct = default);
    Task<UserVehicleEventResponse> GetEventAsync(Guid vehicleId, Guid eventId, CancellationToken ct = default);
    Task UpdateEventAsync(Guid vehicleId, Guid eventId, UpdateUserVehicleEventRequest request, CancellationToken ct = default);
    Task RemoveEventAsync(Guid vehicleId, Guid eventId, CancellationToken ct = default);
}
