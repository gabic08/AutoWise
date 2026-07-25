
using AutoWise.VehiclesCatalog.API.Utils;
using Grpc.Core;

namespace AutoWise.VehiclesCatalog.API.Grpc.Services;

public class VehicleSpecificationsService(GetVehicleSpecificationsConfig vehicleSpecificationsConfig, MongoDbService mongoDbService, IDistributedCache cache, ILogger<VehicleSpecificationsService> logger)
    : VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceBase
{
    public override async Task<GetVehicleSpecificationsResponseList> GetVehicleSpecifications(GetVehicleSpecificationsRequest request, ServerCallContext context)
    {
        var cacheKey = $"vehicle-specifications:{request.Vin}";

        var cachedSpecifications = await cache.GetStringAsync(cacheKey, context.CancellationToken);
        if (!cachedSpecifications.NullOrEmpty())
        {
            return BuildResponse(JsonSerializer.Deserialize<List<VehicleSpecification>>(cachedSpecifications));
        }

        var vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");

        var existingSpecifications = await ImportVehicleSpecificationsUtils.GetExistingVehicleSpecificationsAsync(request.Vin, vehiclesDbSet);
        if (ImportVehicleSpecificationsUtils.VehicleSpecificationsAreAlreadyImported(existingSpecifications))
        {
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(existingSpecifications), context.CancellationToken);
            return BuildResponse(existingSpecifications);
        }

        var specificationsToImport = await ImportVehicleSpecificationsUtils.FetchVehicleSpecificationsAsync(request.Vin, vehicleSpecificationsConfig, logger);
        await ImportVehicleSpecificationsUtils.SaveNewVehicleSpecificationsAsync(request.Vin, specificationsToImport, vehiclesDbSet);

        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(specificationsToImport), context.CancellationToken);
        return BuildResponse(specificationsToImport);
    }

    private static GetVehicleSpecificationsResponseList BuildResponse(IEnumerable<VehicleSpecification> specifications)
    {
        var response = new GetVehicleSpecificationsResponseList();
        response.Specifications.AddRange(specifications.Select(s => new GetVehicleSpecificationsResponse
        {
            Label = s.Label,
            Value = s.Value
        }));
        return response;
    }
}
