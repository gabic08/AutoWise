namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.GetVehicleSpecifications;

public class GetVehicleSpecificationsQueryHandler(MongoDbService mongoDbService) : IQueryHandler<GetVehicleSpecificationsQuery, GetVehicleSpecificationsResult>
{
    public async Task<GetVehicleSpecificationsResult> Handle(GetVehicleSpecificationsQuery query, CancellationToken cancellationToken)
    {
        var vehicleNotFoundErrMsg = $"Vehicle with VIN '{query.Vin}' not found.";
        if (query.Vin?.Length != 17)
        {
            throw new NotFoundException(vehicleNotFoundErrMsg);
        }

        var vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

        var specifications = await vehiclesDbSet
            .Find(v => v.Vin == query.Vin)
            .Project(v => v.Specifications)
            .FirstOrDefaultAsync(cancellationToken);

        if (specifications.NullOrEmpty())
        {
            throw new NotFoundException(vehicleNotFoundErrMsg);
        }

        return new GetVehicleSpecificationsResult(specifications);
    }
}
