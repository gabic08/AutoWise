namespace AutoWise.VehiclesCatalog.API.Features.VehicleSpecifications.ImportVehicleSpecifications;

public class ImportVehicleSpecificationsValidator : AbstractValidator<ImportVehicleSpecificationsCommand>
{
    public ImportVehicleSpecificationsValidator()
    {
        RuleFor(x => x.Vin)
            .NotEmpty().WithMessage("VIN must be provided.")
            .Length(17).WithMessage("VIN must be exactly 17 characters long.");
    }
}
