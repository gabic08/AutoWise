using AutoWise.VehiclesCatalog.API.Utils;

namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.ImportVehicleSpecifications;

public class ImportVehicleSpecificationsCommandHandler : ICommandHandler<ImportVehicleSpecificationsCommand, ImportVehicleSpecificationsResult>
{
    private readonly GetVehicleSpecificationsConfig _vehicleSpecificationsConfig;
    private readonly IMongoCollection<Vehicle> _vehiclesDbSet;
    private readonly ILogger<ImportVehicleSpecificationsCommandHandler> _logger;
    private readonly IDistributedCache _cache;

    public ImportVehicleSpecificationsCommandHandler(GetVehicleSpecificationsConfig vehicleSpecificationsConfig, MongoDbService mongoDbService,
        ILogger<ImportVehicleSpecificationsCommandHandler> logger, IDistributedCache cache)
    {
        _vehicleSpecificationsConfig = vehicleSpecificationsConfig;
        _vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

        _logger = logger;
        _cache = cache;
    }

    public async Task<ImportVehicleSpecificationsResult> Handle(ImportVehicleSpecificationsCommand command, CancellationToken cancellationToken)
    {
        var existingSpecifications = await ImportVehicleSpecificationsUtils.GetExistingVehicleSpecificationsAsync(command.Vin, _vehiclesDbSet, _cache, cancellationToken);
        if (ImportVehicleSpecificationsUtils.VehicleSpecificationsAreAlreadyImported(existingSpecifications))
        {
            return new ImportVehicleSpecificationsResult(existingSpecifications);
        }

        var specificationsToImport = await ImportVehicleSpecificationsUtils.FetchVehicleSpecificationsAsync(command.Vin, _vehicleSpecificationsConfig, _logger, cancellationToken);
        await ImportVehicleSpecificationsUtils.SaveNewVehicleSpecificationsAsync(command.Vin, specificationsToImport, _vehiclesDbSet, cancellationToken);

        await _cache.SetStringAsync(command.Vin, JsonSerializer.Serialize(specificationsToImport), cancellationToken);

        return new ImportVehicleSpecificationsResult(specificationsToImport);
    }
}
