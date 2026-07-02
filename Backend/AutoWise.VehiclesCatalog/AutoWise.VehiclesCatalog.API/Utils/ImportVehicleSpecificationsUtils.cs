namespace AutoWise.VehiclesCatalog.API.Utils;

public static class ImportVehicleSpecificationsUtils
{
    public static async Task<IEnumerable<VehicleSpecification>> GetExistingVehicleSpecificationsAsync(string vin, IMongoCollection<Vehicle> vehiclesDbSet, IDistributedCache cache, CancellationToken cancellationToken = default)
    {
        var cachedVehicleSpecifications = await cache.GetStringAsync(vin, cancellationToken);
        if (cachedVehicleSpecifications.NotNullOrEmpty())
        {
            return JsonSerializer.Deserialize<List<VehicleSpecification>>(cachedVehicleSpecifications);
        }

        return await vehiclesDbSet
            .Find(v => v.Vin == vin)
            .Project(v => v.Specifications)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static bool VehicleSpecificationsAreAlreadyImported(IEnumerable<VehicleSpecification> existingSpecifications)
    {
        return existingSpecifications.NotNullOrEmpty();
    }

    public static async Task<IEnumerable<VehicleSpecification>> FetchVehicleSpecificationsAsync(string vin, GetVehicleSpecificationsConfig vehicleSpecificationsConfig, ILogger logger, CancellationToken cancellationToken = default)
    {
        var getVehicleSpecificationUrl = vehicleSpecificationsConfig.GetUrl(vin);

        using var client = new HttpClient();
        var response = await client.GetAsync(getVehicleSpecificationUrl, cancellationToken);


        if (response.IsSuccessStatusCode)
        {
            var responseAsString = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogInformationMessage("Fetched vehicle specifications for VIN '{Vin}': {Response}", vin, responseAsString);

            var specifications = GetSpecificationsFromStringResponse(responseAsString);
            if (specifications.NotNullOrEmpty())
            {
                return specifications;
            }
        }

        logger.LogErrorMessage("Failed to fetch vehicle specifications for VIN '{Vin}'. {ErrMsg}", vin, response.ReasonPhrase);
        throw new BadRequestException($"Failed to retrieve specifications for vehicle with VIN '{vin}'");
    }

    public static List<VehicleSpecification> GetSpecificationsFromStringResponse(string responseAsString)
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


    public static async Task SaveNewVehicleSpecificationsAsync(string vin, IEnumerable<VehicleSpecification> specifications, IMongoCollection<Vehicle> vehiclesDbSet, CancellationToken cancellationToken = default)
    {
        var vehicle = new Vehicle
        {
            Vin = vin,
            Specifications = [.. specifications]
        };

        await vehiclesDbSet.InsertOneAsync(vehicle, options: null, cancellationToken);
    }
}
