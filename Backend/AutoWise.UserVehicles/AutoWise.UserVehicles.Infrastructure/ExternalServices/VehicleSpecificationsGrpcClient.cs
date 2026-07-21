namespace AutoWise.UserVehicles.Infrastructure.ExternalServices;

public class VehicleSpecificationsGrpcClient : IVehicleSpecificationsService
{
    private readonly VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceClient _grpcClient;
    private readonly ILogger<VehicleSpecificationsGrpcClient> _logger;

    public VehicleSpecificationsGrpcClient(
        VehicleSpecificationsProtoService.VehicleSpecificationsProtoServiceClient grpcClient,
        ILogger<VehicleSpecificationsGrpcClient> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<IEnumerable<VehicleSpecificationDto>> GetSpecificationsAsync(string vin, CancellationToken ct = default)
    {
        try
        {
            var request = new GetVehicleSpecificationsRequest { Vin = vin };
            var response = await _grpcClient.GetVehicleSpecificationsAsync(request, cancellationToken: ct);

            var specifications = response.Specifications
                .Select(s => new VehicleSpecificationDto(s.Label, s.Value));

            return specifications;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicle specifications for VIN {Vin}", vin);
            throw new BadRequestException($"Vehicle specifications service unavailable: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving vehicle specifications for VIN {Vin}", vin);
            throw new BadRequestException($"An unexpected error occurred: {ex.Message}");
        }
    }
}
