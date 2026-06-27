namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.GetVehicleSpecifications;

public class GetVehicleSpecificationsQueryHandler(MongoDbService mongoDbService, IDistributedCache cache)
    : IQueryHandler<GetVehicleSpecificationsQuery, GetVehicleSpecificationsResult>
{
    public async Task<GetVehicleSpecificationsResult> Handle(GetVehicleSpecificationsQuery query, CancellationToken cancellationToken)
    {
        var vehicleNotFoundErrMsg = $"Vehicle with VIN '{query.Vin}' not found.";
        if (query.Vin?.Length != 17)
        {
            throw new NotFoundException(vehicleNotFoundErrMsg);
        }

        List<VehicleSpecification> specifications;

        var cachedVehicleSpecifications = await cache.GetStringAsync(query.Vin, cancellationToken);
        if (cachedVehicleSpecifications.NullOrEmpty())
        {
            var vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

            specifications = await vehiclesDbSet
                .Find(v => v.Vin == query.Vin)
                .Project(v => v.Specifications)
                .FirstOrDefaultAsync(cancellationToken);

            if (specifications.NullOrEmpty())
            {
                throw new NotFoundException(vehicleNotFoundErrMsg);
            }

            var serializedSpecs = JsonSerializer.Serialize(specifications);
            await cache.SetStringAsync(query.Vin, serializedSpecs, cancellationToken);
        }
        else
        {
            specifications = JsonSerializer.Deserialize<List<VehicleSpecification>>(cachedVehicleSpecifications);
        }


        return new GetVehicleSpecificationsResult(specifications);
    }
}
