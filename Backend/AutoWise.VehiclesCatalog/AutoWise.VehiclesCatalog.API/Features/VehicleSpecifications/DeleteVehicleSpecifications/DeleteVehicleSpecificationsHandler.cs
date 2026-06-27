namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.DeleteVehicleSpecifications;

public class DeleteVehicleSpecificationsCommandHandler(MongoDbService mongoDbService, IDistributedCache cache) 
    : ICommandHandler<DeleteVehicleSpecificationsCommand, DeleteVehicleSpecificationsResult>
{
    public async Task<DeleteVehicleSpecificationsResult> Handle(DeleteVehicleSpecificationsCommand command, CancellationToken cancellationToken)
    {
        var vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

        var deleteResult = await vehiclesDbSet.DeleteOneAsync(v => v.Vin == command.Vin, cancellationToken);

        var success = deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        if (success)
        {
            await cache.RemoveAsync(command.Vin, cancellationToken);
            return new DeleteVehicleSpecificationsResult(true);
        }

        return new DeleteVehicleSpecificationsResult(false);
    }
}