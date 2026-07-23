namespace AutoWise.UserVehicles.Application.Services.Interfaces;

public interface IVehicleSpecificationsService
{
    Task<IEnumerable<VehicleSpecificationDto>> GetSpecificationsAsync(string vin, CancellationToken ct = default);
}
