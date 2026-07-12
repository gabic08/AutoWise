using AutoWise.UserVehicles.Application.Features.UserVehicles.Dtos;

namespace AutoWise.UserVehicles.Application.Features.UserVehicles.Interfaces;

public interface IVehicleSpecificationsService
{
    Task<IEnumerable<VehicleSpecificationDto>> GetSpecificationsAsync(string vin, CancellationToken ct = default);
}
