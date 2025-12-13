namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.ImportVehicleSpecifications;

public record ImportVehicleSpecificationsCommand(string Vin) : ICommand<ImportVehicleSpecificationsResult>;
public record ImportVehicleSpecificationsResult(IEnumerable<VehicleSpecification> Specifications);


public class ImportVehicleSpecificationsCommandHandler : ICommandHandler<ImportVehicleSpecificationsCommand, ImportVehicleSpecificationsResult>
{
    private readonly GetVehicleSpecificationsConfig _vehicleSpecificationsConfig;
    private readonly IMongoCollection<Vehicle> _vehiclesDbSet;
    private readonly ILogger _logger;

    public ImportVehicleSpecificationsCommandHandler(GetVehicleSpecificationsConfig vehicleSpecificationsConfig, MongoDbService mongoDbService)
    {
        _vehicleSpecificationsConfig = vehicleSpecificationsConfig;
        _vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");
        //_logger = logger;
    }

    public async Task<ImportVehicleSpecificationsResult> Handle(ImportVehicleSpecificationsCommand command, CancellationToken cancellationToken)
    {
        var existingSpecifications = await GetExistingVehicleSpecificationsAsync(command.Vin, cancellationToken);
        if (existingSpecifications.NotNullOrEmpty())
        {
            return new ImportVehicleSpecificationsResult(existingSpecifications);
        }

        var specifications = await FetchVehicleSpecificationsAsync(command.Vin, cancellationToken);
        await CreateNewVehicleSpecificationsAsync(command.Vin, specifications, cancellationToken);

        return new ImportVehicleSpecificationsResult(specifications);
    }


    #region Private Methods

    private async Task<IEnumerable<VehicleSpecification>> GetExistingVehicleSpecificationsAsync(string vin, CancellationToken cancellationToken)
    {
        return await _vehiclesDbSet
            .Find(v => v.Vin == vin)
            .Project(v => v.Specifications)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IEnumerable<VehicleSpecification>> FetchVehicleSpecificationsAsync(string vin, CancellationToken cancellationToken)
    {
        var getVehicleSpecificationUrl = _vehicleSpecificationsConfig.GetUrl(vin);

        using var client = new HttpClient();
        var response = await client.GetAsync(getVehicleSpecificationUrl, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var responseAsString = await response.Content.ReadAsStringAsync(cancellationToken);
            var specifications = GetSpecificationsFromStringResponse(responseAsString);

            if (specifications.NotNullOrEmpty())
            {
                return specifications;
            }
        }

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


    private async Task CreateNewVehicleSpecificationsAsync(string vin, IEnumerable<VehicleSpecification> specifications, CancellationToken cancellationToken)
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