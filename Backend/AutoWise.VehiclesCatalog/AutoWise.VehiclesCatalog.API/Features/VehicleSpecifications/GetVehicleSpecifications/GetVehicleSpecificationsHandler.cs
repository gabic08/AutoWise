namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.GetVehicleSpecifications;

public record GetVehicleSpecificationsQuery(string Vin) : IQuery<GetVehicleSpecificationsResult>;
public record GetVehicleSpecificationsResult(IEnumerable<VehicleSpecification> Specifications);

public class GetVehicleSpecificationsQueryHandler(MongoDbService mongoDbService) : IQueryHandler<GetVehicleSpecificationsQuery, GetVehicleSpecificationsResult>
{
    public async Task<GetVehicleSpecificationsResult> Handle(GetVehicleSpecificationsQuery query, CancellationToken cancellationToken)
    {
        var vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

        var specifications = await vehiclesDbSet
            .Find(v => v.Vin == query.Vin)
            .Project(v => v.Specifications)
            .FirstOrDefaultAsync(cancellationToken);

        if (specifications.NullOrEmpty())
        {
            throw new NotFoundException($"Vehicle with VIN '{query.Vin}' not found.");
        }

        return new GetVehicleSpecificationsResult(specifications);
    }
}
