namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

public record AddVehicleSpecificationsCommand(string Vin, IEnumerable<VehicleSpecification> Specifications) : ICommand<AddVehicleSpecificationsResult>;
public record AddVehicleSpecificationsResult(Guid Id, string Vin);
