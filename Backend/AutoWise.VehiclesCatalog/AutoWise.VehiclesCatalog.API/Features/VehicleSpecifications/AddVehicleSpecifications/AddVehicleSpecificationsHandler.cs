using AutoWise.CommonUtilities.MediatRAbstractions.Cqrs.Commands;

namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

public record AddVehicleSpecificationsCommand(string Vin) : ICommand<AddVehicleSpecificationsResult>;

public record AddVehicleSpecificationsResult(string Vin);

public class AddVehicleSpecificationsCommandHandler : ICommandHandler<AddVehicleSpecificationsCommand, AddVehicleSpecificationsResult>
{
    public async Task<AddVehicleSpecificationsResult> Handle(AddVehicleSpecificationsCommand command, CancellationToken cancellationToken)
    {
        return command.Vin is null
            ? throw new ArgumentNullException(nameof(command))
            : await Task.FromResult(new AddVehicleSpecificationsResult(command.Vin));
    }
}
