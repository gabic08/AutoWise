namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.DeleteVehicleSpecifications;

public record DeleteVehicleSpecificationsCommand(string Vin) : ICommand<DeleteVehicleSpecificationsResult>;
public record DeleteVehicleSpecificationsResult(bool Success);

public class DeleteVehicleSpecificationsCommandHandler(MongoDbService mongoDbService) : ICommandHandler<DeleteVehicleSpecificationsCommand, DeleteVehicleSpecificationsResult>
{
    public async Task<DeleteVehicleSpecificationsResult> Handle(DeleteVehicleSpecificationsCommand command, CancellationToken cancellationToken)
    {
        var vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

        var deleteResult = await vehiclesDbSet.DeleteOneAsync(v => v.Vin == command.Vin, cancellationToken);
        return new DeleteVehicleSpecificationsResult(deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0);
    }
}