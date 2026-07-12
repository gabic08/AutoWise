using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;

namespace AutoWise.UserVehicles.API.AppService.Interfaces;

public interface IUserVehicleService
{
    Task<Guid> AddUserVehicleAsync(CreateUserVehicleRequest request, Guid sessionUserId, CancellationToken cancellationToken);
}
