namespace AutoWise.UserVehicles.API.AppService.Interfaces;

public interface IUserVehicleService
{
    Task<Guid> AddUserVehicleAsync(AddVehicleRequest request, Guid sessionUserId, CancellationToken cancellationToken);
}
