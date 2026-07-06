
using AutoWise.VehiclesCatalog.API.Utils;
using Grpc.Core;

namespace AutoWise.VehiclesCatalog.API.Grpc.Services;

public class VehicleSpecificationsService(GetVehicleSpecificationsConfig vehicleSpecificationsConfig, MongoDbService mongoDbService, ILogger<VehicleSpecificationsService> logger)
    : VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceBase
{
    public override async Task<GetVehicleSpecificationsResponseList> GetVehicleSpecifications(GetVehicleSpecificationsRequest request, ServerCallContext context)
    {
        var vehiclesDbSet = mongoDbService.Database.GetCollection<Vehicle>("vehicles");
        var response = new GetVehicleSpecificationsResponseList();


        var existingSpecifications = await ImportVehicleSpecificationsUtils.GetExistingVehicleSpecificationsAsync(request.Vin, vehiclesDbSet);
        if (ImportVehicleSpecificationsUtils.VehicleSpecificationsAreAlreadyImported(existingSpecifications))
        {
            response.Specifications.AddRange(existingSpecifications.Select(s => new GetVehicleSpecificationsResponse
            {
                Label = s.Label,
                Value = s.Value
            }));

            return response;
        }

        var specificationsToImport = await ImportVehicleSpecificationsUtils.FetchVehicleSpecificationsAsync(request.Vin, vehicleSpecificationsConfig, logger);
        await ImportVehicleSpecificationsUtils.SaveNewVehicleSpecificationsAsync(request.Vin, specificationsToImport, vehiclesDbSet);

        response.Specifications.AddRange(specificationsToImport.Select(s => new GetVehicleSpecificationsResponse
        {
            Label = s.Label,
            Value = s.Value
        }));
        return response;
    }
}
