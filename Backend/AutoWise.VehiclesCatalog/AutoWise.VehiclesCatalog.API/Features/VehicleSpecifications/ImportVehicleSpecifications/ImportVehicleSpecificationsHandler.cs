namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.ImportVehicleSpecifications;

public class ImportVehicleSpecificationsCommandHandler : ICommandHandler<ImportVehicleSpecificationsCommand, ImportVehicleSpecificationsResult>
{
    private readonly GetVehicleSpecificationsConfig _vehicleSpecificationsConfig;
    private readonly IMongoCollection<Vehicle> _vehiclesDbSet;
    private readonly ILogger<ImportVehicleSpecificationsCommandHandler> _logger;

    public ImportVehicleSpecificationsCommandHandler(GetVehicleSpecificationsConfig vehicleSpecificationsConfig, MongoDbService mongoDbService, ILogger<ImportVehicleSpecificationsCommandHandler> logger)
    {
        _vehicleSpecificationsConfig = vehicleSpecificationsConfig;
        _vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

        _logger = logger;
    }

    public async Task<ImportVehicleSpecificationsResult> Handle(ImportVehicleSpecificationsCommand command, CancellationToken cancellationToken)
    {
        var existingSpecifications = await GetExistingVehicleSpecificationsAsync(command.Vin, cancellationToken);
        if (VehicleSpecificationsAreAlreadyImported(existingSpecifications))
        {
            return new ImportVehicleSpecificationsResult(existingSpecifications);
        }

        var specificationsToImport = await FetchVehicleSpecificationsAsync(command.Vin, cancellationToken);
        await SaveNewVehicleSpecificationsAsync(command.Vin, specificationsToImport, cancellationToken);

        return new ImportVehicleSpecificationsResult(specificationsToImport);
    }


    #region Private Methods

    private async Task<IEnumerable<VehicleSpecification>> GetExistingVehicleSpecificationsAsync(string vin, CancellationToken cancellationToken)
    {
        return await _vehiclesDbSet
            .Find(v => v.Vin == vin)
            .Project(v => v.Specifications)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    private static bool VehicleSpecificationsAreAlreadyImported(IEnumerable<VehicleSpecification> existingSpecifications)
    {
        return existingSpecifications.NotNullOrEmpty();
    }

    private async Task<IEnumerable<VehicleSpecification>> FetchVehicleSpecificationsAsync(string vin, CancellationToken cancellationToken)
    {
        var getVehicleSpecificationUrl = _vehicleSpecificationsConfig.GetUrl(vin);

        using var client = new HttpClient();
        var response = await client.GetAsync(getVehicleSpecificationUrl, cancellationToken);


        if (response.IsSuccessStatusCode)
        {
            var responseAsString = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformationMessage("Fetched vehicle specifications for VIN '{Vin}': {Response}", vin, responseAsString);

            var specifications = GetSpecificationsFromStringResponse(responseAsString);
            if (specifications.NotNullOrEmpty())
            {
                return specifications;
            }
        }

        _logger.LogErrorMessage("Failed to fetch vehicle specifications for VIN '{Vin}'. {ErrMsg}", vin, response.ReasonPhrase);
        throw new BadRequestException($"Failed to retrieve specifications for vehicle with VIN '{vin}'");
    }

    private static List<VehicleSpecification> GetSpecificationsFromStringResponse(string responseAsString)
    {
        dynamic deserializedResult = new JavaScriptSerializer().DeserializeObject(responseAsString);

        var specifications = new List<VehicleSpecification>();
        foreach (var item in deserializedResult.Decode)
        {
            //if (item.Label is not string)
            //{
            //    continue;
            //}
            specifications.Add(new VehicleSpecification
            {
                Label = Convert.ToString(item.Label),
                Value = Convert.ToString(item.Value)
            });
        }

        return specifications;
    }


    private async Task SaveNewVehicleSpecificationsAsync(string vin, IEnumerable<VehicleSpecification> specifications, CancellationToken cancellationToken)
    {
        var vehicle = new Vehicle
        {
            Vin = vin,
            Specifications = [.. specifications]
        };

        await _vehiclesDbSet.InsertOneAsync(vehicle, options: null, cancellationToken);
    }

    #endregion

}
