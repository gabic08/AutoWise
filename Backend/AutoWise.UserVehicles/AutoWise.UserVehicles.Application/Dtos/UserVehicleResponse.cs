namespace AutoWise.UserVehicles.Application.Dtos;

public record UserVehicleResponse(Guid Id, string LicensePlateNumber, string Make, string Model, string Vin, int? Year);
