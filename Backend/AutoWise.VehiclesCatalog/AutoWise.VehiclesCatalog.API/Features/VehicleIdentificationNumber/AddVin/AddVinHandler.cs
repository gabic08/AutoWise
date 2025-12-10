namespace AutoWise.VehiclesCatalog.API.Features.VehicleIdentificationNumber.AddVin;

public record AddVinCommand(string Vin) : ICommand<AddVinResult>;
public record AddVinResult(IEnumerable<VehicleSpecification> Specifications);


public class AddVinHandler : ICommandHandler<AddVinCommand, AddVinResult>
{
    private readonly GetVehicleSpecificationsConfig _vehicleSpecificationsConfig;
    private readonly IMongoCollection<Vehicle> _vehiclesDbSet;

    public AddVinHandler(GetVehicleSpecificationsConfig vehicleSpecificationsConfig, MongoDbService mongoDbService)
    {
        _vehicleSpecificationsConfig = vehicleSpecificationsConfig;
        _vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");
    }

    public async Task<AddVinResult> Handle(AddVinCommand command, CancellationToken cancellationToken)
    {
        var existingSpecifications = await GetExistingVehicleSpecificationsAsync(command.Vin, cancellationToken);
        if (existingSpecifications is not null && existingSpecifications.Any())
        {
            return new AddVinResult(existingSpecifications);
        }

        var specifications = await FetchVehicleSpecificationsAsync(command.Vin, cancellationToken);
        await CreateNewVehicleSpecificationsAsync(command.Vin, specifications, cancellationToken);

        return new AddVinResult(specifications);
    }

    private async Task CreateNewVehicleSpecificationsAsync(string vin, IEnumerable<VehicleSpecification> specifications, CancellationToken cancellationToken)
    {
        var vehicle = new Vehicle
        {
            Vin = vin,
            Specifications = specifications
        };

        await _vehiclesDbSet.InsertOneAsync(vehicle, options: null, cancellationToken);
    }

    private async Task<IEnumerable<VehicleSpecification>> GetExistingVehicleSpecificationsAsync(string vin, CancellationToken cancellationToken)
    {
        var vinFilter = Builders<Vehicle>.Filter.Eq(v => v.Vin, vin);
        return await _vehiclesDbSet
            .Find(vinFilter)
            .Project(v => v.Specifications)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IEnumerable<VehicleSpecification>> FetchVehicleSpecificationsAsync(string vin, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        var getVehicleSpecificationUrl = _vehicleSpecificationsConfig.GetUrl(vin);

        var response = await client.GetAsync(getVehicleSpecificationUrl, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var responseAsString = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseStringSpecifications(responseAsString);
        }
        else
        {
            throw new Exception("Failed to retrieve vehicle specifications from external service.");
        }
    }

    public IEnumerable<VehicleSpecification> ParseStringSpecifications(dynamic result, string vin)
    {
        foreach (var item in result.Decode)
        {
            if (!(item.Label is string || item.Label is int || item.Label is bool))
            {
                continue; // ignore non-string, not-int and non-bool values
            }
            decodeList.Add(new Specification
            { Id = Guid.NewGuid(), Label = item.Label, Order = order++, Value = Convert.ToString(item.Value), Vin = vin });
        }
    }
}