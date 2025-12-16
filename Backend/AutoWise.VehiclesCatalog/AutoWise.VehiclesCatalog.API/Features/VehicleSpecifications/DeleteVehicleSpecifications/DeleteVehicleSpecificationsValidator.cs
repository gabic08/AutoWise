namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.DeleteVehicleSpecifications;

public class DeleteVehicleSpecificationsValidator : AbstractValidator<DeleteVehicleSpecificationsCommand>
{
    public DeleteVehicleSpecificationsValidator()
    {
        RuleFor(x => x.Vin)
            .NotEmpty().WithMessage("VIN must be provided.");
    }
}
