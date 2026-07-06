using AutoWise.VehiclesCatalog.API.Utils;

namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.ImportVehicleSpecifications;

public class ImportVehicleSpecificationsCommandHandler : ICommandHandler<ImportVehicleSpecificationsCommand, ImportVehicleSpecificationsResult>
{
    private readonly GetVehicleSpecificationsConfig _vehicleSpecificationsConfig;
    private readonly IMongoCollection<Vehicle> _vehiclesDbSet;
    private readonly ILogger<ImportVehicleSpecificationsCommandHandler> _logger;

    public ImportVehicleSpecificationsCommandHandler(GetVehicleSpecificationsConfig vehicleSpecificationsConfig, MongoDbService mongoDbService,
        ILogger<ImportVehicleSpecificationsCommandHandler> logger)
    {
        _vehicleSpecificationsConfig = vehicleSpecificationsConfig;
        _vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");
        _logger = logger;
    }

    public async Task<ImportVehicleSpecificationsResult> Handle(ImportVehicleSpecificationsCommand command, CancellationToken cancellationToken)
    {
        var existingSpecifications = await ImportVehicleSpecificationsUtils.GetExistingVehicleSpecificationsAsync(command.Vin, _vehiclesDbSet, cancellationToken);
        if (ImportVehicleSpecificationsUtils.VehicleSpecificationsAreAlreadyImported(existingSpecifications))
        {
            return new ImportVehicleSpecificationsResult(existingSpecifications);
        }

        var specificationsToImport = await ImportVehicleSpecificationsUtils.FetchVehicleSpecificationsAsync(command.Vin, _vehicleSpecificationsConfig, _logger, cancellationToken);
        await ImportVehicleSpecificationsUtils.SaveNewVehicleSpecificationsAsync(command.Vin, specificationsToImport, _vehiclesDbSet, cancellationToken);

        return new ImportVehicleSpecificationsResult(specificationsToImport);
    }
}
