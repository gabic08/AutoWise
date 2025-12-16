namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.GetVehicleSpecifications;

public record GetVehicleSpecificationsQuery(string Vin) : IQuery<GetVehicleSpecificationsResult>;
public record GetVehicleSpecificationsResult(IEnumerable<VehicleSpecification> Specifications);
