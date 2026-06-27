namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

public class AddVehicleSpecificationsCommandHandler (MongoDbService mongoDbService, IDistributedCache cache) 
    : ICommandHandler<AddVehicleSpecificationsCommand, AddVehicleSpecificationsResult>
{
    public async Task<AddVehicleSpecificationsResult> Handle(AddVehicleSpecificationsCommand command, CancellationToken cancellationToken)
    {
        var vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

        var newVehicleSpecifications = command.Specifications.Select(s => new VehicleSpecification
        {
            Label = s.Label,
            Value = s.Value
        }).ToList();

        var newVehicle = new Vehicle
        {
            Vin = command.Vin,
            Specifications = newVehicleSpecifications
        };

        await vehiclesDbSet.InsertOneAsync(newVehicle, options: null, cancellationToken);

        await cache.SetStringAsync(command.Vin, JsonSerializer.Serialize(newVehicleSpecifications), cancellationToken);

        return new AddVehicleSpecificationsResult(newVehicle.Id, command.Vin);
    }
}
