namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.AddVehicleSpecifications;

public class AddVehicleSpecificationsValidator : AbstractValidator<AddVehicleSpecificationsCommand>
{
    public AddVehicleSpecificationsValidator()
    {
        RuleFor(x => x.Vin)
            .NotEmpty().WithMessage("VIN must be provided.")
            .Length(17).WithMessage("VIN must be exactly 17 characters long.");

        RuleFor(x => x.Specifications)
            .NotEmpty().WithMessage("At least one vehicle specification must be provided.");
    }
}
