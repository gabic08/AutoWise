using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;
using AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;

namespace AutoWise.UserVehicles.Application.Features.UserVehicles.Services;

public class UserVehicleEventService : IUserVehicleEventService
{
    public Task<Guid> AddEventAsync(Guid vehicleId, CreateUserVehicleEventRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<UserVehicleEventResponse> GetEventAsync(Guid vehicleId, Guid eventId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveEventAsync(Guid vehicleId, Guid eventId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateEventAsync(Guid vehicleId, Guid eventId, UpdateUserVehicleEventRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
