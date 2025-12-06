using MediatR;

namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

public record AddVehicleSpecificationsCommand(string Vin) : IRequest<AddVehicleSpecificationsResult>;

public record AddVehicleSpecificationsResult(string Vin);

public class AddVehicleSpecificationsCommandHandler : IRequestHandler<AddVehicleSpecificationsCommand, AddVehicleSpecificationsResult>
{
    public async Task<AddVehicleSpecificationsResult> Handle(AddVehicleSpecificationsCommand request, CancellationToken cancellationToken)
    {
        return request.Vin is null
            ? throw new ArgumentNullException(nameof(request))
            : await Task.FromResult(new AddVehicleSpecificationsResult(request.Vin));
    }
}
