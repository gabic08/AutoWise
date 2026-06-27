using Grpc.Core;

namespace AutoWise.VehiclesCatalog.API.Grpc.Services;

public class VehicleSpecificationsService : VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceBase
{
    public override Task<GetVehicleSpecificationsResponseList> GetVehicleSpecifications(GetVehicleSpecificationsRequest request, ServerCallContext context)
    {
        return base.GetVehicleSpecifications(request, context);
    }
}
