namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

public record AddVehicleSpecificationsCommand(string Vin, List<VehicleSpecification> Specifications) : ICommand<AddVehicleSpecificationsResult>;
public record AddVehicleSpecificationsResult(Guid Id, string Vin);

public class AddVehicleSpecificationsCommandHandler (MongoDbService mongoDbService)
    : ICommandHandler<AddVehicleSpecificationsCommand, AddVehicleSpecificationsResult>
{
    public async Task<AddVehicleSpecificationsResult> Handle(AddVehicleSpecificationsCommand command, CancellationToken cancellationToken)
    {
        var vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

        var newVehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Vin = command.Vin,
            Specifications = [.. command.Specifications.Select(s => new VehicleSpecification
            {
                Label = s.Label,
                Value = s.Value
            })]
        };

        await vehiclesDbSet.InsertOneAsync(newVehicle, options: null, cancellationToken);

        return new AddVehicleSpecificationsResult(newVehicle.Id, command.Vin);
    }
}
